using AutoMapper;
using MediatR;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using InventoryManagement.Application.UOMConversion.Command.UpdateUOMConversion;
using InventoryManagement.Application.UOMConversion.Queries.GetAllUOMConversion;
using InventoryManagement.Domain.Events;
using InventoryManagement.UnitTests.TestData;

namespace InventoryManagement.UnitTests.Application.UOMConversion.Commands
{
    public sealed class UpdateUOMConversionCommandHandlerTests
    {
        private readonly Mock<IUOMConversionCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IUOMConversionQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateUOMConversionCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private void SetupHappyPath(int id = 1)
        {
            var entity = UOMConversionBuilders.ValidEntity(id);
            var dto = UOMConversionBuilders.ValidDto(id);

            _mockMapper
                .Setup(m => m.Map<InventoryManagement.Domain.Entities.UOMConversion>(It.IsAny<object>()))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOMConversion>()))
                .ReturnsAsync(entity);
            _mockMapper
                .Setup(m => m.Map<UOMConversionDto>(It.IsAny<object>()))
                .Returns(dto);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(
                UOMConversionBuilders.ValidUpdateCommand(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            SetupHappyPath();
            await CreateSut().Handle(UOMConversionBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<int>(), It.IsAny<InventoryManagement.Domain.Entities.UOMConversion>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(UOMConversionBuilders.ValidUpdateCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionDetail == "Update"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
