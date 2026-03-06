using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;

namespace BackgroundService.Presentation.Validation.NotificationWhatsAppGroup
{
    public class DeleteNotificationWhatsAppGroupCommandValidator 
        : AbstractValidator<BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup.DeleteNotificationWhatsAppGroupCommand>
    {
        public DeleteNotificationWhatsAppGroupCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("Id is required.");
        }
    }
}
