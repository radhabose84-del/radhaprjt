using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.UpdateTripSheet;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;
using SalesManagement.Presentation.Validation.TripSheet;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.TripSheet
{
    public sealed class UpdateTripSheetCommandValidatorTests
    {
        private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateTripSheetCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateTripSheetCommand ValidCommand() => new()
        {
            Id = 1,
            TripDate = new DateOnly(2026, 1, 15),
            VehicleNo = "KA01AB1234",
            IsActive = 1,
            Details = new List<CreateTripSheetDetailItem>
            {
                new() { DispatchAdviceHeaderId = 1, SequenceNo = 1 }
            }
        };

        private void SetupValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.DispatchExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.DispatchAlreadyInTripAsync(1, It.IsAny<int?>())).ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.IsActive);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task VehicleNo_Empty_FailsValidation(string? vehicleNo)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.VehicleNo = vehicleNo;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VehicleNo);
        }
    }
}
