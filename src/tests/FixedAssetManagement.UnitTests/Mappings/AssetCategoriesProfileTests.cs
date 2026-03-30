using AutoMapper;
using FAM.Application.AssetCategories.Command.CreateAssetCategories;
using FAM.Application.AssetCategories.Command.DeleteAssetCategories;
using FAM.Application.AssetCategories.Command.UpdateAssetCategories;
using FAM.Application.Common.Mappings;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class AssetCategoriesProfileTests
    {
        private readonly IMapper _mapper;

        public AssetCategoriesProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<AssetCategoriesProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateAssetCategoriesCommand
            {
                CategoryName = "TestCat",
                AssetGroupId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetCategories>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateAssetCategoriesCommand { CategoryName = "TestCat" };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetCategories>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateAssetCategoriesCommand { Id = 1, CategoryName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetCategories>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateAssetCategoriesCommand { Id = 1, CategoryName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetCategories>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteAssetCategoriesCommand { Id = 7 };

            var entity = _mapper.Map<FAM.Domain.Entities.AssetCategories>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
