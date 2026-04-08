using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IPoMethodLookup;
using PurchaseManagement.Application.PurchaseOrder.CombinePO;
using PurchaseManagement.Application.PurchaseOrder.CombinePO.Validators;

namespace PurchaseManagement.UnitTests.Validators.PurchaseOrder.CombinePO
{
    public sealed class CombinePODtoValidatorTests
    {
        private readonly Mock<IPoMethodLookup> _mockLookup = new(MockBehavior.Loose);

        private CreateCombinePODtoValidator CreateValidator() => new(_mockLookup.Object);

        [Fact]
        public async Task Validate_InvalidPOMethodId_FailsValidation()
        {
            _mockLookup.Setup(l => l.IsValidAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var dto = new CreateCombinePODto { POMethodId = 999 };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.ShouldHaveValidationErrorFor(x => x.POMethodId);
        }

        [Fact]
        public async Task Validate_BothPayloadsNull_FailsValidation()
        {
            _mockLookup.Setup(l => l.IsValidAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var dto = new CreateCombinePODto { POMethodId = 1, Local = null, Import = null };

            var result = await CreateValidator().TestValidateAsync(dto);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
