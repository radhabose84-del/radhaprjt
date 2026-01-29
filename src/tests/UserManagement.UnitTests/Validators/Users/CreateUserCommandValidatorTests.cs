using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.ICompany;
using Core.Application.Common.Interfaces.IDepartment;
using Core.Application.Common.Interfaces.IDivision;
using Core.Application.Common.Interfaces.IUnit;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.Common.Interfaces.IUserRole;
using Core.Application.Users.Commands.CreateUser;
using Core.Domain.Entities;
using FluentAssertions;
using FluentValidation.TestHelper;
using Moq;
using Xunit;

using Microsoft.EntityFrameworkCore;
using UserManagement.Infrastructure.Data;

// Use the real DTO types your validator expects
using UserCompanyDTO = Core.Application.Users.Queries.GetUsers.UserCompanyDTO;
using UserUnitDTO = Core.Application.Users.Queries.GetUsers.UserUnitDTO;
using UserDivisionDTO = Core.Application.Users.Commands.CreateUser.UserDivisionDTO;
using UserDepartmentDTO = Core.Application.Users.Queries.GetUsers.UserDepartmentDTO;
using UserRoleAllocationDTO = Core.Application.Users.Queries.GetUsers.UserRoleAllocationDTO;
using UserManagement.API.Validation.Users;
using UserManagement.API.Validation.Common;

namespace UserManagement.UnitTests.Validators.Users
{
    public sealed class CreateUserCommandValidatorTests
    {
        private readonly Mock<IUserQueryRepository> _user = new(MockBehavior.Strict);
        private readonly Mock<ICompanyQueryRepository> _company = new(MockBehavior.Strict);
        private readonly Mock<IDivisionQueryRepository> _division = new(MockBehavior.Strict);
        private readonly Mock<IDepartmentQueryRepository> _department = new(MockBehavior.Strict);
        private readonly Mock<IUserRoleQueryRepository> _role = new(MockBehavior.Strict);
        private readonly Mock<IUnitQueryRepository> _unit = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Strict);

        private CreateUserCommandValidator CreateValidator(string groupCode = "ADMIN")
        {
            _ip.Setup(x => x.GetGroupcode()).Returns(groupCode);

            // Default allow role FK so strict mocks don’t fail in tests that don’t care about roles.
            _role.Setup(r => r.FKColumnExistValidation(It.IsAny<int>()))
                 .ReturnsAsync(true);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);

            var db = new ApplicationDbContext(options, _ip.Object, tz.Object);
            var maxLenProvider = new MaxLengthProvider(db);

