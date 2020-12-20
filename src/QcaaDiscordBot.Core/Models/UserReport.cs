using System;

namespace QcaaDiscordBot.Core.Models
{
    public class UserReport
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public ulong UserId { get; set; }
        public ulong ReportingUserId { get; set; }
    }
}