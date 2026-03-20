using FluentValidation;
using ProductionManagement.Application.Common.Interfaces.IPackType;
using ProductionManagement.Application.PackType.Commands.CreatePackType;
using ProductionManagement.Presentation.Validation.Common;
using Shared.Validation.Common;

namespace ProductionManagement.Presentation.Validation.PackType
{
    public class CreatePackTypeCommandValidator : AbstractValidator<CreatePackTypeCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        private readonly IPackTypeQueryRepository _queryRepository;

        public CreatePackTypeCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IPackTypeQueryRepository queryRepository)
        {
            _queryRepository = queryRepository;

            var maxLengthCode = maxLengthProvider.GetMaxLength<ProductionManagement.Domain.Entities.PackType>("PackTypeCode") ?? 20;
            var maxLengthName = maxLengthProvider.GetMaxLength<ProductionManagement.Domain.Entities.PackType>("PackTypeName") ?? 100;

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
                        RuleFor(x => x.PackTypeCode)
                            .NotNull()
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeCode)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeCode)} {rule.Error}");

                        RuleFor(x => x.PackTypeName)
                            .NotNull()
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeName)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PackTypeCode)
                            .MaximumLength(maxLengthCode)
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeCode)} {rule.Error} {maxLengthCode} characters.");

                        RuleFor(x => x.PackTypeName)
                            .MaximumLength(maxLengthName)
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeName)} {rule.Error} {maxLengthName} characters.");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.PackTypeCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PackTypeCode));
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.PackTypeCode)
                            .MustAsync(async (code, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(code!))
                            .WithMessage($"{nameof(CreatePackTypeCommand.PackTypeCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PackTypeCode));
                        break;

                    case "GreaterThan":
                        RuleFor(x => x.NetWeight)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreatePackTypeCommand.NetWeight)} {rule.Error}");
                        break;

                    case "GreaterThanOrEqualToZero":
                        RuleFor(x => x.TareWeight)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreatePackTypeCommand.TareWeight)} {rule.Error}");

                        RuleFor(x => x.ConesPerBag)
                            .GreaterThanOrEqualTo(0)
                            .WithMessage($"{nameof(CreatePackTypeCommand.ConesPerBag)} {rule.Error}")
                            .When(x => x.ConesPerBag.HasValue);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
