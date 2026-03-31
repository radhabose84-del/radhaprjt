using AutoMapper;
using UserManagement.Application.Divisions.Commands.CreateDivision;
using UserManagement.Application.Divisions.Commands.UpdateDivision;
using UserManagement.Application.Divisions.Commands.DeleteDivision;
using UserManagement.Application.Common.Mappings;
using UserManagement.Domain.Entities;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class DivisionProfileTests
    {
        private readonly IMapper _mapper;

        public DivisionProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<DivisionProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateDivisionCommand_MapsTo_Division_WithActiveAndNotDeleted()
        {
            var command = new CreateDivisionCommand
            {
                ShortName = "DIV01",
                Name = "Test Division",
                CompanyId = 1
            };

            var entity = _mapper.Map<Division>(command);

            entity.ShortName.Should().Be("DIV01");
            entity.Name.Should().Be("Test Division");
            entity.CompanyId.Should().Be(1);
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateDivisionCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateDivisionCommand
            {
                Id = 1,
                ShortName = "DIV01",
                Name = "Updated",
                CompanyId = 1,
                IsActive = 1
            };

            var entity = _mapper.Map<Division>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateDivisionCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateDivisionCommand
            {
                Id = 1,
                ShortName = "DIV01",
                Name = "Updated",
                CompanyId = 1,
                IsActive = 0
            };

            var entity = _mapper.Map<Division>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteDivisionCommand_MapsTo_Division_WithDeleted()
        {
            var command = new DeleteDivisionCommand { Id = 1 };

            var entity = _mapper.Map<Division>(command);

            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }
    }
}
