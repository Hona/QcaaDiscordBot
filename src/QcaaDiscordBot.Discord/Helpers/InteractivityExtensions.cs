﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;

namespace QcaaDiscordBot.Discord.Helpers
{
    public static class InteractivityExtensions
    {
        public static async Task<string> GetInteractiveInput(this CommandContext context, string question)
        {
            var interactivity = context.Client.GetInteractivity();
            
            await context.RespondAsync(embed: new DiscordEmbedBuilder
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor
                {
                    Name = context.Member.DisplayName,
                    IconUrl = context.Member.AvatarUrl ?? context.Member.DefaultAvatarUrl
                },
                Description = question,
                Color = DiscordColor.Goldenrod,
                Footer = new DiscordEmbedBuilder.EmbedFooter
                {
                    Text = "Type 'skip' to either keep your current value, or not answer"
                }
            });

            var responseResult = await interactivity.WaitForMessageAsync(x => x.Author.Id == context.User.Id);

            if (responseResult.TimedOut)
            {
                throw new Exception($"Timed out waiting for {context.Member.Mention}'s response");
            }

            var response = responseResult.Result.Content;

            return response.Trim().ToLower() == "skip" ? null : response;
        }

        public static async Task<T> GetInteractiveInput<T>(this CommandContext context, string question) 
        {
            var stringResponse = await context.GetInteractiveInput(question);

            if (stringResponse == null)
            {
                // Propagate the null up
                return default;
            }

            var genericConversion = stringResponse.Convert<T>();

            if (EqualityComparer<T>.Default.Equals(genericConversion, default))
            {
                throw new Exception($"You must enter a {typeof(T).Name}");
            }

            return genericConversion;
        }
    }
}