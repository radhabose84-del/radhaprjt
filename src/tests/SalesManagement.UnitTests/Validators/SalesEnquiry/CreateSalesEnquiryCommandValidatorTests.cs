using FluentValidation.TestHelper;
using SalesManagement.Application.Common.Interfaces.ISalesEnquiry;
using SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry;
using SalesManagement.Presentation.Validation.SalesEnquiry;
using SalesManagement.UnitTests.TestHelpers;
using static SalesManagement.Application.SalesEnquiry.Commands.CreateSalesEnquiry.CreateSalesEnquiryDto;

namespace SalesManagement.UnitTests.Validators.SalesEnquiry
{
    public sealed class CreateSalesEnquiryCommandValidatorTests
    {
        private readonly Mock<ISalesEnquiryQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private CreateSalesEnquiryCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object);

        private static CreateSalesEnquiryDetailDto ValidDetail() => new()
        {
            ItemId = 1,
            Quantity = 10
        };

        private static CreateSalesEnquiryCommand ValidCommand() => new()
        {
            SalesEnquiryDetails = new CreateSalesEnquiryDto
            {
                PartyId = 1,
                EnquiryDate = DateTimeOffset.UtcNow,
                SalesEnquiryDetails = new List<CreateSalesEnquiryDetailDto> { ValidDetail() }
            }
        };

        private void SetupAllAsyncMocks(int partyId = 1, int itemId = 1)
        {
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
        public async Task PartyId_ZeroOrNegative_FailsValidation(int partyId)
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.PartyId = partyId;
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EmptyDetails_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.SalesEnquiryDetails = new List<CreateSalesEnquiryDetailDto>();
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task NullDetails_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.SalesEnquiryDetails = null;
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task PartyNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task ItemNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task DetailItemId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.SalesEnquiryDetails![0].ItemId = 0;
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }
    }
}
