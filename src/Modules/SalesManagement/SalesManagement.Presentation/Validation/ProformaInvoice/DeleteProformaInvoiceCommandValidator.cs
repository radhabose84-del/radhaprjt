using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.DeleteProformaInvoice;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.ProformaInvoice
{
    public class DeleteProformaInvoiceCommandValidator : AbstractValidator<DeleteProformaInvoiceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IProformaInvoiceQueryRepository _queryRepository;

        public DeleteProformaInvoiceCommandValidator(IProformaInvoiceQueryRepository queryRepository)
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
                            .WithMessage($"{nameof(DeleteProformaInvoiceCommand.Id)} {rule.Error}");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"ProformaInvoice {rule.Error}");
                        break;

                    default:
                        break;
                }
            }

            // Only Draft status proformas can be deleted
            RuleFor(x => x.Id)
                .MustAsync(async (id, ct) => await _queryRepository.IsDraftStatusAsync(id))
                .WithMessage("Only Proforma Invoices in Draft status can be deleted.")
                .When(x => x.Id > 0);
        }
    }
}
