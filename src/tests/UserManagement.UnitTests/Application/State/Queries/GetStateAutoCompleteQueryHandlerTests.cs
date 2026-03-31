using AutoMapper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.State.Queries.GetStateAutoComplete;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;
using FluentValidation;

namespace UserManagement.UnitTests.Application.State.Queries
{
    public class GetStateAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetStateAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsList()
        {
            var states = new List<States> { StateBuilders.ValidEntity() };
            var dtos = new List<StateAutoCompleteDTO> { StateBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByStateNameAsync("Maha"))
                .ReturnsAsync(states);
            _mockMapper
                .Setup(m => m.Map<List<StateAutoCompleteDTO>>(states))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateAutoCompleteQuery { SearchPattern = "Maha" },
                CancellationToken.None);

            result.Should().HaveCount(1);
            result[0].StateName.Should().Be("Maharashtra");
        }

        [Fact]
        public async Task Handle_EmptyResult_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByStateNameAsync("xyz"))
                .ReturnsAsync(new List<States>());

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new GetStateAutoCompleteQuery { SearchPattern = "xyz" },
                CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>()
                .WithMessage("*No States found*");
        }

        [Fact]
        public async Task Handle_NullSearchPattern_UsesEmptyString()
        {
            var states = new List<States> { StateBuilders.ValidEntity() };
            var dtos = new List<StateAutoCompleteDTO> { StateBuilders.ValidAutoCompleteDto() };

            _mockQueryRepo
                .Setup(r => r.GetByStateNameAsync(string.Empty))
                .ReturnsAsync(states);
            _mockMapper
                .Setup(m => m.Map<List<StateAutoCompleteDTO>>(states))
                .Returns(dtos);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateAutoCompleteQuery { SearchPattern = null },
                CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
