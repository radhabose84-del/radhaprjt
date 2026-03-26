using AutoMapper;
using SalesManagement.Application.Common.Mappings;
using SalesManagement.Application.AgentCommissionConfig.Commands.CreateAgentCommissionConfig;
using SalesManagement.Application.AgentCommissionConfig.Commands.UpdateAgentCommissionConfig;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Mappings
{
    public sealed class AgentCommissionConfigProfileTests
    {
        private readonly IMapper _mapper;

        public AgentCommissionConfigProfileTests()
        {
            var config = new MapperConfiguration(cfg => cfg.AddProfile<AgentCommissionConfigProfile>());
            _mapper = config.CreateMapper();
        }


        [Fact]
        public void CreateCommand_To_Entity_MapsFields()
        {
            var command = new CreateAgentCommissionConfigCommand
            {
                AgentId = 1,
                SalesSegmentId = 2,
                CommissionTypeId = 4,
                CommissionBasisId = 5,
                ApplicableLevelId = 6,
                CommissionPercentage = 10.5m,
                CurrencyId = 7,
                ValidityFrom = new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero),
                ValidityTo = new DateTimeOffset(2026, 12, 31, 0, 0, 0, TimeSpan.Zero)
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.AgentCommissionConfig>(command);

            entity.AgentId.Should().Be(1);
            entity.SalesSegmentId.Should().Be(2);
            entity.CommissionTypeId.Should().Be(4);
            entity.CommissionBasisId.Should().Be(5);
            entity.ApplicableLevelId.Should().Be(6);
            entity.CommissionPercentage.Should().Be(10.5m);
            entity.CurrencyId.Should().Be(7);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsActive_Active()
        {
            var command = new CreateAgentCommissionConfigCommand
            {
                AgentId = 1,
                SalesSegmentId = 2,
                CommissionTypeId = 4,
                CommissionPercentage = 10m,
                ValidityFrom = DateTimeOffset.UtcNow,
                ValidityTo = DateTimeOffset.UtcNow.AddMonths(6)
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.AgentCommissionConfig>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void CreateCommand_To_Entity_SetsIsDeleted_NotDeleted()
        {
            var command = new CreateAgentCommissionConfigCommand
            {
                AgentId = 1,
                SalesSegmentId = 2,
                CommissionTypeId = 4,
                CommissionPercentage = 10m,
                ValidityFrom = DateTimeOffset.UtcNow,
                ValidityTo = DateTimeOffset.UtcNow.AddMonths(6)
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.AgentCommissionConfig>(command);

            entity.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }

        [Fact]
        public void UpdateCommand_IsActive1_MapsTo_StatusActive()
        {
            var command = new UpdateAgentCommissionConfigCommand
            {
                Id = 1,
                AgentId = 1,
                SalesSegmentId = 2,
                CommissionTypeId = 4,
                CommissionPercentage = 10m,
                ValidityFrom = DateTimeOffset.UtcNow,
                ValidityTo = DateTimeOffset.UtcNow.AddMonths(6),
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.AgentCommissionConfig>(command);

            entity.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public void UpdateCommand_IsActive0_MapsTo_StatusInactive()
        {
            var command = new UpdateAgentCommissionConfigCommand
            {
                Id = 1,
                AgentId = 1,
                SalesSegmentId = 2,
                CommissionTypeId = 4,
                CommissionPercentage = 10m,
                ValidityFrom = DateTimeOffset.UtcNow,
                ValidityTo = DateTimeOffset.UtcNow.AddMonths(6),
                IsActive = 0
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.AgentCommissionConfig>(command);

            entity.IsActive.Should().Be(Status.Inactive);
        }

        [Fact]
        public void UpdateCommand_To_Entity_MapsFields()
        {
            var command = new UpdateAgentCommissionConfigCommand
            {
                Id = 5,
                AgentId = 10,
                SalesSegmentId = 20,
                CommissionTypeId = 40,
                CommissionBasisId = 50,
                ApplicableLevelId = 60,
                CommissionPercentage = 15.5m,
                CurrencyId = 70,
                ValidityFrom = new DateTimeOffset(2026, 3, 1, 0, 0, 0, TimeSpan.Zero),
                ValidityTo = new DateTimeOffset(2026, 9, 30, 0, 0, 0, TimeSpan.Zero),
                IsActive = 1
            };

            var entity = _mapper.Map<SalesManagement.Domain.Entities.AgentCommissionConfig>(command);

            entity.Id.Should().Be(5);
            entity.AgentId.Should().Be(10);
            entity.CommissionPercentage.Should().Be(15.5m);
            entity.CommissionBasisId.Should().Be(50);
        }
    }
}
