using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using QcaaDiscordBot.Discord.Services;

namespace QcaaDiscordBot.Discord.Commands
{
    [Group("newmembers")]
    [RequireUserPermissions(Permissions.Administrator)]
    public class AdminModule : QcaaModuleBase
    {
        public NewMemberService NewMemberService { get; set; }
        
        [Command("reload")]
        [Description("Reloads the newmembers service, use when the messages change")]
        public async Task ReloadFaqAsync(CommandContext context)
        {
            await ReplyNewEmbedAsync(context, "Reloading newmembers service...", DiscordColor.Blue);
            await NewMemberService.HookToLastMessageAsync();

            await ReplyNewEmbedAsync(context, "Reloaded newmembers service", DiscordColor.Blue);
        }

        [Command("lock")]
        [Description("Locks users from being verified from the newmembers channel, use when getting spammed by user bots")]
        public async Task LockFaqAsync(CommandContext context)
        {
            NewMemberService.Lock();

            await ReplyNewEmbedAsync(context, "Locked users from being verified in the newmembers channel",
                DiscordColor.Blue);
        }

        [Command("unlock")]
        [Description("Allows users to be verified from the newmembers channel")]
        public async Task UnlockFaqAsync(CommandContext context)
        {
            await NewMemberService.UnlockAsync();

            await ReplyNewEmbedAsync(context, "Allowed users to be verified in the newmembers channel", DiscordColor.Blue);
        }

        [Command("verify")]
        [Description("Checks if there are any unhandled reactions")]
        public async Task VerifyFaqAsync(CommandContext context)
        {
            await NewMemberService.AddUnhandedReactionRolesAsync();

            await ReplyNewEmbedAsync(context, "Done", DiscordColor.Blue);
        }

        [Command("reset")]
        [Description("Resets all users to be unverified")]
        public async Task ResetAsync(CommandContext context, DiscordRole role)
        {
            foreach (var member in context.Guild.Members.Select(x => x.Value))
            {
                try
                {
                    Console.WriteLine($"Giving the role to {member}");
                    await member.GrantRoleAsync(role);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Error giving role to " + member);
                    throw;
                }

            }
        }
    }
}