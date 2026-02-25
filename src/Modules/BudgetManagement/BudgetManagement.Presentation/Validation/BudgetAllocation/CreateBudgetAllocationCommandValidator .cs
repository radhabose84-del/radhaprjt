using BudgetManagement.Presentation.Validation.Common;
using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Application.Common.Interfaces.IBudgetAllocation;
using FluentValidation;
using Shared.Validation.Common;


namespace BudgetManagement.Presentation.Validation.BudgetAllocation
{
    public class CreateBudgetAllocationCommandValidator 
        : AbstractValidator<CreateBudgetAllocationCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IBudgetAllocationQueryRepository _ibudgetAllocationQueryRepository;
        
        private readonly IIPAddressService _ipAddressService;

        public CreateBudgetAllocationCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IBudgetAllocationQueryRepository ibudgetAllocationQueryRepository,
            IIPAddressService ipAddressService)
        {
            _ibudgetAllocationQueryRepository = ibudgetAllocationQueryRepository;
            _ipAddressService = ipAddressService;
            _validationRules = ValidationRuleLoader.LoadValidationRules();

        
            if (_validationRules == null || !_validationRules.Any())
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {

                   case "AlreadyExists":
                            RuleForEach(x => x.createBudgetAllocations)
                            .MustAsync(async (dto, ct) =>
                            {
                                var userUnitId = _ipAddressService.GetUnitId(); // moved here

                                return !await _ibudgetAllocationQueryRepository.ExistsAsync(
                                    userUnitId,
                                    dto.FinancialYearId,
                                    dto.RequestById,
                                    dto.RequestMonthId,
                                    dto.BudgetGroupId,
                                    dto.AllocationTypeId,
                                    ct);
                            })
                            .WithMessage("Duplicate Budget Allocation found for this FinancialYear, RequestBy, RequestMonth, BudgetGroup, Unit, AllocationType.");
                        break;
                    case "NotEmpty":

                        // Validate List
                        RuleFor(x => x.createBudgetAllocations)
                            .NotEmpty()
                            .WithMessage($"Budget Allocation list {rule.Error}");

                        // Validate each item
                        RuleForEach(x => x.createBudgetAllocations)
                            .ChildRules(item =>
                            {
                                item.RuleFor(pt => pt.FinancialYearId)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.FinancialYearId)} {rule.Error}");

                                item.RuleFor(pt => pt.RequestById)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.RequestById)} {rule.Error}");

                                item.RuleFor(pt => pt.RequestMonthId)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.RequestMonthId)} {rule.Error}");

                                item.RuleFor(pt => pt.UnitId)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.UnitId)} {rule.Error}");

                                item.RuleFor(pt => pt.BudgetGroupId)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.BudgetGroupId)} {rule.Error}");

                                item.RuleFor(pt => pt.AllocationTypeId)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.AllocationTypeId)} {rule.Error}");

                                item.RuleFor(pt => pt.ApprovedAmount)
                                    .NotEmpty()
                                    .GreaterThan(0)
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.ApprovedAmount)} {rule.Error}");


                               /*  item.RuleFor(pt => pt.SpindleCount)
                                    .GreaterThan(0)
                                    .When(pt => pt.SpindleCount.HasValue);

                                item.RuleFor(pt => pt.RatePerSpindle)
                                    .GreaterThan(0)
                                    .When(pt => pt.RatePerSpindle.HasValue); */

                                item.RuleFor(pt => pt.FromDate)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.FromDate)} {rule.Error}");

                                item.RuleFor(pt => pt.ToDate)
                                    .NotEmpty()
                                    .WithMessage($"{nameof(CreateBudgetAllocationDto.ToDate)} {rule.Error}");

                                item.RuleFor(pt => pt)
                                    .Must(pt => pt.FromDate <= pt.ToDate)
                                    .WithMessage("FromDate must be earlier than ToDate.");

                            });

                        break;
                }
            }
        }
    }
}
