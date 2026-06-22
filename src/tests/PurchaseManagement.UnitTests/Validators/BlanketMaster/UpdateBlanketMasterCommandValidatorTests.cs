using FluentValidation.TestHelper;
using PurchaseManagement.Application.BlanketMaster.Commands.Update;
using PurchaseManagement.Application.Common.Interfaces.IBlanketMaster;
using PurchaseManagement.Presentation.Validation.BlanketMaster;

namespace PurchaseManagement.UnitTests.Validators.BlanketMaster
{
    public sealed class UpdateBlanketMasterCommandValidatorTests
    {
        private readonly Mock<IBlanketMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateBlanketMasterCommandValidator CreateValidator() => new(_mockQueryRepo.Object);

        private static UpdateBlanketMasterCommand ValidCommand()
        {
            var now = new DateTimeOffset(2026, 1, 15, 0, 0, 0, TimeSpan.Zero);
            return new UpdateBlanketMasterCommand
            {
                Id = 1,
                VendorId = 1,
                CurrencyId = 1,
                ProcurementTypeId = 1,
                ValidityFrom = now,
                ValidityTo = now.AddMonths(6),
                StatusId = 10,
                IsActive = 1,
                Details = new List<UpdateBlanketMasterDetailItem>
                {
                    new()
                    {
                        Id = 0, ItemSno = 1, ItemId = 10, UOMId = 1, EstimatedQuantity = 100m, Rate = 10m,
                        Schedules = new List<UpdateBlanketMasterScheduleItem>
                        {
                            new() { Id = 0, ScheduleNo = 1, ScheduleDate = now.AddDays(30), ScheduleQuantity = 50m }
                        }
                    }
                }
            };
        }

        private void SetupValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
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
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.Id = id;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockQueryRepo
                .Setup(r => r.HasOverlappingBlanketAsync(
                    It.IsAny<int>(), It.IsAny<List<int>>(), It.IsAny<DateTimeOffset>(),
                    It.IsAny<DateTimeOffset>(), It.IsAny<int?>()))
                .ReturnsAsync(false);

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
    }
}
