using E7BuildShare.Bot.Services;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors;
using DSharpPlus.Commands.Processors.SlashCommands;
using E7BuildShare.Bot.Commands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationManager();
configuration.Sources.Clear();
configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
    .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false);

var botOptions = configuration.GetSection("Bot").Get<BotOptions>()
    ?? throw new InvalidOperationException("The 'Bot' configuration section is missing.");
if (string.IsNullOrWhiteSpace(botOptions.Token))
    throw new InvalidOperationException("Bot:Token must be configured in AppSettings.");

var storageOptions = configuration.GetSection("NasStorage").Get<NasStorageOptions>()
    ?? throw new InvalidOperationException("The 'NasStorage' configuration section is missing.");
if (string.IsNullOrWhiteSpace(storageOptions.SharePath) ||
    string.IsNullOrWhiteSpace(storageOptions.Username) ||
    string.IsNullOrWhiteSpace(storageOptions.Password))
    throw new InvalidOperationException("NAS storage settings must be configured in AppSettings.");

var databaseOptions = configuration.GetSection("Database").Get<DatabaseOptions>()
    ?? throw new InvalidOperationException("The 'Database' configuration section is missing.");
if (string.IsNullOrWhiteSpace(databaseOptions.Path))
    throw new InvalidOperationException("Database:Path must be configured.");

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(botOptions);
services.AddSingleton(storageOptions);
services.AddSingleton(databaseOptions);
services.AddSingleton<NasStorageService>();
services.AddSingleton<SqliteDatabaseProvider>();
services.AddSingleton<BuildLookupService>();

var clientBuilder = DiscordClientBuilder.CreateDefault(
    botOptions.Token,
    DiscordIntents.All,
    services);

clientBuilder.UseCommands((_, extension) =>
{
    extension.AddCommands<UploadCommand>();
    extension.AddCommands<RetrieveCommand>();
    extension.AddProcessor(new SlashCommandProcessor());
}, new CommandsConfiguration
{
    DebugGuildId = botOptions.DebugGuildId
});

var client = clientBuilder.Build();
await client.ServiceProvider.GetRequiredService<SqliteDatabaseProvider>().InitializeAsync();
await client.ConnectAsync();

using var shutdown = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    shutdown.Cancel();
};

try
{
    await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
}
catch (OperationCanceledException)
{
    await client.DisconnectAsync();
}
