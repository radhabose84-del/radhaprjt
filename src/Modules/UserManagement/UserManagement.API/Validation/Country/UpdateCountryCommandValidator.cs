using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;            // PreValidate
using Core.Domain.Entities;
using Core.Application.Country.Commands.UpdateCountry;
using Core.Application.Common.Interfaces.ICountry;
using UserManagement.API.Validation.Common; // MaxLengthProvider

namespace UserManagement.API.Validation.Country
{
    public class UpdateCountryCommandValidator : AbstractValidator<UpdateCountryCommand>
    {
        private static readonly Regex CodePattern = new(@"^[A-Za-z0-9-]+$", RegexOptions.Compiled);

        public UpdateCountryCommandValidator(
            ICountryQueryRepository countryQueryRepo,ICountryCommandRepository countryCommandRepo,
            MaxLengthProvider maxLengthProvider)
        {
            // Dynamic max lengths with safe fallbacks
            var codeMax = maxLengthProvider.GetMaxLength<Countries>("CountryCode") ?? 5;
            var nameMax = maxLengthProvider.GetMaxLength<Countries>("CountryName") ?? 50;

            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage($"{nameof(UpdateCountryCommand.Id)} must be greater than 0.");

            RuleFor(x => x.CountryCode)
                .NotEmpty().WithMessage($"{nameof(UpdateCountryCommand.CountryCode)} is required.")
                .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage($"{nameof(UpdateCountryCommand.CountryCode)} cannot be whitespace.")
                .MaximumLength(codeMax).WithMessage($"{nameof(UpdateCountryCommand.CountryCode)} must be at most {codeMax} characters.")
                .Must(v => v == null || CodePattern.IsMatch(v))
                    .WithMessage($"{nameof(UpdateCountryCommand.CountryCode)} must be alphanumeric or hyphen only.");

            RuleFor(x => x.CountryName)
                .NotEmpty().WithMessage($"{nameof(UpdateCountryCommand.CountryName)} is required.")
                .Must(v => !string.IsNullOrWhiteSpace(v)).WithMessage($"{nameof(UpdateCountryCommand.CountryName)} cannot be whitespace.")
                .MaximumLength(nameMax).WithMessage($"{nameof(UpdateCountryCommand.CountryName)} must be at most {nameMax} characters.");

            // 0/1 only; your handler maps to enum
            RuleFor(x => x.IsActive)
                .InclusiveBetween((byte)0, (byte)1)
                .WithMessage($"{nameof(UpdateCountryCommand.IsActive)} must be 0 (Inactive) or 1 (Active).");

            // Async uniqueness (exclude current Id)
            RuleFor(x => x)
                .MustAsync(async (cmd, ct) =>
                {
                    // If either empty, other rules will catch it.
                    if (string.IsNullOrEmpty(cmd.CountryCode) || string.IsNullOrEmpty(cmd.CountryName))
                        return true;

                    var codeExists = await countryCommandRepo.ExistsByCodeAsync(cmd.CountryCode, cmd.Id, ct);
                    if (codeExists) return false;

                    var nameExists = await countryCommandRepo.ExistsByNameAsync(cmd.CountryName, cmd.Id, ct);
                    if (nameExists) return false;

                    return true;
                })
                .WithMessage("CountryCode or CountryName already exists for a different country.");
        }

        // Normalize inputs since Transform() isn’t available in your FV version
        protected override bool PreValidate(ValidationContext<UpdateCountryCommand> context, ValidationResult result)
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
