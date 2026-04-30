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
        private readonly Mock<IMarketingOfficerAccessFilter> _mockAccessFilter = new(MockBehavior.Loose);

        private CreateSalesEnquiryCommandValidator CreateValidator()
            => new(TestMaxLengthProviderFactory.Create(), _mockQueryRepo.Object, _mockAccessFilter.Object);

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
                EnquiryTypeId = 245,
                SalesEnquiryDetails = new List<CreateSalesEnquiryDetailDto> { ValidDetail() }
            }
        };

        private void SetupAllAsyncMocks(int partyId = 1, int itemId = 1, int enquiryTypeId = 245)
        {
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(partyId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(itemId)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(enquiryTypeId)).ReturnsAsync(true);
            _mockAccessFilter.Setup(f => f.CanAccessCustomerAsync(partyId, It.IsAny<CancellationToken>())).ReturnsAsync(true);
        }

        [Fact]
        public async Task ValidCommand_PassesValidation()
        {
            SetupAllAsyncMocks();
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task PartyId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.PartyId = 0;
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EmptyDetails_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.SalesEnquiryDetails = new List<CreateSalesEnquiryDetailDto>();
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task NullDetails_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.SalesEnquiryDetails = null;
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task PartyNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task ItemNotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(false);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task DetailItemId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.SalesEnquiryDetails![0].ItemId = 0;
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveAnyValidationError();
        }

        [Fact]
        public async Task EnquiryTypeId_Zero_FailsValidation()
        {
            var cmd = ValidCommand();
            cmd.SalesEnquiryDetails.EnquiryTypeId = 0;
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            var result = await CreateValidator().TestValidateAsync(cmd);
            result.ShouldHaveValidationErrorFor(x => x.SalesEnquiryDetails.EnquiryTypeId);
        }

        [Fact]
        public async Task EnquiryTypeId_NotFound_FailsValidation()
        {
            _mockQueryRepo.Setup(r => r.PartyExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.ItemExistsAsync(1)).ReturnsAsync(true);
            _mockQueryRepo.Setup(r => r.EnquiryTypeExistsAsync(245)).ReturnsAsync(false);
            var result = await CreateValidator().TestValidateAsync(ValidCommand());
            result.ShouldHaveValidationErrorFor(x => x.SalesEnquiryDetails.EnquiryTypeId);
        }
    }
}
