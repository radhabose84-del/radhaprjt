using Contracts.Common;
using LogisticsManagement.Application.FreightMaster.Commands.CreateFreightMaster;
using LogisticsManagement.Application.FreightMaster.Commands.UpdateFreightMaster;
using LogisticsManagement.Application.FreightMaster.Dto;
using LogisticsManagement.Domain.Common;

namespace LogisticsManagement.UnitTests.TestData
{
    public static class FreightMasterBuilders
    {
        public static CreateFreightMasterCommand ValidCreateCommand(
            int freightModeId = 1,
            int rateMethodId = 2,
            decimal rate = 100.50m,
            int moduleId = 1) =>
            new CreateFreightMasterCommand
            {
                FreightModeId = freightModeId,
                RateMethodId = rateMethodId,
                Rate = rate,
                ModuleId = moduleId
            };

        public static UpdateFreightMasterCommand ValidUpdateCommand(
            int id = 1,
            int freightModeId = 1,
            int rateMethodId = 2,
            decimal rate = 200.75m,
            int moduleId = 1,
            int isActive = 1) =>
            new UpdateFreightMasterCommand
            {
                Id = id,
                FreightModeId = freightModeId,
                RateMethodId = rateMethodId,
                Rate = rate,
                ModuleId = moduleId,
                IsActive = isActive
            };

        public static FreightMasterDto ValidDto(
            int id = 1,
            int freightModeId = 1,
            string? freightModeName = "INNER",
            int rateMethodId = 2,
            string? rateMethodName = "PER_KG",
            decimal rate = 100.50m,
            int moduleId = 1) =>
            new FreightMasterDto
            {
                Id = id,
                FreightModeId = freightModeId,
                FreightModeName = freightModeName,
                RateMethodId = rateMethodId,
                RateMethodName = rateMethodName,
                Rate = rate,
                ModuleId = moduleId,
                IsActive = true,
                IsDeleted = false
            };

        public static FreightMasterLookupDto ValidLookupDto(
            int id = 1,
            string? freightModeName = "INNER",
            string? rateMethodName = "PER_KG",
            decimal rate = 100.50m) =>
            new FreightMasterLookupDto
            {
                Id = id,
                FreightModeName = freightModeName,
                RateMethodName = rateMethodName,
                Rate = rate
            };

        public static global::LogisticsManagement.Domain.Entities.FreightMaster ValidEntity(int id = 1) =>
            new global::LogisticsManagement.Domain.Entities.FreightMaster
            {
                Id = id,
                FreightModeId = 1,
                RateMethodId = 2,
                Rate = 100.50m,
                ModuleId = 1,
                IsActive = BaseEntity.Status.Active,
                IsDeleted = BaseEntity.IsDelete.NotDeleted
            };
    }
}
