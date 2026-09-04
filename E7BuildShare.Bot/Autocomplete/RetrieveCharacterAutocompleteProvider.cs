using DSharpPlus.Entities;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using E7BuildShare.Bot.Services;

namespace E7BuildShare.Bot.Autocomplete;

public sealed class RetrieveCharacterAutocompleteProvider : IAutoCompleteProvider
{
    private readonly BuildLookupService _lookup;

    public RetrieveCharacterAutocompleteProvider(BuildLookupService lookup) => _lookup = lookup;

    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var query = context.UserInput ?? string.Empty;
        var names = await _lookup.GetCharacterNamesAsync();
        return names.Where(name => name.Contains(query, StringComparison.OrdinalIgnoreCase))
            .Take(25).Select(name => new DiscordAutoCompleteChoice(name, name));
    }
}
