using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using QcaaDiscordBot.Core.Models;
using QcaaDiscordBot.Core.Repositories;

namespace QcaaDiscordBot.Core.Services
{
    public class UserReportService : IUserReportService
    {
        private readonly IUserReportRepository _userReportRepository;
        private readonly IConfiguration _config;
        
        public UserReportService(IUserReportRepository userReportRepository, IConfiguration config)
        {
            _userReportRepository = userReportRepository;
            _config = config;
        }

        public async Task ReportUserAsync(ulong userId, ulong reportingUserId, Action tempBanThresholdReached)
        {
            var userReport = new UserReport
            {
                UserId = (long)userId,
                ReportingUserId = (long)reportingUserId
            };

            await _userReportRepository.Add(userReport);
            
            // Check if the user has enough reports
            var userReports = await _userReportRepository.GetByUserId(userId);

            if (userReports.Count() >= int.Parse(_config["UserReports:Threshold"]))
            {
                tempBanThresholdReached();
            }
        }
    }
}