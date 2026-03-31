using AutoMapper;
using FAM.Application.AssetSubCategories.Command.CreateAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.DeleteAssetSubCategories;
using FAM.Application.AssetSubCategories.Command.UpdateAssetSubCategories;
using FAM.Application.Common.Mappings;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetSubCategoriesProfileTests
    {
        private readonly IMapper _mapper;

        public AssetSubCategoriesProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetSubCategoriesProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateAssetSubCategoriesCommand
            {
                SubCategoryName = "TestSubCat",
                AssetCategoriesId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubCategories>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateAssetSubCategoriesCommand { SubCategoryName = "TestSubCat" };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubCategories>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateAssetSubCategoriesCommand { Id = 1, SubCategoryName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubCategories>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateAssetSubCategoriesCommand { Id = 1, SubCategoryName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubCategories>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteAssetSubCategoriesCommand { Id = 5 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetSubCategories>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
