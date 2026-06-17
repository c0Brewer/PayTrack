// <copyright file="PushNotificationService.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Buffers.Binary;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PayTrack.Application.Dto.Notification;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Model;

namespace PayTrack.Application.Services.Implementation
{
    /// <inheritdoc/>
    public class PushNotificationService(
        IPushSubscriptionRepository repository,
        IOptions<PushNotificationSettings> options,
        HttpClient httpClient,
        ILogger<PushNotificationService> logger) : IPushNotificationService
    {
        private const int SaltLength = 16;
        private const int AesGcmTagLength = 16;
        private const int RecordSize = 4096;

        private readonly IPushSubscriptionRepository repository = repository;
        private readonly PushNotificationSettings settings = options.Value;
        private readonly HttpClient httpClient = httpClient;
        private readonly ILogger<PushNotificationService> logger = logger;

        /// <inheritdoc/>
        public async Task<PushNotificationConfigDto> GetConfigAsync(int userId)
        {
            return new PushNotificationConfigDto
            {
                IsConfigured = this.settings.IsConfigured,
                VapidPublicKey = this.settings.IsConfigured ? this.settings.PublicKey : null,
                Enabled = await this.repository.HasEnabledSubscriptionAsync(userId),
            };
        }

        /// <inheritdoc/>
        public async Task SaveSubscriptionAsync(int userId, SavePushSubscriptionDto subscription)
        {
            await this.repository.UpsertAsync(new PushSubscription
            {
                UserId = userId,
                Endpoint = subscription.Endpoint,
                P256dh = subscription.P256dh,
                Auth = subscription.Auth,
                IsEnabled = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            });
        }

        /// <inheritdoc/>
        public async Task DisableSubscriptionAsync(int userId, string endpoint)
        {
            await this.repository.DisableAsync(userId, endpoint);
        }

        /// <inheritdoc/>
        public async Task SendWorkflowStatusChangedAsync(int userId, string title, string body, string url)
        {
            if (!this.settings.IsConfigured)
            {
                this.logger.LogInformation("Push notification skipped because VAPID settings are not configured.");
                return;
            }

            var subscriptions = await this.repository.GetEnabledForUserAsync(userId);
            if (subscriptions.Count == 0)
            {
                return;
            }

            var payload = JsonSerializer.Serialize(new
            {
                notification = new
                {
                    title,
                    body,
                    icon = "/icons/icon-192x192.png",
                    badge = "/icons/icon-192x192.png",
                    data = new
                    {
                        url,
                        onActionClick = new
                        {
                            @default = new
                            {
                                operation = "openWindow",
                                url,
                            },
                        },
                    },
                },
            });

            foreach (var subscription in subscriptions)
            {
                await this.SendToSubscriptionAsync(subscription, payload);
            }
        }

        private static byte[] Base64UrlDecode(string value)
        {
            var padded = value.Replace('-', '+').Replace('_', '/');
            padded = padded.PadRight(padded.Length + ((4 - (padded.Length % 4)) % 4), '=');
            return Convert.FromBase64String(padded);
        }

        private static string Base64UrlEncode(byte[] value)
        {
            return Convert.ToBase64String(value)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Concat(params byte[][] arrays)
        {
            var length = arrays.Sum(a => a.Length);
            var result = new byte[length];
            var offset = 0;

            foreach (var array in arrays)
            {
                Buffer.BlockCopy(array, 0, result, offset, array.Length);
                offset += array.Length;
            }

            return result;
        }

        private static byte[] HmacSha256(byte[] key, byte[] value)
        {
            using var hmac = new HMACSHA256(key);
            return hmac.ComputeHash(value);
        }

        private static byte[] HkdfExpand(byte[] pseudoRandomKey, byte[] info, int length)
        {
            var result = new List<byte>(length);
            var previous = Array.Empty<byte>();
            byte counter = 1;

            while (result.Count < length)
            {
                previous = HmacSha256(pseudoRandomKey, Concat(previous, info, [counter]));
                result.AddRange(previous);
                counter++;
            }

            return result.Take(length).ToArray();
        }

        private static ECParameters GetPublicKeyParameters(byte[] publicKey)
        {
            if (publicKey.Length != 65 || publicKey[0] != 0x04)
            {
                throw new InvalidOperationException("The push subscription p256dh key is not a valid P-256 public key.");
            }

            return new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = publicKey[1..33],
                    Y = publicKey[33..65],
                },
            };
        }

        private static byte[] ExportRawPublicKey(ECDiffieHellman key)
        {
            var parameters = key.ExportParameters(false);
            return Concat([0x04], parameters.Q.X!, parameters.Q.Y!);
        }

