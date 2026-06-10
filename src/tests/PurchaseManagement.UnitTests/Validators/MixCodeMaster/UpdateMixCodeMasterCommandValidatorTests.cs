using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMixCodeMaster;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.MixCodeMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.MixCodeMaster
{
    public sealed class UpdateMixCodeMasterCommandValidatorTests
    {
        private readonly Mock<IMixCodeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateMixCodeMasterCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockQueryRepo.Object);

        private void SetupExists() =>
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupExists();
            var command = MixCodeMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyDesc_FailsValidation()
        {
            SetupExists();
            var command = MixCodeMasterBuilders.ValidUpdateCommand(desc: "");

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.MixCodeDesc);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            var command = MixCodeMasterBuilders.ValidUpdateCommand(id: 99);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_InvalidIsActive_FailsValidation()
        {
            SetupExists();
            var command = MixCodeMasterBuilders.ValidUpdateCommand(isActive: 5);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }
    }
}
