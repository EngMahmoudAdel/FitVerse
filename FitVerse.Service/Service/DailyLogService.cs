using FitVerse.Core.IService;
using FitVerse.Core.UnitOfWork;
using FitVerse.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitVerse.Core.Services
{
    public class DailyLogService : IDailyLogService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DailyLogService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public IEnumerable<DailyLog> GetClientLogs(string clientId)
            => _unitOfWork.DailyLogsRepository.GetLogsByClient(clientId);

        public IEnumerable<DailyLog> GetCoachLogs(string coachId)
            => _unitOfWork.DailyLogsRepository.GetLogsByCoach(coachId);

        public void AddClientLog(DailyLog log)
        {
            _unitOfWork.DailyLogsRepository.Add(log);
            _unitOfWork.Complete();
        }

        public void CoachReviewLog(int logId, string feedback, int? rating)
        {
            var log = _unitOfWork.DailyLogsRepository.GetById(logId);
            if (log == null) return;

            log.CoachFeedback = feedback;
            log.CoachRating = rating;
            log.IsReviewed = true;

            _unitOfWork.DailyLogsRepository.Update(log);
            _unitOfWork.Complete();
        }

        public DailyLog GetById(int id)
        {
            return _unitOfWork.DailyLogsRepository.GetById(id);
        }

        // الميثود الناقصة: آخر سجل للعميل
        public DailyLog? GetLastLog(string clientId)
        {
            return _unitOfWork.DailyLogsRepository
                .GetLogsByClient(clientId)
                .OrderByDescending(l => l.LogDate)
                .FirstOrDefault();
        }

        // الميثود الناقصة: السجلات خلال الأسبوع الأخير
        public IEnumerable<DailyLog> GetLogsThisWeek(string clientId)
        {
            var weekAgo = DateTime.UtcNow.AddDays(-7);
            return _unitOfWork.DailyLogsRepository
                .GetLogsByClient(clientId)
                .Where(l => l.LogDate >= weekAgo)
                .ToList();
        }
    }
}
