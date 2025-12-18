using Project2.SignalRChat.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add SignalR services
builder.Services.AddSignalR()
    .AddStackExchangeRedis(options =>
    {
        options.Configuration.EndPoints.Add("localhost:6379");
        options.Configuration.AbortOnConnectFail = false;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseStaticFiles();

app.MapHub<ChatHub>("/chatHub");
app.MapGet("/", () => Results.Redirect("/index.html"));

app.Run();
