using FAM.Application.SpecificationMaster.Commands.CreateSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.DeleteSpecificationMaster;
using FAM.Application.SpecificationMaster.Commands.UpdateSpecificationMaster;
using FAM.Application.SpecificationMaster.Queries.GetSpecificationMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class SpecificationMasterBuilders
    {
        public static CreateSpecificationMasterCommand ValidCreateCommand(
            string specificationName = "TestSpec",
            int assetGroupId = 1,
            byte isDefault = 0) =>
            new CreateSpecificationMasterCommand
            {
                SpecificationName = specificationName,
                AssetGroupId = assetGroupId,
                ISDefault = isDefault
            };

        public static UpdateSpecificationMasterCommand ValidUpdateCommand(
            int id = 1,
            string specificationName = "UpdatedSpec",
            int assetGroupId = 1,
            byte isActive = 1) =>
            new UpdateSpecificationMasterCommand
            {
                Id = id,
                SpecificationName = specificationName,
                AssetGroupId = assetGroupId,
                IsDefault = 0,
                IsActive = isActive
            };

        public static DeleteSpecificationMasterCommand ValidDeleteCommand(int id = 1) =>
            new DeleteSpecificationMasterCommand { Id = id };

        public static SpecificationMasterDTO ValidDto(int id = 1) =>
            new SpecificationMasterDTO
            {
                Id = id,
                SpecificationName = "TestSpec",
                AssetGroupId = 1,
                ISDefault = 0,
                IsActive = Status.Active,
                CreatedDate = DateTimeOffset.UtcNow
            };

        public static SpecificationMasterAutoCompleteDTO ValidAutoCompleteDto(int id = 1) =>
            new SpecificationMasterAutoCompleteDTO
            {
                Id = id,
                SpecificationName = "TestSpec",
                ISDefault = 0
            };

        public static FAM.Domain.Entities.SpecificationMasters ValidEntity(int id = 1) =>
            new FAM.Domain.Entities.SpecificationMasters
            {
                Id = id,
                SpecificationName = "TestSpec",
                AssetGroupId = 1,
                ISDefault = 0,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
