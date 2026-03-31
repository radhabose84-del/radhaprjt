using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using InventoryManagement.Presentation.Validation.UOMConversion;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UOMConversion
{
    public sealed class UpdateUOMConversionCommandValidatorTests
    {
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public UpdateUOMConversionCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(UOMConversionBuilders.ValidDto());
        }

        private UpdateUOMConversionCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UOMConversionBuilders.ValidUpdateCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_SameFromAndToUOM_FailsValidation()
        {
            var command = new UpdateUOMConversionCommand
            {
                Id = 1,
                FromUOMId = 1,
                ToUOMId = 1,
                ConversionValue = 1000m,
                IsActive = 1
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_DuplicatePair_FailsValidation()
        {
            var command = UOMConversionBuilders.ValidUpdateCommand();

            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.FromUOMId, command.ToUOMId, command.Id))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
