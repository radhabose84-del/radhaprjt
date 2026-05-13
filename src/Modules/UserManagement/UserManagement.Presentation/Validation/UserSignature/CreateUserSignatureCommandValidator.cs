using FluentValidation;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.CreateUserSignature;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.Presentation.Validation.UserSignature
{
    public class CreateUserSignatureCommandValidator : AbstractValidator<CreateUserSignatureCommand>
    {
        private const int MaxFileSizeBytes = 500 * 1024; // 500 KB
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

        private readonly List<ValidationRule> _validationRules;
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;

        public CreateUserSignatureCommandValidator(
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
                        RuleFor(x => x.UserId)
                            .GreaterThan(0)
                            .WithMessage($"{nameof(CreateUserSignatureCommand.UserId)} {rule.Error}");

                        RuleFor(x => x.FileName)
                            .NotNull().WithMessage($"{nameof(CreateUserSignatureCommand.FileName)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateUserSignatureCommand.FileName)} {rule.Error}");

                        RuleFor(x => x.ContentType)
                            .NotNull().WithMessage($"{nameof(CreateUserSignatureCommand.ContentType)} {rule.Error}")
                            .NotEmpty().WithMessage($"{nameof(CreateUserSignatureCommand.ContentType)} {rule.Error}");

                        RuleFor(x => x.SignatureImage)
                            .Must(b => b != null && b.Length > 0)
                            .WithMessage($"{nameof(CreateUserSignatureCommand.SignatureImage)} {rule.Error}");
                        break;

                    case "MaxLength":
                        RuleFor(x => x.FileName)
                            .MaximumLength(fileNameMaxLength)
                            .WithMessage($"{nameof(CreateUserSignatureCommand.FileName)} {rule.Error} {fileNameMaxLength} characters.");

                        RuleFor(x => x.ContentType)
                            .MaximumLength(contentTypeMaxLength)
                            .WithMessage($"{nameof(CreateUserSignatureCommand.ContentType)} {rule.Error} {contentTypeMaxLength} characters.");
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

                    case "FKColumnDelete":
                        RuleFor(x => x.UserId)
                            .MustAsync(async (userId, ct) => await _userSignatureQueryRepository.UserExistsAsync(userId))
                            .WithMessage($"{nameof(CreateUserSignatureCommand.UserId)} {rule.Error}")
                            .When(x => x.UserId > 0);
                        break;

                    case "AlreadyExists":
                        RuleFor(x => x.UserId)
                            .MustAsync(async (userId, ct) => !await _userSignatureQueryRepository.UserHasSignatureAsync(userId))
                            .WithMessage($"Signature for this UserId {rule.Error}")
                            .When(x => x.UserId > 0);
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
