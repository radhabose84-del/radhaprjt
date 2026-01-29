
    namespace BackgroundService.Application.Interfaces.Notification
    {
        public interface IEmailSender
        {
             Task<bool> SendEmailAsync(List<string> emails, string subject,string header, string message, string footer,List<string>? CcEmails = null, List<string>? BccEmails = null,int channelId = 0, int eventTypeId = 0, int eventRuleId = 0,
             IEnumerable<(string FileName, string ContentType, byte[] Data)>? attachments = null);
        }
    }