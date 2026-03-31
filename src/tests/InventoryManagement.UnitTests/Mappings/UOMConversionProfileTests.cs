using AutoMapper;
using InventoryManagement.Application.Common.Mappings;
using InventoryManagement.Application.UOMConversion.Command.CreateUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using static InventoryManagement.Domain.Common.BaseEntity;

namespace InventoryManagement.UnitTests.Mappings
{
    public sealed class UOMConversionProfileTests
    {
        private readonly IMapper _mapper;

        public UOMConversionProfileTests()
        {
            var config = new MapperConfiguration(cfg =>
                cfg.AddProfile<UOMConversionProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateUOMConversionCommand { FromUOMId = 1, ToUOMId = 2, ConversionValue = 1000m };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateUOMConversionCommand { FromUOMId = 1, ToUOMId = 2, ConversionValue = 1000m };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateUOMConversionCommand { Id = 1, FromUOMId = 1, ToUOMId = 2, ConversionValue = 500m, IsActive = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateUOMConversionCommand { Id = 1, FromUOMId = 1, ToUOMId = 2, ConversionValue = 500m, IsActive = 0 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteUOMConversionCommand { Id = 1 };

            var entity = _mapper.Map<InventoryManagement.Domain.Entities.UOMConversion>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
