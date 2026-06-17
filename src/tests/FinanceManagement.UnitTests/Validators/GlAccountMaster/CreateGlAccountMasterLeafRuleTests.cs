using Contracts.Interfaces;
using FluentValidation.TestHelper;
using FinanceManagement.Application.Common.Interfaces.IGlAccountMaster;
using FinanceManagement.Application.GlAccountMaster.Commands.CreateGlAccountMaster;
using FinanceManagement.Presentation.Validation.GlAccountMaster;
using FinanceManagement.UnitTests.TestHelpers;

namespace FinanceManagement.UnitTests.Validators.GlAccountMaster
{
    // Focused on the AC2 guard: a GL account may attach only to a leaf AccountGroup.
    public sealed class CreateGlAccountMasterLeafRuleTests
    {
        private readonly Mock<IGlAccountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateGlAccountMasterCommandValidator CreateValidator()
        {
            _mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            return new CreateGlAccountMasterCommandValidator(
                TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockIp.Object);
        }

        [Fact]
        public async Task Validate_NonLeafAccountGroup_FailsWithLeafMessage()
        {
            _mockQueryRepo.Setup(r => r.AccountGroupExistsForCompanyAsync(2, 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AccountGroupIsLeafForCompanyAsync(2, 1)).ReturnsAsync(false);

            var command = new CreateGlAccountMasterCommand { AccountGroupId = 2 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.AccountGroupId)
                .WithErrorMessage("Accounts attach only at leaf level — select a leaf group.");
        }

        [Fact]
        public async Task Validate_LeafAccountGroup_NoAccountGroupError()
        {
            _mockQueryRepo.Setup(r => r.AccountGroupExistsForCompanyAsync(4, 1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.AccountGroupIsLeafForCompanyAsync(4, 1)).ReturnsAsync(true);

            var command = new CreateGlAccountMasterCommand { AccountGroupId = 4 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.AccountGroupId);
        }
    }
}
