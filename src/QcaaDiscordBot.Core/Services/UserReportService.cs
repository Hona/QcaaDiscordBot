using System;
using System.Collections;
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

        public async Task ReportUserAsync(ulong userId, ulong reportingUserId, string reason, Func<Task> tempBanThresholdReachedAsync)
        {
            // Don't let users report the same user multiple times
            var existingReport = await _userReportRepository.GetByUserAndReporterId(userId, reportingUserId);

            if (existingReport != null)
            {
                throw new Exception("You cannot report the same user twice");
            }
            
            var userReport = new UserReport
            {
                UserId = (long)userId,
                ReportingUserId = (long)reportingUserId,
                Reason = reason
            };

            await _userReportRepository.Add(userReport);
            
            // Check if the user has enough reports
            var userReports = await _userReportRepository.GetByUserId(userId);

            if (userReports.Count() >= int.Parse(_config["UserReports:Threshold"]))
            {
                await tempBanThresholdReachedAsync();
            }
        }

        public async Task ClearUserReportsAsync(ulong userId)
        {
            var userReports = await _userReportRepository.GetByUserId(userId);

            foreach (var userReport in userReports)
            {
                await _userReportRepository.Delete(userReport);
            }
        }
    }
}