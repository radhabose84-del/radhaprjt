using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Queries.GetStateByCountryId;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.State.Queries
{
    public sealed class GetStateByCountryIdQueryHandlerTests
    {
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetStateByCountryIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCountryId_ReturnsStates()
        {
            var entities = new List<UserManagement.Domain.Entities.States> { new() { Id = 1 } };
            var dtoList = new List<StateDto> { new() { Id = 1, StateName = "TestState" } };

            _mockQueryRepo
                .Setup(r => r.GetStateByCountryIdAsync(5))
                .ReturnsAsync(entities);

            _mockMapper
                .Setup(m => m.Map<List<StateDto>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateByCountryIdQuery { Id = 5 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_NoStatesFound_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetStateByCountryIdAsync(999))
                .ReturnsAsync(new List<UserManagement.Domain.Entities.States>());

            Func<Task> act = () => CreateSut().Handle(
                new GetStateByCountryIdQuery { Id = 999 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
