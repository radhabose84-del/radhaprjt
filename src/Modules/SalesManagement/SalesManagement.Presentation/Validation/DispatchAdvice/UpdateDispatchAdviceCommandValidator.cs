using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Commands.UpdateDispatchAdvice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAdvice
{
    public class UpdateDispatchAdviceCommandValidator : AbstractValidator<UpdateDispatchAdviceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAdviceQueryRepository _queryRepository;

        public UpdateDispatchAdviceCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IDispatchAdviceQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNo = maxLengthProvider.GetMaxLength<Domain.Entities.DispatchAdviceHeader>("VehicleNo") ?? 50;
            var maxLengthDriverName = maxLengthProvider.GetMaxLength<Domain.Entities.DispatchAdviceHeader>("DriverName") ?? 100;
            var maxLengthLRNo = maxLengthProvider.GetMaxLength<Domain.Entities.DispatchAdviceHeader>("LRNo") ?? 50;

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
                        RuleFor(x => x.SalesOrderId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.SalesOrderId)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.DispatchAddressId)
                            .NotEmpty()
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.DispatchAddressId)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNo)
                            .MaximumLength(maxLengthVehicleNo)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.VehicleNo)} {rule.Error} {maxLengthVehicleNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNo));

                        RuleFor(x => x.DriverName)
                            .MaximumLength(maxLengthDriverName)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.DriverName)} {rule.Error} {maxLengthDriverName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DriverName));

                        RuleFor(x => x.LRNo)
                            .MaximumLength(maxLengthLRNo)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.LRNo)} {rule.Error} {maxLengthLRNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.LRNo));
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOrderId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOrderExistsAsync(id))
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.SalesOrderId)} {rule.Error}")
                            .When(x => x.SalesOrderId > 0);

                        RuleFor(x => x.DispatchAddressId)
                            .MustAsync(async (id, ct) => await _queryRepository.DispatchAddressExistsAsync(id))
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.DispatchAddressId)} {rule.Error}")
                            .When(x => x.DispatchAddressId > 0);
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.IsActive)} {rule.Error}");
                        break;

                    case "GreaterThan":
                        RuleForEach(x => x.Details)
                            .ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemId)
                                    .GreaterThan(0)
                                    .WithMessage($"ItemId {rule.Error}");

                                detail.RuleFor(d => d.LotId)
                                    .GreaterThan(0)
                                    .WithMessage($"LotId {rule.Error}");

                                detail.RuleFor(d => d.SalesOrderDetailId)
                                    .GreaterThan(0)
                                    .WithMessage($"SalesOrderDetailId {rule.Error}");

                                detail.RuleFor(d => d.StartPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"StartPackNo {rule.Error}");

                                detail.RuleFor(d => d.EndPackNo)
                                    .GreaterThan(0)
                                    .WithMessage($"EndPackNo {rule.Error}");

                                detail.RuleFor(d => d.DispatchQty)
                                    .GreaterThan(0)
                                    .WithMessage($"DispatchQty {rule.Error}");
                            })
                            .When(x => x.Details != null && x.Details.Any());
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TotOrderQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.TotOrderQty)} {rule.Error}");

                        RuleFor(x => x.TotDispatchedQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.TotDispatchedQty)} {rule.Error}");

                        RuleFor(x => x.TotPendingQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateDispatchAdviceCommand.TotPendingQty)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
