
namespace Core.Domain.Common
{
   public class EmailJobSettings
    {
        public List<EmailJobDetail>? DailyJobs { get; set; }
        public List<EmailJobDetail>? WeeklyJobs { get; set; }
        public List<EmailJobDetail>? MonthlyJobs { get; set; }
        public List<EmailJobDetail>? YearlyJobs { get; set; }
    }

    public class EmailJobDetail
    {
        public string? Action { get; set; }
        public string? WeeklyRunDay { get; set; } // For weekly jobs, specify the day of the week (e.g., "Monday")
        public int RunHour { get; set; } // Hour of execution
        public int RunMinute { get; set; } // Minute of execution
        public int MonthlyRunDay { get; set; } // For monthly jobs, specify the day of the month (e.g., 1 for the 1st day)
        public int YearlyRunMonth { get; set; } // For yearly jobs, specify the month (e.g., 1 for January)
        public int YearlyRunDay { get; set; } 
    }
}