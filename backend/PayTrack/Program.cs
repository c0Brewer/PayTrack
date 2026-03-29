// <copyright file="Program.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PayTrack.Api.Endpoints;
using PayTrack.Api.Middleware;
using PayTrack.Application.Exceptions;
using PayTrack.Application.Services.Implementation;
using PayTrack.Application.Services.Model;
using PayTrack.Data;
using PayTrack.Data.Repositories.Implementation;
using PayTrack.Data.Repositories.Model;

var builder = WebApplication.CreateBuilder(args);

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

// Repositories
builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddExceptionHandler<EndpointExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();

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
builder.Services.AddAuthorization();

var corsOrigin = builder.Configuration["CORS:Origins"] ?? throw new InternalErrorException("Could not load CORS Origins");
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
        .WithOrigins(corsOrigin)
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-apply migrations (According to Config)
var migrationsRunConfig = builder.Configuration.GetValue<bool>("Migrations:Auto");
if (migrationsRunConfig && !isTestEnv)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();
app.UseCors("frontend");

var apiV1 = app
    .MapGroup("/api/v1")
    .WithTags("API V1");

apiV1.MapTeamEndpoints();
apiV1.MapAuthEndpoints();

await app.RunAsync();
