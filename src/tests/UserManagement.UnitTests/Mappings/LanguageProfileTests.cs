using AutoMapper;
using UserManagement.Application.Common.Mappings;
using UserManagement.Application.Language.Commands.CreateLanguage;
using UserManagement.Application.Language.Commands.DeleteLanguage;
using UserManagement.Application.Language.Commands.UpdateLanguage;
using UserManagement.Application.Language.Queries.GetLanguages;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class LanguageProfileTests
    {
        private readonly IMapper _mapper;

        public LanguageProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<LanguageProfile>());
            // Do not call AssertConfigurationIsValid() because the LanguageProfile
            // intentionally leaves audit fields (CreatedBy, CreatedAt, etc.) unmapped
            // since they are auto-populated by ApplicationDbContext.SaveChangesAsync().
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_Maps_To_Entity_With_Active_And_NotDeleted()
        {
            // Arrange
            var command = new CreateLanguageCommand
            {
                Code = "EN",
                Name = "English"
            };

            // Act
            var entity = _mapper.Map<UserManagement.Domain.Entities.Language>(command);

            // Assert
            entity.Code.Should().Be("EN");
            entity.Name.Should().Be("English");
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
            entity.Id.Should().Be(0);
        }

        [Fact]
        public void UpdateCommand_Maps_IsActive_Correctly()
        {
            // Arrange - IsActive = 1 -> Active
            var commandActive = new UpdateLanguageCommand
            {
                Id = 1,
                Code = "EN",
                Name = "English",
                IsActive = 1
            };

            // Act
            var entityActive = _mapper.Map<UserManagement.Domain.Entities.Language>(commandActive);

            // Assert
            entityActive.Name.Should().Be("English");
            entityActive.IsActive.Should().Be(Status.Active);

            // Arrange - IsActive = 0 -> Inactive
            var commandInactive = new UpdateLanguageCommand
            {
                Id = 2,
                Code = "FR",
                Name = "French",
                IsActive = 0
            };

            // Act
            var entityInactive = _mapper.Map<UserManagement.Domain.Entities.Language>(commandInactive);

            // Assert
            entityInactive.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_Maps_IsDeleted_To_Deleted()
        {
            // Arrange
            var command = new DeleteLanguageCommand { Id = 5 };

            // Act
            var entity = _mapper.Map<UserManagement.Domain.Entities.Language>(command);

            // Assert
            entity.Id.Should().Be(5);
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void Entity_Maps_To_LanguageDTO()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.Language
            {
                Id = 1,
                Code = "EN",
                Name = "English",
                IsActive = Status.Active
            };

            // Act
            var dto = _mapper.Map<LanguageDTO>(entity);

            // Assert
            dto.Id.Should().Be(1);
            dto.Code.Should().Be("EN");
            dto.Name.Should().Be("English");
            dto.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void Entity_Maps_To_LanguageAutoCompleteDTO()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.Language
            {
                Id = 1,
                Code = "HI",
                Name = "Hindi"
            };

            // Act
            var dto = _mapper.Map<LanguageAutoCompleteDTO>(entity);

            // Assert
            dto.Id.Should().Be(1);
            dto.Name.Should().Be("Hindi");
        }
    }
}
