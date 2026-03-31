using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.MiscMaster;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockMaxLength.Object);

        private void SetupAllAsyncMocks(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(true);
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
