using System.Linq;
using FAM.Application.AssetMaster.AssetWarranty.Commands.UploadAssetWarranty;
using FAM.API.Validation.Common;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Shared.Validation.Common;

namespace FAM.API.Validation.AssetMaster.AssetWarranty
{
    public class UploadAssetWarrantyCommandValidator : AbstractValidator<UploadFileAssetWarrantyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UploadAssetWarrantyCommandValidator()
        {
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
                        RuleFor(x => x.File)
                            .NotNull()
                            .WithMessage($"{nameof(UploadFileAssetWarrantyCommand.File)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UploadFileAssetWarrantyCommand.File)} {rule.Error}");
                        break;
                    case "FileValidation":
                        RuleFor(x => x.File)
                            .Must(file => file != null && IsValidFileType(file, rule.allowedExtensions))
                            .WithMessage($"{nameof(UploadFileAssetWarrantyCommand.File)} {rule.Error}")
                            .Must(file => file != null && file.Length <= 2 * 1024 * 1024) // 2MB size limit
                            .WithMessage($"{nameof(UploadFileAssetWarrantyCommand.File)} {rule.Error}");
                        break;                   
                }
            }        
        }
       private bool IsValidFileType(IFormFile file, List<string> allowedExtensions)
        {
            if (file == null || allowedExtensions == null || !allowedExtensions.Any())
            {
                return false;
            }

            var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            return allowedExtensions.Contains(fileExtension);
        }
    }
}