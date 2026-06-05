using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Production;
using Contracts.Dtos.Lookups.QC;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Production;
using Contracts.Interfaces.Lookups.QC;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IOCREntry;
using PurchaseManagement.Application.OCREntry.Commands.CreateOCREntry;
using PurchaseManagement.Application.OCREntry.Dto;
using PurchaseManagement.Presentation.Validation.OCREntry;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Validators.OCREntry
{
    public sealed class CreateOCREntryCommandValidatorTests
    {
        private readonly Mock<IOCREntryQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<ISupplierLookup> _mockSupplier = new(MockBehavior.Loose);
        private readonly Mock<ILocationMasterLookup> _mockLocation = new(MockBehavior.Loose);
        private readonly Mock<IStationLookup> _mockStation = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItem = new(MockBehavior.Loose);
        private readonly Mock<ICountMasterLookup> _mockCount = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUom = new(MockBehavior.Loose);
        private readonly Mock<IQualityTemplateLookup> _mockQualityTemplate = new(MockBehavior.Loose);

        private CreateOCREntryCommandValidator CreateValidator() =>
            new(_mockQueryRepo.Object, _mockSupplier.Object, _mockLocation.Object,
                _mockStation.Object, _mockItem.Object, _mockCount.Object, _mockUom.Object, _mockQualityTemplate.Object);

        private void SetupAllValid()
        {
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PaymentTermExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockSupplier.Setup(s => s.GetActiveSupplierByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SupplierLookupDto { Id = 10, VendorName = "Ginner" });
            _mockLocation.Setup(l => l.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new LocationMasterLookupDto { Id = 11, LocationName = "CBE" });
            _mockStation.Setup(s => s.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new StationLookupDto { Id = 12, StationName = "CBE Station" });
            _mockItem.Setup(i => i.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ItemLookupDto> { new() { Id = 13, ItemName = "Cotton" } });
            _mockCount.Setup(c => c.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CountMasterLookupDto> { new() { Id = 14, CountDescription = "30s" } });
            _mockUom.Setup(u => u.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UOMLookupDto> { new() { Id = 22, Code = "CANDY", UOMName = "Candy" } });
            _mockQualityTemplate.Setup(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityTemplateLookupDto> { new() { Id = 20, TemplateName = "Cotton Passing" } });
            _mockQualityTemplate.Setup(q => q.GetParametersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityTemplateParameterLookupDto> { new() { QualityParameterId = 30, ParameterName = "Staple" } });
        }

        [Fact]
        public async Task Validate_ValidCommand_PassesValidation()
        {
            SetupAllValid();
            var result = await CreateValidator().TestValidateAsync(OCREntryBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ZeroQuantity_FailsValidation()
        {
            SetupAllValid();
            var command = OCREntryBuilders.ValidCreateCommand();
            command.Quantity = 0;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        [Fact]
        public async Task Validate_InvalidSupplier_FailsValidation()
        {
            SetupAllValid();
            _mockSupplier.Setup(s => s.GetActiveSupplierByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((SupplierLookupDto?)null);

            var result = await CreateValidator().TestValidateAsync(OCREntryBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.SupplierId);
        }

        [Fact]
        public async Task Validate_ZeroRate_FailsValidation()
        {
            SetupAllValid();
            var command = OCREntryBuilders.ValidCreateCommand();
            command.Rate = 0;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.Rate);
        }

        [Fact]
        public async Task Validate_WithValidTemplateAndParameters_PassesValidation()
        {
            SetupAllValid();
            var command = OCREntryBuilders.ValidCreateCommand();
            command.QualityTemplateId = 20;
            command.QualityParameters = new List<OCRQualityParameterInputDto>
            {
                new() { ParamId = 30, Value = "29.50+ MM" }
            };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InvalidModeOfTransport_FailsValidation()
        {
            SetupAllValid();
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(false);

            var command = OCREntryBuilders.ValidCreateCommand();
            command.ModeOfTransportId = 999;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.ModeOfTransportId);
        }

        [Fact]
        public async Task Validate_InvalidQualityTemplate_FailsValidation()
        {
            SetupAllValid();
            _mockQualityTemplate.Setup(q => q.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityTemplateLookupDto>());

            var command = OCREntryBuilders.ValidCreateCommand();
            command.QualityTemplateId = 999;

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor(x => x.QualityTemplateId);
        }

        [Fact]
        public async Task Validate_InvalidParamId_FailsValidation()
        {
            SetupAllValid();
            _mockQualityTemplate.Setup(q => q.GetParametersByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<QualityTemplateParameterLookupDto>());

            var command = OCREntryBuilders.ValidCreateCommand();
            command.QualityTemplateId = 20;
            command.QualityParameters = new List<OCRQualityParameterInputDto>
            {
                new() { ParamId = 999, Value = "x" }
            };

            var result = await CreateValidator().TestValidateAsync(command);
            result.ShouldHaveValidationErrorFor("QualityParameters[0].ParamId");
        }
    }
}
