using System.Threading.Tasks;
using Core.Application.Common.Interfaces;
using Core.Application.Common.Interfaces.IUser;
using Core.Application.Users.Commands.UpdateUser;
using Core.Domain.Entities;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using UserManagement.API.Validation.Common;
using UserManagement.API.Validation.Users;
using UserManagement.Infrastructure.Data;
using Xunit;

namespace UserManagement.UnitTests.Validation.Users
{
    public sealed class UpdateUserCommandValidatorTests
    {
        private readonly Mock<IUserQueryRepository> _userRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _ip = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _tz = new(MockBehavior.Loose);

        private UpdateUserCommandValidator CreateValidator()
        {
            var dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            _ip.Setup(x => x.GetGroupcode()).Returns("ADMIN");

            var db = new ApplicationDbContext(dbOptions, _ip.Object, _tz.Object);
            var maxProvider = new MaxLengthProvider(db);

            return new UpdateUserCommandValidator(maxProvider, _userRepo.Object);
        }

        private static UpdateUserCommand ValidCmd() => new()
        {
            UserId = 1,
            UserGroupId = 1,
            FirstName = "Neo",
            LastName = "Anderson",
            UserName = "neo",
            EmailId = "neo@matrix.io",
            Mobile = "9999999999"
        };

        [Fact]
        public async Task Valid_payload_passes()
        {
            var cmd = ValidCmd();

            _userRepo.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, cmd.UserId)).ReturnsAsync(false);
            _userRepo.Setup(r => r.NotFoundAsync(cmd.UserId)).ReturnsAsync(true);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NotEmpty_rules_trigger_when_missing_fields()
        {
            var cmd = new UpdateUserCommand(); // all defaults

            // Broad default setups to satisfy async rules invoked by the validator:
            _userRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                     .ReturnsAsync(false);
            _userRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                     .ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.FirstName);
            result.ShouldHaveValidationErrorFor(x => x.LastName);
            result.ShouldHaveValidationErrorFor(x => x.UserName);
            result.ShouldHaveValidationErrorFor(x => x.UserId);
            result.ShouldHaveValidationErrorFor(x => x.UserGroupId);
        }

        [Fact]
        public async Task MaxLength_for_FirstName_is_enforced()
        {
            _userRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string?>(), It.IsAny<int?>()))
                     .ReturnsAsync(false);
            _userRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>()))
                     .ReturnsAsync(true);

            var v = CreateValidator();

            var cmd = ValidCmd();
            cmd.FirstName = new string('a', 200); // exceed typical limits safely

            var result = await v.TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.FirstName);
        }

        [Theory]
        [InlineData("bad")]
        [InlineData("bad@")]
        [InlineData("@bad.com")]
        public async Task Email_invalid_formats_are_rejected(string email)
        {
            var cmd = ValidCmd();
            cmd.EmailId = email;

            _userRepo.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, cmd.UserId)).ReturnsAsync(false);
            _userRepo.Setup(r => r.NotFoundAsync(cmd.UserId)).ReturnsAsync(true);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.EmailId);
        }

        [Fact]
        public async Task Mobile_invalid_pattern_is_rejected()
        {
            var cmd = ValidCmd();
            cmd.Mobile = "abc";

            _userRepo.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, cmd.UserId)).ReturnsAsync(false);
            _userRepo.Setup(r => r.NotFoundAsync(cmd.UserId)).ReturnsAsync(true);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Mobile);
        }

        [Fact]
        public async Task AlreadyExists_rule_blocks_duplicate_username()
        {
            var cmd = ValidCmd();

            _userRepo.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, cmd.UserId)).ReturnsAsync(true);
            _userRepo.Setup(r => r.NotFoundAsync(cmd.UserId)).ReturnsAsync(true);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task NotFound_rule_fails_when_userId_does_not_exist()
        {
            var cmd = ValidCmd();

            _userRepo.Setup(r => r.NotFoundAsync(cmd.UserId)).ReturnsAsync(false);
            _userRepo.Setup(r => r.AlreadyExistsAsync(cmd.UserName!, cmd.UserId)).ReturnsAsync(false);

            var v = CreateValidator();
            var result = await v.TestValidateAsync(cmd);

            result.ShouldHaveAnyValidationError();
        }
    }
}
