using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Commands.DeleteDispatchAdvice;

using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DispatchAdvice.Commands
{
    public sealed class DeleteDispatchAdviceCommandHandlerTests
    {
        private readonly Mock<IDispatchAdviceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteDispatchAdviceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMiscRepo.Object, _mockMediator.Object);

        private void SetupMiscStatuses()
        {
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });
        }

        [Fact]
        public async Task Handle_ExistingId_ReturnsTrue()
        {
            SetupMiscStatuses();
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeleteDispatchAdviceCommand(1), CancellationToken.None);
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NonExistentId_ThrowsExceptionRules()
        {
            SetupMiscStatuses();
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(99, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(new DeleteDispatchAdviceCommand(99), CancellationToken.None);
            await act.Should().ThrowAsync<ExceptionRules>();
        }

        [Fact]
        public async Task Handle_ExistingId_PublishesAuditEvent()
        {
            SetupMiscStatuses();
            _mockCommandRepo
                .Setup(r => r.SoftDeleteAsync(1, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeleteDispatchAdviceCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISPATCHADVICE_DELETE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
