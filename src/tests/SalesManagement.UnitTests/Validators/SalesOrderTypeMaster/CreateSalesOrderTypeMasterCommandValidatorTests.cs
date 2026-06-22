using Contracts.Dtos.Lookups.Finance;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Users;
using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesOrderTypeMaster;
using SalesManagement.Application.SalesOrderTypeMaster.Commands.CreateSalesOrderTypeMaster;
using SalesManagement.Presentation.Validation.SalesOrderTypeMaster;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesOrderTypeMaster
{
    public sealed class CreateSalesOrderTypeMasterCommandValidatorTests
    {
        private readonly Mock<ISalesOrderTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<ITransactionTypeLookup> _mockTxLookup = new(MockBehavior.Loose);
        private readonly Mock<ICurrencyLookup> _mockCurrencyLookup = new(MockBehavior.Loose);

        private CreateSalesOrderTypeMasterCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockTxLookup.Object, _mockCurrencyLookup.Object);

        private static CreateSalesOrderTypeMasterCommand ValidCommand() => new()
        {
            SoTypeId = 1,
            TaxTypeId = 2,
            TypeName = "Normal Sales Order"
        };

        private void SetupValid(int soTypeId = 1, int taxTypeId = 2)
        {
            _mockQueryRepo.Setup(r => r.IsValidSoTypeAsync(soTypeId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(soTypeId, taxTypeId, It.IsAny<int?>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.GetSoTypeCodeAsync(soTypeId)).ReturnsAsync("SO_NORMAL");

            _mockTxLookup.Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new List<TransactionTypeLookupDto> { new() { Id = taxTypeId } });
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
        public async Task SoTypeId_ZeroOrNegative_FailsValidation(int soTypeId)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.SoTypeId = soTypeId;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.SoTypeId);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task TaxTypeId_ZeroOrNegative_FailsValidation(int taxTypeId)
        {
            SetupValid();
            var cmd = ValidCommand();
            cmd.TaxTypeId = taxTypeId;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.TaxTypeId);
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

        [Fact]
        public async Task InvalidSoType_FailsValidation()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.IsValidSoTypeAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.SoTypeId);
        }

        [Fact]
        public async Task DuplicateCombination_FailsValidation()
        {
            SetupValid();
            _mockQueryRepo.Setup(r => r.AlreadyExistsAsync(1, 2, It.IsAny<int?>())).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }
    }
}
