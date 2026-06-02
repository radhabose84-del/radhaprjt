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

        private void SetupHappyPath(string templateName = "Test Template", int moduleId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByModuleAndNameAsync(moduleId, templateName, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand();
            SetupHappyPath(command.TemplateName, command.ModuleId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyTemplateName_FailsValidation(string? name)
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand(templateName: name!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TemplateName);
        }

        [Fact]
        public async Task Validate_ZeroModuleId_FailsValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand(moduleId: 0);
            _mockQueryRepo
                .Setup(r => r.ExistsByModuleAndNameAsync(It.IsAny<int>(), It.IsAny<string>(), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ModuleId);
        }

        [Fact]
        public async Task Validate_DuplicateTemplate_FailsValidation()
        {
            var command = TnCTemplateMasterBuilders.ValidCreateCommand();
            _mockQueryRepo
                .Setup(r => r.ExistsByModuleAndNameAsync(command.ModuleId, command.TemplateName, null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
