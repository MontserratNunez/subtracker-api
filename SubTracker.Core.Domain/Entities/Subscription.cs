using SubTracker.Core.Domain.Common.Enums;

namespace SubTracker.Core.Domain.Entities
{
    public class Subscription
    {
        int Id { get; set; }
        public string UserId { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public SubscriptionFrequency Frequency { get; set; }
        public int PaymentDay { get; set; }
        public int? PaymentMonth { get; set; }
        public int ReminderDaysBefore { get; set; }

        public int? TypeId { get; set; }
        public SubscriptionCategory? Type { get; set; }


        public DateTime? InactiveDate { get; set; }
        public DateTime Created { get; set; }

        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }

        public DateTime GetNextPaymentDate(DateTime fromDate)
        {
            if (Frequency == SubscriptionFrequency.MONTHLY)
            {
                int daysInMonth = DateTime.DaysInMonth(fromDate.Year, fromDate.Month);
                int validDay = Math.Min(PaymentDay, daysInMonth);
                var targetDate = new DateTime(fromDate.Year, fromDate.Month, validDay);

                return targetDate < fromDate ? targetDate.AddMonths(1) : targetDate;
            }
            else
            {
                int month = PaymentMonth ?? 1;
                int daysInMonth = DateTime.DaysInMonth(fromDate.Year, month);
                int validDay = Math.Min(PaymentDay, daysInMonth);
                var targetDate = new DateTime(fromDate.Year, month, validDay);

                return targetDate < fromDate ? targetDate.AddYears(1) : targetDate;
            }
        }

        public DateTime GetNextReminderDate(DateTime fromDate)
        {
            return GetNextPaymentDate(fromDate).AddDays(-ReminderDaysBefore);
        }

    }
}
