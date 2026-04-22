using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRawMaterialType;
using ProductionManagement.Application.RawMaterialType.Commands.DeleteRawMaterialType;
using ProductionManagement.Presentation.Validation.RawMaterialType;

namespace ProductionManagement.UnitTests.Validators.RawMaterialType
{
    public sealed class DeleteRawMaterialTypeCommandValidatorTests
    {
        private readonly Mock<IRawMaterialTypeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteRawMaterialTypeCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteRawMaterialTypeCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFoundId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(999)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteRawMaterialTypeCommand(999));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteRawMaterialTypeCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
