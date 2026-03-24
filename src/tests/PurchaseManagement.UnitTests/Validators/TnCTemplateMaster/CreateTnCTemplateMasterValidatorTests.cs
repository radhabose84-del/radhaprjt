using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.ITnCTemplateMaster;
using PurchaseManagement.Presentation.Validation.TnCTemplateMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.TnCTemplateMaster
{
    public sealed class CreateTnCTemplateMasterValidatorTests
    {
        private readonly Mock<ITnCTemplateMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateTnCTemplateMasterValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(string templateName = "Test Template", int templateTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByTypeAndNameAsync(templateTypeId, templateName, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command.TemplateName, command.TemplateTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTemplateName_FailsValidation(string? name)
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand(templateName: name!);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task Validate_ZeroTemplateTypeId_FailsValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand(templateTypeId: 0);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateTypeId);
        }

        [Fact]
        public async Task Validate_DuplicateTemplate_FailsValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand();
            _mockQueryRepo
                .Setup(r => r.ExistsByTypeAndNameAsync(command.TemplateTypeId, command.TemplateName, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.ApplicabilitiesExistAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }
    }
}
