using FluentValidation;
using SalesManagement.Application.Common.Interfaces.ISalesReturn;
using SalesManagement.Application.SalesReturn.Commands.CreateSalesReturn;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.SalesReturn
{
    public class CreateSalesReturnCommandValidator : AbstractValidator<CreateSalesReturnCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly ISalesReturnQueryRepository _queryRepo;

        public CreateSalesReturnCommandValidator(ISalesReturnQueryRepository queryRepo)
        {
            _queryRepo = queryRepo;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.ComplaintHeaderId)
                            .GreaterThan(0)
                            .WithMessage("Complaint is required.");

                        RuleFor(x => x.CustomerId)
                            .GreaterThan(0)
                            .WithMessage("Customer is required.");

                        RuleFor(x => x.WarehouseId)
                            .GreaterThan(0)
                            .WithMessage("Warehouse is required.");

                        RuleFor(x => x.BinId)
                            .GreaterThan(0)
                            .WithMessage("Bin is required.");

                        RuleFor(x => x.InvoiceDetails)
                            .NotNull()
                            .WithMessage("At least one invoice detail is required.")
                            .Must(d => d != null && d.Count > 0)
                            .WithMessage("At least one invoice detail is required.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepo.ComplaintExistsAsync(id))
                            .WithMessage("Complaint not found.")
                            .When(x => x.ComplaintHeaderId > 0);

                        RuleFor(x => x.ComplaintHeaderId)
                            .MustAsync(async (id, ct) => await _queryRepo.IsComplaintReturnEligibleAsync(id))
                            .WithMessage("Complaint is not eligible for Sales Return. Resolution type must be 'Sales Return'.")
                            .When(x => x.ComplaintHeaderId > 0);
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.InvoiceDetails).ChildRules(invoice =>
                        {
                            invoice.RuleFor(i => i.InvoiceHeaderId)
                                .GreaterThan(0)
                                .WithMessage("Invoice is required.");

                            invoice.RuleForEach(i => i.Items).ChildRules(item =>
                            {
                                item.RuleFor(d => d.InvoiceDetailId)
                                    .GreaterThan(0)
                                    .WithMessage("Invoice detail is required.");

                                item.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage("Item is required.");

                                item.RuleFor(d => d.StartPackNo)
                                    .GreaterThan(0)
                                    .WithMessage("Start Pack No must be greater than zero.");

                                item.RuleFor(d => d.EndPackNo)
                                    .GreaterThan(0)
                                    .WithMessage("End Pack No must be greater than zero.");

                                item.RuleFor(d => d.BagStatusId)
                                    .GreaterThan(0)
                                    .WithMessage("Bag Status is required.");
                            }).When(i => i.Items != null && i.Items.Count > 0);
                        }).When(x => x.InvoiceDetails != null && x.InvoiceDetails.Count > 0);
                        break;

                    case "DateCompare":
                        RuleForEach(x => x.InvoiceDetails).ChildRules(invoice =>
                        {
                            invoice.RuleForEach(i => i.Items).ChildRules(item =>
                            {
                                item.RuleFor(d => d.EndPackNo)
                                    .GreaterThanOrEqualTo(d => d.StartPackNo)
                                    .WithMessage("End Pack No must be greater than or equal to Start Pack No.");
                            }).When(i => i.Items != null && i.Items.Count > 0);
                        }).When(x => x.InvoiceDetails != null && x.InvoiceDetails.Count > 0);
                        break;

                    default:
                        break;
                }
            }

            // Pack range validations (outside switch - always applied)
            RuleForEach(x => x.InvoiceDetails).ChildRules(invoice =>
            {
                invoice.RuleForEach(i => i.Items).ChildRules(item =>
                {
                    item.RuleFor(d => d)
                        .MustAsync(async (d, ct) =>
                            await _queryRepo.PackRangeExistsInDispatchAsync(d.InvoiceDetailId, d.StartPackNo, d.EndPackNo))
                        .WithMessage("Pack range does not exist in original dispatch.")
                        .When(d => d.InvoiceDetailId > 0 && d.StartPackNo > 0 && d.EndPackNo > 0);

                    item.RuleFor(d => d)
                        .MustAsync(async (d, ct) =>
                            !await _queryRepo.PackRangeOverlapsAsync(d.InvoiceDetailId, d.StartPackNo, d.EndPackNo))
                        .WithMessage("Pack range overlaps with a previously returned range.")
                        .When(d => d.InvoiceDetailId > 0 && d.StartPackNo > 0 && d.EndPackNo > 0);
                }).When(i => i.Items != null && i.Items.Count > 0);
            }).When(x => x.InvoiceDetails != null && x.InvoiceDetails.Count > 0);
        }
    }
}
