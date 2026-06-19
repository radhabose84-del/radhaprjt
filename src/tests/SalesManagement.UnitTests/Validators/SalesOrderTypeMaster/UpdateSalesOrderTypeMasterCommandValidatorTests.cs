using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.UpdateSalesOrderTypeMaster;
using SalesManagement.Presentation.Validation.SalesOrderTypeMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrderTypeMaster
{
    public sealed class UpdateSalesOrderTypeMasterCommandValidatorTests
    {
        private readonly Mock<ISalesOrderTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private UpdateSalesOrderTypeMasterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockCurrencyLookup.Object);

        private static UpdateSalesOrderTypeMasterCommand ValidCommand() => new()
        {
            Id = 1,
            TypeName = "Updated Sales Order Type",
            IsActive = 1
        };

        private void SetupValid()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetSoTypeIdByRowIdAsync(It.IsAny<int>())).ReturnsAsync((int?)5);
            _mockQueryRepo.Setup(r => r.GetSoTypeCodeAsync(5)).ReturnsAsync("SO_NORMAL");
            _mockCurrencyLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<CurrencyLookupDto>());
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
            SetupValid();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task TypeName_Empty_FailsValidation(string? name)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.TypeName = name;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.TypeName);
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
