using E7BuildShare.Bot.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.Sources.Clear();
builder.Configuration
    //.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.Development.json", optional: true, reloadOnChange: false);

builder.Services.AddSingleton(serviceProvider =>
    serviceProvider.GetRequiredService<IConfiguration>()
        .GetSection("NasStorage")
        .Get<NasStorageOptions>()
        ?? throw new InvalidOperationException(
            "The 'NasStorage' configuration section is missing."));
builder.Services.AddSingleton<NasStorageService>();
builder.Services.AddHostedService<DiscordBotService>();

var host = builder.Build();
await host.RunAsync();
