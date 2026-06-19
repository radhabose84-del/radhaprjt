using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IScheduleIII;
using FinanceManagement.Application.ScheduleIII.Commands.BulkCreateMaster;
using FluentValidation;

namespace FinanceManagement.Presentation.Validation.ScheduleIII
{
    public class BulkCreateMasterCommandValidator : AbstractValidator<BulkCreateMasterCommand>
    {
        private readonly IScheduleIIIQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public BulkCreateMasterCommandValidator(IScheduleIIIQueryRepository queryRepository, IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            RuleFor(x => x.Lines)
                .NotEmpty().WithMessage("At least one line is required.");

            RuleForEach(x => x.Lines).ChildRules(line =>
            {
                line.RuleFor(l => l.ScheduleIIISectionId)
                    .GreaterThan(0).WithMessage("ScheduleIIISectionId is required.");
                line.RuleFor(l => l.ScheduleIIISectionItemId)
                    .GreaterThan(0).WithMessage("ScheduleIIISectionItemId is required.");
            });

            // Within-batch uniqueness — no duplicate lines or display orders in the same request.
            RuleFor(x => x.Lines)
                .Must(lines => lines.Select(l => l.ScheduleIIISectionItemId).Distinct().Count() == lines.Count)
                .WithMessage("Duplicate line items in the request.")
                .Must(lines =>
                {
                    var orders = lines.Where(l => l.DisplayOrder > 0).Select(l => l.DisplayOrder).ToList();
                    return orders.Distinct().Count() == orders.Count;
                })
                .WithMessage("Duplicate display orders in the request.")
                .When(x => x.Lines != null && x.Lines.Count > 0);

            // Structure not locked + every line valid against the DB (token company/division).
            RuleFor(x => x.Lines)
                .MustAsync(async (lines, ct) =>
                {
                    var companyId = _ipAddressService.GetCompanyId();
                    var divisionId = _ipAddressService.GetDivisionId();
                    if (companyId is null || divisionId is null) return true;

                    if (await _queryRepository.IsStructureLockedAsync(companyId.Value, divisionId.Value)) return false;

                    foreach (var l in lines)
                    {
                        if (await _queryRepository.LineItemNotFoundAsync(l.ScheduleIIISectionItemId)) return false;
                        if (!await _queryRepository.SectionExistsAsync(l.ScheduleIIISectionId)) return false;
                        if (await _queryRepository.DetailLineExistsAsync(companyId.Value, divisionId.Value, l.ScheduleIIISectionItemId)) return false;
                        if (l.DisplayOrder > 0 && await _queryRepository.DetailDisplayOrderExistsAsync(companyId.Value, divisionId.Value, l.DisplayOrder)) return false;
                    }
                    return true;
                })
                .WithMessage("One or more lines are invalid: section/line not found, line already in the structure, display order already taken, or the structure is locked.")
                .When(x => x.Lines != null && x.Lines.Count > 0);
        }
    }
}
