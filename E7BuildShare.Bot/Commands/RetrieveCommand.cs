using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;
using E7BuildShare.Bot.Autocomplete;
using E7BuildShare.Bot.Services;
using System.ComponentModel;

namespace E7BuildShare.Bot.Commands;

public sealed class RetrieveCommand
{
    private readonly NasStorageService _storage;
    private readonly BuildLookupService _lookup;

    public RetrieveCommand(NasStorageService storage, BuildLookupService lookup)
    {
        _storage = storage;
        _lookup = lookup;
    }

    [Command("retrieve")]
    [Description("Retrieve an uploaded Epic Seven build")]
    public async Task RetrieveAsync(SlashCommandContext context,
        [Parameter("unit")]
        [Description("Unit name")]
        [SlashAutoCompleteProvider<RetrieveCharacterAutocompleteProvider>]
        string unitName,
        [Parameter("person")]
        [Description("Person who uploaded the build")]
        [SlashAutoCompleteProvider<UserAutocompleteProvider>]
        string personId)
    {
        await context.DeferResponseAsync();
        try
        {
            if (!ulong.TryParse(personId, out var uploaderId))
            {
                await context.EditResponseAsync(new DiscordMessageBuilder()
                    .WithContent("Please select a person from the autocomplete results."));
                return;
            }

            var build = await _lookup.GetLatestBuildAsync(uploaderId, unitName);
            if (build is null)
            {
                await context.EditResponseAsync(new DiscordMessageBuilder()
                    .WithContent($"No uploaded build was found for **{unitName}**."));
                return;
            }

            await using var file = await _storage.OpenReadAsync(build.StoragePath);
            await context.EditResponseAsync(new DiscordMessageBuilder()
                .WithContent($"Latest uploaded build for **{unitName}**:")
                .AddFile(build.OriginalFileName, file));
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Retrieve failed: {ex.Message}");
            await context.EditResponseAsync(new DiscordWebhookBuilder()
                .WithContent("Fatal Error. Command could not be completed."));
        }
    }
}
