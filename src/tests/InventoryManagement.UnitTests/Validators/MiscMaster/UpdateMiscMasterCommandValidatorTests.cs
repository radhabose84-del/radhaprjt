using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMiscMaster;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.MiscMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.MiscMaster
{
    public sealed class UpdateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        public UpdateMiscMasterCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        private UpdateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = MiscMasterBuilders.ValidUpdateCommand(code: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = MiscMasterBuilders.ValidUpdateCommand();

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.Code!, command.MiscTypeId, command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
