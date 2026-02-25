using PurchaseManagement.Application.IssueReturn.Command.CreateIssueReturn;
using FluentValidation;
using PurchaseManagement.Presentation.Validation.Common;
using Shared.Validation.Common;
using Contracts.Interfaces.Lookups.Workflow;

namespace PurchaseManagement.Presentation.Validation.IssueReturn
{
    public class CreateIssueReturnEntryCommandValidator : AbstractValidator<CreateIssueReturnEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IWorkflowLookup _workflowLookup;

        public CreateIssueReturnEntryCommandValidator(MaxLengthProvider maxLengthProvider, IWorkflowLookup workflowLookup)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _workflowLookup = workflowLookup;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "Workflow":
                            RuleFor(x => x.IssueReturnEntry.UnitId)
                                .MustAsync(async (unitId, cancellation) =>
                                    await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                        PurchaseManagement.Domain.Common.MiscEnumEntity.MaterialRequest, // entity type
                                        unitId,
                                        0))                      // DepartmentId not required, pass null
                                .WithMessage(rule.Error);
                            break;

                    case "NotEmpty":
                        RuleFor(x => x.IssueReturnEntry.RequestCategoryId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(CreateIssueReturnEntryCommand.IssueReturnEntry.RequestCategoryId)} {rule.Error}");

                        RuleFor(x => x.IssueReturnEntry.DepartmentId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(CreateIssueReturnEntryCommand.IssueReturnEntry.DepartmentId)} {rule.Error}");

                        RuleForEach(x => x.IssueReturnEntry.IssueReturnDetails).ChildRules(GateEntry =>
                       {
                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.ItemId)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateIssueReturnEntryCommand.IssueReturnEntry.IssueReturnDetails)}.{nameof(CreateIssueReturnDto.CreateIssueReturnDetailDto.ItemId)} {rule.Error}");


                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.ReturnQuantity)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueReturnEntryCommand.IssueReturnEntry.IssueReturnDetails)}.{nameof(CreateIssueReturnDto.CreateIssueReturnDetailDto.ReturnQuantity)} {rule.Error}");

                             // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.ReturnValue)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueReturnEntryCommand.IssueReturnEntry.IssueReturnDetails)}.{nameof(CreateIssueReturnDto.CreateIssueReturnDetailDto.ReturnValue)} {rule.Error}");

                              GateEntry.RuleFor(pt => pt.ReasonId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueReturnEntryCommand.IssueReturnEntry.IssueReturnDetails)}.{nameof(CreateIssueReturnDto.CreateIssueReturnDetailDto.ReasonId)} {rule.Error}");

                           // ✅ Optional: DcQuantity ≥ 0
                           GateEntry.RuleFor(pt => pt.ReturnQuantity)
                               .GreaterThanOrEqualTo(0)
                               .WithMessage("Request Quantity must be a positive value.");
                       });
                        break;
                }
            }
           
        }
    }
}