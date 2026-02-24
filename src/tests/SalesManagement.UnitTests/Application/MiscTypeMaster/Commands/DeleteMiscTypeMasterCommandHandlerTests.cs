#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IMiscTypeMaster;
using SalesManagement.Application.MiscTypeMaster.Commands.DeleteMiscTypeMaster;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MiscTypeMaster.Commands
{
    public class DeleteMiscTypeMasterCommandHandlerTests
    {
        private readonly Mock<IMiscTypeMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscTypeMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteMiscTypeMasterCommandHandler CreateSut() =>
            new DeleteMiscTypeMasterCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object);

        [Fact]
        public async Task Handle_EntityExists_SoftDeleteSucceeds_ReturnsTrue()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            var existingDto = MiscTypeMasterBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsSoftDeleteAsync_Once()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            var existingDto = MiscTypeMasterBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            var existingDto = MiscTypeMasterBuilders.ValidDto(id: 1, code: "MISC001");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.ActionCode == "MISC_TYPE_DELETE" &&
                        e.Module == "MiscTypeMaster"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsExceptionRules()
        {
            var command = new DeleteMiscTypeMasterCommand(99);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscTypeMaster.Dto.MiscTypeMasterDto)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            var command = new DeleteMiscTypeMasterCommand(99);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.MiscTypeMaster.Dto.MiscTypeMasterDto)null);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_ThrowsExceptionRules()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            var existingDto = MiscTypeMasterBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Failed*");
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_DoesNotPublishAuditEvent()
        {
            var command = new DeleteMiscTypeMasterCommand(1);
            var existingDto = MiscTypeMasterBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            try { await CreateSut().Handle(command, CancellationToken.None); } catch { }

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }
    }
}
