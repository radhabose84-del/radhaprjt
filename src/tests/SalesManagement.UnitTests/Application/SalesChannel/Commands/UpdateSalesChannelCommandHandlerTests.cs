#nullable disable
using AutoMapper;
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Commands.UpdateSalesChannel;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesChannel.Commands
{
    public class UpdateSalesChannelCommandHandlerTests
    {
        private readonly Mock<ISalesChannelCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesChannelCommandHandler CreateSut() =>
            new UpdateSalesChannelCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001");
            var entity = SalesChannelBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesChannelBuilders.ValidDto(id: 1);
            var entity = SalesChannelBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateAsync_Once()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesChannelBuilders.ValidDto(id: 1);
            var entity = SalesChannelBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(r => r.UpdateAsync(entity), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001");
            var entity = SalesChannelBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Update" &&
                        e.ActionCode == "SALES_CHANNEL_UPDATE" &&
                        e.Module == "SalesChannel"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsChannelCode()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001");
            var entity = SalesChannelBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesChannel>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "CH001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesChannel.Dto.SalesChannelDto)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallUpdateAsync()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesChannel.Dto.SalesChannelDto)null);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesChannel>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotPublishAuditEvent()
        {
            var command = SalesChannelBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesChannel.Dto.SalesChannelDto)null);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
