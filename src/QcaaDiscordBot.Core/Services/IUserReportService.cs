﻿using System;
using System.Collections;
using System.Threading.Tasks;
using QcaaDiscordBot.Core.Models;

namespace QcaaDiscordBot.Core.Services
{
    public interface IUserReportService
    {
        Task ReportUserAsync(ulong userId, ulong reportingUserId, string reason, Func<Task> tempBanThresholdReachedAsync);
        Task ClearUserReportsAsync(ulong userId);
    }
}