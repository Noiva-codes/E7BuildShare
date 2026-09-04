using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using E7BuildShare.Bot.Services;

namespace E7BuildShare.Bot.Commands;

public sealed class UploadCommand : ApplicationCommandModule
{
    private readonly NasStorageService _storage;

    public UploadCommand(NasStorageService storage) => _storage = storage;

    [SlashCommand("upload", "Upload an Epic Seven build image")]
    public async Task UploadAsync(InteractionContext context,
        [Option("unit", "Unit name")] string unitName,
        [Option("image", "Image attachment")] DiscordAttachment image)
    {
        await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        try
        {
            await _storage.SaveAsync(context.User.Id, unitName, new Uri(image.Url), image.FileName);
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent($"Uploaded **{unitName}** successfully."));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Upload failed: {ex.Message}");
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Upload failed. Check the bot logs for details."));
        }
    }
}
