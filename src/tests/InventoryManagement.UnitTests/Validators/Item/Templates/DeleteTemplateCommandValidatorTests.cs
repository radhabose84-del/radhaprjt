using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using InventoryManagement.Presentation.Validation.Item.Templates;

namespace InventoryManagement.UnitTests.Validators.Item.Templates
{
    public sealed class DeleteTemplateCommandValidatorTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        public DeleteTemplateCommandValidatorTests()
        {
            _mockQueryRepo
                .Setup(r => r.ExistsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidationAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
        }

        private DeleteTemplateCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidId_PassesValidation()
        {
            var command = new DeleteTemplateCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var command = new DeleteTemplateCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_NegativeId_FailsValidation()
        {
            var command = new DeleteTemplateCommand { Id = -1 };
            var result = await CreateValidator().TestValidateAsync(command);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
