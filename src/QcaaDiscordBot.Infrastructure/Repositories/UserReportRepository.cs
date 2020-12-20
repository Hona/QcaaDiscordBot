using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Marten;
using QcaaDiscordBot.Core.Models;
using QcaaDiscordBot.Core.Repositories;

namespace QcaaDiscordBot.Infrastructure.Repositories
{
    public class UserReportRepository : IUserReportRepository
    {
        private readonly IDocumentStore _store;
        
        public UserReportRepository(IDocumentStore store)
        {
            _store = store;
        }

        public async Task Add(UserReport userReport)
        {
            using var session = _store.DirtyTrackedSession();
            
            session.Insert(userReport);

            await session.SaveChangesAsync();
        }

        public async Task Update(UserReport userReport)
        {
            using var session = _store.DirtyTrackedSession();
            
            session.Update(userReport);

            await session.SaveChangesAsync();
        }

        public async Task<IEnumerable<UserReport>> GetByUserId(ulong userId)
        {
            using var session = _store.QuerySession();

            return await session.Query<UserReport>()
                .Where(x => x.UserId == (long)userId)
                .ToListAsync();
        }

        public async Task<UserReport> GetByUserAndReporterId(ulong userId, ulong reporterId)
        {
            using var session = _store.QuerySession();

            return await session.Query<UserReport>()
                .FirstOrDefaultAsync(x => x.UserId == (long)userId && x.ReportingUserId == (long)reporterId);
        }

        public async Task Delete(UserReport userReport)
        {
            using var session = _store.DirtyTrackedSession();
            
            session.Delete(userReport);

            await session.SaveChangesAsync();
        }
    }
}