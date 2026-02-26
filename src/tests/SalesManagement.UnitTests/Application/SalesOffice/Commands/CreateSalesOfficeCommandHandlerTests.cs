using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOffice;
using SalesManagement.Application.SalesOffice.Commands.CreateSalesOffice;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;
using static SalesManagement.Domain.Common.BaseEntity;

namespace SalesManagement.UnitTests.Application.SalesOffice.Commands
{
    public class CreateSalesOfficeCommandHandlerTests
    {
        private readonly Mock<ISalesOfficeCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateSalesOfficeCommandHandler CreateSut() =>
            new CreateSalesOfficeCommandHandler(
                _mockCommandRepo.Object,
                _mockMediator.Object,
                _mockMapper.Object);

        // ── Helper ────────────────────────────────────────────────────────────

        private void SetupHappyPath(CreateSalesOfficeCommand command, int newId = 1)
        {
            var entity = SalesOfficeBuilders.ValidEntity(newId);

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command))
                .Returns(entity);

            _mockCommandRepo
                .Setup(r => r.CreateAsync(entity))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
            result.Message.Should().Contain("created");
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewEntityId()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            const int expectedId = 42;
            SetupHappyPath(command, newId: expectedId);
            var sut = CreateSut();

            var result = await sut.Handle(command, CancellationToken.None);

            result.Data.Should().Be(expectedId);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateAsync_Once()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOffice>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "SALES_OFFICE_CREATE" &&
                        e.Module == "SalesOffice"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_MapsCommandToEntity_Once()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            SetupHappyPath(command);
            var sut = CreateSut();

            await sut.Handle(command, CancellationToken.None);

            _mockMapper.Verify(
                m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsEntityStatusActive()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.SalesOffice? capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.SalesOffice
            {
                SalesOfficeName = command.SalesOfficeName,
                SalesOrganisationId = command.SalesOrganisationId,
                CityId = command.CityId,
                Pincode = command.Pincode,
                Phone = command.Phone,
                Email = command.Email,
                ResponsibleManager = command.ResponsibleManager,
                RegionTerritory = command.RegionTerritory,
                Address = command.Address
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOffice>()))
                .Callback<SalesManagement.Domain.Entities.SalesOffice>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsActive.Should().Be(Status.Active);
        }

        [Fact]
        public async Task Handle_ValidCommand_SetsIsDeletedNotDeleted()
        {
            var command = SalesOfficeBuilders.ValidCreateCommand();
            SalesManagement.Domain.Entities.SalesOffice? capturedEntity = null;

            var entity = new SalesManagement.Domain.Entities.SalesOffice
            {
                SalesOfficeName = command.SalesOfficeName,
                SalesOrganisationId = command.SalesOrganisationId,
                CityId = command.CityId,
                Pincode = command.Pincode,
                Phone = command.Phone,
                Email = command.Email,
                ResponsibleManager = command.ResponsibleManager,
                RegionTerritory = command.RegionTerritory,
                Address = command.Address
            };

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.SalesOffice>(command))
                .Returns(entity);
            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.SalesOffice>()))
                .Callback<SalesManagement.Domain.Entities.SalesOffice>(e => capturedEntity = e)
                .ReturnsAsync(1);
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var sut = CreateSut();
            await sut.Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.IsDeleted.Should().Be(IsDelete.NotDeleted);
        }
    }
}
