using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InventoryManagement.Application.Issue.Command.CreateIssueEntry;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using static InventoryManagement.Application.Issue.Command.CreateIssueEntry.CreateIssueEntryDto;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Issue
{
    public class CreateIssueEntryCommandValidator : AbstractValidator<CreateIssueEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public CreateIssueEntryCommandValidator(MaxLengthProvider maxLengthProvider)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.IssueEntry.MrsHeaderId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.MrsHeaderId)} {rule.Error}");

                        // RuleFor(x => x.IssueEntry.SubStoresWarehouseId)
                        // .NotEmpty()
                        // .GreaterThanOrEqualTo(0)
                        //  .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.SubStoresWarehouseId)} {rule.Error}");


                        RuleForEach(x => x.IssueEntry.IssueDetails).ChildRules(GateEntry =>
                       {
                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.RequestQuantity)
                               .NotEmpty()
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.RequestQuantity)} {rule.Error}");


                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.ItemId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.ItemId)} {rule.Error}");


                           GateEntry.RuleFor(pt => pt.UomId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.UomId)} {rule.Error}");


                           GateEntry.RuleFor(pt => pt.WarehouseStockId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.WarehouseStockId)} {rule.Error}");


                           GateEntry.RuleFor(pt => pt.StorageTypeId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.StorageTypeId)} {rule.Error}");

                           GateEntry.RuleFor(pt => pt.TargetId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.TargetId)} {rule.Error}");


                           GateEntry.RuleFor(pt => pt.IssueQuantity)
                              .NotEmpty()
                                .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.IssueQuantity)} {rule.Error}");

                           GateEntry.RuleFor(pt => pt.IssueValue)
                              .NotEmpty()
                                .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.IssueValue)} {rule.Error}");

                           GateEntry.RuleFor(pt => pt.CostCenterId)
                              .NotEmpty()
                                .GreaterThan(0)
                               .WithMessage($"{nameof(CreateIssueEntryCommand.IssueEntry.IssueDetails)}.{nameof(CreateIssueDetailDto.CostCenterId)} {rule.Error}");


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
