// <copyright file="Program.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayTrack.Api.Endpoints;
using PayTrack.Api.Middleware;
using PayTrack.Application.Dto.Health;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Application.Settings;
using PayTrack.Data;
using PayTrack.Data.Clients.Implementation;
using PayTrack.Data.Clients.Model;
using PayTrack.Data.Entities;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Data.Repositories.Model;

var builder = WebApplication.CreateBuilder(args);
LoadGoogleConfigFromDotEnv(builder);

var isTestEnv = builder.Environment.IsEnvironment("Test");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (!isTestEnv)
{
    builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));
}

// Service
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IPaymentRequestByUserService, PaymentRequestByUserService>();
builder.Services.AddScoped<IPaymentRequestByTeamService, PaymentRequestByTeamService>();
builder.Services.AddScoped<ICostCentreService, CostCentreService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<IGoogleDriveArchiveClient, GoogleDriveArchiveClient>();
builder.Services.AddScoped<IBudgetService, BudgetService>();
builder.Services.AddScoped<ISeasonService, SeasonService>();

// Notification
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("Email"));
builder.Services.Configure<SlackSettings>(builder.Configuration.GetSection("Slack"));
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddHttpClient<NotificationDispatchService>();
builder.Services.AddScoped<INotificationDispatchService, NotificationDispatchService>();

// Repositories
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<IFileRepository, FileRepository>();
builder.Services.AddScoped<ICostCentreRepository, CostCentreRepository>();
builder.Services.AddScoped<IBudgetRepository, BudgetRepository>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<ISeasonRepository, SeasonRepository>();

builder.Services.AddExceptionHandler<EndpointExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<HealthState>();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddHttpClient();

var jwtSecret = builder.Configuration["JWT:Secret"] ?? throw new InternalErrorException("Could not load JWT Secret");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(nameof(Role.Admin), policy =>
            policy.RequireRole(nameof(Role.Admin)))
    .AddPolicy(nameof(Role.TeamLead), policy =>
            policy.RequireRole(nameof(Role.TeamLead), nameof(Role.Admin)));

var corsOrigin = builder.Configuration["CORS:Origins"] ?? throw new InternalErrorException("Could not load CORS Origins");
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        // WithOrigins("*") does not mean "allow all origins" in ASP.NET Core.
        // The development config uses "*", so we translate that case explicitly.
        if (corsOrigin == "*")
        {
            policy
                .AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod();
            return;
        }

        policy
            .WithOrigins(corsOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();
var frontendIndexPath = Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "index.html");
var hasFrontendBundle = File.Exists(frontendIndexPath);

// Auto-apply migrations (According to Config)
var migrationsRunConfig = builder.Configuration.GetValue<bool>("Migrations:Auto");
if (migrationsRunConfig && !isTestEnv)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

var seedDataConfig = builder.Configuration.GetValue<bool>("SeedData:Auto");
if (seedDataConfig && !isTestEnv)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await DbSeeder.SeedAsync(db);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseForwardedHeaders();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("frontend");
app.MapHealthEndpoints();

if (hasFrontendBundle)
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

var apiV1 = app
    .MapGroup("/api/v1")
    .AddEndpointFilter<AutoValidationFilter>()
    .WithTags("API V1");

apiV1.MapTeamEndpoints();
apiV1.MapAuthEndpoints();
apiV1.MapUserEndpoints();
apiV1.MapTransactionEndpoints();
apiV1.MapCostCentreEndpoints();
apiV1.MapBankAccountEndpoints();
apiV1.MapBudgetEndpoints();
apiV1.MapSeasonEndpoints();
apiV1.MapNotificationEndpoints();

if (hasFrontendBundle)
{
    app.MapFallbackToFile("index.html");
}

await app.RunAsync();

static void LoadGoogleConfigFromDotEnv(WebApplicationBuilder builder)
{
    var dotEnvPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "..", ".env"));

    if (!File.Exists(dotEnvPath))
    {
        return;
    }

    var values = new Dictionary<string, string?>();

    foreach (var rawLine in File.ReadLines(dotEnvPath))
    {
        var line = rawLine.Trim();

        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || !line.Contains('='))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=', StringComparison.Ordinal);
        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');

        switch (key)
        {
            case "GOOGLE_CLIENT_ID":
                values["Authentication:Google:ClientId"] = value;
                break;
            case "GOOGLE_CLIENT_SECRET":
                values["Authentication:Google:ClientSecret"] = value;
                break;
            case "GOOGLE_DRIVE_ENABLED":
                values["GoogleDrive:Enabled"] = value;
                break;
            case "GOOGLE_DRIVE_ROOT_FOLDER_ID":
                values["GoogleDrive:RootFolderId"] = value;
                break;
            case "GOOGLE_DRIVE_SERVICE_ACCOUNT_KEY_PATH":
                values["GoogleDrive:ServiceAccountKeyPath"] = value;
                break;
            case "GOOGLE_DRIVE_SERVICE_ACCOUNT_KEY_BASE64":
                values["GoogleDrive:ServiceAccountKeyBase64"] = value;
                break;
        }
    }

    if (values.Count > 0)
    {
        builder.Configuration.AddInMemoryCollection(values);
    }
}
