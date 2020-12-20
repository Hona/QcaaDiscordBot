using System.Threading.Tasks;
using QcaaDiscordBot.Core.Models;

namespace QcaaDiscordBot.Core.Repositories
{
    public interface IUserProfileRepository
    {
        Task Add(UserProfile profile);
        Task Update(UserProfile profile);
        Task Delete(UserProfile profile);
        Task<UserProfile> GetByUserId(ulong id);
    }
}