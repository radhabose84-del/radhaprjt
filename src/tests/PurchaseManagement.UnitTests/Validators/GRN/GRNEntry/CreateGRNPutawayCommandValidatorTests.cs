using FluentValidation.TestHelper;
using PurchaseManagement.Application.GRN.GRNEntry.Commands.CreateGRNPutaway;
using PurchaseManagement.Presentation.Validation.GRN.GRNEntry;

namespace PurchaseManagement.UnitTests.Validators.GRN.GRNEntry
{
    public sealed class CreateGRNPutawayCommandValidatorTests
    {
        private CreateGRNPutawayCommandValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_NullPutawayList_FailsValidation()
        {
            var command = new CreateGRNPutawayCommand { PutawayList = null! };

            var result = await CreateValidator().TestValidateAsync(command);

            result.ShouldHaveValidationErrorFor(x => x.PutawayList);
        }

        [Fact]
        public async Task Validate_EmptyPutawayList_FailsValidation()
        {
            var command = new CreateGRNPutawayCommand { PutawayList = new List<CreateGRNPutawayDto>() };

            var result = await CreateValidator().TestValidateAsync(command);

            result.Errors.Should().NotBeEmpty();
        }
    }
}
