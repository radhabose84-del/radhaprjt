using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Commands.UpdateProduction;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Presentation.Validation.Production;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.Production
{
    public sealed class UpdateProductionCommandValidatorTests
    {
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private UpdateProductionCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateProductionDto ValidDto(int id = 1) => new()
        {
            Id = id,
            WarehouseId = 1,
            IsActive = 1,
            ItemId = 1,
            LotId = 1,
            PackTypeId = 1,
            StartPackNo = 1,
            EndPackNo = 5,
            BinId = 1,
            QualityStatusId = 1,
            NetWeightPerPack = 10m,
            TotalBags = 5,
            TotalNetWeight = 50m,
            ProductionKgs = 48m,
            LooseConeKgs = 2m
        };

        [Fact]
        public async Task Validate_NullDetails_FailsValidation()
        {
            var cmd = new UpdateProductionCommand { ProductionPackEntries = null };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            var cmd = new UpdateProductionCommand
            {
                ProductionPackEntries = ValidDto(id: 0)
            };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesRequiredChecks()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.WarehouseExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.LotExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.BinExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.QualityStatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackOverlapExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);

            var cmd = new UpdateProductionCommand
            {
                ProductionPackEntries = ValidDto(id: 1)
            };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
