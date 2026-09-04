using DSharpPlus.Entities;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using E7BuildShare.Bot.Services;

namespace E7BuildShare.Bot.Autocomplete;

public sealed class UserAutocompleteProvider : IAutoCompleteProvider
{
    private readonly BuildLookupService _lookup;

    public UserAutocompleteProvider(BuildLookupService lookup) => _lookup = lookup;

    public async ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
    {
        var query = context.UserInput ?? string.Empty;
        var choices = new List<DiscordAutoCompleteChoice>();

        foreach (var userId in await _lookup.GetUploaderIdsAsync())
        {
            try
            {
                var user = await context.Client.GetUserAsync(userId);
                var member = context.Guild is null
                    ? null
                    : await context.Guild.GetMemberAsync(userId);
                var displayName = member?.DisplayName ?? user.Username;
                if (!userId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) &&
                    !displayName.Contains(query, StringComparison.OrdinalIgnoreCase) &&
                    !user.Username.Contains(query, StringComparison.OrdinalIgnoreCase))
                    continue;

                choices.Add(new DiscordAutoCompleteChoice($"{displayName} (@{user.Username})", userId.ToString()));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Could not resolve Discord user {userId}: {ex.Message}");
                if (userId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
                    choices.Add(new DiscordAutoCompleteChoice(userId.ToString(), userId.ToString()));
            }

            if (choices.Count >= 25)
                break;
        }

        return choices;
    }
}
