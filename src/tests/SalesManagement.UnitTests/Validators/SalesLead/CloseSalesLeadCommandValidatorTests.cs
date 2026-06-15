using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces;
using SalesManagement.Application.Common.Interfaces.ISalesLead;
using SalesManagement.Application.SalesLead.Commands.CloseSalesLead;
using SalesManagement.Presentation.Validation.SalesLead;
using SalesManagement.UnitTests.TestHelpers;

namespace SalesManagement.UnitTests.Validators.SalesLead
{
    public sealed class CloseSalesLeadCommandValidatorTests
    {
        private readonly Mock<ISalesLeadQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private CloseSalesLeadCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

        // Closure Type Id 1 = Won, 2 = non-Won (Lost)
        private void SetupCommon(int closureTypeId, bool isWon)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.IsClosedAsync(It.IsAny<int>())).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.MiscMasterExistsAsync(It.IsAny<int>())).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.IsWonClosureTypeAsync(closureTypeId)).ReturnsAsync(isWon);
        }

        private static CloseSalesLeadCommand LostCommand() => new()
        {
            Id = 1, ClosureTypeId = 2, ClosureReasonId = 5, ClosureRemarks = "Competitor selected"
        };

        private static CloseSalesLeadCommand WonCommand() => new()
        {
            Id = 1, ClosureTypeId = 1, ConvertWonLeadToId = 9, ClosureRemarks = "Confirmed"
        };

        [Fact]
        public async Task NonWon_WithReason_PassesValidation()
        {
            SetupCommon(closureTypeId: 2, isWon: false);
            var result = await CreateValidator().TestValidateAsync(LostCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Won_WithTarget_NoReason_PassesValidation()
        {
            SetupCommon(closureTypeId: 1, isWon: true);
            var result = await CreateValidator().TestValidateAsync(WonCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task EmptyRemarks_FailsValidation(string? remarks)
        {
            SetupCommon(closureTypeId: 2, isWon: false);
            var cmd = LostCommand();
            cmd.ClosureRemarks = remarks;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ClosureRemarks);
        }

        [Fact]
        public async Task MissingClosureType_FailsValidation()
        {
            SetupCommon(closureTypeId: 0, isWon: false);
            var cmd = LostCommand();
            cmd.ClosureTypeId = 0;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ClosureTypeId);
        }

        [Fact]
        public async Task NonWon_WithoutReason_FailsValidation()
        {
            SetupCommon(closureTypeId: 2, isWon: false);
            var cmd = LostCommand();
            cmd.ClosureReasonId = null;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ClosureReasonId);
        }

        [Fact]
        public async Task Won_WithoutTarget_FailsValidation()
        {
            SetupCommon(closureTypeId: 1, isWon: true);
            var cmd = WonCommand();
            cmd.ConvertWonLeadToId = null;
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.ConvertWonLeadToId);
        }

        [Fact]
        public async Task AlreadyClosed_FailsValidation()
        {
            SetupCommon(closureTypeId: 2, isWon: false);
            _mockQueryRepo.Setup(r => r.IsClosedAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(LostCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            SetupCommon(closureTypeId: 2, isWon: false);
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(LostCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }
    }
}
