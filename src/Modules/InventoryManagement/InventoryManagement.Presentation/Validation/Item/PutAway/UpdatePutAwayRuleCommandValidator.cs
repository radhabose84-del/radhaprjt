using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Application.Common.Interfaces.Item.PutAway;
using InventoryManagement.Application.Item.PutAway.Commands.CreatePutAwayRule;
using InventoryManagement.Application.Item.PutAway.Commands.UpdatePutAwayRule;
using FluentValidation;
using InventoryManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace InventoryManagement.Presentation.Validation.Item.PutAway
{
    public class UpdatePutAwayRuleCommandValidator : AbstractValidator<UpdatePutAwayRuleCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPutAwayRuleCommandRepository _ruleRepo;
        private readonly IMiscMasterQueryRepository _miscRepo;

        public UpdatePutAwayRuleCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IPutAwayRuleCommandRepository ruleRepo,
            IMiscMasterQueryRepository miscRepo)
        {
            _ruleRepo = ruleRepo;
            _miscRepo = miscRepo;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || !_validationRules.Any())
                throw new ArgumentException("Validation rules could not be loaded.");

            // ✅ 1) Id validation first
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage($"{nameof(UpdatePutAwayRuleCommand.Id)} is required.")
                .DependentRules(() =>
                {
                    // ✅ 2) Only if Id > 0 → hit DB
                    RuleFor(x => x.Id)
                        .MustAsync(async (id, ct) => await _ruleRepo.ExistsAsync(id, ct))
                        .WithMessage("PutAway Rule not found.");
                });

            // ✅ Only validate Body rules when request Body exists AND Id is valid
            When(x => x.Id > 0 && x.Body != null, () =>
            {
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
                    }
                }

                RuleForEach(x => x.Body.Strategies).ChildRules(s =>
                {
                    s.RuleFor(y => y.StorageTypeId).GreaterThan(0)
                        .WithMessage($"{nameof(CreatePutAwayStrategyRequest.StorageTypeId)} is required.");
                    s.RuleFor(y => y.PriorityId).GreaterThan(0)
                        .WithMessage($"{nameof(CreatePutAwayStrategyRequest.PriorityId)} is required.");
                    s.RuleFor(y => y.TargetId).GreaterThan(0)
                        .WithMessage($"{nameof(CreatePutAwayStrategyRequest.TargetId)} is required.");
                });

                RuleFor(x => x.Body.Strategies.Select(s => s.PriorityId))
                    .Must(p => p.Count() == p.Distinct().Count())
                    .WithMessage("Priority must be unique within the rule.");
            });
        }
    }
}
