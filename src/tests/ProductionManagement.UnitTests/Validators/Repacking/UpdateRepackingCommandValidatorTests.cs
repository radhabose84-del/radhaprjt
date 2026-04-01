using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;
using ProductionManagement.Application.Repacking.Commands.UpdateRepacking;
using ProductionManagement.Presentation.Validation.Repacking;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.Repacking
{
    public sealed class UpdateRepackingCommandValidatorTests
    {
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateRepackingCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new UpdateRepackingCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesRequiredChecks()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.OldPackHeaderExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.LotExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackDetailExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var cmd = new UpdateRepackingCommand
            {
                Id = 1,
                RepackingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TotalBags = 10,
                NetWeight = 100m,
                LooseConeKgs = 5m,
                OldPackHeaderId = 1,
                IsActive = 1,
                RepackingDetails = new List<CreateRepackingDetailDto>
                {
                    new() { OldPackDetailId = 1, LotId = 1, PackTypeId = 1, StartPackNo = 1, EndPackNo = 5, NetWeightPerPack = 10m, TotalBags = 5, NetWeight = 50m }
                }
            };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
