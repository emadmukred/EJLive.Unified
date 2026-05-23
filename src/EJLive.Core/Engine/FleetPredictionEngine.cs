using System;
using EJLive.Core.Models;

namespace EJLive.Core.Engine
{
    /// <summary>
    /// Lightweight operational prediction for NOC decisions.
    /// It uses live telemetry already available in ATMInfo, so it works before a larger ML model is introduced.
    /// </summary>
    public class FleetPredictionEngine
    {
        public FleetPredictionResult Predict(ATMInfo atm)
        {
            if (atm == null)
            {
                return new FleetPredictionResult
                {
                    Level = "غير معروف",
                    RiskScore = 100,
                    Reason = "لا توجد بيانات صراف",
                    RecommendedAction = "تحديث الاتصال أو اختيار صراف صحيح",
                    EstimatedImpact = "غير قابل للتقييم"
                };
            }

            var heartbeatAge = SafeMinutesSince(atm.LastHeartbeatUtc);
            var syncAge = SafeMinutesSince(atm.LastSyncUtc);
            var risk = Math.Max(0, 100 - atm.HealthScore);

            if (atm.ConnectionStatus == ConnectionStatus.Disconnected) risk += 35;
            if (atm.ConnectionStatus == ConnectionStatus.Syncing) risk -= 8;
            if (heartbeatAge > AppConstants.AlertDisconnectWarningMin) risk += 20;
            if (heartbeatAge > AppConstants.AlertDisconnectCriticalMin) risk += 20;
            if (syncAge > AppConstants.AlertNoDataWarningMin) risk += 15;
            if (syncAge > AppConstants.AlertNoDataCriticalMin) risk += 20;
            risk += Math.Min(30, atm.ConsecutiveSyncFailures * 10);
            if (atm.Latency_ms > 2000) risk += 10;
            if (!string.IsNullOrWhiteSpace(atm.LastErrorCode)) risk += 12;

            risk = Math.Max(0, Math.Min(100, risk));

            var result = new FleetPredictionResult
            {
                RiskScore = risk,
                Reason = BuildReason(atm),
                EstimatedImpact = EstimateImpact(atm, risk)
            };

            if (risk >= 75)
            {
                result.Level = "حرج";
                result.RecommendedAction = "فحص الاتصال فوراً، إرسال PING ثم Force Sync، وبعدها تصعيد للدعم إذا لم يصل Heartbeat";
            }
            else if (risk >= 40)
            {
                result.Level = "مراقبة";
                result.RecommendedAction = "تشغيل مزامنة فورية ومراجعة آخر جورنال والتنبيهات النشطة";
            }
            else
            {
                result.Level = "مستقر";
                result.RecommendedAction = "استمرار المراقبة الدورية";
            }

            return result;
        }

        private static double SafeMinutesSince(DateTime utc)
        {
            if (utc == default(DateTime)) return double.MaxValue;
            return Math.Max(0, (DateTime.UtcNow - utc).TotalMinutes);
        }

        private static string BuildReason(ATMInfo atm)
        {
            return $"صحة={atm.HealthScore}%، حالة={atm.ConnectionStatus}، Heartbeat={atm.GetElapsed(atm.LastHeartbeatUtc)}، مزامنة={atm.GetElapsed(atm.LastSyncUtc)}، فشل={atm.ConsecutiveSyncFailures}";
        }

        private static string EstimateImpact(ATMInfo atm, int risk)
        {
            if (risk >= 75) return "احتمال توقف خدمة أو فقدان مزامنة خلال فترة قصيرة";
            if (risk >= 40) return "احتمال تأخر بيانات أو تدهور اتصال";
            return "لا يوجد أثر تشغيلي واضح حالياً";
        }
    }

    public class FleetPredictionResult
    {
        public string Level { get; set; }
        public int RiskScore { get; set; }
        public string Reason { get; set; }
        public string RecommendedAction { get; set; }
        public string EstimatedImpact { get; set; }
    }
}
