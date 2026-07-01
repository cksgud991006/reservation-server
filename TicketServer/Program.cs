using Microsoft.EntityFrameworkCore;
using TicketServer.Api.Endpoints;
using TicketServer.Application.Services;
using TicketServer.Application.Repositories;
using TicketServer.Application.BackgroundJobs;
using TicketServer.Infrastructure.Database;
using TicketServer.Infrastructure.Redis;
using StackExchange.Redis;
using TicketServer.Application.Schedule;
using TicketServer.Api.Dto;

var builder = WebApplication.CreateBuilder(args);

// register DIs
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


var scenario = builder.Configuration["TestScenario"] ?? "normal";
builder.Configuration.AddJsonFile($"scenarios/{scenario}.json", optional: false, reloadOnChange: true);

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisOptions));
builder.Services.AddSingleton<ISqlTaskProcessor, BookSqlTaskProcessor>();
builder.Services.AddScoped<IJobScheduler<Guid>, WaitQueueScheduler>();
builder.Services.AddScoped<IJobScheduler<SqlTask>, SqlTaskScheduler>();
builder.Services.AddScoped<IQueueingService, QueueingService>();
builder.Services.AddScoped<IRedisSession, RedisSession>();
builder.Services.AddScoped<ISeatInventoryService, SeatInventoryService>();
builder.Services.AddScoped<ISeatInventoryRepository, SeatInventoryRepository>();
builder.Services.AddHostedService<WaitQueueRunner>();
builder.Services.AddHostedService<SqlTaskRunner>();
builder.Services.AddHostedService<DbInitializer>();
builder.Services.AddHostedService<SeatInventoryLoader>();

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

app.MapTicketEndPoints();

app.Run();
