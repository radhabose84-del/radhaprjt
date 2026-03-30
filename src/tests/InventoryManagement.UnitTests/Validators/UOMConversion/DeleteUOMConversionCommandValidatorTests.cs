using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Presentation.Validation.UOMConversion;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UOMConversion
{
    public sealed class DeleteUOMConversionCommandValidatorTests
    {
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeleteUOMConversionCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(UOMConversionBuilders.ValidDto());
        }

        private DeleteUOMConversionCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            var command = UOMConversionBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteUOMConversionCommand { Id = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            var command = UOMConversionBuilders.ValidDeleteCommand(id: 999);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion.UOMConversionDto?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
