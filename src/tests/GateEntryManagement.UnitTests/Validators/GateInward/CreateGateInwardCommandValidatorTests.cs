using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using FluentValidation.TestHelper;
using GateEntryManagement.Application.Common.Interfaces.IGateInward;
using GateEntryManagement.Application.GateInward.Commands.CreateGateInward;
using GateEntryManagement.Application.GateInward.Dto;
using GateEntryManagement.Presentation.Validation.Common;
using GateEntryManagement.Presentation.Validation.GateInward;

namespace GateEntryManagement.UnitTests.Validators.GateInward
{
    public sealed class CreateGateInwardCommandValidatorTests
    {
        private readonly Mock<IGateInwardQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Strict);

        private CreateGateInwardCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null), _mockQueryRepo.Object, _mockPartyLookup.Object);

        private static CreateGateInwardCommand ValidCommand() =>
            new()
            {
                VehicleMovementRecordId = 1,
                PartyId = 1099,
                ReceivingTypeId = 9, // Vehicle
                CourierNumber = null,
                UnitId = 1,
                GrossWeight = 100,
                TareWeight = 50,
                QAInspectionRequired = false,
                QAStatusId = null,
                Remarks = "Test remarks",
                GateInwardDetails = new List<CreateGateInwardDetailDto>
                {
                    new() { ReferenceDocTypeId = 1, ReferenceDocNo = "DOC001", PartyName = "Test Party" }
                }
            };

        private void SetupAllAsyncMocks(int vmrId = 1, int unitId = 1, int partyId = 1099, int receivingTypeId = 9, bool isCourier = false)
        {
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(vmrId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(unitId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(receivingTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsCourierReceivingTypeAsync(receivingTypeId)).ReturnsAsync(isCourier);
            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(partyId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = partyId, PartyName = "Test Party" });
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            var command = ValidCommand();
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_EmptyVMRId_NowPassesValidation()
        {
            // VehicleMovementRecordId is now OPTIONAL — Courier/Manual flows skip the VMR link.
            var command = ValidCommand();
            command.VehicleMovementRecordId = null;
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsCourierReceivingTypeAsync(9)).ReturnsAsync(false);
            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(1099, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1099, PartyName = "Test Party" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.VehicleMovementRecordId);
        }

        [Fact]
        public async Task Validate_EmptyDetails_FailsValidation()
        {
            var command = ValidCommand();
            command.GateInwardDetails = null;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GateInwardDetails);
        }

        [Fact]
        public async Task Validate_EmptyUnitId_FailsValidation()
        {
            var command = ValidCommand();
            command.UnitId = 0;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsCourierReceivingTypeAsync(9)).ReturnsAsync(false);
            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(1099, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1099, PartyName = "Test Party" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.UnitId);
        }

        [Fact]
        public async Task Validate_EmptyPartyId_NowPassesValidation()
        {
            // PartyId is now OPTIONAL — null accepted.
            var command = ValidCommand();
            command.PartyId = null;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsCourierReceivingTypeAsync(9)).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.PartyId);
        }

        [Fact]
        public async Task Validate_InvalidPartyId_FailsValidation()
        {
            // PartyId still rejected when an unknown id is sent.
            var command = ValidCommand();
            command.PartyId = 9999;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsCourierReceivingTypeAsync(9)).ReturnsAsync(false);
            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(9999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PartyLookupDto?)null);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PartyId);
        }

        [Fact]
        public async Task Validate_EmptyReceivingTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.ReceivingTypeId = null;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(1099, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1099, PartyName = "Test Party" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceivingTypeId);
        }

        [Fact]
        public async Task Validate_InvalidReceivingTypeId_FailsValidation()
        {
            var command = ValidCommand();
            command.ReceivingTypeId = 9999;
            _mockQueryRepo.Setup(r => r.VehicleMovementRecordExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.UnitExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(9999)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsCourierReceivingTypeAsync(9999)).ReturnsAsync(false);
            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(1099, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 1099, PartyName = "Test Party" });

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.ReceivingTypeId);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_CourierType_MissingCourierNumber_FailsValidation(string? courierNumber)
        {
            var command = ValidCommand();
            command.ReceivingTypeId = 10; // Courier
            command.CourierNumber = courierNumber;
            // IsCourierReceivingTypeAsync(10) -> true triggers the conditional NotEmpty on CourierNumber.
            SetupAllAsyncMocks(receivingTypeId: 10, isCourier: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.CourierNumber);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task Validate_VehicleType_CourierNumberOptional_Passes(string? courierNumber)
        {
            var command = ValidCommand();
            command.ReceivingTypeId = 9; // Vehicle — courier number not required
            command.CourierNumber = courierNumber;
            SetupAllAsyncMocks(receivingTypeId: 9, isCourier: false);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.CourierNumber);
        }

        [Fact]
        public async Task Validate_CourierType_WithCourierNumber_Passes()
        {
            var command = ValidCommand();
            command.ReceivingTypeId = 10;
            command.CourierNumber = "DTDC-AWB-12345";
            SetupAllAsyncMocks(receivingTypeId: 10, isCourier: true);

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.CourierNumber);
        }

        [Fact]
        public async Task Validate_NegativeGrossWeight_FailsValidation()
        {
            var command = ValidCommand();
            command.GrossWeight = -1;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.GrossWeight);
        }

        [Fact]
        public async Task Validate_NegativeTareWeight_FailsValidation()
        {
            var command = ValidCommand();
            command.TareWeight = -1;
            SetupAllAsyncMocks();

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.TareWeight);
        }
    }
}
