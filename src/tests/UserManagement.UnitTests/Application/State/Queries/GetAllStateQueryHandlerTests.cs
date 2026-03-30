using AutoMapper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.State.Queries.GetCountries;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;

namespace UserManagement.UnitTests.Application.State.Queries
{
    public class GetAllStateQueryHandlerTests
    {
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetStateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var states = new List<States> { StateBuilders.ValidEntity() };
            var dtos = new List<StateDto> { StateBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllStatesAsync(1, 10, null))
                .ReturnsAsync((states, 1));
            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(states))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var states = new List<States> { StateBuilders.ValidEntity() };
            var dtos = new List<StateDto> { StateBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllStatesAsync(2, 5, "search"))
                .ReturnsAsync((states, 11));
            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(states))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateQuery { PageNumber = 2, PageSize = 5, SearchTerm = "search" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(11);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllStatesAsync(1, 10, null))
                .ReturnsAsync((new List<States>(), 0));
            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(It.IsAny<List<States>>()))
                .Returns(new List<StateDto>());
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var states = new List<States> { StateBuilders.ValidEntity() };
            var dtos = new List<StateDto> { StateBuilders.ValidDto() };

            _mockQueryRepo
                .Setup(r => r.GetAllStatesAsync(1, 10, null))
                .ReturnsAsync((states, 1));
            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(states))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAll" && e.Module == "State"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetStateQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetAll"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
