
using MaintenanceManagement.Application.WorkOrder.Command.UploadFileWorOrder;
using FluentValidation;
using MaintenanceManagement.Presentation.Validation.Common;
using Microsoft.AspNetCore.Http;
using Shared.Validation.Common;

namespace MaintenanceManagement.Presentation.Validation.WorkOrder
{
    public class UploadWorkOrderCommandValidator : AbstractValidator<UploadFileWorkOrderCommand>
    {
        private readonly List<ValidationRule> _validationRules;
        public UploadWorkOrderCommandValidator()
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
                            .WithMessage($"{nameof(UploadFileWorkOrderCommand.File)} {rule.Error}")
                            .NotEmpty()
                            .WithMessage($"{nameof(UploadFileWorkOrderCommand.File)} {rule.Error}");
                        break;
                    case "FileValidation":
                        RuleFor(x => x.File)
                            .Must(file => file != null && IsValidFileType(file, rule.allowedExtensions))
                            .WithMessage($"{nameof(UploadFileWorkOrderCommand.File)} {rule.Error}")
                            .Must(file => file != null && file.Length <= 2 * 1024 * 1024) // 2MB size limit
                            .WithMessage($"{nameof(UploadFileWorkOrderCommand.File)} {rule.Error}");
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