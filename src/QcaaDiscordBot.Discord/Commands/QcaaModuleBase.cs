using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace QcaaDiscordBot.Discord.Commands
{
    [ModuleLifespan(ModuleLifespan.Transient)]
    public class QcaaModuleBase
    {
        public ILogger Logger { get; set; }

        protected async Task<DiscordMessage> ReplyNewEmbedAsync(CommandContext context, string text, DiscordColor color)
        {
            var embed = new DiscordEmbedBuilder
            {
                Description = text,
                Color = color
            }.Build();

            return await context.RespondAsync(embed: embed);
        }
    }
}