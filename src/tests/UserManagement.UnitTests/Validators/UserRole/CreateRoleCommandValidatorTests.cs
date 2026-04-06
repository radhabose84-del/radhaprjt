using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.UserRole.Commands.CreateRole;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.UserRole;

namespace UserManagement.UnitTests.Validators.UserRole
{
    public sealed class CreateRoleCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserRoleCreateDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static CreateRoleCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static CreateRoleCommand ValidCommand() =>
            new CreateRoleCommand { RoleName = "Admin", Description = "Administrator role" };

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task RoleName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.RoleName = name;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.RoleName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Description_Empty_FailsValidation(string? description)
        {
            var cmd = ValidCommand();
            cmd.Description = description;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task RoleName_ExceedsMaxLength_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.RoleName = new string('A', 55);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.RoleName);
        }
    }
}
