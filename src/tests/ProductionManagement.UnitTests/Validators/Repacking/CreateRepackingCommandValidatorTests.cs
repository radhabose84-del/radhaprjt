using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IRepacking;
using ProductionManagement.Application.Repacking.Commands.CreateRepacking;
using ProductionManagement.Presentation.Validation.Repacking;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.Repacking
{
    public sealed class CreateRepackingCommandValidatorTests
    {
        private readonly Mock<IRepackingQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateRepackingCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        [Fact]
        public async Task Validate_EmptyCommand_FailsValidation()
        {
            var result = await CreateValidator().TestValidateAsync(new CreateRepackingCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesRequiredChecks()
        {
            _mockQueryRepo.Setup(r => r.OldPackHeaderExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.LotExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackDetailExistsAsync(It.IsAny<int>())).ReturnsAsync(true);

            var cmd = new CreateRepackingCommand
            {
                RepackingDate = DateOnly.FromDateTime(DateTime.UtcNow),
                TotalBags = 10,
                NetWeight = 100m,
                LooseConeKgs = 5m,
                OldPackHeaderId = 1,
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
