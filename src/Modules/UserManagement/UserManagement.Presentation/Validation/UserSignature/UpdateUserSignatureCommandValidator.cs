using FluentValidation;
using Microsoft.AspNetCore.Http;
using Shared.Validation.Common;
using UserManagement.Application.Common.Interfaces.IUserSignature;
using UserManagement.Application.UserSignature.Command.UpdateUserSignature;

namespace UserManagement.Presentation.Validation.UserSignature
{
    public class UpdateUserSignatureCommandValidator : AbstractValidator<UpdateUserSignatureCommand>
    {
        private const long MaxFileSizeBytes = 5L * 1024 * 1024; // 5 MB
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png",
            "image/jpg"
        };
        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg",
            ".jpeg",
            ".png"
        };

        private readonly List<ValidationRule> _validationRules;
        private readonly IUserSignatureQueryRepository _userSignatureQueryRepository;

        public UpdateUserSignatureCommandValidator(IUserSignatureQueryRepository userSignatureQueryRepository)
        {
            _userSignatureQueryRepository = userSignatureQueryRepository;

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
                        break;

                    case "FileValidation":
                        // File is optional on Update — only validate when supplied
                        RuleFor(x => x.File!.ContentType)
                            .Must(ct => ct != null && AllowedMimeTypes.Contains(ct))
                            .WithMessage("Only JPEG, JPG and PNG signatures are allowed.")
                            .When(x => x.File != null && x.File.Length > 0);

                        RuleFor(x => x.File!.FileName)
                            .Must(fn => fn != null && AllowedExtensions.Contains(Path.GetExtension(fn)))
                            .WithMessage("Signature file extension must be .jpg, .jpeg or .png.")
                            .When(x => x.File != null && x.File.Length > 0);

                        RuleFor(x => x.File!.Length)
                            .LessThanOrEqualTo(MaxFileSizeBytes)
                            .WithMessage("Signature file size cannot exceed 5 MB.")
                            .When(x => x.File != null && x.File.Length > 0);
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
