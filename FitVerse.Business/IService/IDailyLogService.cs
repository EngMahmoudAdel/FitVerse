using FitVerse.Core.UnitOfWork;
using FitVerse.Data.Models;
using System.Collections.Generic;

namespace FitVerse.Core.IService
{
    public interface IDailyLogService
    {
        IEnumerable<DailyLog> GetClientLogs(string clientId);
        IEnumerable<DailyLog> GetCoachLogs(string coachId);
        void AddClientLog(DailyLog log);
        void CoachReviewLog(int logId, string feedback, int? rating);
        DailyLog GetById(int id);

    }
}
