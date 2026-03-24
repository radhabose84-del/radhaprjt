using AutoMapper;
using PurchaseManagement.Application.Common.Mappings;
using PurchaseManagement.UnitTests.TestData;
using static PurchaseManagement.Domain.Common.BaseEntity;

namespace PurchaseManagement.UnitTests.Mappings
{
    public sealed class TnCTemplateMasterProfileTests
    {
        private readonly IMapper _mapper;

        public TnCTemplateMasterProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<TnCTemplateMasterProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void Configuration_IsValid()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<TnCTemplateMasterProfile>());
            config.AssertConfigurationIsValid();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = TnCTemplateMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = TnCTemplateMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateCommand_To_Entity_TemplateCode_IsNull()
        {
            var cmd = TnCTemplateMasterBuilders.ValidCreateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(cmd);

            entity.TemplateCode.Should().BeNull();
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = TnCTemplateMasterBuilders.ValidUpdateCommand(isActive: 1);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = TnCTemplateMasterBuilders.ValidUpdateCommand(isActive: 0);

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_TemplateCode_IsIgnored()
        {
            var cmd = TnCTemplateMasterBuilders.ValidUpdateCommand();

            var entity = _mapper.Map<PurchaseManagement.Domain.Entities.TnCTemplateMaster>(cmd);

            entity.TemplateCode.Should().BeNull();
        }
    }
}
