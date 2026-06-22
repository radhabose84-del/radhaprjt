using FluentValidation.TestHelper;
using PurchaseManagement.Application.BlanketMaster.Commands.Create;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Presentation.Validation.BlanketMaster;

namespace PurchaseManagement.UnitTests.Validators.BlanketMaster
{
    public sealed class CreateBlanketMasterCommandValidatorTests
    {
        private readonly Mock<IBlanketMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateBlanketMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private static CreateBlanketMasterCommand ValidCommand()
        {
            var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            return new CreateBlanketMasterCommand
            {
                BlanketDate = now,
                VendorId = 1,
                CurrencyId = 1,
                ProcurementTypeId = 1,
                ValidityFrom = now,
                ValidityTo = now.AddMonths(6),
                Details = new List<CreateBlanketMasterDetailItem>
                {
                    new()
                    {
                        ItemSno = 1, ItemId = 10, UOMId = 1, EstimatedQuantity = 100m, Rate = 10m,
                        Schedules = new List<CreateBlanketMasterScheduleItem>
                        {
                            new() { ScheduleNo = 1, ScheduleDate = now.AddDays(30), ScheduleQuantity = 50m }
                        }
                    }
                }
            };
        }

        private void SetupValid()
        {
            _mockQueryRepo
                .Setup(r => r.HasOverlappingBlanketAsync(
                    It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(false);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupValid();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task VendorId_ZeroOrNegative_FailsValidation(int vendorId)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.VendorId = vendorId;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.VendorId);
        }

        [Fact]
        public async Task ValidityToBeforeValidityFrom_FailsValidation()
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.ValidityTo = cmd.ValidityFrom.AddDays(-1);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ValidityTo);
        }

        [Fact]
        public async Task NoDetails_FailsValidation()
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.Details = new List<CreateBlanketMasterDetailItem>();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Details);
        }

        [Fact]
        public async Task OverlappingBlanket_FailsValidation()
        {
            _mockQueryRepo
                .Setup(r => r.HasOverlappingBlanketAsync(
                    It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(true);

            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
