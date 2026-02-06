using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using UserManagement.Application.Country.Commands.CreateCountry;
using UserManagement.Domain.Entities;
using UserManagement.API.Validation.Common;   // MaxLengthProvider, ValidationRuleLoader
using UserManagement.Application.Common.Interfaces.ICountry;
using Shared.Validation.Common; // repo with ExistsBy* methods

namespace UserManagement.API.Validation.Country
{
    public class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
    {
        private static readonly Regex CodePattern = new(@"^[A-Za-z0-9-]+$", RegexOptions.Compiled);
        private readonly List<ValidationRule> _rules;

        public CreateCountryCommandValidator(
            MaxLengthProvider maxLengthProvider,
            ICountryCommandRepository repo) // or ICountryCommandRepository if that’s where ExistsBy* lives
        {
            var codeMax = maxLengthProvider.GetMaxLength<Countries>("CountryCode") ?? 5;
            var nameMax = maxLengthProvider.GetMaxLength<Countries>("CountryName") ?? 50;

            _rules = ValidationRuleLoader.LoadValidationRules()
                     ?? throw new InvalidOperationException("Validation rules could not be loaded.");
            if (_rules.Count == 0)
                throw new InvalidOperationException("Validation rules could not be loaded.");

            foreach (var rule in _rules)
            {
                switch (rule.Rule)
                {
                    case "NotEmpty":
                        RuleFor(x => x.CountryName)
                            .NotEmpty().WithMessage($"{nameof(CreateCountryCommand.CountryName)} {rule.Error}");
                        RuleFor(x => x.CountryCode)
                            .NotEmpty().WithMessage($"{nameof(CreateCountryCommand.CountryCode)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.CountryName)
                            .MaximumLength(nameMax).WithMessage($"{nameof(CreateCountryCommand.CountryName)} {rule.Error} {nameMax}");
                        RuleFor(x => x.CountryCode)
                            .MaximumLength(codeMax).WithMessage($"{nameof(CreateCountryCommand.CountryCode)} {rule.Error} {codeMax}");
                        break;
                }
            }

            // Extra safety & shape
            RuleFor(x => x.CountryName)
                .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage($"{nameof(CreateCountryCommand.CountryName)} cannot be whitespace.");

            RuleFor(x => x.CountryCode)
                .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage($"{nameof(CreateCountryCommand.CountryCode)} cannot be whitespace.")
                .Must(v => v == null || CodePattern.IsMatch(v)).WithMessage($"{nameof(CreateCountryCommand.CountryCode)} must be alphanumeric or hyphen only.");

            // Uniqueness (create => excludeId = 0)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    if (string.IsNullOrEmpty(cmd.CountryCode) || string.IsNullOrEmpty(cmd.CountryName))
                        return true; // other rules will fail

                    if (await repo.ExistsByCodeAsync(cmd.CountryCode, 0, ct)) return false;
                    if (await repo.ExistsByNameAsync(cmd.CountryName, 0, ct)) return false;
                    return true;
                })
                .WithMessage("CountryCode or CountryName already exists.");
        }

        // Trim inputs (no Transform available)
        protected override bool PreValidate(ValidationContext<CreateCountryCommand> context, ValidationResult result)
        {
            var m = context.InstanceToValidate;
            if (m is not null)
            {
                m.CountryCode = m.CountryCode?.Trim();
                m.CountryName = m.CountryName?.Trim();
            }
            return base.PreValidate(context, result);
        }
    }
}