        private static byte[] BuildEncryptedPayload(string payload, string p256dh, string auth)
        {
            var receiverPublicKeyBytes = Base64UrlDecode(p256dh);
            var receiverAuthSecret = Base64UrlDecode(auth);
            var salt = RandomNumberGenerator.GetBytes(SaltLength);

            using var senderKey = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            using var receiverPublicKey = ECDiffieHellman.Create(GetPublicKeyParameters(receiverPublicKeyBytes));

            var senderPublicKeyBytes = ExportRawPublicKey(senderKey);
            var sharedSecret = senderKey.DeriveRawSecretAgreement(receiverPublicKey.PublicKey);

            var keyInfo = Concat(
                Encoding.ASCII.GetBytes("WebPush: info\0"),
                receiverPublicKeyBytes,
                senderPublicKeyBytes);
            var pseudoRandomKey = HmacSha256(receiverAuthSecret, sharedSecret);
            var inputKeyingMaterial = HmacSha256(pseudoRandomKey, Concat(keyInfo, [0x01]));
            var contentEncryptionKeyInfo = Encoding.ASCII.GetBytes("Content-Encoding: aes128gcm\0");
            var nonceInfo = Encoding.ASCII.GetBytes("Content-Encoding: nonce\0");
            var contentEncryptionKey = HkdfExpand(HmacSha256(salt, inputKeyingMaterial), contentEncryptionKeyInfo, 16);
            var nonce = HkdfExpand(HmacSha256(salt, inputKeyingMaterial), nonceInfo, 12);
            var plainText = Concat(Encoding.UTF8.GetBytes(payload), [0x02]);
            var cipherText = new byte[plainText.Length];
            var tag = new byte[AesGcmTagLength];

            using (var aes = new AesGcm(contentEncryptionKey, AesGcmTagLength))
            {
                aes.Encrypt(nonce, plainText, cipherText, tag);
            }

            var recordSizeBytes = new byte[4];
            BinaryPrimitives.WriteUInt32BigEndian(recordSizeBytes, RecordSize);

            return Concat(
                salt,
                recordSizeBytes,
                [(byte)senderPublicKeyBytes.Length],
                senderPublicKeyBytes,
                cipherText,
                tag);
        }

        private string CreateVapidJwt(string endpoint)
        {
            var audience = new Uri(endpoint).GetLeftPart(UriPartial.Authority);
            var header = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { typ = "JWT", alg = "ES256" })));
            var payload = Base64UrlEncode(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
            {
                aud = audience,
                exp = DateTimeOffset.UtcNow.AddHours(12).ToUnixTimeSeconds(),
                sub = this.settings.Subject,
            })));
            var token = $"{header}.{payload}";

            var privateKey = Base64UrlDecode(this.settings.PrivateKey);
            using var ecdh = ECDiffieHellman.Create(new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                D = privateKey,
            });
            using var ecdsa = ECDsa.Create(ecdh.ExportParameters(true));
            var signature = ecdsa.SignData(
                Encoding.ASCII.GetBytes(token),
                HashAlgorithmName.SHA256,
                DSASignatureFormat.IeeeP1363FixedFieldConcatenation);

            return $"{token}.{Base64UrlEncode(signature)}";
        }

        private async Task SendToSubscriptionAsync(PushSubscription subscription, string payload)
        {
            try
            {
                var encryptedPayload = BuildEncryptedPayload(payload, subscription.P256dh, subscription.Auth);
                using var request = new HttpRequestMessage(HttpMethod.Post, subscription.Endpoint)
                {
                    Content = new ByteArrayContent(encryptedPayload),
                };

                request.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
                request.Content.Headers.ContentEncoding.Add("aes128gcm");
                request.Headers.TryAddWithoutValidation("Authorization", $"vapid t={this.CreateVapidJwt(subscription.Endpoint)}, k={this.settings.PublicKey}");
                request.Headers.TryAddWithoutValidation("TTL", this.settings.TimeToLiveSeconds.ToString());

                var response = await this.httpClient.SendAsync(request);
                if (response.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
                {
                    await this.repository.DisableByEndpointAsync(subscription.Endpoint);
                    return;
                }

                if (!response.IsSuccessStatusCode)
                {
                    this.logger.LogWarning(
                        "Push notification delivery failed for subscription {SubscriptionId} with status {StatusCode}.",
                        subscription.Id,
                        response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(ex, "Push notification delivery failed for subscription {SubscriptionId}.", subscription.Id);
            }
        }
    }
}
