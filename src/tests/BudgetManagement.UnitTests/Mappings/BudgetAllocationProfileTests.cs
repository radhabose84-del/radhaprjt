using AutoMapper;
using BudgetManagement.Application.BudgetAllocation.Command.Create;
using BudgetManagement.Application.Common.Mappings;
using BudgetManagement.UnitTests.TestData;
using static BudgetManagement.Domain.Common.BaseEntity;

namespace BudgetManagement.UnitTests.Mappings
{
    public sealed class BudgetAllocationProfileTests
    {
        private readonly IMapper _mapper;

        public BudgetAllocationProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<BudgetAllocationProfile>());
            _mapper = config.CreateMapper();
        }

        [Fact]
        public void CreateDto_MapsToEntity_WithActiveAndNotDeleted()
        {
            var dto = BudgetAllocationBuilders.ValidCreateDto();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetAllocation>(dto);

            entity.IsActive.Should().Be(Status.Active);
            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void CreateDto_MapsFields_Correctly()
        {
            var dto = new CreateBudgetAllocationDto
            {
                FinancialYearId = 5,
                RequestById = 10,
                UnitId = 2,
                BudgetGroupId = 3,
                AllocationTypeId = 4,
                ApprovedAmount = 75000m,
                RequestMonthId = 6
            };

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetAllocation>(dto);

            entity.FinancialYearId.Should().Be(5);
            entity.RequestById.Should().Be(10);
            entity.UnitId.Should().Be(2);
            entity.BudgetGroupId.Should().Be(3);
            entity.AllocationTypeId.Should().Be(4);
            entity.ApprovedAmount.Should().Be(75000m);
            entity.RequestMonthId.Should().Be(6);
        }

        [Fact]
        public void CreateDto_IdIsIgnored()
        {
            var dto = BudgetAllocationBuilders.ValidCreateDto();

            var entity = _mapper.Map<BudgetManagement.Domain.Entities.BudgetAllocation>(dto);

            entity.Id.Should().Be(0);
        }

    }
}
