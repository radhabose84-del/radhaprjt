using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ITripSheet;
using SalesManagement.Application.TripSheet.Commands.CreateTripSheet;
using SalesManagement.Presentation.Validation.TripSheet;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.TripSheet
{
    public sealed class CreateTripSheetCommandValidatorTests
    {
        private readonly Mock<ITripSheetQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateTripSheetCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateTripSheetCommand ValidCommand() => new()
        {
            TripDate = new DateOnly(2026, 1, 15),
            VehicleNo = "KA01AB1234",
            Details = new List<CreateTripSheetDetailItem>
            {
                new() { DispatchAdviceHeaderId = 1, SequenceNo = 1 }
            }
        };

        private void SetupValid()
        {
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

        [Fact]
        public async Task DispatchNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.DispatchExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.DispatchAlreadyInTripAsync(1, It.IsAny<int?>())).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task DispatchAlreadyInAnotherTrip_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.DispatchExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.DispatchAlreadyInTripAsync(1, It.IsAny<int?>())).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
