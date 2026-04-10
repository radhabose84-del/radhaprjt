using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.IUOM;
using InventoryManagement.Application.UOM.Command.DeleteUOM;
using InventoryManagement.Presentation.Validation.UOM;

namespace InventoryManagement.UnitTests.Validators.UOM
{
    public sealed class DeleteUOMCommandValidatorTests
    {
        private readonly Mock<IUOMQueryRepository> _mockRepo = new(MockBehavior.Strict);

        private DeleteUOMCommandValidator CreateValidator() => new(_mockRepo.Object);

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteUOMCommand { Id = 0 };
            _mockRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.SoftDeleteValidation(0)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_ValidId_Exists_PassesValidation()
        {
            var command = new DeleteUOMCommand { Id = 1 };
            _mockRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockRepo.Setup(r => r.SoftDeleteValidation(1)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NotFound_FailsValidation()
        {
            var command = new DeleteUOMCommand { Id = 99 };
            _mockRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockRepo.Setup(r => r.SoftDeleteValidation(99)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
