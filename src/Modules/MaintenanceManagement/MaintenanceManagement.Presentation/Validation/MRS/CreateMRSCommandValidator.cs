using MaintenanceManagement.Application.MRS.Command.CreateMRS;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.MRS
{
    public class CreateMRSCommandValidator : AbstractValidator<CreateMRSCommand>
    {
       private readonly List<ValidationRule> _validationRules;
        public CreateMRSCommandValidator(MaxLengthProvider maxLengthProvider)
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
                        RuleFor(x => x.Header!.Divcode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMRSCommand.Header.Divcode)} {rule.Error}");

                             RuleFor(x => x.Header!.Depcode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMRSCommand.Header.Depcode)} {rule.Error}");

                             RuleFor(x => x.Header!.SubDepcode)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMRSCommand.Header.SubDepcode)} {rule.Error}");

                            //  RuleFor(x => x.Header!.Refno)
                            // .NotEmpty()
                            // .WithMessage($"{nameof(CreateMRSCommand.Header.Refno)} {rule.Error}");

                            RuleFor(x => x.Header!.MaintenanceType)
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMRSCommand.Header.MaintenanceType)} {rule.Error}");
                            
                             RuleFor(x => x.Header!.IrDate)
                             .NotEmpty()
                            .Must(date => date.Date == DateTime.Now.Date)
                            .WithMessage("Date must be today's date.");

                            
                             // Inline validation for each item in the Details list
                            RuleForEach(x => x.Header!.Details!).ChildRules(detail =>
                            {
                                detail.RuleFor(d => d.ItemCode)
                                    .NotEmpty()
                                    .WithMessage("ItemCode is required.");

                                // detail.RuleFor(d => d.Macno)
                                //     .NotEmpty()
                                //     .WithMessage("Macno is required.");

                                detail.RuleFor(d => d.CatCode)
                                    .NotEmpty()
                                    .WithMessage("SubCost Code is required.");

                                detail.RuleFor(d => d.CcCode)
                                    .NotEmpty()
                                    .WithMessage("Finance Consumption Code is required.");

                                detail.RuleFor(d => d.QtyReqd)
                                .NotNull()
                                .WithMessage("QtyReqd is required.")
                                .GreaterThan(0)
                                .WithMessage("QtyReqd must be greater than 0.");
                                // .LessThanOrEqualTo(d => d.CurrStk)
                                // .WithMessage("QtyReqd cannot exceed current stock (CurrStk).");
                            });

                        break;
                    default:
                        break;
                }
            }
        }

       

    }
}