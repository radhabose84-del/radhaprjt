using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.UpdateProformaInvoice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ProformaInvoice
{
    public class UpdateProformaInvoiceCommandValidator : AbstractValidator<UpdateProformaInvoiceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProformaInvoiceQueryRepository _queryRepository;

        public UpdateProformaInvoiceCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IProformaInvoiceQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ProformaInvoice>("Remarks") ?? 500;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateProformaInvoiceCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"ProformaInvoice {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.StatusId)
                            .MustAsync(async (id, ct) => await _queryRepository.StatusExistsAsync(id!.Value))
                            .WithMessage($"{nameof(UpdateProformaInvoiceCommand.StatusId)} {rule.Error}")
                            .When(x => x.StatusId.HasValue && x.StatusId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateProformaInvoiceCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
