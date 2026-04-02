using FluentValidation.TestHelper;
using InventoryManagement.Application.Common.Interfaces.Item.Templates;
using InventoryManagement.Application.Item.ItemDetail.Queries.GetAllItems;
using InventoryManagement.Presentation.Validation.Item.ItemDetail;

namespace InventoryManagement.UnitTests.Validators.Item.ItemDetail
{
    public sealed class ItemQualityDtoValidatorTests
    {
        private readonly Mock<ITemplateQueryRepository> _mockTemplates = new(MockBehavior.Loose);

        public ItemQualityDtoValidatorTests()
        {
            _mockTemplates
                .Setup(r => r.ExistsByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
        }

        private ItemQualityDtoValidator CreateValidator() => new(_mockTemplates.Object);

        [Fact]
        public async Task Validate_EmptyDto_PassesValidation()
        {
            var dto = new ItemQualityDto();
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_ValidDtoWithTemplate_PassesValidation()
        {
            var dto = new ItemQualityDto
            {
                InspectionRequired = true,
                InspectionTemplateId = 1
            };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_InspectionRequiredWithoutTemplate_FailsValidation()
        {
            var dto = new ItemQualityDto { InspectionRequired = true, InspectionTemplateId = null };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_InvalidTemplateId_FailsValidation()
        {
            _mockTemplates
                .Setup(r => r.ExistsByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var dto = new ItemQualityDto { InspectionTemplateId = 99 };
            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ZeroCertificateTypeId_FailsValidation()
        {
            var dto = new ItemQualityDto { CertificateTypeId = 0 };
            var result = await CreateValidator().TestValidateAsync(dto);
            result.Errors.Should().NotBeEmpty();
        }
    }
}
