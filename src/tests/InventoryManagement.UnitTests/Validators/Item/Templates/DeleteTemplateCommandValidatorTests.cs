using FluentValidation.TestHelper;
using InventoryManagement.Application.Item.Templates.Commands.DeleteTemplate;
using InventoryManagement.Presentation.Validation.Item.Templates;

namespace InventoryManagement.UnitTests.Validators.Item.Templates
{
    public sealed class DeleteTemplateCommandValidatorTests
    {
        private DeleteTemplateCommandValidator CreateValidator() => new();

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
