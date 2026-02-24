#nullable disable
using Contracts.Common;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.DeleteSalesOrganisation;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Commands
{
    public class DeleteSalesOrganisationCommandHandlerTests
    {
        private readonly Mock<ISalesOrganisationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private DeleteSalesOrganisationCommandHandler CreateSut() =>
            new DeleteSalesOrganisationCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_SoftDeleteSucceeds_ReturnsTrue()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_CallsSoftDeleteAsync_Once()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001", name: "Test Sales Organisation");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "SoftDelete" &&
                        e.Module == "SalesOrganisation"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsOrganisationCodeAsActionCode()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert — ActionCode = SalesOrganisationCode per handler implementation
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "ORG001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsExceptionRules()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesOrganisation.Dto.SalesOrganisationDto)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallSoftDelete()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesOrganisation.Dto.SalesOrganisationDto)null);

            var sut = CreateSut();

            // Act
            try { await sut.Handle(command, CancellationToken.None); } catch { }

            // Assert
            _mockCommandRepo.Verify(
                r => r.SoftDeleteAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_ThrowsExceptionRules()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<ExceptionRules>()
                .WithMessage("*Failed*");
        }

        [Fact]
        public async Task Handle_SoftDeleteFails_DoesNotPublishAuditEvent()
        {
            // Arrange
            var command = new DeleteSalesOrganisationCommand(1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockCommandRepo.Setup(r => r.SoftDeleteAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

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
