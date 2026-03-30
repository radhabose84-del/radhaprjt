using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.SalesGroup.Commands.CreateSalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class SalesGroupProfileTests
    {
        private readonly IMapper _mapper;

        public SalesGroupProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SalesGroupProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateSalesGroupCommand
            {
                SalesGroupName = "Group A",
                SalesOfficeId = 1,
                ResponsibleManager = "Manager A",
                ProductCategoryId = 10,
                RegionTerritory = "North"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesGroup>(command);

            entity.SalesGroupName.Should().Be("Group A");
            entity.SalesOfficeId.Should().Be(1);
            entity.ResponsibleManager.Should().Be("Manager A");
            entity.ProductCategoryId.Should().Be(10);
            entity.RegionTerritory.Should().Be("North");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateSalesGroupCommand { SalesGroupName = "Group A", SalesOfficeId = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesGroup>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateSalesGroupCommand { SalesGroupName = "Group A", SalesOfficeId = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesGroup>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateSalesGroupCommand { Id = 1, SalesGroupName = "Updated", SalesOfficeId = 1, IsActive = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesGroup>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateSalesGroupCommand { Id = 1, SalesGroupName = "Updated", SalesOfficeId = 1, IsActive = 0 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesGroup>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateSalesGroupCommand
            {
                Id = 5,
                SalesGroupName = "Updated Group",
                SalesOfficeId = 2,
                ResponsibleManager = "Manager B",
                ProductCategoryId = 20,
                RegionTerritory = "South",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesGroup>(command);

            entity.Id.Should().Be(5);
            entity.SalesGroupName.Should().Be("Updated Group");
            entity.SalesOfficeId.Should().Be(2);
            entity.ResponsibleManager.Should().Be("Manager B");
            entity.ProductCategoryId.Should().Be(20);
            entity.RegionTerritory.Should().Be("South");
        }
    }
}
