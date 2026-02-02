using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Command.UpdateMrsEntry;
using FluentValidation;
using PurchaseManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace PurchaseManagement.API.Validation.MRS
{
    public class UpdateMrsEntryCommandValidator : AbstractValidator<UpdateMrsEntryCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMrsEntryQueryRepository _iMrsEntryQueryRepository;
        public UpdateMrsEntryCommandValidator(MaxLengthProvider maxLengthProvider, IMrsEntryQueryRepository iMrsEntryQueryRepository)
        {
            _validationRules = ValidationRuleLoader.LoadValidationRules();
            _iMrsEntryQueryRepository= iMrsEntryQueryRepository;
            if (_validationRules == null || !_validationRules.Any())
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.updateMrsEntry.RequestCategoryId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(UpdateMrsEntryCommand.updateMrsEntry.RequestCategoryId)} {rule.Error}");

                        RuleFor(x => x.updateMrsEntry.DepartmentId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(UpdateMrsEntryCommand.updateMrsEntry.DepartmentId)} {rule.Error}");

                        RuleFor(x => x.updateMrsEntry.SubDepartmentId)
                        .NotEmpty()
                        .GreaterThanOrEqualTo(0)
                         .WithMessage($"{nameof(UpdateMrsEntryCommand.updateMrsEntry.SubDepartmentId)} {rule.Error}");

                        RuleForEach(x => x.updateMrsEntry.UpdateMrsDetails).ChildRules(GateEntry =>
                       {
                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.RequestQuantity)
                               .NotEmpty()
                               .WithMessage($"{nameof(UpdateMrsEntryCommand.updateMrsEntry.UpdateMrsDetails)}.{nameof(UpdateMrsEntryDto.UpdateMrsDetailDto.RequestQuantity)} {rule.Error}");


                           // ✅ Rule 1: DcQuantity is required
                           GateEntry.RuleFor(pt => pt.ItemId)
                               .NotEmpty()
                               .GreaterThan(0)
                               .WithMessage($"{nameof(UpdateMrsEntryCommand.updateMrsEntry.UpdateMrsDetails)}.{nameof(UpdateMrsEntryDto.UpdateMrsDetailDto.ItemId)} {rule.Error}");


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