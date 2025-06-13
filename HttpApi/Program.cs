using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(5000);
});

// 로그 레벨 수동 조정
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// 특정 Provider 레벨 따로 조정
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);



builder.Services.AddSingleton<MatchQueue>();
builder.Services.AddSingleton<RoomMapping>();
builder.Services.AddHostedService<MatchMakerService>();
builder.Services.AddSingleton<GameServerConnector>();
builder.Services.AddControllers();



var app = builder.Build();
app.MapControllers();
app.Run();