            return new CreateUserCommandValidator(
                maxLenProvider,
                _user.Object,
                _company.Object,
                _division.Object,
                _department.Object,
                _role.Object,
                _unit.Object,
                _ip.Object);
        }

        private static CreateUserCommand ValidAdminCommand() => new()
        {
            FirstName = "Neo",
            LastName = "Anderson",
            UserName = "neo",
            EmailId = "neo@matrix.io",
            Mobile = "9999999999",
            Password = "Strong#1",
            UserGroupId = 1,
            UserCompanies = new List<UserCompanyDTO>(),
            userUnits = new List<UserUnitDTO>(),
            userDivisions = new List<UserDivisionDTO>(),
            userDepartments = new List<UserDepartmentDTO>(),
            userRoleAllocations = new List<UserRoleAllocationDTO> { new() { UserRoleId = 10 } }
        };

        private static CreateUserCommand ValidUserCommand() => new()
        {
            FirstName = "Trinity",
            LastName = "Trin",
            UserName = "trinity",
            EmailId = "t@company.com",
            Mobile = "8887776666",
            Password = "Password#9",
            UserGroupId = 1,
            UserCompanies = new List<UserCompanyDTO> { new() { CompanyId = 1 } },
            userUnits = new List<UserUnitDTO> { new() { UnitId = 101 } },
            userDivisions = new List<UserDivisionDTO> { new() { DivisionId = 201 } },
            userDepartments = new List<UserDepartmentDTO> { new() { DepartmentId = 301 } },
            userRoleAllocations = new List<UserRoleAllocationDTO> { new() { UserRoleId = 10 } },
        };

        [Fact]
        public async Task Valid_admin_payload_passes()
        {
            var cmd = ValidAdminCommand();
            _user.Setup(r => r.AlreadyExistsAsync("neo", It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator("ADMIN");
            var result = await v.TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Valid_user_payload_passes_and_checks_FKs()
        {
            var cmd = ValidUserCommand();

            _user.Setup(r => r.AlreadyExistsAsync("trinity", It.IsAny<int?>()))
                 .ReturnsAsync(false);
            _company.Setup(r => r.FKColumnExistValidation(1)).ReturnsAsync(true);
            _unit.Setup(r => r.FKColumnExistValidation(101)).ReturnsAsync(true);
            _division.Setup(r => r.FKColumnExistValidation(201)).ReturnsAsync(true);
            _department.Setup(r => r.FKColumnExistValidation(301)).ReturnsAsync(true);
            // role FK already defaulted to true in CreateValidator

            var v = CreateValidator("USER");
            var result = await v.TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task FirstName_notempty_rule_triggers(string? bad)
        {
            var cmd = ValidAdminCommand();
            cmd.FirstName = bad;

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }

        [Fact]
        public async Task MaxLength_is_enforced_from_MaxLengthProvider()
        {
            // Ensure IP group code is set before we construct our own provider below.
            _ip.Setup(x => x.GetGroupcode()).Returns("ADMIN");

            // Compute the actual configured max for FirstName using the same provider the validator uses.
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(System.Guid.NewGuid().ToString())
                .Options;
            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var db = new ApplicationDbContext(options, _ip.Object, tz.Object);
            var provider = new MaxLengthProvider(db);
            var max = provider.GetMaxLength<User>("FirstName") ?? 25;

            var cmd = ValidAdminCommand();
            cmd.FirstName = new string('a', max + 1); // exceed effective max

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator(); // validator will enforce the same limit
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }

        [Fact]
        public async Task AlreadyExists_rule_blocks_duplicate_username()
        {
            var cmd = ValidAdminCommand();
            _user.Setup(r => r.AlreadyExistsAsync("neo", It.IsAny<int?>()))
                 .ReturnsAsync(true);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("already"));
        }

        [Theory]
        [InlineData("short#1")]   // too short
        [InlineData("NoNumber#")] // missing digit
        [InlineData("nonumber#1")]// no uppercase
        [InlineData("NONUMBER#1")]// no lowercase
        [InlineData("NoSpecial1")]// no special
        public async Task Password_policy_is_enforced(string pwd)
        {
            var cmd = ValidAdminCommand();
            cmd.Password = pwd;

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
            result.Errors.Should().Contain(e => e.ErrorMessage.Contains("Password"));
        }

        [Theory]
        [InlineData("bad")]
        [InlineData("bad@")]
        [InlineData("bad@@example.com")]  // reliably invalid across all modes
        public async Task Email_must_be_valid_format(string email)
        {
            var cmd = ValidAdminCommand();
            cmd.EmailId = email;

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.EmailId);
        }

        [Fact]
        public async Task Mobile_must_match_pattern()
        {
            var cmd = ValidAdminCommand();
            cmd.Mobile = "abc";

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Mobile);
        }

        [Fact]
        public async Task USER_group_requires_companies_units_divisions_departments_and_roles()
        {
            var cmd = ValidUserCommand();

            // Wipe to trigger the USER-only NotEmpty rules
            cmd.UserCompanies.Clear();
            cmd.userUnits.Clear();
            cmd.userDivisions.Clear();
            cmd.userDepartments.Clear();
            cmd.userRoleAllocations.Clear();

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator("USER");
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor("UserCompanies");
            result.ShouldHaveValidationErrorFor("userUnits");
            result.ShouldHaveValidationErrorFor("userDivisions");
            result.ShouldHaveValidationErrorFor("userDepartments");
            result.ShouldHaveValidationErrorFor("userRoleAllocations");
        }

        [Fact]
        public async Task FK_rules_fail_when_repos_return_false()
        {
            var cmd = ValidUserCommand();

            _user.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, It.IsAny<int?>()))
                 .ReturnsAsync(false);
            _company.Setup(r => r.FKColumnExistValidation(1)).ReturnsAsync(false);
            _unit.Setup(r => r.FKColumnExistValidation(101)).ReturnsAsync(false);
            _division.Setup(r => r.FKColumnExistValidation(201)).ReturnsAsync(false);
            _department.Setup(r => r.FKColumnExistValidation(301)).ReturnsAsync(false);
            _role.Setup(r => r.FKColumnExistValidation(10)).ReturnsAsync(false); // override default

            var v = CreateValidator("USER");
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Sql_injection_patterns_in_username_are_blocked()
        {
            var cmd = ValidAdminCommand();
            cmd.UserName = "neo; DROP TABLE Users;--";

            _user.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int?>()))
                 .ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.UserName);
        }
    }
}
