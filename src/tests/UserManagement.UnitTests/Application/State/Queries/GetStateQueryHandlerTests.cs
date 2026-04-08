using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Queries.GetCountries;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.State.Queries
{
    public sealed class GetStateQueryHandlerTests
    {
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStateQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.States> { new() { Id = 1 } };
            var dtoList = new List<StateDto> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllStatesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(entities))
                .Returns(dtoList);

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
            var entities = new List<UserManagement.Domain.Entities.States>();

            _mockQueryRepo
                .Setup(r => r.GetAllStatesAsync(2, 5, "test"))
                .ReturnsAsync((entities, 0));

            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(entities))
                .Returns(new List<StateDto>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }
    }
}
