using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Presentation.Validation.TnCTemplateMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.TnCTemplateMaster
{
    public sealed class UpdateTnCTemplateMasterValidatorTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateTnCTemplateMasterValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int id = 1, string templateName = "Test Template", int templateTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByTypeAndNameAsync(templateTypeId, templateName, id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidUpdateCommand();
            SetupHappyPath(command.Id, command.TemplateName, command.TemplateTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidUpdateCommand(id: 0);
            SetupHappyPath(0, command.TemplateName, command.TemplateTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTemplateName_FailsValidation(string? name)
        {
            var command = TnCTemplateMasterBuilders.ValidUpdateCommand(templateName: name!);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task Validate_DuplicateName_FailsValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidUpdateCommand();
            _mockQueryRepo
                .Setup(r => r.ExistsByTypeAndNameAsync(command.TemplateTypeId, command.TemplateName, command.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
