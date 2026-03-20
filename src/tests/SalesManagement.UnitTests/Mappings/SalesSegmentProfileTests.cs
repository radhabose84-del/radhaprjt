using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.SalesSegment.Commands.CreateSalesSegment;
using SalesManagement.Application.SalesSegment.Commands.UpdateSalesSegment;
using SalesManagement.Application.SalesSegment.Dto;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class SalesSegmentProfileTests
    {
        private readonly IMapper _mapper;

        public SalesSegmentProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SalesSegmentProfile>());
            _mapper = config.CreateMapper();
        }

        // Note: AssertConfigurationIsValid will fail because Entity → SalesSegmentDto has unmapped
        // cross-module lookup fields (SalesOrganisationName, SalesChannelName, BusinessUnitName, CurrencyName)
        // and Entity → SalesSegmentLookupDto has unmapped navigation properties.
        // This is by design — those fields are populated in the query repository, not by AutoMapper.

        // --- CreateCommand → Entity ---

        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateSalesSegmentCommand
            {
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3,
                CurrencyId = 4,
                ValidFrom = new DateTime(2026, 1, 1),
                SegmentName = "Test Segment"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesSegment>(command);

            entity.SalesOrganisationId.Should().Be(1);
            entity.SalesChannelId.Should().Be(2);
            entity.BusinessUnitId.Should().Be(3);
            entity.CurrencyId.Should().Be(4);
            entity.SegmentName.Should().Be("Test Segment");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateSalesSegmentCommand
            {
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesSegment>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateSalesSegmentCommand
            {
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesSegment>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        // --- UpdateCommand → Entity ---

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateSalesSegmentCommand { Id = 1, IsActive = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesSegment>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateSalesSegmentCommand { Id = 1, IsActive = 0 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesSegment>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateSalesSegmentCommand
            {
                Id = 5,
                CurrencyId = 10,
                ValidFrom = new DateTime(2026, 6, 1),
                SegmentName = "Updated Segment",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesSegment>(command);

            entity.Id.Should().Be(5);
            entity.CurrencyId.Should().Be(10);
            entity.SegmentName.Should().Be("Updated Segment");
        }

        // --- Entity → DTO (boolean conversion) ---

        [Fact]
        public void Entity_To_Dto_IsActive_Active_MapsToTrue()
        {
            var entity = new SalesManagement.Domain.Entities.SalesSegment
            {
                Id = 1,
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<SalesSegmentDto>(entity);

            dto.IsActive.Should().BeTrue();
        }

        [Fact]
        public void Entity_To_Dto_IsActive_Inactive_MapsToFalse()
        {
            var entity = new SalesManagement.Domain.Entities.SalesSegment
            {
                Id = 1,
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3,
                IsActive = Status.Inactive,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<SalesSegmentDto>(entity);

            dto.IsActive.Should().BeFalse();
        }

        [Fact]
        public void Entity_To_Dto_IsDeleted_Deleted_MapsToTrue()
        {
            var entity = new SalesManagement.Domain.Entities.SalesSegment
            {
                Id = 1,
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.Deleted
            };

            var dto = _mapper.Map<SalesSegmentDto>(entity);

            dto.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public void Entity_To_Dto_IsDeleted_NotDeleted_MapsToFalse()
        {
            var entity = new SalesManagement.Domain.Entities.SalesSegment
            {
                Id = 1,
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3,
                IsActive = Status.Active,
                IsDeleted = IsDelete.NotDeleted
            };

            var dto = _mapper.Map<SalesSegmentDto>(entity);

            dto.IsDeleted.Should().BeFalse();
        }

        // --- Entity → LookupDto ---

        [Fact]
        public void Entity_To_LookupDto_MapsFields()
        {
            var entity = new SalesManagement.Domain.Entities.SalesSegment
            {
                Id = 2,
                SegmentName = "Segment Two",
                SalesOrganisationId = 1,
                SalesChannelId = 2,
                BusinessUnitId = 3
            };

            var dto = _mapper.Map<SalesSegmentLookupDto>(entity);

            dto.Id.Should().Be(2);
            dto.SegmentName.Should().Be("Segment Two");
            dto.SalesOrganisationId.Should().Be(1);
            dto.SalesChannelId.Should().Be(2);
            dto.BusinessUnitId.Should().Be(3);
        }
    }
}
