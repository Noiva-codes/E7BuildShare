using DSharpPlus.Entities;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;

namespace E7BuildShare.Bot.Autocomplete;

public sealed class UploadCharacterAutocompleteProvider : IAutoCompleteProvider
{
    public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context) =>
        ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(Array.Empty<DiscordAutoCompleteChoice>());
}
