using FluentValidation.TestHelper;
using ProductionManagement.Application.Common.Interfaces.IProductionPack;
using ProductionManagement.Application.ProductionPack.Commands.CreateProduction;
using ProductionManagement.Application.ProductionPack.Dto;
using ProductionManagement.Presentation.Validation.Production;
using ProductionManagement.UnitTests.TestHelpers;

namespace ProductionManagement.UnitTests.Validators.Production
{
    public sealed class CreateProductionCommandValidatorTests
    {
        private readonly Mock<IProductionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateProductionCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateProductionDto ValidDto() => new()
        {
            WarehouseId = 1,
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
            var cmd = new CreateProductionCommand { ProductionPackDetails = null };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesRequiredChecks()
        {
            _mockQueryRepo.Setup(r => r.WarehouseExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.LotExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackTypeExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.BinExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.QualityStatusExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PackOverlapExistsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int?>())).ReturnsAsync(false);

            var cmd = new CreateProductionCommand
            {
                ProductionPackDetails = ValidDto()
            };
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
