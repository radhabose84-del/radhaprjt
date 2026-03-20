using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.SalesChannel.Commands.CreateSalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class SalesChannelProfileTests
    {
        private readonly IMapper _mapper;

        public SalesChannelProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<SalesChannelProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateSalesChannelCommand
            {
                SalesChannelCode = "SC001",
                SalesChannelName = "Direct Sales"
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesChannel>(command);

            entity.SalesChannelCode.Should().Be("SC001");
            entity.SalesChannelName.Should().Be("Direct Sales");
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateSalesChannelCommand { SalesChannelCode = "SC001", SalesChannelName = "Test" };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesChannel>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateSalesChannelCommand { SalesChannelCode = "SC001", SalesChannelName = "Test" };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesChannel>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateSalesChannelCommand { Id = 1, SalesChannelName = "Updated", IsActive = 1 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesChannel>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateSalesChannelCommand { Id = 1, SalesChannelName = "Updated", IsActive = 0 };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesChannel>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateSalesChannelCommand
            {
                Id = 5,
                SalesChannelName = "Updated Channel",
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.SalesChannel>(command);

            entity.Id.Should().Be(5);
            entity.SalesChannelName.Should().Be("Updated Channel");
        }
    }
}
