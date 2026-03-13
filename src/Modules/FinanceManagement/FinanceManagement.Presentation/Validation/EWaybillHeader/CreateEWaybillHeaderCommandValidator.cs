using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EWaybillHeader
{
    public class CreateEWaybillHeaderCommandValidator : AbstractValidator<CreateEWaybillHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEWaybillHeaderQueryRepository _queryRepository;

        public CreateEWaybillHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IEWaybillHeaderQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthEWBNumber    = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EWaybillHeader>("EWBNumber")    ?? 50;
            var maxLengthInvoiceNo    = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EWaybillHeader>("InvoiceNo")    ?? 30;
            var maxLengthVehicleNo    = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EWaybillHeader>("VehicleNo")    ?? 20;
            var maxLengthTransDocNo   = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EWaybillHeader>("TransDocNo")   ?? 30;

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
                        RuleFor(x => x.UnitId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.UnitId)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.EWBNumber)
                            .MaximumLength(maxLengthEWBNumber)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.EWBNumber)} {rule.Error} {maxLengthEWBNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.EWBNumber));

                        RuleFor(x => x.InvoiceNo)
                            .MaximumLength(maxLengthInvoiceNo)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.InvoiceNo)} {rule.Error} {maxLengthInvoiceNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.InvoiceNo));

                        RuleFor(x => x.VehicleNo)
                            .MaximumLength(maxLengthVehicleNo)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.VehicleNo)} {rule.Error} {maxLengthVehicleNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNo));

                        RuleFor(x => x.TransDocNo)
                            .MaximumLength(maxLengthTransDocNo)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.TransDocNo)} {rule.Error} {maxLengthTransDocNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.TransDocNo));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.EWBNumber)
                            .MustAsync(async (ewbNumber, ct) =>
                                !await _queryRepository.EWBNumberExistsAsync(ewbNumber!))
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.EWBNumber)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.EWBNumber));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.InvoiceValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.InvoiceValue)} {rule.Error}");

                        RuleFor(x => x.TotalValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.TotalValue)} {rule.Error}");

                        RuleFor(x => x.CGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.CGST)} {rule.Error}");

                        RuleFor(x => x.SGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.SGST)} {rule.Error}");

                        RuleFor(x => x.IGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.IGST)} {rule.Error}");

                        RuleFor(x => x.Cess)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEWaybillHeaderCommand.Cess)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
