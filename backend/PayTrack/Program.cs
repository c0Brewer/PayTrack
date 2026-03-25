// <copyright file="Program.cs" company="PayTrack">
// Copyright (c) PayTrack. All rights reserved.
// </copyright>

using Microsoft.EntityFrameworkCore;
using PayTrack.Api.Endpoints;
using PayTrack.Api.Middleware;
using PayTrack.Application.Services;
using PayTrack.Application.Services.Impl;
using PayTrack.Data;
using PayTrack.Data.Repositories;
using PayTrack.Data.Repositories.Impl;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddScoped<ITeamRepository, TeamRepository>();
builder.Services.AddScoped<ITeamService, TeamService>();

builder.Services.AddExceptionHandler<EndpointExceptionHandler>();
builder.Services.AddProblemDetails();

// TODO: Properly set Origin for Production
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy
        .WithOrigins("*")
        .AllowAnyHeader()
        .AllowAnyMethod();
    });
});

var app = builder.Build();

// Auto-apply migrations (According to Config)
var migrationsRunConfig = builder.Configuration.GetValue<bool>("Migrations:Auto");
if (migrationsRunConfig)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseHttpsRedirection();
app.UseCors("frontend");

app.MapTeamEndpoints();

app.Run();

/// <summary>
/// Expose Program for Testing.
/// </summary>
public partial class Program
{
}
