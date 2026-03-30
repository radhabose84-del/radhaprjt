using AutoMapper;
using UserManagement.Application.Common.Interfaces.IState;
using UserManagement.Application.State.Queries.GetStates;
using UserManagement.Application.State.Queries.GetStateById;
using UserManagement.Domain.Entities;
using UserManagement.Domain.Events;
using UserManagement.UnitTests.TestData;
using MediatR;

namespace UserManagement.UnitTests.Application.State.Queries
{
    public class GetStateByIdQueryHandlerTests
    {
        private readonly Mock<IStateQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GetStateByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            var state = StateBuilders.ValidEntity(id: 1);
            var dto = StateBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(state);
            _mockMapper
                .Setup(m => m.Map<StateDto>(state))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetStateByIdQuery { Id = 1 },
                CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
            result.StateCode.Should().Be("MH");
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            var state = StateBuilders.ValidEntity(id: 1);
            var dto = StateBuilders.ValidDto(id: 1);

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(state);
            _mockMapper
                .Setup(m => m.Map<StateDto>(state))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetById" && e.Module == "State"),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(
                new GetStateByIdQuery { Id = 1 },
                CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "GetById"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NullResult_ThrowsValidationException()
        {
            // The handler maps before null check, so it calls Map on null then checks
            // Based on actual code: it maps first, then checks if state is null
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((States?)null);
            _mockMapper
                .Setup(m => m.Map<StateDto>((States?)null))
                .Returns((StateDto?)null!);

            var sut = CreateSut();

            Func<Task> act = async () => await sut.Handle(
                new GetStateByIdQuery { Id = 999 },
                CancellationToken.None);

            await act.Should().ThrowAsync<FluentValidation.ValidationException>();
        }
    }
}
