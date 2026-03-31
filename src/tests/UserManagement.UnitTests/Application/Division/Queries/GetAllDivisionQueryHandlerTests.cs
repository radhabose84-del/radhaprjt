using AutoMapper;
using MediatR;
using UserManagement.Application.Divisions.Queries.GetDivisions;
using UserManagement.Application.Common.Interfaces.IDivision;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;

namespace UserManagement.UnitTests.Application.Division.Queries
{
    public sealed class GetAllDivisionQueryHandlerTests
    {
        private readonly Mock<IDivisionQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetDivisionQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Division> { DivisionBuilders.ValidEntity() };
            var dtos = new List<DivisionDTO> { DivisionBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllDivisionAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<DivisionDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDivisionQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.Division> { DivisionBuilders.ValidEntity() };
            var dtos = new List<DivisionDTO> { DivisionBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllDivisionAsync(2, 5, "test"))
                .ReturnsAsync((entities, 11));

            _mockMapper
                .Setup(m => m.Map<List<DivisionDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDivisionQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var emptyEntities = new List<UserManagement.Domain.Entities.Division>();
            var emptyDtos = new List<DivisionDTO>();

            _mockQueryRepo
                .Setup(r => r.GetAllDivisionAsync(1, 10, null))
                .ReturnsAsync((emptyEntities, 0));

            _mockMapper
                .Setup(m => m.Map<List<DivisionDTO>>(emptyEntities))
                .Returns(emptyDtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetDivisionQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var entities = new List<UserManagement.Domain.Entities.Division> { DivisionBuilders.ValidEntity() };
            var dtos = new List<DivisionDTO> { DivisionBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllDivisionAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<DivisionDTO>>(entities))
                .Returns(dtos);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetDivisionQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "GetDivisions" &&
                        e.Module == "Division"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
