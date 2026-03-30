using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using UserManagement.Application.Units.Queries.GetUnits;
using UserManagement.Application.Common.Interfaces.IUnit;
using UserManagement.Domain.Events;
using DomainUnit = UserManagement.Domain.Entities.Unit;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.UnitEntity.Queries
{
    public sealed class GetAllUnitQueryHandlerTests
    {
        private readonly Mock<IUnitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<ILogger<GetUnitQueryHandler>> _mockLogger = new(MockBehavior.Loose);

        private GetUnitQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object, _mockLogger.Object);

        private void SetupHappyPath(List<DomainUnit> entities, List<GetUnitsDTO> dtos, int totalCount)
        {
            _mockQueryRepo
                .Setup(r => r.GetAllUnitsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((entities, totalCount));

            _mockMapper
                .Setup(m => m.Map<List<GetUnitsDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = UnitEntityBuilders.ValidEntityList();
            var dtos = new List<GetUnitsDTO> { UnitEntityBuilders.ValidGetUnitsDto() };
            SetupHappyPath(entities, dtos, 1);

            var result = await CreateSut().Handle(
                new GetUnitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = UnitEntityBuilders.ValidEntityList();
            var dtos = new List<GetUnitsDTO> { UnitEntityBuilders.ValidGetUnitsDto() };
            SetupHappyPath(entities, dtos, 15);

            var result = await CreateSut().Handle(
                new GetUnitQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(15);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyList = new List<DomainUnit>();
            var emptyDtos = new List<GetUnitsDTO>();

            _mockQueryRepo
                .Setup(r => r.GetAllUnitsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>()))
                .ReturnsAsync((emptyList, 0));

            _mockMapper
                .Setup(m => m.Map<List<GetUnitsDTO>>(emptyList))
                .Returns(emptyDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetUnitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_ValidResult_PublishesAuditEvent()
        {
            var entities = UnitEntityBuilders.ValidEntityList();
            var dtos = new List<GetUnitsDTO> { UnitEntityBuilders.ValidGetUnitsDto() };
            SetupHappyPath(entities, dtos, 1);

            await CreateSut().Handle(
                new GetUnitQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetUnitQuery" &&
                        e.Module == "Unit"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
