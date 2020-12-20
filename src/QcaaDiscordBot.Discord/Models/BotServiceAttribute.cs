using System;

namespace QcaaDiscordBot.Discord.Models
{
    public class BotServiceAttribute : Attribute
    {
        public BotServiceAttribute(BotServiceType microserviceType = BotServiceType.Manual) =>
            Type = microserviceType;

        public BotServiceType Type { get; }
    }
}