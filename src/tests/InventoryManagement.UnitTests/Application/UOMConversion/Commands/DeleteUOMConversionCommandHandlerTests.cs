using AutoMapper;
using MediatR;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.DeleteUOMConversion;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UOMConversion.Commands
{
    public sealed class DeleteUOMConversionCommandHandlerTests
    {
        private readonly Mock<IUOMConversionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteUOMConversionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_SuccessfulDelete_ReturnsTrue()
        {
            var entity = UOMConversionBuilders.ValidEntity(1);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOMConversion>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOMConversion>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                UOMConversionBuilders.ValidDeleteCommand(1), CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_FailedDelete_ThrowsException()
        {
            var entity = UOMConversionBuilders.ValidEntity(1);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOMConversion>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOMConversion>()))
                .ReturnsAsync(false);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            Func<Task> act = async () => await CreateSut().Handle(
                UOMConversionBuilders.ValidDeleteCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task Handle_SuccessfulDelete_PublishesAuditEvent()
        {
            var entity = UOMConversionBuilders.ValidEntity(1);
            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOMConversion>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.DeleteAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOMConversion>()))
                .ReturnsAsync(true);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(UOMConversionBuilders.ValidDeleteCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Delete"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
