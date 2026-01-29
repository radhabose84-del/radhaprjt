using Core.Domain.Common;

namespace Core.Domain.Entities
{
    public class Emailconfig:BaseEntity
    {
        public int Id { get; set; }
        public string? Subject { get; set; }
        public string? FromAddress { get; set; }
        public string? ToAddress { get; set; }
        public string? CcAddress { get; set; }
        public string? BccAddress  { get; set; }                
        public string? MailType { get; set; }
        public string? MailTime { get; set; }
        public int MailDay { get; set; }
        public int MailMonth { get; set; }
        public DateTime SentDate { get; set; }
        public byte IsActive { get; set; } 
    }
}