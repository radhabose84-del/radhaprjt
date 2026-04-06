using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.CreateMiscTypeMaster;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.MiscTypeMaster;

namespace UserManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class CreateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"MiscTypeMasterDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, CreateMaxLengthProvider());

        private static CreateMiscTypeMasterCommand ValidCommand() =>
            new CreateMiscTypeMasterCommand
            {
                MiscTypeCode = "MT001",
                Description = "Test Misc Type"
            };

        private void SetupAllAsyncMocks(string code = "MT001")
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, null))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.MiscTypeCode!);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_MiscTypeCodeExceedsMaxLength_FailsValidation()
        {
            var longCode = new string('A', 51);
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(longCode, null))
                .ReturnsAsync(false);

            var command = ValidCommand();
            command.MiscTypeCode = longCode;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
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
        public async Task Validate_DuplicateMiscTypeCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MT001", null))
                .ReturnsAsync(true);

            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
