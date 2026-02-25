#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Commands.UpdateSalesGroup;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesGroup.Commands
{
    public class UpdateSalesGroupCommandHandlerTests
    {
        private readonly Mock<ISalesGroupCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesGroupCommandHandler CreateSut() =>
            new UpdateSalesGroupCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesGroupBuilders.ValidDto(id: 1);
            var entity = SalesGroupBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesGroupBuilders.ValidDto(id: 1);
            var entity = SalesGroupBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesGroupBuilders.ValidDto(id: 1);
            var entity = SalesGroupBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesGroupBuilders.ValidDto(id: 1, name: "Test Sales Group");
            var entity = SalesGroupBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesGroup>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "SALES_GROUP_UPDATE" &&
                        e.Module == "SalesGroup"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_NotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesGroup.Dto.SalesGroupDto)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotCallUpdateAsync()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesGroup.Dto.SalesGroupDto)null);

            var sut = CreateSut();

            // Act
            try { await sut.Handle(command, CancellationToken.None); } catch { }

            // Assert
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesGroup>()), Times.Never);
        }

        [Fact]
        public async Task Handle_NotFound_DoesNotPublishAuditEvent()
        {
            // Arrange
            var command = SalesGroupBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesGroup.Dto.SalesGroupDto)null);

            var sut = CreateSut();

            // Act
            try { await sut.Handle(command, CancellationToken.None); } catch { }

            // Assert
            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
