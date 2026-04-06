using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.ICustomField;
using UserManagement.Application.CustomFields.Commands.DeleteCustomField;
using UserManagement.Presentation.Validation.CustomFields;

namespace UserManagement.UnitTests.Validators.CustomFields
{
    public sealed class DeleteCustomFieldCommandValidatorTests
    {
        private readonly Mock<ICustomFieldQuery> _mockCustomFieldQuery = new(MockBehavior.Strict);

        private DeleteCustomFieldCommandValidator CreateValidator() =>
            new(_mockCustomFieldQuery.Object);

        [Fact]
        public async Task Validate_ValidId_CustomFieldExists_PassesValidation()
        {
            _mockCustomFieldQuery
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var command = new DeleteCustomFieldCommand { Id = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockCustomFieldQuery
                .Setup(r => r.NotFoundAsync(0))
                .ReturnsAsync(false);

            var command = new DeleteCustomFieldCommand { Id = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_CustomFieldNotFound_FailsValidation()
        {
            _mockCustomFieldQuery
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(false);

            var command = new DeleteCustomFieldCommand { Id = 99 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveAnyValidationError();
        }
    }
}
