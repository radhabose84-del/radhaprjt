using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IMiscTypeMaster;
using UserManagement.Application.MiscTypeMaster.Command.UpdateMiscTypeMaster;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.MiscTypeMaster;

namespace UserManagement.UnitTests.Validators.MiscTypeMaster
{
    public sealed class UpdateMiscTypeMasterCommandValidatorTests
    {
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UpdateMiscTypeMasterDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private UpdateMiscTypeMasterCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, CreateMaxLengthProvider());

        private static UpdateMiscTypeMasterCommand ValidCommand() =>
            new UpdateMiscTypeMasterCommand
            {
                Id = 1,
                MiscTypeCode = "MT001",
                Description = "Updated Misc Type",
                IsActive = 1
            };

        private void SetupAllAsyncMocks(string code = "MT001", int id = 1)
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync(code, id))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(id))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks(command.MiscTypeCode!, command.Id);
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_EmptyMiscTypeCode_FailsValidation(string? code)
        {
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(It.IsAny<string>(), It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var command = ValidCommand();
            command.MiscTypeCode = code;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.MiscTypeCode);
        }

        [Fact]
        public async Task Validate_DuplicateMiscTypeCode_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MT001", 1))
                .ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(1))
                .ReturnsAsync(true);

            var command = ValidCommand();
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_RecordNotFound_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.AlreadyExistsAsync("MT001", 99))
                .ReturnsAsync(false);
            _mockQueryRepo
                .Setup(r => r.NotFoundAsync(99))
                .ReturnsAsync(false);

            var command = ValidCommand();
            command.Id = 99;
            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveAnyValidationError();
        }
    }
}
