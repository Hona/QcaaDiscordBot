using System;
using System.Globalization;
using System.Threading.Tasks;
using Baseline.Reflection;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using QcaaDiscordBot.Core.Models;
using QcaaDiscordBot.Core.Repositories;

namespace QcaaDiscordBot.Discord.Commands.General
{
    [Group("profile")]
    public class UserProfileModule : QcaaModuleBase
    {
        public IUserProfileRepository UserProfileRepository { get; set; }
        
        [GroupCommand]
        public async Task GetUserProfileAsync(CommandContext context, DiscordMember member)
        {
            var profile = await UserProfileRepository.GetByUserId(member.Id);

            if (profile == null)
            {
                throw new Exception("No profile for " + member.Mention);
            }

            var embedBuilder = new DiscordEmbedBuilder
            {
                Title = "User Profile",
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = member.DisplayName,
                    IconUrl = member.AvatarUrl ?? member.DefaultAvatarUrl
                },
                Color = DiscordColor.Goldenrod
            }.AddField("Gender", profile.Gender)
                .AddField("Preferred Name", profile.PreferredName)
                .AddField("Pronouns", profile.PreferredPronouns)
                .AddField("Age", profile.Age.ToString())
                .AddField("ATAR", profile.Atar.ToString(CultureInfo.CurrentCulture));

            await context.RespondAsync(embed: embedBuilder.Build());
        }
        
        [Command("set")]
        public async Task GetUserProfileAsync(CommandContext context)
        {
            var profile = await UserProfileRepository.GetByUserId(context.User.Id);

            var profileExists = profile != null;
            
            profile ??= new UserProfile
            {
                UserId = (long)context.User.Id
            };

            var interactivity = context.Client.GetInteractivity();

            // Name
            await context.RespondAsync("Enter your prefered name (or 'keep'): ");
            var nameResponse = await interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id);

            if (nameResponse.TimedOut)
            {
                return;
            }

            if (nameResponse.Result.Content.ToLower() != "keep")
            {
                profile.PreferredName = nameResponse.Result.Content;
            }
            
            // Gender
            await context.RespondAsync("Enter your prefered gender (or 'keep'): ");
            var genderResponse = await interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id);

            if (genderResponse.TimedOut)
            {
                return;
            }

            if (genderResponse.Result.Content.ToLower() != "keep")
            {
                profile.Gender = genderResponse.Result.Content;
            }
            
            // Pronouns
            await context.RespondAsync("Enter your prefered pronouns (or 'keep'): ");
            var pronounsResponse = await interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id);

            if (pronounsResponse.TimedOut)
            {
                return;
            }

            if (pronounsResponse.Result.Content.ToLower() != "keep")
            {
                profile.PreferredPronouns = pronounsResponse.Result.Content;
            }
            
            // Age
            await context.RespondAsync("Enter your prefered age (or 'keep'): ");
            var ageResponse = await interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id);

            if (ageResponse.TimedOut)
            {
                return;
            }

            if (ageResponse.Result.Content.ToLower() != "keep")
            {
                profile.Age = int.Parse(ageResponse.Result.Content);
            }
            
            // Age
            await context.RespondAsync("Enter your ATAR (or 'keep'): ");
            var atarResponse = await interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id);

            if (atarResponse.TimedOut)
            {
                return;
            }

            if (atarResponse.Result.Content.ToLower() != "keep")
            {
                profile.Atar = decimal.Parse(atarResponse.Result.Content);
            }
            
            // Infrastructure
            if (profileExists)
            {
                await UserProfileRepository.Update(profile);
            }
            else
            {
                await UserProfileRepository.Add(profile);
            }

            await ReplyNewEmbedAsync(context, "Updated profile successfully", DiscordColor.Goldenrod);
        }
    }
}