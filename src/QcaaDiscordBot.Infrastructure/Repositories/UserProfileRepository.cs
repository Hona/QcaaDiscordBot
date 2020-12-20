using System.Threading.Tasks;
using Marten;
using Marten.Linq;
using QcaaDiscordBot.Core.Models;
using QcaaDiscordBot.Core.Repositories;

namespace QcaaDiscordBot.Infrastructure.Repositories
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly IDocumentStore _store;
        
        public UserProfileRepository(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Add(UserProfile profile)
        {
            using var session = _store.DirtyTrackedSession();
            
            session.Insert(profile);

            await session.SaveChangesAsync();
        }

        public async Task Update(UserProfile profile)
        {
            using var session = _store.DirtyTrackedSession();
            
            session.Update(profile);

            await session.SaveChangesAsync();
        }

        public async Task Delete(UserProfile profile)
        {
            using var session = _store.DirtyTrackedSession();
            
            session.Delete(profile);

            await session.SaveChangesAsync();
        }

        public async Task<UserProfile> GetByUserId(ulong id)
        {
            using var session = _store.QuerySession();

            return await session.Query<UserProfile>().SingleOrDefaultAsync(x => x.UserId == (long) id);
        }
    }
}