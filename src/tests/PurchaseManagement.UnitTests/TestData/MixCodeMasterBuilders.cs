using PurchaseManagement.Application.MixCodeMaster.Commands.CreateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Commands.UpdateMixCodeMaster;
using PurchaseManagement.Application.MixCodeMaster.Dto;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.TestData
{
    public static class MixCodeMasterBuilders
    {
        public static CreateMixCodeMasterCommand ValidCreateCommand(
            string code = "MIX001",
            string desc = "Test Mix") =>
            new() { MixCode = code, MixCodeDesc = desc };

        public static UpdateMixCodeMasterCommand ValidUpdateCommand(
            int id = 1,
            string desc = "Updated Mix",
            int isActive = 1) =>
            new() { Id = id, MixCodeDesc = desc, IsActive = isActive };

        public static MixCodeMasterDto ValidDto(
            int id = 1,
            string code = "MIX001",
            string desc = "Test Mix",
            int isActive = 1) =>
            new() { Id = id, MixCode = code, MixCodeDesc = desc, IsActive = isActive };

        public static MixCodeMasterLookupDto ValidLookup(int id = 1) =>
            new() { Id = id, MixCode = "MIX001", MixCodeDesc = "Test Mix" };

        public static PurchaseManagement.Domain.Entities.MixCodeMaster ValidEntity(int id = 1) =>
            new()
            {
                Id = id,
                MixCode = "MIX001",
                MixCodeDesc = "Test Mix",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
