using Contracts.Interfaces.Lookups.Workflow;
using InventoryManagement.Application.MRS.Command.CreateMrsEntry;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using static InventoryManagement.Application.MRS.Command.CreateMrsEntry.CreateMrsEntryDto;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.MRS
{
    public class CreateMrsEntryCommandValidator  : AbstractValidator<CreateMrsEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IWorkflowLookup _workflowLookup;
        public CreateMrsEntryCommandValidator(MaxLengthProvider maxLengthProvider, IWorkflowLookup workflowLookup)
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
                            RuleFor(x => x.MrsEntry.UnitId)
                                .MustAsync(async (unitId, cancellation) =>
                                    await _workflowLookup.IsApproveWorkflowConfigureAsync(
                                        InventoryManagement.Domain.Common.MiscEnumEntity.MaterialRequest, // entity type
                                        unitId,
                                        0))                      // DepartmentId not required, pass null
                                .WithMessage(rule.Error);
                            break;
                    case "NotEmpty":
                        RuleFor(x => x.MrsEntry.RequestCategoryId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(CreateMrsEntryCommand.MrsEntry.RequestCategoryId)} {rule.Error}");

                        RuleFor(x => x.MrsEntry.DepartmentId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(CreateMrsEntryCommand.MrsEntry.DepartmentId)} {rule.Error}");

                        RuleFor(x => x.MrsEntry.SubDepartmentId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(CreateMrsEntryCommand.MrsEntry.SubDepartmentId)} {rule.Error}");


                        RuleForEach(x => x.MrsEntry.MrsDetails).ChildRules(GateEntry =>
                       {
                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.RequestQuantity)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateMrsEntryCommand.MrsEntry.MrsDetails)}.{nameof(CreateMrsDetailDto.RequestQuantity)} {rule.Error}");


                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.ItemId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateMrsEntryCommand.MrsEntry.MrsDetails)}.{nameof(CreateMrsDetailDto.ItemId)} {rule.Error}");


                           // ✅ Optional: DcQuantity ≥ 0
                           GateEntry.RuleFor(pt => pt.RequestQuantity)
                               .GreaterThanOrEqualTo(0)
                               .WithMessage("Request Quantity must be a positive value.");
                       });
                        break;
                }
            }
        }
    }
}
