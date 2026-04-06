using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry;
using SalesManagement.Presentation.Validation.SalesEnquiry;
using SalesManagement.UnitTests.TestHelpers;
using static SalesManagement.Application.SalesEnquiry.Commands.UpdateSalesEnquiry.UpdateSalesEnquiryCommand;

namespace SalesManagement.UnitTests.Validators.SalesEnquiry
{
    public sealed class UpdateSalesEnquiryCommandValidatorTests
    {
        private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private UpdateSalesEnquiryCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static UpdateSalesEnquiryDetailDto ValidDetail() => new()
        {
            ItemId = 1,
            Quantity = 10
        };

        private static UpdateSalesEnquiryCommand ValidCommand() => new()
        {
            Id = 1,
            PartyId = 1,
            EnquiryDate = DateTimeOffset.UtcNow,
            IsActive = 1,
            SalesEnquiryDetails = new List<UpdateSalesEnquiryDetailDto> { ValidDetail() }
        };

        private void SetupAllAsyncMocks(int id = 1, int partyId = 1, int itemId = 1)
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(id)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(partyId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(itemId)).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Id_ZeroOrNegative_FailsValidation(int id)
        {
            var cmd = ValidCommand();
            cmd.Id = id;
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Fact]
        public async Task NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.Id);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task PartyId_ZeroOrNegative_FailsValidation(int partyId)
        {
            var cmd = ValidCommand();
            cmd.PartyId = partyId;
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EmptyDetails_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails = new List<UpdateSalesEnquiryDetailDto>();
            _mockQueryRepo.Setup(r => r.NotFoundAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Theory]
        [InlineData(2)]
        [InlineData(-1)]
        public async Task IsActive_InvalidValue_FailsValidation(int isActive)
        {
            var cmd = ValidCommand();
            cmd.IsActive = isActive;
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }
    }
}
