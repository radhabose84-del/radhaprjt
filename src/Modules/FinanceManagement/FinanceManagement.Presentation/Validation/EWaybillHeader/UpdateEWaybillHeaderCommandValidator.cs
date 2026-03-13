using FluentValidation;
using FinanceManagement.Application.Common.Interfaces.IEWaybillHeader;
using FinanceManagement.Application.EWaybillHeader.Commands.UpdateEWaybillHeader;
using FinanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace FinanceManagement.Presentation.Validation.EWaybillHeader
{
    public class UpdateEWaybillHeaderCommandValidator : AbstractValidator<UpdateEWaybillHeaderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IEWaybillHeaderQueryRepository _queryRepository;

        public UpdateEWaybillHeaderCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IEWaybillHeaderQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthVehicleNo  = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EWaybillHeader>("VehicleNo")  ?? 20;
            var maxLengthTransDocNo = maxLengthProvider.GetMaxLength<FinanceManagement.Domain.Entities.EWaybillHeader>("TransDocNo") ?? 30;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
            {
                throw new InvalidOperationException("Validation rules could not be loaded.");
            }

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "MaxLength":
                        RuleFor(x => x.VehicleNo)
                            .MaximumLength(maxLengthVehicleNo)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.VehicleNo)} {rule.Error} {maxLengthVehicleNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.VehicleNo));

                        RuleFor(x => x.TransDocNo)
                            .MaximumLength(maxLengthTransDocNo)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.TransDocNo)} {rule.Error} {maxLengthTransDocNo} characters.")
                            .When(x => !string.IsNullOrWhiteSpace(x.TransDocNo));
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"EWaybill Header {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TotalValue)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.TotalValue)} {rule.Error}");

                        RuleFor(x => x.CGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.CGST)} {rule.Error}");

                        RuleFor(x => x.SGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.SGST)} {rule.Error}");

                        RuleFor(x => x.IGST)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.IGST)} {rule.Error}");

                        RuleFor(x => x.Cess)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.Cess)} {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateEWaybillHeaderCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
