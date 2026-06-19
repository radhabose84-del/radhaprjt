using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.UpdateVoucherType;
using FinanceManagement.Presentation.Validation.Common;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.VoucherType
{
    public class UpdateVoucherTypeCommandValidator : AbstractValidator<UpdateVoucherTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;

        public UpdateVoucherTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IVoucherTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthName = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.VoucherTypeMaster>("VoucherTypeName") ?? 100;

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
                        RuleFor(x => x.VoucherTypeName)
                            .NotNull().WithMessage($"{nameof(UpdateVoucherTypeCommand.VoucherTypeName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateVoucherTypeCommand.VoucherTypeName)} {rule.Error}");

                        RuleFor(x => x.NumberPadding)
                            .InclusiveBetween(1, 10)
                            .WithMessage($"{nameof(UpdateVoucherTypeCommand.NumberPadding)} must be between 1 and 10.");

                        RuleFor(x => x.AllowedAccountTypeIds)
                            .NotNull().WithMessage($"{nameof(UpdateVoucherTypeCommand.AllowedAccountTypeIds)} {rule.Error}")
                            .Must(ids => ids != null && ids.Count > 0)
                            .WithMessage("At least one allowed account type is required.");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VoucherTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(UpdateVoucherTypeCommand.VoucherTypeName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Voucher Type {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateVoucherTypeCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
