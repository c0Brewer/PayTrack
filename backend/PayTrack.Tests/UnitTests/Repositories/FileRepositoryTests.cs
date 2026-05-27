using FluentAssertions;
using Microsoft.Extensions.Configuration;
using PayTrack.Application.Exceptions;
using PayTrack.Data.Repositories.Implementation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using PayTrack.Application.Services.Model;
using PayTrack.Data.Clients.Model;

namespace PayTrack.Tests.UnitTests.Repositories
{
    public class FileRepositoryTests
    {
        // ----------------------------
        // Helper: temp directory
        // ----------------------------
        private static string CreateTempFolder(string name)
        {
            var path = Path.Combine(
                Path.GetTempPath(),
                "PayTrackTests",
                name,
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(path);
            return path;
        }

        private static IConfiguration CreateConfig(
            string path,
            bool googleDriveEnabled = false,
            string? googleDriveRootFolderId = null,
            string? googleDriveServiceAccountKeyPath = null,
            string? googleDriveServiceAccountKeyBase64 = null)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Data:FileUploadPath"] = path,
                    ["GoogleDrive:Enabled"] = googleDriveEnabled.ToString(),
                    ["GoogleDrive:RootFolderId"] = googleDriveRootFolderId,
                    ["GoogleDrive:ServiceAccountKeyPath"] = googleDriveServiceAccountKeyPath,
                    ["GoogleDrive:ServiceAccountKeyBase64"] = googleDriveServiceAccountKeyBase64,
                })
                .Build();
        }

        private static IConfiguration CreateConfigWithoutGoogleDriveEnabled(string path)
        {
            return new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Data:FileUploadPath"] = path,
                })
                .Build();
        }

        private static IFormFile CreateMockFile(byte[] data, string fileName = "test.pdf")
        {
            var formFile = new FormFile(
                new MemoryStream(data),
                0,
                data.Length,
                "file",
                fileName
            );

            formFile.Headers = new HeaderDictionary();
            formFile.ContentType = fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase)
                ? "application/pdf"
                : "application/octet-stream";

            return formFile;
        }

        private static FileRepository CreateRepository(
            IConfiguration config,
            Mock<IGoogleDriveArchiveClient>? googleDriveArchiveClientMock = null,
            Mock<INotificationDispatchService>? notificationDispatchServiceMock = null)
        {
            return new FileRepository(
                config,
                googleDriveArchiveClientMock?.Object ?? Mock.Of<IGoogleDriveArchiveClient>(),
                notificationDispatchServiceMock?.Object ?? Mock.Of<INotificationDispatchService>(),
                Mock.Of<ILogger<FileRepository>>());
        }

        // ----------------------------
        // SaveFile
        // ----------------------------
        [Fact]
        public async Task SaveFile_ShouldSaveFile_AndReturnPath()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFile");
            var config = CreateConfig(folder);
            var repo = CreateRepository(config);

            var file = CreateMockFile([1, 2, 3], "test.pdf");

            // Act
            var result = await repo.SaveFile(file, "invoice_123");

            // Assert
            File.Exists(result).Should().BeTrue();

            var content = await File.ReadAllBytesAsync(result);
            content.Should().Equal(1, 2, 3);
        }

        [Fact]
        public async Task SaveFile_ShouldSaveFileLocallyAndArchiveToGoogleDrive_WhenGoogleDriveIsEnabled()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveArchive");
            var config = CreateConfig(folder, googleDriveEnabled: true, googleDriveRootFolderId: "root-folder-id");
            var googleDriveArchiveClientMock = new Mock<IGoogleDriveArchiveClient>();
            var repo = CreateRepository(config, googleDriveArchiveClientMock);

            var file = CreateMockFile([1, 2, 3], "receipt.pdf");

            // Act
            var result = await repo.SaveFile(file, "../unsafe/invoice_123");

            // Assert
            result.Should().Be(Path.Combine(folder, "invoice_123.pdf"));
            File.Exists(result).Should().BeTrue();
            googleDriveArchiveClientMock.Verify(
                client => client.ArchiveAsync(result, "invoice_123.pdf"),
                Times.Once);
        }

        [Fact]
        public async Task SaveFile_ShouldNotArchiveToGoogleDrive_WhenEnabledFlagIsMissing()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveDefaultDisabled");
            var config = CreateConfigWithoutGoogleDriveEnabled(folder);
            var googleDriveArchiveClientMock = new Mock<IGoogleDriveArchiveClient>();
            var repo = CreateRepository(config, googleDriveArchiveClientMock);

            var file = CreateMockFile([1, 2, 3], "test.pdf");

            // Act
            var result = await repo.SaveFile(file, "invoice_123");

            // Assert
            File.Exists(result).Should().BeTrue();
            googleDriveArchiveClientMock.Verify(
                client => client.ArchiveAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never);
        }

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenFileIsNull()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileNull");
            var config = CreateConfig(folder);
            var repo = CreateRepository(config);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(null!, "name");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidFileException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task SaveFile_ShouldThrow_WhenFileIsEmpty()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileEmpty");
            var config = CreateConfig(folder);
            var repo = CreateRepository(config);

            var file = CreateMockFile([]);

            // Act
            Func<Task> act = async () =>
                await repo.SaveFile(file, "name");

            // Assert
            await act.Should()
                .ThrowAsync<InvalidFileException>()
                .WithMessage("No file uploaded");
        }

        [Fact]
        public async Task SaveFile_ShouldReturnLocalPathAndNotifyFailureRecipient_WhenGoogleDriveArchiveFails()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveArchiveFails");
            var config = CreateConfig(folder, googleDriveEnabled: true);
            var googleDriveArchiveClientMock = new Mock<IGoogleDriveArchiveClient>();
            googleDriveArchiveClientMock
                .Setup(client => client.ArchiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Drive API unavailable"));
            var notificationDispatchServiceMock = new Mock<INotificationDispatchService>();
            var repo = CreateRepository(
                config,
                googleDriveArchiveClientMock,
                notificationDispatchServiceMock);

            var file = CreateMockFile([1, 2, 3]);

            // Act
            var result = await repo.SaveFile(file, "invoice_123");

            // Assert
            result.Should().Be(Path.Combine(folder, "invoice_123.pdf"));
            File.Exists(result).Should().BeTrue();
            notificationDispatchServiceMock.Verify(
                service => service.SendEmailAsync(
                    "gitlab@racing.tuwien.ac.at",
                    It.Is<string>(subject => subject.Contains("invoice_123.pdf", StringComparison.Ordinal)),
                    It.Is<string>(body => body.Contains("Drive API unavailable", StringComparison.Ordinal)),
                    It.Is<IReadOnlyCollection<EmailAttachment>>(attachments =>
                        attachments.Count == 1 &&
                        attachments.Single().FileName == "invoice_123.pdf" &&
                        attachments.Single().Content.SequenceEqual(new byte[] { 1, 2, 3 }) &&
                        attachments.Single().ContentType == "application/pdf")),
                Times.Once);
            notificationDispatchServiceMock.Verify(
                service => service.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<EmailAttachment>>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveFile_ShouldReturnLocalPath_WhenGoogleDriveArchiveAndAdminNotificationFail()
        {
            // Arrange
            var folder = CreateTempFolder("SaveFileDriveArchiveAndNotificationFail");
            var config = CreateConfig(folder, googleDriveEnabled: true);
            var googleDriveArchiveClientMock = new Mock<IGoogleDriveArchiveClient>();
            googleDriveArchiveClientMock
                .Setup(client => client.ArchiveAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Drive API unavailable"));
            var notificationDispatchServiceMock = new Mock<INotificationDispatchService>();
            notificationDispatchServiceMock
                .Setup(service => service.SendEmailAsync(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<EmailAttachment>>()))
                .ThrowsAsync(new InvalidOperationException("SMTP unavailable"));
            var repo = CreateRepository(
                config,
                googleDriveArchiveClientMock,
                notificationDispatchServiceMock);

            var file = CreateMockFile([1, 2, 3]);

            // Act
            var result = await repo.SaveFile(file, "invoice_123");

            // Assert
            result.Should().Be(Path.Combine(folder, "invoice_123.pdf"));
            File.Exists(result).Should().BeTrue();
            notificationDispatchServiceMock.Verify(
                service => service.SendEmailAsync(
                    "gitlab@racing.tuwien.ac.at",
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<IReadOnlyCollection<EmailAttachment>>()),
                Times.Once);
        }

        // ----------------------------
        // GetByPath
        // ----------------------------
        [Fact]
        public async Task GetByPath_ShouldReturnFileContent()
        {
            // Arrange
            var folder = CreateTempFolder("GetFile");
            var config = CreateConfig(folder);
            var repo = CreateRepository(config);

            var filePath = Path.Combine(folder, "test.txt");
            var data = new byte[] { 1, 2, 3 };

            await File.WriteAllBytesAsync(filePath, data);

            // Act
            var result = await repo.GetByPath(filePath);

            // Assert
            result.Should().Equal(data);
        }

        [Fact]
        public async Task GetByPath_ShouldThrow_WhenFileNotFound()
        {
            // Arrange
            var folder = CreateTempFolder("MissingFile");
            var config = CreateConfig(folder);
            var repo = CreateRepository(config);

            var fakePath = Path.Combine(folder, "missing.txt");

            // Act
            Func<Task> act = async () =>
                await repo.GetByPath(fakePath);

            // Assert
            await act.Should()
                .ThrowAsync<NotFoundException>()
                .WithMessage("Could not find receipt.");
        }

        [Fact]
        public async Task GetByPath_ShouldThrow_WhenPathIsOutsideBaseFolder()
        {
            // Arrange
            var folder = CreateTempFolder("UnsafePath");
            var config = CreateConfig(folder);
            var repo = CreateRepository(config);

            const string outsidePath = "/etc/passwd";

            // Act
            Func<Task> act = async () =>
                await repo.GetByPath(outsidePath);

            // Assert
            await act.Should()
                .ThrowAsync<InternalErrorException>()
                .WithMessage("Accessing unallowed path");
        }

    }
}
