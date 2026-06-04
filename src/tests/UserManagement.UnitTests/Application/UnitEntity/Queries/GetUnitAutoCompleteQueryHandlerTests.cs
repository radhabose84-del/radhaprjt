using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using Contracts.Interfaces;
using UserManagement.Application.Units.Queries.GetUnitAutoComplete;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Domain.Events;
using DomainUnit = UserManagement.Domain.Entities.Unit;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UnitEntity.Queries
{
    public sealed class GetUnitAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUnitAutoCompleteQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetUnitAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_SuperAdmin_ReturnsSuperAdminResults()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("SUPER_ADMIN");

            var entities = UnitEntityBuilders.ValidEntityList();
            var dtos = UnitEntityBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetUnit_SuperAdmin("Test"))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UnitAutoCompleteDTO>>(entities))
                .Returns(dtos);

            var result = await CreateSut().Handle(
                new GetUnitAutoCompleteQuery { SearchPattern = "Test", CompanyId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].UnitName.Should().Be("Test Unit");
        }

        [Fact]
        public async Task Handle_RegularUser_ReturnsFilteredResults()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("USER");
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);

            var entities = UnitEntityBuilders.ValidEntityList();
            var dtos = UnitEntityBuilders.ValidAutoCompleteList();

            _mockQueryRepo
                .Setup(r => r.GetUnit("Test", 1, 1))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UnitAutoCompleteDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitAutoCompleteQuery { SearchPattern = "Test", CompanyId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsEmpty()
        {
            _mockIpService.Setup(s => s.GetGroupCode()).Returns("USER");
            _mockIpService.Setup(s => s.GetUserId()).Returns(1);

            List<DomainUnit> emptyList = new();
            _mockQueryRepo
                .Setup(r => r.GetUnit("NoMatch", 1, 1))
                .ReturnsAsync(emptyList);

            // No matches is a normal autocomplete outcome → returns an empty list (200), not a throw.
            var result = await CreateSut().Handle(
                new GetUnitAutoCompleteQuery { SearchPattern = "NoMatch", CompanyId = 1 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
