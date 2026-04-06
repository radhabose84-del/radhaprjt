using FluentValidation.TestHelper;
using UserManagement.Application.Common.Interfaces.IEntity;
using UserManagement.Application.Entity.Commands.DeleteEntity;
using UserManagement.Presentation.Validation.Entity;

namespace UserManagement.UnitTests.Validators.Entity
{
    public sealed class DeleteEntityCommandValidatorTests
    {
        private readonly Mock<IEntityQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteEntityCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ValidEntityId_NoDependencies_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(false);

            var command = new DeleteEntityCommand { EntityId = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroEntityId_FailsValidation()
        {
            var command = new DeleteEntityCommand { EntityId = 0 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EntityId);
        }

        [Fact]
        public async Task Validate_EntityHasDependencies_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(1))
                .ReturnsAsync(true);

            var command = new DeleteEntityCommand { EntityId = 1 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.EntityId);
        }

        [Fact]
        public async Task Validate_EntityNoDependencies_PassesSoftDeleteValidation()
        {
            _mockQueryRepo
                .Setup(r => r.SoftDeleteValidation(5))
                .ReturnsAsync(false);

            var command = new DeleteEntityCommand { EntityId = 5 };
            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.EntityId);
        }
    }
}
