using System;
using System.Threading.Tasks;

namespace QcaaDiscordBot.Core.Services
{
    public interface IUserReportService
    {
        Task ReportUserAsync(ulong userId, ulong reportingUserId, Action tempBanThresholdReached);
    }
}