using PurchaseManagement.Application.DutyMaster;
using PurchaseManagement.Application.DutyMaster.Command.Create;
using PurchaseManagement.Application.Purchase.DutyMaster.Command.Update;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class DutyMasterBuilders
    {
        public static CreateDutyMasterCommand ValidCreateCommand(
            string tariffNumber = "1234.56.78",
            int hsnId = 1,
            int dutyCategoryId = 1) =>
            new CreateDutyMasterCommand
            {
                Model = new DutyMasterDto
                {
                    TariffNumber = tariffNumber,
                    HsnId = hsnId,
                    DutyCategoryId = dutyCategoryId,
                    BasicCustomsDutyPercentage = 10m,
                    IGSTPercentage = 18m,
                    SocialWelfareSurchargePercentage = 10m,
                    EffectiveFrom = DateTimeOffset.UtcNow,
                    CountryOfOriginApplicability = 1,
                    IsActive = 1
                }
            };

        public static UpdateDutyMasterCommand ValidUpdateCommand(int id = 1) =>
            new UpdateDutyMasterCommand
            {
                Model = new DutyMasterDto
                {
                    Id = id,
                    TariffNumber = "1234.56.78",
                    HsnId = 1,
                    DutyCategoryId = 1,
                    BasicCustomsDutyPercentage = 15m,
                    IGSTPercentage = 18m,
                    SocialWelfareSurchargePercentage = 10m,
                    EffectiveFrom = DateTimeOffset.UtcNow,
                    CountryOfOriginApplicability = 1,
                    IsActive = 1
                }
            };

        public static DutyMasterDto ValidDto(int id = 1) =>
            new DutyMasterDto
            {
                Id = id,
                TariffNumber = "1234.56.78",
                HsnId = 1,
                DutyCategoryId = 1,
                BasicCustomsDutyPercentage = 10m,
                IGSTPercentage = 18m,
                SocialWelfareSurchargePercentage = 10m,
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = 1
            };

        public static PurchaseManagement.Domain.Entities.DutyMaster ValidEntity(int id = 1) =>
            new PurchaseManagement.Domain.Entities.DutyMaster
            {
                Id = id,
                DutyCode = "DC001",
                TariffNumber = "1234.56.78",
                HsnId = 1,
                DutyCategoryId = 1,
                BasicCustomsDutyPercentage = 10m,
                IGSTPercentage = 18m,
                SocialWelfareSurchargePercentage = 10m,
                EffectiveFrom = DateTimeOffset.UtcNow,
                IsActive = PurchaseManagement.Domain.Common.BaseEntity.Status.Active,
                IsDeleted = PurchaseManagement.Domain.Common.BaseEntity.IsDelete.NotDeleted
            };
    }
}
