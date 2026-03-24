using AutoMapper;
using BudgetManagement.Application.Common.Mappings;
using BudgetManagement.UnitTests.TestData;

namespace BudgetManagement.UnitTests.Mappings
{
    public sealed class BudgetRequestProfileTests
    {
        private readonly IMapper _mapper;

        public BudgetRequestProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BudgetRequestProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateCommand_MapsToEntity()
        {
            var command = BudgetRequestBuilders.ValidCreateCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetRequest>(command);

            entity.UnitId.Should().Be(command.UnitId);
            entity.CurrencyId.Should().Be(command.CurrencyId);
            entity.RequestTypeId.Should().Be(command.RequestTypeId);
            entity.RequestAmount.Should().Be(command.RequestAmount);
            entity.FinancialYearId.Should().Be(command.FinancialYearId);
        }

        [Fact]
        public void UpdateCommand_MapsToEntity()
        {
            var command = BudgetRequestBuilders.ValidUpdateCommand();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetRequest>(command);

            entity.Id.Should().Be(command.Id);
            entity.UnitId.Should().Be(command.UnitId);
            entity.RequestAmount.Should().Be(command.RequestAmount);
        }

        [Fact]
        public void Entity_MapsToDto()
        {
            var entity = BudgetRequestBuilders.ValidEntity(7);

            var dto = _mapper.Map<BudgetManagement.Application.BudgetRequest.BudgetRequestDto>(entity);

            dto.Id.Should().Be(7);
            dto.RequestCode.Should().Be(entity.RequestCode);
            dto.UnitId.Should().Be(entity.UnitId);
            dto.RequestAmount.Should().Be(entity.RequestAmount);
        }

        [Fact]
        public void Profile_ConfigurationIsValid()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BudgetRequestProfile>());
            config.AssertConfigurationIsValid();
        }
    }
}
