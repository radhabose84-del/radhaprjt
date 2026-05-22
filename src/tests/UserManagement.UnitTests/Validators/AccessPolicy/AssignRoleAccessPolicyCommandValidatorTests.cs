using FluentValidation.TestHelper;
using UserManagement.Application.AccessPolicy.Commands.AssignRoleAccessPolicy;
using UserManagement.Application.Common.Interfaces.IAccessPolicy;
using UserManagement.Presentation.Validation.AccessPolicy;

namespace UserManagement.UnitTests.Validators.AccessPolicy
{
    public sealed class AssignRoleAccessPolicyCommandValidatorTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private AssignRoleAccessPolicyCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object);

        private void SetupHappyPath(int accessPolicyId = 1, int roleId = 1, int valueId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(accessPolicyId)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UserRoleExistsAsync(roleId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RoleValueAssignmentExistsAsync(accessPolicyId, roleId, valueId, null))
                .ReturnsAsync(false);
        }

        private static AssignRoleAccessPolicyCommand ValidCommand() => new()
        {
            AccessPolicyId = 1,
            RoleId         = 1,
            ValueId        = 1
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupHappyPath();

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroAccessPolicyId_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.AccessPolicyId = 0;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.AccessPolicyId);
        }

        [Fact]
        public async Task Validate_ZeroRoleId_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.RoleId = 0;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.RoleId);
        }

        [Fact]
        public async Task Validate_ZeroValueId_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.ValueId = 0;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.ValueId);
        }

        [Fact]
        public async Task Validate_NonExistentAccessPolicyId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(99)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UserRoleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RoleValueAssignmentExistsAsync(99, 1, 1, null)).ReturnsAsync(false);

            var cmd = ValidCommand();
            cmd.AccessPolicyId = 99;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.AccessPolicyId);
        }

        [Fact]
        public async Task Validate_NonExistentRoleId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UserRoleExistsAsync(99)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.RoleValueAssignmentExistsAsync(1, 99, 1, null)).ReturnsAsync(false);

            var cmd = ValidCommand();
            cmd.RoleId = 99;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.RoleId);
        }

        [Fact]
        public async Task Validate_DuplicateAssignment_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.UserRoleExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.RoleValueAssignmentExistsAsync(1, 1, 1, null)).ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveAnyValidationError();
        }
    }
}
