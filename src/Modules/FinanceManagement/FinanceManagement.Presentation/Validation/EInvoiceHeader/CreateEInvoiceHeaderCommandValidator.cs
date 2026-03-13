using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.CreateEInvoiceHeader;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EInvoiceHeader
{
    public class CreateEInvoiceHeaderCommandValidator : AbstractValidator<CreateEInvoiceHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;

        public CreateEInvoiceHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IEInvoiceHeaderQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthInvoiceNo  = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EInvoiceHeader>("InvoiceNo")  ?? 30;
            var maxLengthIrnNumber  = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EInvoiceHeader>("IrnNumber")  ?? 100;
            var maxLengthAckNo      = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EInvoiceHeader>("AckNo")      ?? 50;
            var maxLengthGstNo      = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EInvoiceHeader>("GstNo")      ?? 20;

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
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.InvoiceNo)
                            .NotNull().WithMessage($"{nameof(CreateEInvoiceHeaderCommand.InvoiceNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateEInvoiceHeaderCommand.InvoiceNo)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.InvoiceNo)
                            .MaximumLength(maxLengthInvoiceNo)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.InvoiceNo)} {rule.Error} {maxLengthInvoiceNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.InvoiceNo));

                        RuleFor(x => x.IrnNumber)
                            .MaximumLength(maxLengthIrnNumber)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.IrnNumber)} {rule.Error} {maxLengthIrnNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.IrnNumber));

                        RuleFor(x => x.AckNo)
                            .MaximumLength(maxLengthAckNo)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.AckNo)} {rule.Error} {maxLengthAckNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.AckNo));

                        RuleFor(x => x.GstNo)
                            .MaximumLength(maxLengthGstNo)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.GstNo)} {rule.Error} {maxLengthGstNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.GstNo));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.IrnNumber)
                            .MustAsync(async (irnNumber, ct) =>
                                !await _queryRepository.IrnNumberExistsAsync(irnNumber!))
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.IrnNumber)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.IrnNumber));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.CGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.CGST)} {rule.Error}");

                        RuleFor(x => x.SGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.SGST)} {rule.Error}");

                        RuleFor(x => x.IGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.IGST)} {rule.Error}");

                        RuleFor(x => x.TCS)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.TCS)} {rule.Error}");

                        RuleFor(x => x.Discount)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.Discount)} {rule.Error}");

                        RuleFor(x => x.Cess)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.Cess)} {rule.Error}");

                        RuleFor(x => x.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.OtherCharges)} {rule.Error}");
                        break;

                    case "GstFormat":
                        RuleFor(x => x.GstNo)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateEInvoiceHeaderCommand.GstNo)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GstNo));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
