using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FitVerse.Core.Enums
{
    public enum NotificationType
    {
        Message = 1,
        NewClient = 2,
        NewCoach = 3,
        PlanAssigned = 4,
        PaymentReceived = 5,
        SubscriptionExpiring = 6,
        WorkoutCompleted = 7,
        FeedbackReceived = 8,
        SystemAlert = 9,
        General = 10,
        DailyLogSubmitted = 11,
        DailyLogReviewed = 12
    }
}
