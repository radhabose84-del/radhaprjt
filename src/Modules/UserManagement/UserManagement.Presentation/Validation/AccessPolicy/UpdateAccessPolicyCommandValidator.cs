using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.AccessPolicy
{
    public class UpdateAccessPolicyCommandValidator : AbstractValidator<UpdateAccessPolicyCommand>
    {
        private readonly List<ValidationRule>        _validationRules;
        private readonly IAccessPolicyQueryRepository _queryRepository;

        public UpdateAccessPolicyCommandValidator(
            IAccessPolicyQueryRepository queryRepository,
            MaxLengthProvider            maxLengthProvider)
        {
            _queryRepository = queryRepository;

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
                        RuleFor(x => x.PolicyName)
                            .NotNull().WithMessage($"{nameof(UpdateAccessPolicyCommand.PolicyName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateAccessPolicyCommand.PolicyName)} {rule.Error}");
                        RuleFor(x => x.EntityName)
                            .NotNull().WithMessage($"{nameof(UpdateAccessPolicyCommand.EntityName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateAccessPolicyCommand.EntityName)} {rule.Error}");
                        RuleFor(x => x.FieldName)
                            .NotNull().WithMessage($"{nameof(UpdateAccessPolicyCommand.FieldName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateAccessPolicyCommand.FieldName)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.PolicyName)
                            .MaximumLength(maxPolicyName)
                            .WithMessage($"{nameof(UpdateAccessPolicyCommand.PolicyName)} {rule.Error} {maxPolicyName} characters.");
                        RuleFor(x => x.EntityName)
                            .MaximumLength(maxEntityName)
                            .WithMessage($"{nameof(UpdateAccessPolicyCommand.EntityName)} {rule.Error} {maxEntityName} characters.");
                        RuleFor(x => x.FieldName)
                            .MaximumLength(maxFieldName)
                            .WithMessage($"{nameof(UpdateAccessPolicyCommand.FieldName)} {rule.Error} {maxFieldName} characters.");
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .GreaterThan(0).WithMessage("Valid Id is required.")
                            .MustAsync(async (id, ct) => !await _queryRepository.NotFoundAsync(id))
                            .WithMessage($"AccessPolicy {rule.Error}");
                        break;

                    case "ByteValue":
                        RuleFor(x => x.IsActive)
                            .InclusiveBetween(0, 1)
                            .WithMessage($"{nameof(UpdateAccessPolicyCommand.IsActive)} {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
