using AutoMapper;
using UserManagement.Application.Common.Mappings;
using UserManagement.Application.Currency.Commands.CreateCurrency;
using UserManagement.Application.Currency.Commands.DeleteCurrency;
using UserManagement.Application.Currency.Commands.UpdateCurrency;
using UserManagement.Application.Currency.Queries.GetCurrency;
using static UserManagement.Domain.Enums.Common.Enums;

namespace UserManagement.UnitTests.Mappings
{
    public sealed class CurrencyProfileTests
    {
        private readonly IMapper _mapper;

        public CurrencyProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<CurrencyProfile>());
            // Do not call AssertConfigurationIsValid() because the CurrencyProfile
            // intentionally leaves audit fields (CreatedBy, CreatedAt, etc.) unmapped
            // since they are auto-populated by ApplicationDbContext.SaveChangesAsync().
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_Maps_To_Entity_With_Active_And_NotDeleted()
        {
            // Arrange
            var command = new CreateCurrencyCommand { Code = "EUR", Name = "Euro" };

            // Act
            var entity = _mapper.Map<UserManagement.Domain.Entities.Currency>(command);

            // Assert
            entity.Code.Should().Be("EUR");
            entity.Name.Should().Be("Euro");
            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
            entity.Id.Should().Be(0); // Id is ignored
        }

        [Fact]
        public void UpdateCommand_Maps_IsActive_Correctly()
        {
            // Arrange - IsActive = 1 -> Active
            var commandActive = new UpdateCurrencyCommand { Id = 1, Name = "Dollar", IsActive = 1 };

            // Act
            var entityActive = _mapper.Map<UserManagement.Domain.Entities.Currency>(commandActive);

            // Assert
            entityActive.Name.Should().Be("Dollar");
            entityActive.IsActive.Should().Be(Status.Active);

            // Arrange - IsActive = 0 -> Inactive
            var commandInactive = new UpdateCurrencyCommand { Id = 2, Name = "Yen", IsActive = 0 };

            // Act
            var entityInactive = _mapper.Map<UserManagement.Domain.Entities.Currency>(commandInactive);

            // Assert
            entityInactive.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void DeleteCommand_Maps_IsDeleted_To_Deleted()
        {
            // Arrange
            var command = new DeleteCurrencyCommand { Id = 5 };

            // Act
            var entity = _mapper.Map<UserManagement.Domain.Entities.Currency>(command);

            // Assert
            entity.Id.Should().Be(5);
            entity.IsDeleted.Should().Be(IsDelete.Deleted);
        }

        [Fact]
        public void Entity_Maps_To_CurrencyDto()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.Currency
            {
                Id = 1,
                Code = "GBP",
                Name = "British Pound",
                IsActive = Status.Active,
                CreatedBy = 1,
                CreatedAt = new DateTime(2026, 1, 1)
            };

            // Act
            var dto = _mapper.Map<CurrencyDto>(entity);

            // Assert
            dto.Id.Should().Be(1);
            dto.Code.Should().Be("GBP");
            dto.Name.Should().Be("British Pound");
            dto.IsActive.Should().Be(Status.Active);
            dto.CreatedBy.Should().Be(1);
        }

        [Fact]
        public void Entity_Maps_To_CurrencyAutoCompleteDto()
        {
            // Arrange
            var entity = new UserManagement.Domain.Entities.Currency
            {
                Id = 1,
                Code = "INR",
                Name = "Indian Rupee"
            };

            // Act
            var dto = _mapper.Map<CurrencyAutoCompleteDto>(entity);

            // Assert
            dto.Id.Should().Be(1);
            dto.Code.Should().Be("INR");
        }
    }
}
