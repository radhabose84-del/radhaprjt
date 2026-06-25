using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.IFinancialYearMaster;
using FinanceManagement.Application.FinancialYearMaster.Commands.UpdateFinancialYearMaster;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.FinancialYearMaster
{
    public class UpdateFinancialYearMasterCommandValidator : AbstractValidator<UpdateFinancialYearMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IFinancialYearMasterQueryRepository _queryRepository;
        private readonly IIPAddressService _ipAddressService;

        public UpdateFinancialYearMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IFinancialYearMasterQueryRepository queryRepository,
            IIPAddressService ipAddressService)
        {
            _queryRepository = queryRepository;
            _ipAddressService = ipAddressService;

            var maxLengthCode = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.FinancialYearMaster>("FinancialYearCode") ?? 9;

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
                        RuleFor(x => x.FinancialYearCode)
                            .NotNull()
                            .WithMessage($"{nameof(UpdateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.FinancialYearCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(UpdateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error} {maxLengthCode} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) =>
                                !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Financial Year {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x)
                            .MustAsync(async (cmd, ct) =>
                            {
                                if (string.IsNullOrWhiteSpace(cmd.FinancialYearCode)) return true;
                                var companyId = _ipAddressService.GetCompanyId() ?? 0;
                                return !await _queryRepository.AlreadyExistsByCodeAsync(cmd.FinancialYearCode, companyId, cmd.Id);
                            })
                            .WithMessage($"{nameof(UpdateFinancialYearMasterCommand.FinancialYearCode)} {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateFinancialYearMasterCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            RuleFor(x => x.FinancialYearCode)
                .Matches(@"^\d{4}-\d{2}$")
                .WithMessage("Financial Year Code must follow format YYYY-YY (e.g. 2024-25).")
                .When(x => !string.IsNullOrWhiteSpace(x.FinancialYearCode));
        }
    }
}
