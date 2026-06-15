using FluentValidation.TestHelper;
using UserManagement.Application.AccessPolicy.Commands.RemoveRoleAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Presentation.Validation.AccessPolicy;

namespace UserManagement.UnitTests.Validators.AccessPolicy
{
    public sealed class RemoveRoleAccessPolicyCommandValidatorTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private RemoveRoleAccessPolicyCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        [Fact]
        public async Task Validate_ExistingId_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.RoleAccessPolicyNotFoundAsync(1))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(new RemoveRoleAccessPolicyCommand(1));

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new RemoveRoleAccessPolicyCommand(0));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.RoleAccessPolicyNotFoundAsync(99))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(new RemoveRoleAccessPolicyCommand(99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
