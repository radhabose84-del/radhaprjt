using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.AccessPolicy.Commands.CreateAccessPolicy;
using UserManagement.Application.Common.Interfaces;
using IAccessPolicyQueryRepository = UserManagement.Application.Common.Interfaces.IAccessPolicy.IAccessPolicyQueryRepository;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.AccessPolicy;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.UnitTests.Validators.AccessPolicy
{
    public sealed class CreateAccessPolicyCommandValidatorTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService>            _ip            = new(MockBehavior.Loose);

        private CreateAccessPolicyCommandValidator CreateValidator()
        {
            _ip.Setup(x => x.GetGroupCode()).Returns("ADMIN");
            _ip.Setup(x => x.GetCompanyId()).Returns(1);
            _ip.Setup(x => x.GetUnitId()).Returns(1);

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var tz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            tz.Setup(x => x.GetCurrentTime(It.IsAny<string>())).Returns(DateTime.UtcNow);

            var db             = new ApplicationDbContext(options, _ip.Object, tz.Object);
            var maxLenProvider = new MaxLengthProvider(db);

            return new CreateAccessPolicyCommandValidator(_mockQueryRepo.Object, maxLenProvider);
        }

        private static CreateAccessPolicyCommand ValidCommand() => new()
        {
            PolicyCode = "AP001",
            PolicyName = "Test Policy",
            EntityName = "SalesOrder",
            FieldName  = "SalesOrderTypeId"
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("AP001", null))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPolicyCode_FailsValidation(string? code)
        {
            var cmd = ValidCommand();
            cmd.PolicyCode = code!;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PolicyCode);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPolicyName_FailsValidation(string? name)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var cmd = ValidCommand();
            cmd.PolicyName = name!;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PolicyName);
        }

        [Theory]
        [InlineData("AP-001")]
        [InlineData("AP 001")]
        [InlineData("AP@01")]
        public async Task Validate_NonAlphanumericPolicyCode_FailsValidation(string code)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(false);

            var cmd = ValidCommand();
            cmd.PolicyCode = code;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PolicyCode);
        }

        [Fact]
        public async Task Validate_DuplicatePolicyCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("AP001", null))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldHaveValidationErrorFor(x => x.PolicyCode);
        }

        [Fact]
        public async Task Validate_PolicyCodeExceedsMaxLength_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), null))
                .ReturnsAsync(false);

            var cmd = ValidCommand();
            cmd.PolicyCode = new string('A', 51);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PolicyCode);
        }
    }
}
