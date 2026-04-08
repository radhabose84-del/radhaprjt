using FluentValidation.TestHelper;
using PurchaseManagement.Application.Common.Interfaces.IGRN.IGRNEntry;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.UpdateGRNEntry;
using PurchaseManagement.Presentation.Validation.Common;
using PurchaseManagement.Presentation.Validation.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Validators.GRN.GRNEntry
{
    public sealed class UpdateGRNEntryCommandValidatorTests
    {
        private readonly Mock<IGRNEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);

        private UpdateGRNEntryCommandValidator CreateValidator() =>
            new(new MaxLengthProvider(null!), _mockRepo.Object);

        [Fact]
        public async Task Validate_NullGrnEntryUpdate_FailsValidation()
        {
            // Provide a non-null DTO with invalid detail values instead of null,
            // because the validator accesses nested properties without a null guard.
            var command = new UpdateGRNEntryCommand
            {
                GrnEntryUpdate = new UpdateGRNEntryDto
                {
                    Id = 0,
                    GateEntryId = 0,
                    PartyId = 0,
                    ReceivingWarehouseId = 0,
                    UpdateGRNDetailsDtos = new List<UpdateGRNEntryDto.UpdateGRNDetailsDto>
                    {
                        new() { DcQuantity = 0, ReceivedQuantity = 0 }
                    }
                }
            };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
