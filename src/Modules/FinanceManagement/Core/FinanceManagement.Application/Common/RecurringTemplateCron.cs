using System.Globalization;

namespace FinanceManagement.Application.Common
{
    // Maps a RECURRING_FREQUENCY code + the template StartDate to a 5-field cron expression, anchored at the
    // start date's day-of-month (clamped to 28 so it fires every month) / month. Used by the Hangfire scheduler.
    public static class RecurringTemplateCron
    {
        public static string For(string? frequencyCode, DateOnly startDate)
        {
            var day = Math.Min(Math.Max(startDate.Day, 1), 28);

            switch ((frequencyCode ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "DAILY":
                    return "0 0 * * *";
                case "WEEKLY":
                    return $"0 0 * * {(int)startDate.DayOfWeek}";
                case "QUARTERLY":
                    var m = startDate.Month;
                    var months = string.Join(",", new[] { m, Wrap(m + 3), Wrap(m + 6), Wrap(m + 9) }
                        .Distinct().OrderBy(x => x).Select(x => x.ToString(CultureInfo.InvariantCulture)));
                    return $"0 0 {day} {months} *";
                case "ANNUALLY":
                case "YEARLY":
                    return $"0 0 {day} {startDate.Month} *";
                case "MONTHLY":
                default:
                    return $"0 0 {day} * *";
            }
        }

        private static int Wrap(int month) => ((month - 1) % 12) + 1;
    }
}
