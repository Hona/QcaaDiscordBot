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
using QcaaDiscordBot.Discord.Helpers;

namespace QcaaDiscordBot.Discord.Commands.General
{
    [Group("profile")]
    public class UserProfileModule : QcaaModuleBase
    {
        public IUserProfileRepository UserProfileRepository { get; set; }
        
        [GroupCommand]
        public async Task GetUserProfileAsync(CommandContext context, DiscordMember member = null)
        {
            member ??= context.Member;
            
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
            }.AddField("Gender", profile.Gender, true)
                .AddField("Preferred Name", profile.PreferredName, true)
                .AddField("Pronouns", profile.PreferredPronouns, true)
                .AddField("Age", profile.Age.ToString(), true)
                .AddField("ATAR", profile.Atar.ToString(CultureInfo.CurrentCulture), true);

            await context.RespondAsync(embed: embedBuilder.Build());
        }
        
        [Command("set")]
        public async Task SetUserProfileAsync(CommandContext context)
        {
            var profile = await UserProfileRepository.GetByUserId(context.User.Id);

            var profileExists = profile != null;
            
            profile ??= new UserProfile
            {
                UserId = (long)context.User.Id
            };

            // Name
            var nameResponse = await context.GetInteractiveInput("Enter your preferred name", x => x.AddField("Current Value", profile.PreferredName));
            profile.PreferredName = nameResponse ?? profile.PreferredName;

            // Gender
            var genderResponse = await context.GetInteractiveInput("Enter your prefered gender", x => x.AddField("Current Value", profile.Gender));
            profile.Gender = genderResponse ?? profile.Gender;
            
            // Pronouns
            var pronounResponse = await context.GetInteractiveInput("Enter your prefered pronoun/s", x => x.AddField("Current Value", profile.PreferredPronouns));
            profile.PreferredPronouns = pronounResponse ?? profile.PreferredPronouns;
            
            // Age
            var ageResponse = await context.GetInteractiveInput<int>("Enter your age", x => x.AddField("Current Value", profile.Age.ToString()));
            profile.Age = ageResponse == default ? profile.Age : ageResponse;
            
            // ATAR
            var atarResponse = await context.GetInteractiveInput<decimal>("Enter your ATAR", x => x.AddField("Current Value", profile.Atar.ToString(CultureInfo.CurrentCulture)));
            profile.Atar= atarResponse == default ? profile.Atar : atarResponse;
            
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