using FinanceManagement.Application.Common.Interfaces.IVoucherTypeMaster;
using FinanceManagement.Application.VoucherType.Commands.DeleteVoucherType;
using FluentValidation;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.VoucherType
{
    public class DeleteVoucherTypeCommandValidator : AbstractValidator<DeleteVoucherTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IVoucherTypeMasterQueryRepository _queryRepository;

        public DeleteVoucherTypeCommandValidator(IVoucherTypeMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Id)
                            .NotEmpty()
                            .WithMessage($"{nameof(DeleteVoucherTypeCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Voucher Type {rule.Error}");
                        break;

                    case "SoftDelete":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.SoftDeleteValidationAsync(id))
                            .WithMessage(rule.Error);
                        break;

                    default:
                        break;
                }
            }

            // System voucher types (JV/PV/RV/CV) are immutable — cannot be deleted.
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => !await _queryRepository.IsSystemAsync(id))
                .WithMessage("System voucher types cannot be deleted.");
        }
    }
}
