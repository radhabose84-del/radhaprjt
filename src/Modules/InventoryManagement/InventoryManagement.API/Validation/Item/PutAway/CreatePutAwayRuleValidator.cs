using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using FluentValidation;
using InventoryManagement.API.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.API.Validation.Item.PutAway
{
    public class CreatePutAwayRuleCommandValidator : AbstractValidator<CreatePutAwayRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPutAwayRuleCommandRepository _ruleRepo;
        private readonly IMiscMasterQueryRepository _miscRepo;
      
        public CreatePutAwayRuleCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IPutAwayRuleCommandRepository ruleRepo,
            IMiscMasterQueryRepository miscRepo
            )
        {
            _ruleRepo = ruleRepo;
            _miscRepo = miscRepo;        

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new ArgumentException("Validation rules could not be loaded.");

            // Drive generic rules from your rule set (like your sample)
            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.Body.UnitId)
                            .NotEmpty().WithMessage($"{nameof(CreatePutAwayRuleRequest.UnitId)} {rule.Error}");
                        RuleFor(x => x.Body.WarehouseId)
                            .NotEmpty().WithMessage($"{nameof(CreatePutAwayRuleRequest.WarehouseId)} {rule.Error}");
                        RuleFor(x => x.Body.ItemGroupId)
                            .NotEmpty().WithMessage($"{nameof(CreatePutAwayRuleRequest.ItemGroupId)} {rule.Error}");
                        RuleFor(x => x.Body.ItemCategoryId)
                            .NotEmpty().WithMessage($"{nameof(CreatePutAwayRuleRequest.ItemCategoryId)} {rule.Error}");
                        RuleFor(x => x.Body.Strategies)
                            .NotEmpty().WithMessage($"{nameof(CreatePutAwayRuleRequest.Strategies)} {rule.Error}");
                        break;
               /*    
                    case "AlreadyExists":
                        // Scope uniqueness: Unit + Warehouse + ItemGroup + ItemCategory + (optional) Item
                        RuleFor(x => x.Body)
                           .MustAsync(async (body, ct) =>
                               !await _ruleRepo.ExistsScopeAsync(
                                   body.UnitId, body.WarehouseId, body.ItemGroupId, body.ItemCategoryId, body.ItemId, null, ct))
                           .WithMessage("A PutAway rule already exists for the given scope.");
                        break;*/
                } 
            }            

            // Strategy-level basic numeric checks
            RuleForEach(x => x.Body.Strategies).ChildRules(s =>
            {
                s.RuleFor(y => y.StorageTypeId).GreaterThan(0)
                    .WithMessage($"{nameof(CreatePutAwayStrategyRequest.StorageTypeId)} is required.");
                s.RuleFor(y => y.PriorityId).GreaterThan(0)
                    .WithMessage($"{nameof(CreatePutAwayStrategyRequest.PriorityId)} is required.");
                s.RuleFor(y => y.TargetId).GreaterThan(0)
                    .WithMessage($"{nameof(CreatePutAwayStrategyRequest.TargetId)} is required.");
            });

            // Unique Priority inside the same rule
            RuleFor(x => x.Body.Strategies.Select(s => s.PriorityId))
                .Must(p => p.Count() == p.Distinct().Count())
                .WithMessage("Priority must be unique within the rule.");       
        }
    }
}
