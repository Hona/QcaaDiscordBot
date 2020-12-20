using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using Marten.Linq.SoftDeletes;
using Microsoft.Extensions.Configuration;
using QcaaDiscordBot.Core.Repositories;
using QcaaDiscordBot.Core.Services;

namespace QcaaDiscordBot.Discord.Commands.General
{
    [Group("report")]
    public class ReportingModule : QcaaModuleBase
    {
        public IUserReportService UserReportService { get; set; }
        public IUserReportRepository UserReportRepository { get; set; }
        public IConfiguration Config { get; set; }
        
        [GroupCommand]
        [Description("Reports a user for any reason, after a certain amount of reports the user will be temp-banned automatically, pending moderator review.")]
        public async Task ReportUserAsync(CommandContext context, DiscordMember reportedMember)
        {
            // Action runs when the number of reports is > than the minimum
            async Task ThresholdReachedAction()
            {
                // TODO: Give temp ban role
                var tempBanRole = context.Guild.GetRole(ulong.Parse(Config["UserReports:TempBanRoleId"]));

                if (tempBanRole == null)
                {
                    throw new Exception("Unable to get temp ban role");
                }
                
                if (reportedMember.Roles.Any(x => x.Id == tempBanRole.Id))
                {
                    throw new Exception("User already has been temp muted");
                }

                await reportedMember.GrantRoleAsync(tempBanRole);
                await ReplyNewEmbedAsync(context, "User has been temp muted", DiscordColor.Chartreuse);

                var adminChannel = context.Guild.GetChannel(ulong.Parse(Config["UserReports:AdminChannelId"]));

                if (adminChannel == null)
                {
                    throw new Exception("Unable to get admin channel");
                }
                
                var adminRole = context.Guild.GetRole(ulong.Parse(Config["UserReports:AdminRoleId"]));

                if (adminRole == null)
                {
                    throw new Exception("Unable to get admin role");
                }
                
                await adminChannel.SendMessageAsync(
                    $"{reportedMember.Mention} has been automatically muted {adminRole.Mention}",
                    mentions: new List<IMention> {new RoleMention(adminRole)});
            }
            
            await UserReportService.ReportUserAsync(reportedMember.Id, context.User.Id, ThresholdReachedAction);

            var autoReportEmoji = DiscordEmoji.FromName(context.Client, ":warning:");

            var messageContentStringBuilder = new StringBuilder();
            messageContentStringBuilder.Append("User reported successfully");
            messageContentStringBuilder.Append(Environment.NewLine);
            messageContentStringBuilder.Append($"React with {autoReportEmoji} to report the user");
            
            var message = await ReplyNewEmbedAsync(context,messageContentStringBuilder.ToString(), DiscordColor.Goldenrod);

            var interactivity = context.Client.GetInteractivity();

            var startTime = DateTime.Now;
            await message.CreateReactionAsync(autoReportEmoji);

            do
            {
                var reaction = await interactivity.WaitForReactionAsync(x => x.Message.Id == message.Id && x.Emoji == autoReportEmoji && x.User.Id != context.Client.CurrentUser.Id);
                
                if (reaction.TimedOut)
                {
                    continue;
                }

                try
                {
                    await UserReportService.ReportUserAsync(reportedMember.Id, reaction.Result.User.Id, ThresholdReachedAction);

                    var numberOfReports = (await UserReportRepository.GetByUserId(reportedMember.Id)).Count();

                    messageContentStringBuilder.Append(Environment.NewLine);
                    messageContentStringBuilder.Append($"{reaction.Result.User.Mention} added a report, now at {numberOfReports}/{Config["UserReports:Threshold"]} reports to temp mute");

                    await message.ModifyAsync(messageContentStringBuilder.ToString());
                }
                catch (Exception e)
                {
                    await ReplyNewEmbedAsync(context, e.Message, DiscordColor.Red);
                }

            } while ((DateTime.Now - startTime).TotalSeconds < 60);
        }

        [Command("cancel")]
        [Description("Cancels your report for a user")]
        public async Task CancelUserReportAsync(CommandContext context, DiscordMember member)
        {
            var userReport = await UserReportRepository.GetByUserAndReporterId(member.Id, context.User.Id);

            if (userReport == null)
            {
                throw new Exception("You cannot cancel a report for a user you have not reported");
            }
            
            await UserReportRepository.Delete(userReport);
            await ReplyNewEmbedAsync(context, "Cancelled your report for " + member.Mention, DiscordColor.Goldenrod);
        }
        
        [Command("list")]
        [Description("Gets a list of the users that reported another user")]
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

        [RequireUserPermissions(Permissions.Administrator)]
        [Command("clear")]
        [Description("Clears the reports for a user")]
        public async Task ClearUserReportsAsync(CommandContext context, DiscordMember member)
        {
            await UserReportService.ClearUserReportsAsync(member.Id);

            await ReplyNewEmbedAsync(context, $"Cleared the reports for {member.Mention}", DiscordColor.Goldenrod);
        }

        [RequireUserPermissions(Permissions.Administrator)]
        [Command("approve")]
        [Description("Approves the user reports, banning the user, and clearing the reports")]
        public async Task ApproveUserReportsAsync(CommandContext context, DiscordMember member)
        {
            await ClearUserReportsAsync(context, member);

            // Clears messages for 14 days too
            await member.BanAsync(14, $"User reports approved by {context.User.Username}");
            
            await ReplyNewEmbedAsync(context, "User has been banned successfully", DiscordColor.Goldenrod);
        }
    }
}