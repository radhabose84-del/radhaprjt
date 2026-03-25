using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IComplaint;
using SalesManagement.Application.Complaint.Commands.UpdateComplaint;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.Complaint
{
    public class UpdateComplaintCommandValidator : AbstractValidator<UpdateComplaintCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IComplaintQueryRepository _queryRepository;

        public UpdateComplaintCommandValidator(
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
                            .WithMessage($"{nameof(UpdateComplaintCommand.CustomerId)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Remarks)
                            .MaximumLength(maxLengthRemarks)
                            .WithMessage($"{nameof(UpdateComplaintCommand.Remarks)} {rule.Error} {maxLengthRemarks} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.Remarks));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"Complaint {rule.Error}");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.CustomerId)
                            .MustAsync(async (id, ct) => await _queryRepository.CustomerExistsAsync(id))
                            .WithMessage($"{nameof(UpdateComplaintCommand.CustomerId)} {rule.Error}")
                            .When(x => x.CustomerId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateComplaintCommand.IsActive)} {rule.Error}");
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
