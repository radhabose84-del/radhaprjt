using Contracts.Interfaces;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Departments.Commands.UpdateDepartment;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Department;
using UserManagement.Infrastructure.Data;
using UserManagement.UnitTests.TestData;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Validators.Department
{
    public sealed class UpdateDepartmentCommandValidatorTests
    {
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        private UpdateDepartmentCommandValidator CreateValidator()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var db = new ApplicationDbContext(dbOptions, _ip.Object, _tz.Object);
            var maxProvider = new MaxLengthProvider(db);

            return new UpdateDepartmentCommandValidator(maxProvider);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = DepartmentBuilders.ValidUpdateCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyDeptName_FailsValidation(string? deptName)
        {
            var command = DepartmentBuilders.ValidUpdateCommand(deptName: deptName);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DeptName);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyShortName_FailsValidation(string? shortName)
        {
            var command = DepartmentBuilders.ValidUpdateCommand(shortName: shortName);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_DeptNameExceedsMaxLength_FailsValidation()
        {
            var command = DepartmentBuilders.ValidUpdateCommand(deptName: new string('A', 51));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.DeptName);
        }

        [Fact]
        public async Task Validate_ShortNameExceedsMaxLength_FailsValidation()
        {
            var command = DepartmentBuilders.ValidUpdateCommand(shortName: new string('A', 11));
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_DeptNameWithinMaxLength_PassesValidation()
        {
            var command = DepartmentBuilders.ValidUpdateCommand(deptName: "ValidName");
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveValidationErrorFor(x => x.DeptName);
        }
    }
}
