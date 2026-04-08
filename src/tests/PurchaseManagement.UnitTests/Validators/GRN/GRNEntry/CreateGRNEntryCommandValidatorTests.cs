using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNEntry;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Validators.GRN.GRNEntry
{
    public sealed class CreateGRNEntryCommandValidatorTests
    {
        private readonly Mock<MaxLengthProvider> _mockMaxLength = new(MockBehavior.Strict, new object[] { null! });
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private CreateGRNEntryCommandValidator CreateValidator() =>
            new(_mockMaxLength.Object, _mockRepo.Object);

        [Fact]
        public async Task Validate_EmptyPartyId_FailsValidation()
        {
            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    PartyId = 0,
                    GRNDetailsDtos = new List<CreateGRNEntryDto.CreateGRNDetailsDto>()
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }

        [Fact]
        public async Task Validate_ValidCommand_WithPartyId_PassesPartyRule()
        {
            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    PartyId = 1,
                    GRNDetailsDtos = new List<CreateGRNEntryDto.CreateGRNDetailsDto>()
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldNotHaveValidationErrorFor("GrnEntryCreate.PartyId");
        }

        [Fact]
        public async Task Validate_NegativeDcQuantity_FailsValidation()
        {
            var command = new CreateGRNEntryCommand
            {
                GrnEntryCreate = new CreateGRNEntryDto
                {
                    PartyId = 1,
                    GRNDetailsDtos = new List<CreateGRNEntryDto.CreateGRNDetailsDto>
                    {
                        new() { DcQuantity = -1, ReceivedQuantity = 0 }
                    }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().Contain(e =>
                e.ErrorMessage.Contains("DC Quantity must be a positive value"));
        }
    }
}
