using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup;

namespace BackgroundService.Presentation.Validation.NotificationWhatsAppGroup
{
    public class UpdateNotificationWhatsAppGroupCommandValidator : AbstractValidator<UpdateNotificationWhatsAppGroupCommand>
    {
        public UpdateNotificationWhatsAppGroupCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id is required.");

            RuleFor(x => x.DepartmentId)
                .GreaterThan(0)
                .WithMessage("Department is required.");

            RuleFor(x => x.GroupName)
                .NotEmpty().WithMessage("Group Name is required.")
                .MaximumLength(250).WithMessage("Group Name cannot exceed 250 characters.")
                .Matches("^[a-zA-Z0-9 ]+$")
                .WithMessage("Group Name can contain only letters, numbers, and spaces.");
                
            RuleFor(x => x.ApiKey)                       
                .NotEmpty().WithMessage("API Key is required.")
                .MaximumLength(500).WithMessage("API Key cannot exceed 500 characters.");

            RuleFor(x => x.IsActive)
                .Must(v => v == 0 || v == 1)
                .WithMessage("IsActive must be 0 (Inactive) or 1 (Active).");
        }
    }
}
