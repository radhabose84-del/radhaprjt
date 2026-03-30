using AutoMapper;
using SalesManagement.Application.BusinessUnit.Dto;
using SalesManagement.Application.BusinessUnit.Commands.CreateBusinessUnit;
using SalesManagement.Application.BusinessUnit.Commands.UpdateBusinessUnit;
using SalesManagement.Application.Common.Mappings;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class BusinessUnitProfileTests
    {
        private readonly IMapper _mapper;

        public BusinessUnitProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BusinessUnitProfile>());
            _mapper = config.CreateMapper();
        }


        // --- CreateCommand → Entity ---

        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateBusinessUnitCommand
            {
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test Unit",
                Description = "Test Desc"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.BusinessUnit>(command);

            entity.BusinessUnitCode.Should().Be("BU001");
            entity.BusinessUnitName.Should().Be("Test Unit");
            entity.Description.Should().Be("Test Desc");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateBusinessUnitCommand
            {
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test Unit"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.BusinessUnit>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateBusinessUnitCommand
            {
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test Unit"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.BusinessUnit>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        // --- UpdateCommand → Entity ---

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateBusinessUnitCommand
            {
                Id = 1,
                BusinessUnitName = "Updated",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.BusinessUnit>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateBusinessUnitCommand
            {
                Id = 1,
                BusinessUnitName = "Updated",
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.BusinessUnit>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateBusinessUnitCommand
            {
                Id = 5,
                BusinessUnitName = "Updated Name",
                Description = "Updated Desc",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.BusinessUnit>(command);

            entity.Id.Should().Be(5);
            entity.BusinessUnitName.Should().Be("Updated Name");
            entity.Description.Should().Be("Updated Desc");
        }

        // --- Entity → DTO (boolean conversion) ---

        [Fact]
        public void Entity_To_Dto_IsActive_Active_MapsToTrue()
        {
            var entity = new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = 1,
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BusinessUnitDto>(entity);

            dto.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Entity_To_Dto_IsActive_Inactive_MapsToFalse()
        {
            var entity = new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = 1,
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test",
                IsActive = Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BusinessUnitDto>(entity);

            dto.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Entity_To_Dto_IsDeleted_Deleted_MapsToTrue()
        {
            var entity = new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = 1,
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test",
                IsActive = Status.Active,
                IsDeleted = IsDelete.Deleted
            };

            var dto = _mapper.Map<BusinessUnitDto>(entity);

            dto.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Entity_To_Dto_IsDeleted_NotDeleted_MapsToFalse()
        {
            var entity = new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = 1,
                BusinessUnitCode = "BU001",
                BusinessUnitName = "Test",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<BusinessUnitDto>(entity);

            dto.IsDeleted.Should().BeFalse();
        }

        [Fact]
        public void Entity_To_Dto_MapsAllFields()
        {
            var now = DateTimeOffset.UtcNow;
            var entity = new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = 3,
                BusinessUnitCode = "BU003",
                BusinessUnitName = "Unit Three",
                Description = "Desc Three",
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted,
                CreatedBy = 1,
                CreatedDate = now,
                CreatedByName = "admin",
                CreatedIP = "127.0.0.1"
            };

            var dto = _mapper.Map<BusinessUnitDto>(entity);

            dto.Id.Should().Be(3);
            dto.BusinessUnitCode.Should().Be("BU003");
            dto.BusinessUnitName.Should().Be("Unit Three");
            dto.Description.Should().Be("Desc Three");
            dto.CreatedBy.Should().Be(1);
            dto.CreatedDate.Should().Be(now);
        }

        // --- Entity → LookupDto ---

        [Fact]
        public void Entity_To_LookupDto_MapsFields()
        {
            var entity = new SalesManagement.Domain.Entities.BusinessUnit
            {
                Id = 2,
                BusinessUnitCode = "BU002",
                BusinessUnitName = "Lookup Unit"
            };

            var dto = _mapper.Map<BusinessUnitLookupDto>(entity);

            dto.Id.Should().Be(2);
            dto.BusinessUnitCode.Should().Be("BU002");
            dto.BusinessUnitName.Should().Be("Lookup Unit");
        }
    }
}
