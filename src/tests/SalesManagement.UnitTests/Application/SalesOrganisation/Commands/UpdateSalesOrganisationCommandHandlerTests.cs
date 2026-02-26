using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Commands.UpdateSalesOrganisation;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Commands
{
    public class UpdateSalesOrganisationCommandHandlerTests
    {
        private readonly Mock<ISalesOrganisationCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesOrganisationCommandHandler CreateSut() =>
            new UpdateSalesOrganisationCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(command, CancellationToken.None);

            // Assert
            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateAsync_Once()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command)).Returns(entity);
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
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command)).Returns(entity);
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
                        e.ActionCode == "SALES_ORG_UPDATE" &&
                        e.Module == "SalesOrganisation"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsOrganisationCode()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
