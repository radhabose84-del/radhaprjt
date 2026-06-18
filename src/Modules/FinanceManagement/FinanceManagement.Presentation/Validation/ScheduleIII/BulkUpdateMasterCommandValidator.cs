using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.BulkUpdateMaster;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class BulkUpdateMasterCommandValidator : AbstractValidator<BulkUpdateMasterCommand>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public BulkUpdateMasterCommandValidator(IScheduleIIIQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("At least one line is required.");

            RuleForEach(x => x.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.Id).GreaterThan(0).WithMessage("A valid line Id is required.");
                line.RuleFor(l => l.IsActive).InclusiveBetween(0, 1).WithMessage("IsActive must be 0 or 1.");
            });

            // Within-batch uniqueness — no duplicate ids or display orders in the same request.
            RuleFor(x => x.Lines)
                .Must(lines => lines.Select(l => l.Id).Distinct().Count() == lines.Count)
                .WithMessage("Duplicate line ids in the request.")
                .Must(lines =>
                {
                    var orders = lines.Where(l => l.DisplayOrder > 0).Select(l => l.DisplayOrder).ToList();
                    return orders.Distinct().Count() == orders.Count;
                })
                .WithMessage("Duplicate display orders in the request.")
                .When(x => x.Lines != null && x.Lines.Count > 0);

            // Structure not locked + every line exists (token company/division).
            RuleFor(x => x.Lines)
                .MustAsync(async (lines, ct) =>
                {
                    var companyId = _ipAddressService.GetCompanyId();
                    var divisionId = _ipAddressService.GetDivisionId();
                    if (companyId is not null && divisionId is not null
                        && await _queryRepository.IsStructureLockedAsync(companyId.Value, divisionId.Value))
                        return false;

                    foreach (var l in lines)
                    {
                        if (await _queryRepository.DetailNotFoundAsync(l.Id)) return false;
                        if (await _queryRepository.LineItemNotFoundAsync(l.ScheduleIIISectionItemId)) return false;
                    }
                    return true;
                })
                .WithMessage("One or more lines are invalid: line not found, referenced item not found, or the structure is locked.")
                .When(x => x.Lines != null && x.Lines.Count > 0);
        }
    }
}
