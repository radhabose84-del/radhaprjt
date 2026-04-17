using Contracts.Common;
using ProductionManagement.Application.Common.Interfaces.IRepackingHeader;
using ProductionManagement.Application.RepackingHeader.Commands.UpdateRepackingHeader;

namespace ProductionManagement.UnitTests.Application.RepackingHeader.Commands
{
    public sealed class UpdateRepackingHeaderCommandHandlerTests
    {
        private readonly Mock<IRepackingHeaderCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IRepackingHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private UpdateRepackingHeaderCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int rowsAffected = 1, bool isLinked = false)
        {
            _mockQueryRepo.Setup(r => r.IsRepackingHeaderLinkedAsync(It.IsAny<int>())).ReturnsAsync(isLinked);
            _mockMapper.Setup(m => m.Map<ProductionManagement.Domain.Entities.RepackingHeader>(It.IsAny<UpdateRepackingHeaderCommand>()))
                .Returns(new ProductionManagement.Domain.Entities.RepackingHeader());
            _mockCommandRepo.Setup(r => r.UpdateAsync(It.IsAny<ProductionManagement.Domain.Entities.RepackingHeader>()))
                .ReturnsAsync(rowsAffected);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_RepackingFlow_ReturnsRepackingMessage()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new UpdateRepackingHeaderCommand { Id = 1, ItemId = 1, OldItemId = 1, IsActive = 1 },
                CancellationToken.None);
            result.Message.Should().Be("Repacking updated successfully.");
        }

        [Fact]
        public async Task Handle_YarnConversionFlow_ReturnsYarnMessage()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                new UpdateRepackingHeaderCommand { Id = 1, ItemId = 1, OldItemId = 2, IsActive = 1 },
                CancellationToken.None);
            result.Message.Should().Be("Yarn Conversion updated successfully.");
        }

        [Fact]
        public async Task Handle_Inactivate_When_Linked_ThrowsExceptionRules()
        {
            SetupHappyPath(isLinked: true);

            Func<Task> act = async () => await CreateSut().Handle(
                new UpdateRepackingHeaderCommand { Id = 1, IsActive = 0, ItemId = 1, OldItemId = 1 },
                CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*linked with other records*");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent_WithCorrectActionCode()
        {
            SetupHappyPath();
            await CreateSut().Handle(
                new UpdateRepackingHeaderCommand { Id = 1, ItemId = 1, OldItemId = 1, IsActive = 1 },
                CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "REPACKING_UPDATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
