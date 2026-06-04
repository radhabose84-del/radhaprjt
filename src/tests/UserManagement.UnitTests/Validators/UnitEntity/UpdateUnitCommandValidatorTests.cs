using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Party;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Commands.UpdateUnit;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Infrastructure.Data;
using UserManagement.Presentation.Validation.Common;
using UserManagement.Presentation.Validation.Unit;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Validators.UnitEntity
{
    public sealed class UpdateUnitCommandValidatorTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IBankAccountLookup> _mockBankLookup = new(MockBehavior.Strict);

        private static MaxLengthProvider CreateMaxLengthProvider()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"UnitUpdateValidatorDb_{Guid.NewGuid()}")
                .Options;
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            return new MaxLengthProvider(ctx);
        }

        private UpdateUnitCommandValidator CreateValidator() =>
            new(CreateMaxLengthProvider(), _mockQueryRepo.Object, _mockBankLookup.Object);

        private void SetupAllAsyncMocks(int unitTypeId = 1)
        {
            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(unitTypeId))
                .ReturnsAsync(true);
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand();
            SetupAllAsyncMocks(command.UpdateUnitDto!.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyUnitName_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand(unitName: "");
            SetupAllAsyncMocks(command.UpdateUnitDto!.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UpdateUnitDto!.UnitName);
        }

        [Fact]
        public async Task Validate_EmptyShortName_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand(shortName: "");
            SetupAllAsyncMocks(command.UpdateUnitDto!.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UpdateUnitDto!.ShortName);
        }

        [Fact]
        public async Task Validate_UnitNameExceedsMaxLength_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand(unitName: new string('A', 51));
            SetupAllAsyncMocks(command.UpdateUnitDto!.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UpdateUnitDto!.UnitName);
        }

        [Fact]
        public async Task Validate_InvalidUnitTypeId_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand(unitTypeId: 0);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UpdateUnitDto!.UnitTypeId);
        }

        [Fact]
        public async Task Validate_InactiveMiscMaster_FailsValidation()
        {
            var command = UnitEntityBuilders.ValidUpdateCommand(unitTypeId: 99);

            _mockQueryRepo
                .Setup(r => r.MiscMasterExistsAsync(99))
                .ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UpdateUnitDto!.UnitTypeId);
        }

        [Fact]
        public async Task Validate_InvalidEmail_FailsValidation()
        {
            var updateDto = UnitEntityBuilders.ValidUpdateUnitsDto();
            updateDto.UnitContactsDto = UnitEntityBuilders.ValidUnitContactsDto(email: "invalid-email");
            var command = new UpdateUnitCommand { UpdateUnitDto = updateDto };
            SetupAllAsyncMocks(updateDto.UnitTypeId);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UpdateUnitDto!.UnitContactsDto!.Email);
        }
    }
}
