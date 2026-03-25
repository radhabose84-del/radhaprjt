using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.CreateComplaint;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.Complaint
{
    public class CreateComplaintCommandValidator : AbstractValidator<CreateComplaintCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintQueryRepository _queryRepository;

        public CreateComplaintCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IComplaintQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthRemarks = maxLengthProvider.GetMaxLength<Domain.Entities.ComplaintHeader>("Remarks") ?? 500;

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
                        RuleFor(x => x.CustomerId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateComplaintCommand.CustomerId)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(CreateComplaintCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (id, ct) => await _queryRepository.CustomerExistsAsync(id))
                            .WithMessage($"{nameof(CreateComplaintCommand.CustomerId)} {rule.Error}")
                            .When(x => x.CustomerId > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.InvoiceHeaderId)
                                    .GreaterThan(0)
                                    .WithMessage($"InvoiceHeaderId {rule.Error}");

                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.NumberOfPacks)
                                    .GreaterThan(0)
                                    .WithMessage($"NumberOfPacks {rule.Error}");

                                detail.RuleFor(d => d.NetWeight)
                                    .GreaterThan(0)
                                    .WithMessage($"NetWeight {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.InvoiceAmount)
                                    .GreaterThanOrEqualTo(0)
                                    .WithMessage($"InvoiceAmount {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    default:
                        break;
                }
            }

            // Business rule: All invoices must belong to selected customer
            RuleFor(x => x)
                .MustAsync(async (command, ct) =>
                {
                    if (command.Details == null || command.Details.Count == 0 || command.CustomerId <= 0)
                        return true;

                    foreach (var detail in command.Details)
                    {
                        if (detail.InvoiceHeaderId > 0)
                        {
                            var belongs = await _queryRepository.InvoiceBelongsToCustomerAsync(detail.InvoiceHeaderId, command.CustomerId);
                            if (!belongs) return false;
                        }
                    }
                    return true;
                })
                .WithMessage("One or more invoices do not belong to the selected customer.")
                .When(x => x.Details != null && x.Details.Any() && x.CustomerId > 0);

            // Nature of Complaint is mandatory per line
            RuleForEach(x => x.Details)
                .ChildRules(detail =>
                {
                    detail.RuleFor(d => d.NatureOfComplaintIds)
                        .NotNull()
                        .WithMessage("At least one Nature of Complaint is required.")
                        .NotEmpty()
                        .WithMessage("At least one Nature of Complaint is required.");
                })
                .When(x => x.Details != null && x.Details.Any());
        }
    }
}
