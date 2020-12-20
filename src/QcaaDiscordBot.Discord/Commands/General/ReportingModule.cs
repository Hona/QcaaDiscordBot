using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using QcaaDiscordBot.Core.Services;

namespace QcaaDiscordBot.Discord.Commands.General
{
    public class ReportingModule : QcaaModuleBase
    {
        public IUserReportService UserReportService { get; set; }
        public IConfiguration Config { get; set; }
        
        [Command("report")]
        [Description("Reports a user for any reason, 5 reports and the user will be temp-banned automatically, pending moderator review.")]
        public async Task ReportUserAsync(CommandContext context, DiscordMember reportedMember)
        {
            await UserReportService.ReportUserAsync(reportedMember.Id, context.User.Id, async () =>
            {
                // TODO: Give temp ban role
                var tempBanRole = context.Guild.GetRole(ulong.Parse(Config["UserReports:TempBanRoleId"]));
                await reportedMember.GrantRoleAsync(tempBanRole);
                await ReplyNewEmbedAsync(context, "User has been temp muted", DiscordColor.Chartreuse);

                var adminChannel = context.Guild.GetChannel(ulong.Parse(Config["UserReports:AdminChannelId"]));
            });

            await ReplyNewEmbedAsync(context,"User reported successfully", DiscordColor.Goldenrod);
        }
    }
}