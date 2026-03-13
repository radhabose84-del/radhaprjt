using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEInvoiceHeader;
using FinanceManagement.Application.EInvoiceHeader.Commands.UpdateEInvoiceHeader;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EInvoiceHeader
{
    public class UpdateEInvoiceHeaderCommandValidator : AbstractValidator<UpdateEInvoiceHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEInvoiceHeaderQueryRepository _queryRepository;

        public UpdateEInvoiceHeaderCommandValidator(
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
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.UnitId)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.InvoiceNo)
                            .NotNull().WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.InvoiceNo)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.InvoiceNo)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.InvoiceNo)
                            .MaximumLength(maxLengthInvoiceNo)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.InvoiceNo)} {rule.Error} {maxLengthInvoiceNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.InvoiceNo));

                        RuleFor(x => x.IrnNumber)
                            .MaximumLength(maxLengthIrnNumber)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.IrnNumber)} {rule.Error} {maxLengthIrnNumber} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.IrnNumber));

                        RuleFor(x => x.AckNo)
                            .MaximumLength(maxLengthAckNo)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.AckNo)} {rule.Error} {maxLengthAckNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.AckNo));

                        RuleFor(x => x.GstNo)
                            .MaximumLength(maxLengthGstNo)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.GstNo)} {rule.Error} {maxLengthGstNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.GstNo));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"EInvoice Header {rule.Error}");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.IrnNumber)
                            .MustAsync(async (command, irnNumber, ct) =>
                                !await _queryRepository.IrnNumberExistsAsync(irnNumber!, command.Id))
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.IrnNumber)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.IrnNumber));
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.CGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.CGST)} {rule.Error}");

                        RuleFor(x => x.SGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.SGST)} {rule.Error}");

                        RuleFor(x => x.IGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.IGST)} {rule.Error}");

                        RuleFor(x => x.TCS)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.TCS)} {rule.Error}");

                        RuleFor(x => x.Discount)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.Discount)} {rule.Error}");

                        RuleFor(x => x.Cess)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.Cess)} {rule.Error}");

                        RuleFor(x => x.OtherCharges)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.OtherCharges)} {rule.Error}");
                        break;

                    case "GstFormat":
                        RuleFor(x => x.GstNo)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.GstNo)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.GstNo));
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateEInvoiceHeaderCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
