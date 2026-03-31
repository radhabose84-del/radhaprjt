using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUsageType;
using InventoryManagement.Presentation.Validation.UsageType;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Validators.UsageType
{
    public sealed class DeleteUsageTypeCommandValidatorTests
    {
        private readonly Mock<IUsageTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteUsageTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var command = UsageTypeBuilders.ValidDeleteCommand();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new InventoryManagement.Application.UsageType.Commands.DeleteUsageType.DeleteUsageTypeCommand(0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
