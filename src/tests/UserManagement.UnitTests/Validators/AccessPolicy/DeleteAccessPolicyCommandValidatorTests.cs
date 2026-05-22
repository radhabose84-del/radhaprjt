using FluentValidation.TestHelper;
using UserManagement.Application.AccessPolicy.Commands.DeleteAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Presentation.Validation.AccessPolicy;

namespace UserManagement.UnitTests.Validators.AccessPolicy
{
    public sealed class DeleteAccessPolicyCommandValidatorTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private DeleteAccessPolicyCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new DeleteAccessPolicyCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new DeleteAccessPolicyCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new DeleteAccessPolicyCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
