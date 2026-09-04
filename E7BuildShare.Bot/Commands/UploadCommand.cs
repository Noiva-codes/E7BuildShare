using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using E7BuildShare.Bot.Services;
using E7BuildShare.Bot.Autocomplete;
using System.ComponentModel;

namespace E7BuildShare.Bot.Commands;

public sealed class UploadCommand
{
    private readonly NasStorageService _storage;

    public UploadCommand(NasStorageService storage) => _storage = storage;

    [Command("upload")]
    [Description("Upload an Epic Seven build image")]
    public async Task UploadAsync(SlashCommandContext context,
        [Parameter("unit")]
        [Description("Unit name")]
        [SlashAutoCompleteProvider<UploadCharacterAutocompleteProvider>]
        string unitName,
        [Parameter("image")]
        [Description("Image attachment")]
        DiscordAttachment image)
    {
        await context.DeferResponseAsync();
        try
        {
            if (string.IsNullOrWhiteSpace(image.Url) || string.IsNullOrWhiteSpace(image.FileName))
            {
                await context.EditResponseAsync(new DiscordMessageBuilder()
                    .WithContent("The uploaded image attachment was invalid."));
                return;
            }

            await _storage.SaveAsync(context.User.Id, unitName, new Uri(image.Url), image.FileName);
            await context.EditResponseAsync(new DiscordMessageBuilder()
                .WithContent($"Uploaded **{unitName}** for **{context.User.Username}** successfully."));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Upload failed: {ex.Message}");
            await context.EditResponseAsync(new DiscordMessageBuilder()
                .WithContent("Fatal Error. Command could not be completed."));
        }
    }
}
