using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.DeleteNotificationWhatsAppGroup;
using BackgroundService.Presentation.Validation.NotificationWhatsAppGroup;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationWhatsAppGroup
{
    public sealed class DeleteNotificationWhatsAppGroupCommandValidatorTests
    {
        private DeleteNotificationWhatsAppGroupCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = new DeleteNotificationWhatsAppGroupCommand { Id = 1 };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteNotificationWhatsAppGroupCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Id is required.");
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = new DeleteNotificationWhatsAppGroupCommand { Id = -1 };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
