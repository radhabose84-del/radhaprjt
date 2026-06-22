using Contracts.Interfaces;
using FinanceManagement.Application.Common.Interfaces.JournalMaster.IJournal;
using FinanceManagement.Application.JournalMaster.Dto;
using FinanceManagement.Presentation.Validation.JournalMaster.Journal;
using FinanceManagement.UnitTests.TestData;
using FinanceManagement.UnitTests.TestHelpers;
using FluentValidation.TestHelper;

namespace FinanceManagement.UnitTests.Validators.Journal
{
    public sealed class CreateJournalCommandValidatorTests
    {
        private readonly Mock<IJournalQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateJournalCommandValidator CreateValidator() =>
            new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);

        private void SetupHappyPath()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)(4, 3));
            _mockQueryRepo.Setup(r => r.VoucherTypeExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GlAccountExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.GetCostCentreMandatoryAccountIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(Array.Empty<int>());
        }

        [Fact]
        public async Task Validate_ValidCommand_Passes()
        {
            SetupHappyPath();
            var result = await CreateValidator().TestValidateAsync(JournalBuilders.ValidCreateCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NoLines_Fails()
        {
            SetupHappyPath();
            var cmd = JournalBuilders.ValidCreateCommand(lines: new List<JournalLineInputDto>());

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Lines);
        }

        [Fact]
        public async Task Validate_Unbalanced_Fails()
        {
            SetupHappyPath();
            var cmd = JournalBuilders.ValidCreateCommand(lines: new List<JournalLineInputDto>
            {
                JournalBuilders.DrLine(amount: 1000m),
                JournalBuilders.CrLine(amount: 900m)
            });

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_OnlyDebitLines_Fails()
        {
            SetupHappyPath();
            var cmd = JournalBuilders.ValidCreateCommand(lines: new List<JournalLineInputDto>
            {
                JournalBuilders.DrLine(amount: 500m),
                JournalBuilders.DrLine(glAccountId: 5200102, amount: 500m)
            });

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_ClosedPeriod_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.GetOpenPeriodByDateAsync(It.IsAny<int>(), It.IsAny<DateOnly>()))
                .ReturnsAsync(((int, int)?)null);

            var result = await CreateValidator().TestValidateAsync(JournalBuilders.ValidCreateCommand());
            result.IsValid.Should().BeFalse();
        }

        [Fact]
        public async Task Validate_NonExistentVoucherType_Fails()
        {
            SetupHappyPath();
            _mockQueryRepo.Setup(r => r.VoucherTypeExistsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(false);

            var result = await CreateValidator().TestValidateAsync(JournalBuilders.ValidCreateCommand());
            result.ShouldHaveValidationErrorFor(x => x.VoucherTypeId);
        }

        [Fact]
        public async Task Validate_PAndLLineWithoutCostCentre_Fails()
        {
            SetupHappyPath();
            // Mark the debit account (5200101) as cost-centre mandatory, and submit it without a cost centre.
            _mockQueryRepo.Setup(r => r.GetCostCentreMandatoryAccountIdsAsync(It.IsAny<IEnumerable<int>>()))
                .ReturnsAsync(new[] { 5200101 });

            var cmd = JournalBuilders.ValidCreateCommand(lines: new List<JournalLineInputDto>
            {
                JournalBuilders.DrLine(amount: 1000m, costCentreId: null),
                JournalBuilders.CrLine(amount: 1000m)
            });

            var result = await CreateValidator().TestValidateAsync(cmd);
            result.IsValid.Should().BeFalse();
        }
    }
}
