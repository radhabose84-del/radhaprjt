using FluentValidation;
using QCManagement.Application.Common.Interfaces.IMiscMaster;
using QCManagement.Application.MiscMaster.Commands.CreateMiscMaster;
using QCManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace QCManagement.Presentation.Validation.MiscMaster
{
    public class CreateMiscMasterCommandValidator : AbstractValidator<CreateMiscMasterCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IMiscMasterQueryRepository _queryRepository;

        public CreateMiscMasterCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IMiscMasterQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<QCManagement.Domain.Entities.MiscMaster>("Code") ?? 20;
            var maxLengthDescription = maxLengthProvider.GetMaxLength<QCManagement.Domain.Entities.MiscMaster>("Description") ?? 250;

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
                        RuleFor(x => x.MiscTypeId)
                            .NotNull()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.MiscTypeId)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.MiscTypeId)} {rule.Error}");

                        RuleFor(x => x.Code)
                            .NotNull()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}");

                        RuleFor(x => x.Description)
                            .NotNull()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.Code)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Code));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.Code)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.Description)
                            .MaximumLength(maxLengthDescription)
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Description)} {rule.Error} {maxLengthDescription} characters.");
                        break;

                    case "FKColumnDelete":
                        RuleFor(x => x.MiscTypeId)
                            .MustAsync(async (miscTypeId, ct) =>
                                await _queryRepository.MiscTypeExistsAsync(miscTypeId))
                            .WithMessage($"{nameof(CreateMiscMasterCommand.MiscTypeId)} {rule.Error}")
                            .When(x => x.MiscTypeId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.Code)
                            .MustAsync(async (command, code, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(code!, command.MiscTypeId))
                            .WithMessage($"{nameof(CreateMiscMasterCommand.Code)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.Code) && x.MiscTypeId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
