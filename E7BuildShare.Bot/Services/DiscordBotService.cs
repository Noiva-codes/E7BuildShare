using DSharpPlus;
using DSharpPlus.SlashCommands;
using E7BuildShare.Bot.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace E7BuildShare.Bot.Services;

public sealed class DiscordBotService(
    IConfiguration configuration,
    IServiceProvider services,
    ILogger<DiscordBotService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var botOptions = configuration.GetSection("Bot").Get<BotOptions>()
            ?? throw new InvalidOperationException("The 'Bot' configuration section is missing.");
        var storageOptions = configuration.GetSection("NasStorage").Get<NasStorageOptions>()
            ?? throw new InvalidOperationException("The 'NasStorage' configuration section is missing.");

        if (string.IsNullOrWhiteSpace(botOptions.Token))
            throw new InvalidOperationException("Bot:Token must be configured in AppSettings.");
        if (string.IsNullOrWhiteSpace(storageOptions.SharePath) ||
            string.IsNullOrWhiteSpace(storageOptions.Username) ||
            string.IsNullOrWhiteSpace(storageOptions.Password))
            throw new InvalidOperationException("NAS storage settings must be configured in AppSettings.");

        var client = new DiscordClient(new DiscordConfiguration
        {
            Token = botOptions.Token,
            TokenType = TokenType.Bot,
            Intents = DiscordIntents.All,
            MinimumLogLevel = LogLevel.Information
        });

        var slashCommands = client.UseSlashCommands(new SlashCommandsConfiguration { Services = services });
        slashCommands.RegisterCommands<UploadCommand>();

        logger.LogInformation("Connecting to Discord...");
        await client.ConnectAsync();
        logger.LogInformation("E7BuildShare bot connected to Discord.");
        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }
}

public sealed class BotOptions
{
    public string Token { get; set; } = string.Empty;
}

public sealed class NasStorageOptions
{
    public string SharePath { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
