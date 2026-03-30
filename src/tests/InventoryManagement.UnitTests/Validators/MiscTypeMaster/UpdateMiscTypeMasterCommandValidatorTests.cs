using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IMiscTypeMaster;
using InventoryManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using InventoryManagement.Presentation.Validation.Common;
using InventoryManagement.Presentation.Validation.MiscTypeMaster;

namespace InventoryManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly MaxLengthProvider _maxLengthProvider = new(null!);

        public UpdateMiscTypeMasterCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                .ReturnsAsync(true);

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _maxLengthProvider);

        private static UpdateMiscTypeMasterCommand ValidUpdateCommand(
            int id = 1,
            string miscTypeCode = "MT001",
            string description = "Updated Misc Type") =>
            new UpdateMiscTypeMasterCommand
            {
                Id = id,
                MiscTypeCode = miscTypeCode,
                Description = description,
                IsActive = 1
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyCode_FailsValidation(string? code)
        {
            var command = ValidUpdateCommand(miscTypeCode: code!);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicateCode_FailsValidation()
        {
            var command = ValidUpdateCommand();

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.MiscTypeCode!, command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
