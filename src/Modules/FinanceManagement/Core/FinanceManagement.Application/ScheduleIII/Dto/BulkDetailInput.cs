namespace FinanceManagement.Application.ScheduleIII.Dto
{
    // One line in a bulk add (ScheduleIIIDetail). Header is resolved from the token.
    public class BulkDetailLineInput
    {
        public int ScheduleIIISectionId { get; set; }
        public int ScheduleIIISectionItemId { get; set; }
        public int DisplayOrder { get; set; }
    }

    // One line in a bulk update (ScheduleIIIDetail). Id is the detail row id.
    public class BulkDetailLineUpdate
    {
        public int Id { get; set; }
        public int ScheduleIIISectionId { get; set; }
        public int ScheduleIIISectionItemId { get; set; }
        public int DisplayOrder { get; set; }
        public int IsActive { get; set; }   // 1 = Active, 0 = Inactive
    }
}
