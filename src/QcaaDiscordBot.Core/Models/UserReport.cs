using System;

namespace QcaaDiscordBot.Core.Models
{
    public class UserReport
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public long ReportingUserId { get; set; }
        public string Reason { get; set; }
    }
}