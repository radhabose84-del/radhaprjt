using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.CreateNotificationTemplate;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationTemplate;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationTemplate
{
    public sealed class CreateNotificationTemplateCommandValidatorTests
    {
        private readonly Mock<INotificationTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);

        private CreateNotificationTemplateCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockCommandRepo.Object);

        private static CreateNotificationTemplateCommand ValidCommand() =>
            new()
            {
                NotificationTypeId = 1,
                NotificationConfigId = 1,
                SubjectTemplate = "Subject",
                HeaderTemplate = "Header",
                BodyTemplate = "Body",
                FooterTemplate = "Footer",
                LanguageCode = "en"
            };

        private void SetupAllAsyncMocks(int configId = 1, int typeId = 1, string languageCode = "en")
        {
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(configId, typeId, languageCode))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptySubjectTemplate_FailsValidation(string? subject)
        {
            var command = ValidCommand();
            command.SubjectTemplate = subject;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.SubjectTemplate);
        }

        [Fact]
        public async Task Validate_EmptyNotificationTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.NotificationTypeId = 0;
            // AlreadyExists case has .NotEmpty() on NotificationConfigId, then .MustAsync with command.NotificationTypeId
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.NotificationTypeId);
        }

        [Fact]
        public async Task Validate_DuplicateCombination_FailsValidation()
        {
            _mockCommandRepo.Setup(r => r.ExistsByCodeAsync(1, 1, "en")).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyBodyTemplate_FailsValidation(string? body)
        {
            var command = ValidCommand();
            command.BodyTemplate = body;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.BodyTemplate);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyLanguageCode_FailsValidation(string? code)
        {
            var command = ValidCommand();
            command.LanguageCode = code;
            // LanguageCode is used in AlreadyExists MustAsync too
            _mockCommandRepo
                .Setup(r => r.ExistsByCodeAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.LanguageCode);
        }
    }
}
