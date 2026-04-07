using AutoMapper;
using Contracts.Common;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseIndent;
using PurchaseManagement.Application.PurchaseIndents.Command.DeletePurchaseIndent;
using PurchaseManagement.Domain.Entities;

namespace PurchaseManagement.UnitTests.Application.PurchaseIndent.Commands
{
    public sealed class DeletePurchaseIndentCommandHandlerTests
    {
        private readonly Mock<IPurchaseIndentCommand> _mockCommandRepo = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<ILogger<DeletePurchaseIndentCommandHandler>> _mockLogger = new(MockBehavior.Loose);

        private DeletePurchaseIndentCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLogger.Object);

        [Fact]
        public async Task Handle_ValidId_ReturnsTrue()
        {
            _mockMapper
                .Setup(m => m.Map<IndentHeader>(It.IsAny<object>()))
                .Returns(new IndentHeader { Id = 1, IndentDetails = new List<IndentDetail>() });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<IndentHeader>()))
                .ReturnsAsync(true);

            var result = await CreateSut().Handle(new DeletePurchaseIndentCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidId_PublishesAuditEvent()
        {
            _mockMapper
                .Setup(m => m.Map<IndentHeader>(It.IsAny<object>()))
                .Returns(new IndentHeader { Id = 1, IndentDetails = new List<IndentDetail>() });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<IndentHeader>()))
                .ReturnsAsync(true);

            await CreateSut().Handle(new DeletePurchaseIndentCommand { Id = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_InvalidId_ThrowsExceptionRules()
        {
            _mockMapper
                .Setup(m => m.Map<IndentHeader>(It.IsAny<object>()))
                .Returns(new IndentHeader { Id = 99, IndentDetails = new List<IndentDetail>() });

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(99, It.IsAny<IndentHeader>()))
                .ReturnsAsync(false);

            Func<Task> act = async () =>
                await CreateSut().Handle(new DeletePurchaseIndentCommand { Id = 99 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>();
        }
    }
}
