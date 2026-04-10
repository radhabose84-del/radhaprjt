using BackgroundService.Application.Notification.NotificationWhatsAppGroup.Commands.CreateNotificationWhatsAppGroup;
using BackgroundService.Presentation.Validation.NotificationWhatsAppGroup;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationWhatsAppGroup
{
    public sealed class CreateNotificationWhatsAppGroupCommandValidatorTests
    {
        private CreateNotificationWhatsAppGroupCommandValidator CreateValidator() => new();

        private static CreateNotificationWhatsAppGroupCommand ValidCommand() =>
            new() { DepartmentId = 1, GroupName = "TestGroup", ApiKey = "key123" };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroDepartmentId_FailsValidation()
        {
            var command = ValidCommand();
            command.DepartmentId = 0;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DepartmentId)
                .WithErrorMessage("Department is required.");
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
        public async Task Validate_SpecialCharsInGroupName_FailsValidation()
        {
            var command = ValidCommand();
            command.GroupName = "Test@Group!";

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName)
                .WithErrorMessage("Group Name can contain only letters, numbers, and spaces.");
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

        [Fact]
        public async Task Validate_GroupNameTooLong_FailsValidation()
        {
            var command = ValidCommand();
            command.GroupName = new string('A', 251);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }
    }
}
