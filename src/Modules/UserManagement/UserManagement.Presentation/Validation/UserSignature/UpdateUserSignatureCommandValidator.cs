using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.UserSignature
{
    public class UpdateUserSignatureCommandValidator : AbstractValidator<UpdateUserSignatureCommand>
    {
        private const int MaxFileSizeBytes = 500 * 1024; // 500 KB
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

        private readonly List<ValidationRule> _validationRules;
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;

        public UpdateUserSignatureCommandValidator(
            MaxLengthProvider maxLengthProvider,
            IUserSignatureQueryRepository userSignatureQueryRepository)
        {
            _userSignatureQueryRepository = userSignatureQueryRepository;

            var fileNameMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.UserSignature>("FileName") ?? 200;
            var contentTypeMaxLength = maxLengthProvider.GetMaxLength<UserManagement.Domain.Entities.UserSignature>("ContentType") ?? 50;

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
                        RuleFor(x => x.Id)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(UpdateUserSignatureCommand.Id)} {rule.Error}");

                        RuleFor(x => x.FileName)
                            .NotNull().WithMessage($"{nameof(UpdateUserSignatureCommand.FileName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateUserSignatureCommand.FileName)} {rule.Error}");

                        RuleFor(x => x.ContentType)
                            .NotNull().WithMessage($"{nameof(UpdateUserSignatureCommand.ContentType)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(UpdateUserSignatureCommand.ContentType)} {rule.Error}");

                        RuleFor(x => x.SignatureImage)
                            .Must(b => b != null && b.Length > 0)
                            .WithMessage($"{nameof(UpdateUserSignatureCommand.SignatureImage)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.FileName)
                            .MaximumLength(fileNameMaxLength)
                            .WithMessage($"{nameof(UpdateUserSignatureCommand.FileName)} {rule.Error} {fileNameMaxLength} characters.");

                        RuleFor(x => x.ContentType)
                            .MaximumLength(contentTypeMaxLength)
                            .WithMessage($"{nameof(UpdateUserSignatureCommand.ContentType)} {rule.Error} {contentTypeMaxLength} characters.");
                        break;

                    case "FileValidation":
                        RuleFor(x => x.ContentType)
                            .Must(ct => ct != null && AllowedMimeTypes.Contains(ct))
                            .WithMessage("Only JPEG and PNG signatures are allowed.")
                            .When(x => !string.IsNullOrEmpty(x.ContentType));

                        RuleFor(x => x.SignatureImage)
                            .Must(b => b == null || b.Length <= MaxFileSizeBytes)
                            .WithMessage("Signature file size cannot exceed 500 KB.")
                            .When(x => x.SignatureImage != null && x.SignatureImage.Length > 0);
                        break;

                    case "NotFound":
                        RuleFor(x => x.Id)
                            .MustAsync(async (id, ct) => !await _userSignatureQueryRepository.NotFoundAsync(id))
                            .WithMessage($"UserSignature {rule.Error}")
                            .When(x => x.Id > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
