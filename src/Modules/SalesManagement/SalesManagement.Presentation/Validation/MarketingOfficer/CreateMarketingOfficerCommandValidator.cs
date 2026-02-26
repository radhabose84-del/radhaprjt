using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.MarketingOfficer
{
    public class CreateMarketingOfficerCommandValidator : AbstractValidator<CreateMarketingOfficerCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMarketingOfficerQueryRepository _queryRepo;

        public CreateMarketingOfficerCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMarketingOfficerQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            var maxLengthEmployeeNo = maxLengthProvider.GetMaxLength<Domain.Entities.MarketingOfficer>("EmployeeNo") ?? 20;
            var maxLengthEmployeeName = maxLengthProvider.GetMaxLength<Domain.Entities.MarketingOfficer>("EmployeeName") ?? 100;
            var maxLengthUnit = maxLengthProvider.GetMaxLength<Domain.Entities.MarketingOfficer>("Unit") ?? 100;
            var maxLengthDepartment = maxLengthProvider.GetMaxLength<Domain.Entities.MarketingOfficer>("Department") ?? 100;
            var maxLengthDesignation = maxLengthProvider.GetMaxLength<Domain.Entities.MarketingOfficer>("Designation") ?? 100;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.EmployeeNo)
                            .NotNull().WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeNo)} {rule.Error}");

                        RuleFor(x => x.EmployeeName)
                            .NotNull().WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeName)} {rule.Error}");

                        RuleFor(x => x.Unit)
                            .NotNull().WithMessage($"{nameof(CreateMarketingOfficerCommand.Unit)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMarketingOfficerCommand.Unit)} {rule.Error}");

                        RuleFor(x => x.Department)
                            .NotNull().WithMessage($"{nameof(CreateMarketingOfficerCommand.Department)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMarketingOfficerCommand.Department)} {rule.Error}");

                        RuleFor(x => x.Designation)
                            .NotNull().WithMessage($"{nameof(CreateMarketingOfficerCommand.Designation)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateMarketingOfficerCommand.Designation)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.EmployeeNo)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeNo)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeNo));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.EmployeeNo)
                            .MaximumLength(maxLengthEmployeeNo)
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeNo)} {rule.Error} {maxLengthEmployeeNo} characters.");

                        RuleFor(x => x.EmployeeName)
                            .MaximumLength(maxLengthEmployeeName)
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeName)} {rule.Error} {maxLengthEmployeeName} characters.");

                        RuleFor(x => x.Unit)
                            .MaximumLength(maxLengthUnit)
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.Unit)} {rule.Error} {maxLengthUnit} characters.");

                        RuleFor(x => x.Department)
                            .MaximumLength(maxLengthDepartment)
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.Department)} {rule.Error} {maxLengthDepartment} characters.");

                        RuleFor(x => x.Designation)
                            .MaximumLength(maxLengthDesignation)
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.Designation)} {rule.Error} {maxLengthDesignation} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOfficeId)
                            .MustAsync(async (id, ct) => await _queryRepo.SalesOfficeExistsAsync(id))
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.SalesOfficeId)} {rule.Error}")
                            .When(x => x.SalesOfficeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.EmployeeNo)
                            .MustAsync(async (code, ct) => !await _queryRepo.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateMarketingOfficerCommand.EmployeeNo)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.EmployeeNo));
                        break;

                    default:
                        break;
                }
            }

            // ── Custom field-specific business rules ────────────────────────
            RuleFor(x => x.MobileNo)
                .Matches(@"^[6-9]\d{9}$")
                .WithMessage($"{nameof(CreateMarketingOfficerCommand.MobileNo)} must be a valid 10-digit mobile number starting with 6-9.")
                .When(x => !string.IsNullOrWhiteSpace(x.MobileNo));

            RuleFor(x => x.Email)
                .EmailAddress()
                .WithMessage($"{nameof(CreateMarketingOfficerCommand.Email)} must be a valid email address.")
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            RuleFor(x => x.SalesGroups)
                .NotNull().WithMessage("SalesGroups is required.")
                .Must(sg => sg != null && sg.Count > 0).WithMessage("At least one Sales Group is required.");

            RuleFor(x => x.SalesGroups)
                .Must(sg => sg == null || sg.Select(g => g.SalesGroupId).Distinct().Count() == sg.Count)
                .WithMessage("Duplicate SalesGroupId values are not allowed.")
                .When(x => x.SalesGroups != null && x.SalesGroups.Count > 0);

            RuleFor(x => x.SalesGroups)
                .MustAsync(async (groups, ct) =>
                {
                    if (groups == null || groups.Count == 0) return true;
                    var ids = groups.Select(g => g.SalesGroupId).Distinct().ToList();
                    return await _queryRepo.SalesGroupsAllExistAsync(ids);
                })
                .WithMessage("One or more SalesGroupId values are inactive or deleted.")
                .When(x => x.SalesGroups != null && x.SalesGroups.Count > 0);
        }
    }
}
