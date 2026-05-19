using FluentValidation;
using GateEntryManagement.Application.GateInward.Commands.UploadGateInwardAttachment;
using Shared.Validation.Common;

namespace GateEntryManagement.Presentation.Validation.GateInward
{
    public class UploadGateInwardAttachmentCommandValidator : AbstractValidator<UploadGateInwardAttachmentCommand>
    {
        private const long MaxFileSize = 5L * 1024 * 1024;

        private readonly List<ValidationRule> _validationRules;

        public UploadGateInwardAttachmentCommandValidator()
        {
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
                        RuleFor(x => x.File)
                            .NotNull()
                            .WithMessage($"File {rule.Error}")
                            .Must(f => f != null && f.Length > 0)
                            .WithMessage($"File {rule.Error}");
                        break;

                    case "FileSize":
                        RuleFor(x => x.File!)
                            .Must(f => f.Length <= MaxFileSize)
                            .When(x => x.File != null)
                            .WithMessage($"File {rule.Error}");
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
