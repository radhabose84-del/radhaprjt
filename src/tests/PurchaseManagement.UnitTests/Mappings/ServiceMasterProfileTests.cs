using AutoMapper;
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Application.ServiceMaster.Commands.CreateService;
using PurchaseManagement.Application.ServiceMaster.Commands.DeleteService;
using PurchaseManagement.Application.ServiceMaster.Commands.UpdateService;
using PurchaseManagement.UnitTests.TestData;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class ServiceMasterProfileTests
    {
        private readonly IMapper _mapper;

        public ServiceMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<ServiceMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = ServiceMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = ServiceMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = ServiceMasterBuilders.ValidUpdateCommand(isActive: 1);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = ServiceMasterBuilders.ValidUpdateCommand(isActive: 0);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = ServiceMasterBuilders.ValidDeleteCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsActive_Inactive()
        {
            var cmd = ServiceMasterBuilders.ValidDeleteCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.ServiceMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }
    }
}
