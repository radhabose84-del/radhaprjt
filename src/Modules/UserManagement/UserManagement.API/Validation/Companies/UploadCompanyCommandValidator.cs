using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UserManagement.API.Validation.Common;
using UserManagement.Application.Companies.Commands.UploadFileCompany;
using FluentValidation;
using Serilog;
using Shared.Validation.Common;

namespace UserManagement.API.Validation.Companies
{
    public class UploadCompanyCommandValidator : AbstractValidator<UploadFileCompanyCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UploadCompanyCommandValidator()
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
                            .WithMessage($"{nameof(UploadFileCompanyCommand.File)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UploadFileCompanyCommand.File)} {rule.Error}");

                    break;

                      case "FileValidation":
                    RuleFor(x => x.File)
                    .Must(file => IsValidFileType(file, rule.allowedExtensions))
                    .WithMessage($"{nameof(UploadFileCompanyCommand.File)} {rule.Error}")
                    .Must(file => file.Length <= 2 * 1024 * 1024)
                    .WithMessage($"{nameof(UploadFileCompanyCommand.File)} {rule.Error}");
                    break;
                }
            }

        }
        private bool IsValidFileType(IFormFile file, List<string> allowedExtensions)
       {
           Log.Information(file.FileName);
           foreach (var extension in allowedExtensions)
           {
               Log.Information(extension);
               
           }
           
           if (file == null) return false;
       
           var fileExtension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
           return allowedExtensions.Contains(fileExtension);
       }
    }
}