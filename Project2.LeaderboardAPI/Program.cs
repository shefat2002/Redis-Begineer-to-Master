using StackExchange.Redis;
using Project2.LeaderboardAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Redis connection
var redisConnection = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnection);
    configuration.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(configuration);
});

// Add services
builder.Services.AddSingleton<ILeaderboardService, LeaderboardService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Add CORS for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.MapControllers();

// Test Redis connection on startup
try
{
    var redis = app.Services.GetRequiredService<IConnectionMultiplexer>();
    var db = redis.GetDatabase();
    await db.PingAsync();
    app.Logger.LogInformation("✅ Successfully connected to Redis at {RedisConnection}", redisConnection);
}
catch (Exception ex)
{
    app.Logger.LogError(ex, "❌ Failed to connect to Redis at {RedisConnection}", redisConnection);
}

app.Run();
