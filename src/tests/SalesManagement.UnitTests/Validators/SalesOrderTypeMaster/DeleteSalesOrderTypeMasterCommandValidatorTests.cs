using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.DeleteSalesOrderTypeMaster;
using SalesManagement.Presentation.Validation.SalesOrderTypeMaster;

namespace SalesManagement.UnitTests.Validators.SalesOrderTypeMaster
{
    public sealed class DeleteSalesOrderTypeMasterCommandValidatorTests
    {
        private readonly Mock<ISalesOrderTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private DeleteSalesOrderTypeMasterCommandValidator CreateValidator()
            => new(_mockQueryRepo.Object);

        [Fact]
        public async Task ValidId_PassesValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesOrderTypeMasterCommand(1));
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task ZeroId_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(0)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesOrderTypeMasterCommand(0));
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(new DeleteSalesOrderTypeMasterCommand(1));
            result.ShouldHaveAnyValidationError();
        }
    }
}
