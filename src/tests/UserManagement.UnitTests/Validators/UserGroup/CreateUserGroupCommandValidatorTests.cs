using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.UserGroup.Commands.CreateUserGroup;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.UserGroup;

namespace UserManagement.UnitTests.Validators.UserGroup
{
    public sealed class CreateUserGroupCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UserGroupCreateDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static CreateUserGroupCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static CreateUserGroupCommand ValidCommand() =>
            new CreateUserGroupCommand { GroupCode = "GRP01", GroupName = "Test Group" };

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GroupName_Empty_FailsValidation(string? name)
        {
            var cmd = ValidCommand();
            cmd.GroupName = name;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task GroupCode_Empty_FailsValidation(string? code)
        {
            var cmd = ValidCommand();
            cmd.GroupCode = code;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.GroupCode);
        }

        [Fact]
        public async Task GroupName_ExceedsMaxLength_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.GroupName = new string('A', 55);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.GroupName);
        }

        [Fact]
        public async Task GroupCode_ExceedsMaxLength_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.GroupCode = new string('A', 10);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.GroupCode);
        }
    }
}
