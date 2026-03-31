using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IHSNMaster;
using InventoryManagement.Presentation.Validation.HSNMaster;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.HSNMaster
{
    public sealed class UpdateHSNMasterCommandValidatorTests
    {
        private readonly Mock<IHSNMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public UpdateHSNMasterCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        private UpdateHSNMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = HSNMasterBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyHSNCode_FailsValidation(string? code)
        {
            var command = HSNMasterBuilders.ValidUpdateCommand(hsnCode: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidUpdateCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidUpdateCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(999))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateHSNCode_FailsValidation()
        {
            var command = HSNMasterBuilders.ValidUpdateCommand(hsnCode: "EXIST01");

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("EXIST01", command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
