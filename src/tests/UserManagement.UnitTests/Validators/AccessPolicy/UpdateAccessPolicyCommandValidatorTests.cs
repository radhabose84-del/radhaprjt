using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.AccessPolicy.Commands.UpdateAccessPolicy;
using UserManagement.Application.Common.Interfaces;
using IAccessPolicyQueryRepository = UserManagement.Application.Common.Interfaces.IAccessPolicy.IAccessPolicyQueryRepository;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.AccessPolicy;
using UserManagement.Presentation.Validation.Common;

namespace UserManagement.UnitTests.Validators.AccessPolicy
{
    public sealed class UpdateAccessPolicyCommandValidatorTests
    {
        private readonly Mock<IAccessPolicyQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService>            _ip            = new(MockBehavior.Loose);

        private UpdateAccessPolicyCommandValidator CreateValidator()
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

            return new UpdateAccessPolicyCommandValidator(_mockQueryRepo.Object, maxLenProvider);
        }

        private void SetupExistingId(int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(false);
        }

        private static UpdateAccessPolicyCommand ValidCommand(int id = 1) => new()
        {
            Id         = id,
            PolicyName = "Updated Policy",
            EntityName = "SalesOrder",
            FieldName  = "SalesOrderTypeId",
            IsActive   = 1
        };

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupExistingId(1);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyPolicyName_FailsValidation(string? name)
        {
            SetupExistingId(1);

            var cmd = ValidCommand();
            cmd.PolicyName = name!;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.PolicyName);
        }

        [Fact]
        public async Task Validate_NonExistentId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand(id: 99));

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(2)]
        public async Task Validate_InvalidIsActive_FailsValidation(int isActive)
        {
            SetupExistingId(1);

            var cmd = ValidCommand();
            cmd.IsActive = isActive;

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            var cmd = ValidCommand(id: 0);

            var result = await CreateValidator().TestValidateAsync(cmd);

            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
