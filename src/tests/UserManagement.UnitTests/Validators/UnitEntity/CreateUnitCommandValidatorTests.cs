using Contracts.Interfaces;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Commands.CreateUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Unit;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UnitEntity
{
    public sealed class CreateUnitCommandValidatorTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UnitValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private CreateUnitCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQueryRepo.Object);

        private void SetupAllAsyncMocks(int unitTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(unitTypeId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand();
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyUnitName_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(unitName: "");
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitName);
        }

        [Fact]
        public async Task Validate_EmptyShortName_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(shortName: "");
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_UnitNameExceedsMaxLength_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(unitName: new string('A', 51));
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitName);
        }

        [Fact]
        public async Task Validate_ShortNameExceedsMaxLength_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(shortName: new string('A', 11));
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ShortName);
        }

        [Fact]
        public async Task Validate_EmptyUnitHeadName_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(unitHeadName: "");
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitHeadName);
        }

        [Fact]
        public async Task Validate_EmptyCINNO_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(cinno: "");
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CINNO);
        }

        [Fact]
        public async Task Validate_InvalidUnitTypeId_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(unitTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitTypeId);
        }

        [Fact]
        public async Task Validate_InactiveMiscMaster_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidCreateCommand(unitTypeId: 99);

            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(99))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitTypeId);
        }

        [Fact]
        public async Task Validate_InvalidEmail_FailsValidation()
        {
            var contactsDto = UnitEntityBuilders.ValidUnitContactsDto(email: "invalid-email");
            var command = UnitEntityBuilders.ValidCreateCommand(unitContactsDto: contactsDto);
            SetupAllAsyncMocks(command.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitContactsDto!.Email);
        }
    }
}
