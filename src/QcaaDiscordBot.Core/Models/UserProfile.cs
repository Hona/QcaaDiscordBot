namespace QcaaDiscordBot.Core.Models
{
    public class UserProfile
    {
        public long UserId { get; set; }
        public string PreferredName { get; set; }
        public string Gender { get; set; }
        public string PreferredPronouns { get; set; }
        public int Age { get; set; }
        public decimal Atar { get; set; }
    }
}