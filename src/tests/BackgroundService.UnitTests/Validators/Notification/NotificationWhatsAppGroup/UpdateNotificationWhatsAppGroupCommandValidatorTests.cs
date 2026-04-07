using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.UpdateNotificationWhatsAppGroup;
using BackgroundService.Presentation.Validation.NotificationWhatsAppGroup;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationWhatsAppGroup
{
    public sealed class UpdateNotificationWhatsAppGroupCommandValidatorTests
    {
        private UpdateNotificationWhatsAppGroupCommandValidator CreateValidator() => new();

        private static UpdateNotificationWhatsAppGroupCommand ValidCommand() =>
            new() { Id = 1, DepartmentId = 1, GroupName = "TestGroup", ApiKey = "key123", IsActive = 1 };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = ValidCommand();
            command.Id = 0;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id)
                .WithErrorMessage("Id is required.");
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = ValidCommand();
            command.DepartmentId = 0;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DepartmentId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyGroupName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.GroupName = name!;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            var command = ValidCommand();
            command.IsActive = 5;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.IsActive)
                .WithErrorMessage("IsActive must be 0 (Inactive) or 1 (Active).");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyApiKey_FailsValidation(string? key)
        {
            var command = ValidCommand();
            command.ApiKey = key!;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ApiKey);
        }
    }
}
