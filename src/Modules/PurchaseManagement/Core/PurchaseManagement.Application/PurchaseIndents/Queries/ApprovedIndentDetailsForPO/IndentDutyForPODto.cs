
    namespace PurchaseManagement.Application.PurchaseIndents.Queries.ApprovedIndentDetailsForPO
    {
        public class IndentDutyForPODto
        {
            public int Id { get; set; }
            public string TariffNumber { get; set; } = default!;
            public string? HsnCode { get; set; }
            public int DutyCategoryId { get; set; }
            public decimal BasicCustomsDutyPercentage { get; set; }
            public decimal SocialWelfareSurchargePercentage { get; set; }
            public decimal IGSTPercentage { get; set; }
            public decimal? AgriInfraDevCessPercentage { get; set; }
            public decimal? AntiDumpingDutyPercentage { get; set; }
            public decimal? SafeguardDutyPercentage { get; set; }
            public decimal? HealthEducationCessPercentage { get; set; }
            public decimal? TotalDuty { get; set; }
            public DateTimeOffset EffectiveFrom { get; set; }
            public DateTimeOffset? EffectiveTo { get; set; }
            public int CountryOfOriginApplicability { get; set; }
            public string? NotificationNumber { get; set; }
            public string? Remarks { get; set; }
            public string? Description { get; set; }
            public int IsActive { get; set; }
            public String? DutyCategoryName { get; set; }
            public String? CountryOfOriginApplicabilityName { get; set; }
        }
    }
