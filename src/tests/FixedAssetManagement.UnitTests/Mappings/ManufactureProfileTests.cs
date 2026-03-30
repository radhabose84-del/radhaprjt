using AutoMapper;
using FAM.Application.Common.Mappings;
using FAM.Application.Manufacture.Commands.CreateManufacture;
using FAM.Application.Manufacture.Commands.DeleteManufacture;
using FAM.Application.Manufacture.Commands.UpdateManufacture;
using static FAM.Domain.Common.BaseEntity;

namespace FixedAssetManagement.UnitTests.Mappings
{
    public sealed class ManufactureProfileTests
    {
        private readonly IMapper _mapper;

        public ManufactureProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<ManufactureProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var cmd = new CreateManufactureCommand
            {
                Code = "MFG001",
                ManufactureName = "TestManufacture",
                CountryId = 1, StateId = 1, CityId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.Manufactures>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var cmd = new CreateManufactureCommand
            {
                Code = "MFG001",
                ManufactureName = "TestManufacture",
                CountryId = 1, StateId = 1, CityId = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.Manufactures>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var cmd = new UpdateManufactureCommand
            {
                Id = 1, Code = "MFG001", ManufactureName = "TestManufacture",
                CountryId = 1, StateId = 1, CityId = 1, IsActive = 1
            };

            var entity = _mapper.Map<FAM.Domain.Entities.Manufactures>(cmd);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var cmd = new UpdateManufactureCommand
            {
                Id = 1, Code = "MFG001", ManufactureName = "TestManufacture",
                CountryId = 1, StateId = 1, CityId = 1, IsActive = 0
            };

            var entity = _mapper.Map<FAM.Domain.Entities.Manufactures>(cmd);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_To_Entity_SetsIsDeleted_Deleted()
        {
            var cmd = new DeleteManufactureCommand { Id = 1 };

            var entity = _mapper.Map<FAM.Domain.Entities.Manufactures>(cmd);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
