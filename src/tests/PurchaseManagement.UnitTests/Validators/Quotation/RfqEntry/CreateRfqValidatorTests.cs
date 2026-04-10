using FluentValidation.TestHelper;
using Contracts.Interfaces;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Create;
using PurchaseManagement.Presentation.Validation.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Validators.Quotation.RfqEntry
{
    public sealed class CreateRfqValidatorTests
    {
        private readonly Mock<IRfqQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private CreateRfqValidator CreateValidator() =>
            new(_mockRepo.Object, _mockIp.Object);

        [Fact]
        public async Task Validate_ZeroInitiationTypeId_FailsValidation()
        {
            var command = new CreateRfqCommand { InitiationTypeId = 0 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.InitiationTypeId);
        }

        [Fact]
        public async Task Validate_ValidInitiationTypeId_PassesValidation()
        {
            var command = new CreateRfqCommand { InitiationTypeId = 1 };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor(x => x.InitiationTypeId);
        }
    }
}
