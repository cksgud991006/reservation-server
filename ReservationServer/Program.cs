using Microsoft.EntityFrameworkCore;
using ReservationServer.Api.Endpoints;
using ReservationServer.Application.Services;
using ReservationServer.Application.Repositories;
using ReservationServer.Application.BackgroundJobs;
using ReservationServer.Infrastructure.Database;
using ReservationServer.Infrastructure.Redis;
using StackExchange.Redis;
using ReservationServer.Application.Schedule;
using ReservationServer.Api.Dto;

var builder = WebApplication.CreateBuilder(args);

var redisSection = builder.Configuration.GetSection("Redis");
var redisOptions = new ConfigurationOptions
{
    EndPoints = { $"{redisSection["Host"]}:{redisSection["Port"]}" },
    Password = redisSection["Password"],
    Ssl = redisSection.GetValue<bool>("UseSsl"),
    AbortOnConnectFail = false,
    ConnectTimeout = 5000,
    SyncTimeout = 5000
};
redisOptions.AbortOnConnectFail = false; // Keep the app alive if Redis is down
redisOptions.ConnectTimeout = 5000;      // Wait 5 seconds before timing out
redisOptions.ConnectRetry = 5;           // Try 5 times to reconnect


var scenario = builder.Configuration["Scenario"] ?? "normal";
builder.Configuration.AddJsonFile($"Scenarios/{scenario}.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
builder.Services.AddSingleton<IDbTaskProcessor, BookSqlTaskProcessor>();

builder.Services.AddScoped<IJobScheduler<Guid>, WaitQueueScheduler>();
builder.Services.AddScoped<IJobScheduler<SqlTask>, SqlTaskScheduler>();
builder.Services.AddScoped<IQueueService, QueueService>();
builder.Services.AddScoped<IRedisSession, RedisSession>();
builder.Services.AddScoped<ISeatInventoryService, SeatInventoryService>();
builder.Services.AddScoped<ISeatInventoryRepository, SeatInventoryRepository>();

builder.Services.AddHostedService<WaitQueueRunner>();
builder.Services.AddHostedService<DbTaskRunner>();
builder.Services.AddHostedService<DbInitializer>();
builder.Services.AddHostedService<RedisInitializer>();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()!;

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowGitHubPages",
    builder =>
    {
        builder.WithOrigins(allowedOrigins)
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
     throw new InvalidOperationException("Connection string 'DefaultConnection'" +
    " not found.");

builder.Services.AddDbContext<FlightDbContext>(options => 
    options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseCors("AllowGitHubPages");

app.MapReservationEndpoints();

app.Run();

public partial class Program { }
