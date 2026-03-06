using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup;

namespace BackgroundService.Presentation.Validation.NotificationWhatsAppGroup
{
    public class CreateNotificationWhatsAppGroupCommandValidator : AbstractValidator<CreateNotificationWhatsAppGroupCommand>
    {
        public CreateNotificationWhatsAppGroupCommandValidator()
        {
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
        }
    }
}