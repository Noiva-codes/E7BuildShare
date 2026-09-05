namespace E7BuildShare.Bot.Services;

public sealed class BotOptions
{
    public string Token { get; set; } = string.Empty;
    public ulong DebugGuildId { get; set; }
}

public sealed class NasStorageOptions
{
    public string SharePath { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
