namespace BackgroundService.Application.DTO
{
    public class NotificationTargetDto
    {
        public string ModuleName { get; set; } = "";
        public string EventType { get; set; } = "";
        public string TargetType { get; set; } = "";
        public string ChannelName { get; set; } = "";
        public string RecipientType { get; set; } = "";
        public string? TargetUserIds { get; set; }
        public string? HeaderTemplate { get; set; }
        public string? BodyTemplate { get; set; }
        public string? FooterTemplate { get; set; }
        public string? LanguageCode { get; set; }
        public string? SubjectTemplate { get; set; }
        public string? TargetEmailIds { get; set; }
        public string? TargetCcEmails { get; set; }
        public string? TargetBccEmails { get; set; }
        public string? TargetMobileNumbers { get; set; }
        public int? EventRuleId { get; set; }
        public int? ChannelId { get; set; }
        public int? EventTypeId { get; set; }
        public int TemplateId { get; set; }
        public bool IsTable { get; set; }
        public string? ApiToken { get; set; } 
    }

}