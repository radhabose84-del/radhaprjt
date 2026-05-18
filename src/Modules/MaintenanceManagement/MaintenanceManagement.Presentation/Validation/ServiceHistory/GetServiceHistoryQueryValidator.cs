using FluentValidation;
using MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory;

namespace MaintenanceManagement.Presentation.Validation.ServiceHistory
{
    public class GetServiceHistoryQueryValidator : AbstractValidator<GetServiceHistoryQuery>
    {
        public GetServiceHistoryQueryValidator()
        {
            RuleFor(x => x)
                .Must(q => (q.MachineId.HasValue && q.MachineId.Value > 0)
                        || (q.AssetId.HasValue && q.AssetId.Value > 0))
                .WithMessage("Either MachineId or AssetId is required.");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("PageNumber must be greater than zero.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than zero.");

            RuleFor(x => x)
                .Must(q => !q.FromDate.HasValue || !q.ToDate.HasValue || q.FromDate.Value <= q.ToDate.Value)
                .WithMessage("FromDate must be less than or equal to ToDate.");
        }
    }
}
