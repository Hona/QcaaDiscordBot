using DSharpPlus.CommandsNext.Attributes;

namespace QcaaDiscordBot.Discord.Commands.Checks
{
    public abstract class DescriptiveCheckBaseAttribute : CheckBaseAttribute
    {
        public string FailureResponse { get; set; }
    }
}