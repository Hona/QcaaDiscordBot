﻿using System.Collections.Generic;
using System.Threading.Tasks;
using QcaaDiscordBot.Core.Models;

namespace QcaaDiscordBot.Core.Repositories
{
    public interface IUserReportRepository
    {
        Task Add(UserReport userReport);
        Task Update(UserReport userReport);
        Task<IEnumerable<UserReport>> GetByUserId(ulong userId);
        Task Delete(UserReport userReport);
    }
}