using FAM.Application.AssetMaster.AssetSpecification.Commands.CreateAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Commands.DeleteAssetSpecification;
using FAM.Application.AssetMaster.AssetSpecification.Queries.GetAssetSpecification;
using FAM.Domain.Entities.AssetMaster;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.TestData
{
    public static class AssetSpecificationBuilders
    {
        public static CreateAssetSpecificationCommand ValidCreateCommand(int assetId = 1) =>
            new CreateAssetSpecificationCommand
            {
                AssetId = assetId,
                Specifications = new List<SpecificationItem>
                {
                    new SpecificationItem
                    {
                        SpecificationId = 10,
                        SpecificationName = "Weight",
                        SpecificationValue = "100kg"
                    }
                }
            };

        public static DeleteAssetSpecificationCommand ValidDeleteCommand(int id = 1) =>
            new DeleteAssetSpecificationCommand { Id = id };

        public static AssetSpecificationJsonDto ValidJsonDto(int assetId = 1) =>
            new AssetSpecificationJsonDto
            {
                AssetId = assetId,
                AssetCode = "AST001",
                AssetName = "Test Asset",
                Specifications = new List<SpecificationDto>
                {
                    new SpecificationDto { SpecificationId = 10, SpecificationName = "Weight", SpecificationValue = "100kg" }
                }
            };

        public static AssetSpecificationDTO ValidSpecificationDto(int id = 1) =>
            new AssetSpecificationDTO
            {
                Id = id,
                AssetId = 1,
                SpecificationId = 10,
                SpecificationValue = "100kg"
            };

        public static AssetSpecifications ValidEntity(int id = 1) =>
            new AssetSpecifications
            {
                Id = id,
                AssetId = 1,
                SpecificationId = 10,
                SpecificationValue = "100kg",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };
    }
}
