using FluentValidation.TestHelper;
using MaintenanceManagement.Application.ServiceHistory.Queries.GetServiceHistory;
using MaintenanceManagement.Presentation.Validation.ServiceHistory;

namespace MaintenanceManagement.UnitTests.Validators.ServiceHistory
{
    public sealed class GetServiceHistoryQueryValidatorTests
    {
        private GetServiceHistoryQueryValidator CreateValidator() => new();

        [Fact]
        public async Task Validate_WithMachineId_Passes()
        {
            var query = new GetServiceHistoryQuery { MachineId = 5, PageNumber = 1, PageSize = 10 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_WithAssetId_Passes()
        {
            var query = new GetServiceHistoryQuery { AssetId = 9, PageNumber = 1, PageSize = 10 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public async Task Validate_NoMachineOrAsset_Fails()
        {
            var query = new GetServiceHistoryQuery { PageNumber = 1, PageSize = 10 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("Either MachineId or AssetId is required.");
        }

        [Fact]
        public async Task Validate_ZeroIds_Fails()
        {
            var query = new GetServiceHistoryQuery { MachineId = 0, AssetId = 0, PageNumber = 1, PageSize = 10 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("Either MachineId or AssetId is required.");
        }

        [Fact]
        public async Task Validate_InvalidPageNumber_Fails()
        {
            var query = new GetServiceHistoryQuery { MachineId = 5, PageNumber = 0, PageSize = 10 };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x.PageNumber);
        }

        [Fact]
        public async Task Validate_FromDateAfterToDate_Fails()
        {
            var query = new GetServiceHistoryQuery
            {
                MachineId = 5,
                FromDate = new DateTimeOffset(2026, 5, 10, 0, 0, 0, TimeSpan.Zero),
                ToDate = new DateTimeOffset(2026, 5, 1, 0, 0, 0, TimeSpan.Zero),
                PageNumber = 1,
                PageSize = 10
            };

            var result = await CreateValidator().TestValidateAsync(query);

            result.ShouldHaveValidationErrorFor(x => x)
                  .WithErrorMessage("FromDate must be less than or equal to ToDate.");
        }
    }
}
