using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;

namespace E7BuildShare.Bot.Autocomplete;

public sealed class RetrieveCharacterAutocompleteProvider : IAutocompleteProvider
{
    public Task<IEnumerable<DiscordAutoCompleteChoice>> Provider(AutocompleteContext context) =>
        Task.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(Array.Empty<DiscordAutoCompleteChoice>());
}
