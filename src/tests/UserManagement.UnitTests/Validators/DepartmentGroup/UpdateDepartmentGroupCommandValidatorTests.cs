using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.DepartmentGroup.Command.UpdateDepartmentGroup;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.DepartmentGroup;

namespace UserManagement.UnitTests.Validators.DepartmentGroup
{
    public sealed class UpdateDepartmentGroupCommandValidatorTests
    {
        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UpdateDeptGroupDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private static UpdateDepartmentGroupCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider());

        private static UpdateDepartmentGroupCommand ValidCommand() =>
            new UpdateDepartmentGroupCommand
            {
                Id = 1,
                DepartmentGroupName = "Updated Group Name",
                DepartmentGroupCode = "UGN"
            };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDepartmentGroupName_FailsValidation(string? name)
        {
            var command = ValidCommand();
            command.DepartmentGroupName = name;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DepartmentGroupName);
        }

        [Fact]
        public async Task Validate_DepartmentGroupNameExceedsMaxLength_FailsValidation()
        {
            var command = ValidCommand();
            command.DepartmentGroupName = new string('A', 51);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DepartmentGroupName);
        }

        [Fact]
        public async Task Validate_DepartmentGroupNameWithinMaxLength_PassesValidation()
        {
            var command = ValidCommand();
            command.DepartmentGroupName = "Valid Group";
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.DepartmentGroupName);
        }
    }
}
