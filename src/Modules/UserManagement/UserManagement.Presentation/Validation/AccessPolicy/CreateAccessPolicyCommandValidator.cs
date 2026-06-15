using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.AccessPolicy
{
    public class CreateAccessPolicyCommandValidator : AbstractValidator<CreateAccessPolicyCommand>
    {
        private readonly List<ValidationRule>        _validationRules;
        private readonly IAccessPolicyQueryRepository _queryRepository;

        public CreateAccessPolicyCommandValidator(
            IAccessPolicyQueryRepository queryRepository,
            MaxLengthProvider            maxLengthProvider)
        {
            _queryRepository = queryRepository;

            var maxPolicyCode = maxLengthProvider.GetMaxLength<Domain.Entities.AccessPolicy>("PolicyCode") ?? 50;
            var maxPolicyName = maxLengthProvider.GetMaxLength<Domain.Entities.AccessPolicy>("PolicyName") ?? 200;
            var maxEntityName = maxLengthProvider.GetMaxLength<Domain.Entities.AccessPolicy>("EntityName") ?? 200;
            var maxFieldName  = maxLengthProvider.GetMaxLength<Domain.Entities.AccessPolicy>("FieldName")  ?? 200;

            _validationRules = ValidationRuleLoader.LoadValidationRules();
            if (_validationRules == null || _validationRules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _validationRules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.PolicyCode)
                            .NotNull().WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyCode)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyCode)} {rule.Error}");
                        RuleFor(x => x.PolicyName)
                            .NotNull().WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyName)} {rule.Error}");
                        RuleFor(x => x.EntityName)
                            .NotNull().WithMessage($"{nameof(CreateAccessPolicyCommand.EntityName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccessPolicyCommand.EntityName)} {rule.Error}");
                        RuleFor(x => x.FieldName)
                            .NotNull().WithMessage($"{nameof(CreateAccessPolicyCommand.FieldName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateAccessPolicyCommand.FieldName)} {rule.Error}");
                        break;

                    case "Alphanumeric":
                        RuleFor(x => x.PolicyCode)
                            .Matches(rule.Pattern)
                            .WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PolicyCode));
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PolicyCode)
                            .MaximumLength(maxPolicyCode)
                            .WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyCode)} {rule.Error} {maxPolicyCode} characters.");
                        RuleFor(x => x.PolicyName)
                            .MaximumLength(maxPolicyName)
                            .WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyName)} {rule.Error} {maxPolicyName} characters.");
                        RuleFor(x => x.EntityName)
                            .MaximumLength(maxEntityName)
                            .WithMessage($"{nameof(CreateAccessPolicyCommand.EntityName)} {rule.Error} {maxEntityName} characters.");
                        RuleFor(x => x.FieldName)
                            .MaximumLength(maxFieldName)
                            .WithMessage($"{nameof(CreateAccessPolicyCommand.FieldName)} {rule.Error} {maxFieldName} characters.");
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.PolicyCode)
                            .MustAsync(async (code, ct) =>
                                !await _queryRepository.AlreadyExistsAsync(code))
                            .WithMessage($"{nameof(CreateAccessPolicyCommand.PolicyCode)} {rule.Error}")
                            .When(x => !string.IsNullOrWhiteSpace(x.PolicyCode));
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
