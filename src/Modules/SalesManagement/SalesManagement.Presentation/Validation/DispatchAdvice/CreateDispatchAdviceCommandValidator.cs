using FluentValidation;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Commands.CreateDispatchAdvice;
using SalesManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace SalesManagement.Presentation.Validation.DispatchAdvice
{
    public class CreateDispatchAdviceCommandValidator : AbstractValidator<CreateDispatchAdviceCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IDispatchAdviceQueryRepository _queryRepository;

        public CreateDispatchAdviceCommandValidator(
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
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.SalesOrderId)} {rule.Error}");

                        RuleFor(x => x.PartyId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.PartyId)} {rule.Error}");

                        RuleFor(x => x.DispatchAddressId)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.DispatchAddressId)} {rule.Error}");

                        RuleFor(x => x.Details)
                            .NotNull()
                            .WithMessage($"Details {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"Details {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.VehicleNo)
                            .MaximumLength(maxLengthVehicleNo)
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.VehicleNo)} {rule.Error} {maxLengthVehicleNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNo));

                        RuleFor(x => x.DriverName)
                            .MaximumLength(maxLengthDriverName)
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.DriverName)} {rule.Error} {maxLengthDriverName} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.DriverName));

                        RuleFor(x => x.LRNo)
                            .MaximumLength(maxLengthLRNo)
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.LRNo)} {rule.Error} {maxLengthLRNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.LRNo));
                        break;

                    case "AmendmentPending":
                        RuleFor(x => x.SalesOrderId)
                            .MustAsync(async (id, ct) => !await _queryRepository.HasPendingAmendmentAsync(id))
                            .WithMessage(rule.Error)
                            .When(x => x.SalesOrderId > 0);
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.SalesOrderId)
                            .MustAsync(async (id, ct) => await _queryRepository.SalesOrderExistsAsync(id))
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.SalesOrderId)} {rule.Error}")
                            .When(x => x.SalesOrderId > 0);

                        RuleFor(x => x.DispatchAddressId)
                            .MustAsync(async (id, ct) => await _queryRepository.DispatchAddressExistsAsync(id))
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.DispatchAddressId)} {rule.Error}")
                            .When(x => x.DispatchAddressId > 0);
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
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.TotOrderQty)} {rule.Error}");

                        RuleFor(x => x.TotDispatchedQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.TotDispatchedQty)} {rule.Error}");

                        RuleFor(x => x.TotPendingQty)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreateDispatchAdviceCommand.TotPendingQty)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
