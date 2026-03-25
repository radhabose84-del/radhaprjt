using AutoMapper;
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.Application.PaymentTermMaster.Command.CreatePaymentTermMaster;
using PurchaseManagement.Application.PaymentTermMaster.Command.UpdatePaymentTermMaster;
using PurchaseManagement.UnitTests.TestData;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class PaymentTermMasterProfileTests
    {
        private readonly IMapper _mapper;

        public PaymentTermMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<PaymentTermProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = PaymentTermMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PaymentTermMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = PaymentTermMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PaymentTermMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActiveTrue_MapsTo_StatusActive()
        {
            var cmd = PaymentTermMasterBuilders.ValidUpdateCommand(1, "PT001", "Test", isActive: true);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PaymentTermMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActiveFalse_MapsTo_StatusInactive()
        {
            var cmd = PaymentTermMasterBuilders.ValidUpdateCommand(1, "PT001", "Test", isActive: false);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.PaymentTermMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }
    }
}
