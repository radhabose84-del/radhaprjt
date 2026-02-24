#nullable disable
using AutoMapper;
using Contracts.Common;
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
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateSalesOrganisationCommandHandler CreateSut() =>
            new UpdateSalesOrganisationCommandHandler(
                _mockCommandRepo.Object,
                _mockQueryRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 1);
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
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
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
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
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1);
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
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
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
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
            var existingDto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");
            var entity = SalesOrganisationBuilders.ValidEntity(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(existingDto);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOrganisation>(command)).Returns(entity);
            _mockCommandRepo.Setup(r => r.UpdateAsync(entity)).ReturnsAsync(1);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();

            // Act
            await sut.Handle(command, CancellationToken.None);

            // Assert — audit event uses the existing code (ORG001) from the fetched DTO
            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "ORG001"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesOrganisation.Dto.SalesOrganisationDto)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(command, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotCallUpdateAsync()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesOrganisation.Dto.SalesOrganisationDto)null);

            var sut = CreateSut();

            // Act
            try { await sut.Handle(command, CancellationToken.None); } catch { }

            // Assert
            _mockCommandRepo.Verify(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOrganisation>()), Times.Never);
        }

        [Fact]
        public async Task Handle_EntityNotFound_DoesNotPublishAuditEvent()
        {
            // Arrange
            var command = SalesOrganisationBuilders.ValidUpdateCommand(id: 99);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesManagement.Application.SalesOrganisation.Dto.SalesOrganisationDto)null);

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
