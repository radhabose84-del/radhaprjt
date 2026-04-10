using BackgroundService.Application.Notification.Common.Interfaces.INotificationTemplate;
using BackgroundService.Application.Notification.NotificationTemplate.Command.UpdateNotificationTemplate;
using BackgroundService.Presentation.Validation.Common;
using BackgroundService.Presentation.Validation.NotificationTemplate;
using FluentValidation.TestHelper;

namespace BackgroundService.UnitTests.Validators.Notification.NotificationTemplate
{
    public sealed class UpdateNotificationTemplateCommandValidatorTests
    {
        private readonly Mock<INotificationTemplateCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<INotificationTemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateNotificationTemplateCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockCommandRepo.Object, _mockQueryRepo.Object);

        private static UpdateNotificationTemplateCommand ValidCommand() =>
            new()
            {
                Id = 1,
                NotificationTypeId = 1,
                NotificationConfigId = 1,
                SubjectTemplate = "Updated Subject",
                HeaderTemplate = "Header",
                BodyTemplate = "Body",
                FooterTemplate = "Footer",
                LanguageCode = "en",
                IsActive = 1
            };

        private void SetupAllAsyncMocks(int id = 1, int configId = 1, int typeId = 1, string languageCode = "en")
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(true);
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(configId, typeId, languageCode, id)).ReturnsAsync(false);
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
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            // AlreadyExists has .NotEmpty() then .MustAsync on NotificationConfigId
            _mockCommandRepo
                .Setup(r => r.IsNameDuplicateAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.SubjectTemplate);
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = ValidCommand();
            command.Id = 99;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(false);
            _mockCommandRepo.Setup(r => r.IsNameDuplicateAsync(1, 1, "en", 99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
