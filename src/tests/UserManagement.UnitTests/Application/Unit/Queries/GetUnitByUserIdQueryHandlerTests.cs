using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Application.Units.Queries.GetUnitByUserId;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Unit.Queries
{
    public sealed class GetUnitByUserIdQueryHandlerTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ILogger<GetUnitByUserIdQueryHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIpService = new(MockBehavior.Loose);

        private GetUnitByUserIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object, _mockIpService.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var entities = new List<UserManagement.Domain.Entities.Unit> { new() { Id = 1 } };
            var dtoList = new List<UnitAutoCompleteDTO> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetUnitByUserId(5, 1))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<UnitAutoCompleteDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitByUserIdQuery { UserId = 5, CompanyId = 1 },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsEmpty()
        {
            _mockQueryRepo
                .Setup(r => r.GetUnitByUserId(999, 1))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.Unit>());

            // No units for this user is a normal outcome → returns an empty list (200), not a throw.
            var result = await CreateSut().Handle(
                new GetUnitByUserIdQuery { UserId = 999, CompanyId = 1 },
                CancellationToken.None);

            result.Should().BeEmpty();
        }
    }
}
