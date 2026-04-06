using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IMiscMaster;
using UserManagement.Application.MiscMaster.Command.CreateMiscMaster;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.MiscMaster;

namespace UserManagement.UnitTests.Validators.MiscMaster
{
    public sealed class CreateMiscMasterCommandValidatorTests
    {
        private readonly Mock<IMiscMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"MiscMasterDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateMiscMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, CreateMaxLengthProvider());

        private static CreateMiscMasterCommand ValidCommand() =>
            new CreateMiscMasterCommand
            {
                Code = "MISC001",
                Description = "Test Misc Master",
                MiscTypeId = 1
            };

        private void SetupAllAsyncMocks(string code = "MISC001", int miscTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, miscTypeId, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.Code!, command.MiscTypeId);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_CodeExceedsMaxLength_FailsValidation()
        {
            var longCode = new string('A', 51);
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(longCode, 1, null))
                .ReturnsAsync(false);

            var command = ValidCommand();
            command.Code = longCode;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Code);
        }

        [Fact]
        public async Task Validate_DescriptionExceedsMaxLength_FailsValidation()
        {
            SetupAllAsyncMocks();
            var command = ValidCommand();
            command.Description = new string('A', 251);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public async Task Validate_DuplicateCodeAndMiscTypeId_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MISC001", 1, null))
                .ReturnsAsync(true);

            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
