using DSharpPlus;
using Microsoft.Extensions.Configuration;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: false)
    .Build();

var options = configuration.GetSection("Bot").Get<BotOptions>()
    ?? throw new InvalidOperationException("The 'Bot' configuration section is missing.");

if (string.IsNullOrWhiteSpace(options.Token))
{
    throw new InvalidOperationException(
        "Discord bot token is missing. Set Bot:Token in the active appsettings file.");
}

if (string.IsNullOrWhiteSpace(options.GuildId) ||
    !ulong.TryParse(options.GuildId, out _))
{
    throw new InvalidOperationException("Bot:GuildId must be a valid Discord server ID.");
}

var client = new DiscordClient(new DiscordConfiguration
{
    Token = options.Token,
    TokenType = TokenType.Bot,
    Intents = DiscordIntents.All,
    MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Information
});

await client.ConnectAsync();

Console.WriteLine("E7BuildShare bot is online. Press Ctrl+C to stop.");
await Task.Delay(Timeout.InfiniteTimeSpan);

public sealed class BotOptions
{
    public string Token { get; set; } = string.Empty;
    public string GuildId { get; set; } = string.Empty;
}
