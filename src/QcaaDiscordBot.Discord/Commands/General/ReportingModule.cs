using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Configuration;
using QcaaDiscordBot.Core.Repositories;
using QcaaDiscordBot.Core.Services;

namespace QcaaDiscordBot.Discord.Commands.General
{
    public class ReportingModule : QcaaModuleBase
    {
        public IUserReportService UserReportService { get; set; }
        public IUserReportRepository UserReportRepository { get; set; }
        public IConfiguration Config { get; set; }
        
        [Command("report")]
        [Description("Reports a user for any reason, 5 reports and the user will be temp-banned automatically, pending moderator review.")]
        public async Task ReportUserAsync(CommandContext context, DiscordMember reportedMember)
        {
            await UserReportService.ReportUserAsync(reportedMember.Id, context.User.Id, async () =>
            {
                // TODO: Give temp ban role
                var tempBanRole = context.Guild.GetRole(ulong.Parse(Config["UserReports:TempBanRoleId"]));

                if (reportedMember.Roles.Any(x => x.Id == tempBanRole.Id))
                {
                    await ReplyNewEmbedAsync(context, "User already has been temp muted", DiscordColor.Chartreuse);
                    return;
                }
                
                await reportedMember.GrantRoleAsync(tempBanRole);
                await ReplyNewEmbedAsync(context, "User has been temp muted", DiscordColor.Chartreuse);

                var adminChannel = context.Guild.GetChannel(ulong.Parse(Config["UserReports:AdminChannelId"]));

                var adminRole = context.Guild.GetRole(ulong.Parse(Config["UserReports:AdminRoleId"]));
                await adminChannel.SendMessageAsync(
                    $"{reportedMember.Mention} has been automatically muted {adminRole.Mention}",
                    mentions: new List<IMention>
                    {
                        new RoleMention(adminRole)
                    });
            });

            await ReplyNewEmbedAsync(context,"User reported successfully", DiscordColor.Goldenrod);
        }

        [Command("report")]
        [Description("Reports a user for any reason, after a certain number of reports and the user will be temp-banned automatically, pending moderator review.")]
        public async Task GetUserReportsAsync(CommandContext context, DiscordMember member)
        {
            var reports = (await UserReportRepository.GetByUserId(member.Id)).ToList();

            if (!reports.Any())
            {
                await ReplyNewEmbedAsync(context, "User has no reports", DiscordColor.Goldenrod);
                return;
            }

            var descriptionStringBuilder = new StringBuilder();

            foreach (var userReport in reports)
            {
                var user = context.Guild.Members.First(x => x.Key == (ulong) userReport.ReportingUserId).Value;
                descriptionStringBuilder.Append(user.Mention);
                descriptionStringBuilder.Append(Environment.NewLine);
            }

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "User Reports",
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = member.DisplayName,
                    IconUrl = member.AvatarUrl ?? member.DefaultAvatarUrl
                },
                Description = string.Join(Environment.NewLine, descriptionStringBuilder.ToString())
            };

            await context.RespondAsync(embed: embedBuilder.Build());
        }
    }
}