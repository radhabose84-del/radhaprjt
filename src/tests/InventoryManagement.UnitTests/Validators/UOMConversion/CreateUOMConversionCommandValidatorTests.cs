using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Presentation.Validation.UOMConversion;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UOMConversion
{
    public sealed class CreateUOMConversionCommandValidatorTests
    {
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateUOMConversionCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int fromUOMId = 1, int toUOMId = 2)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(fromUOMId, toUOMId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UOMConversionBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.FromUOMId, command.ToUOMId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroFromUOMId_FailsValidation()
        {
            var command = UOMConversionBuilders.ValidCreateCommand(fromUOMId: 0);
            SetupAllAsyncMocks(0, command.ToUOMId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.FromUOMId);
        }

        [Fact]
        public async Task Validate_SameFromAndToUOMId_FailsValidation()
        {
            var command = UOMConversionBuilders.ValidCreateCommand(fromUOMId: 1, toUOMId: 1);
            SetupAllAsyncMocks(1, 1);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ToUOMId);
        }

        [Fact]
        public async Task Validate_ZeroConversionValue_FailsValidation()
        {
            var command = UOMConversionBuilders.ValidCreateCommand(conversionValue: 0m);
            SetupAllAsyncMocks(command.FromUOMId, command.ToUOMId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ConversionValue);
        }

        [Fact]
        public async Task Validate_DuplicateUOMPair_FailsValidation()
        {
            var command = UOMConversionBuilders.ValidCreateCommand();
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(command.FromUOMId, command.ToUOMId, null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
