using AutoMapper;
using Contracts.Common;
using FluentAssertions;
using MediatR;
using Moq;
using SalesManagement.Application.Common.Interfaces.IMarketingOfficer;
using SalesManagement.Application.MarketingOfficer.Commands.CreateMarketingOfficer;
using SalesManagement.Domain.Events;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.MarketingOfficer.Commands
{
    public sealed class CreateMarketingOfficerCommandHandlerTests
    {
        private readonly Mock<IMarketingOfficerCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMarketingOfficerQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private CreateMarketingOfficerCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(CreateMarketingOfficerCommand command, int newId = 1)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command))
                .Returns(MarketingOfficerBuilders.ValidEntity(0));

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MarketingOfficer>()))
                .ReturnsAsync(newId);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 1);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand();
            SetupHappyPath(command, newId: 42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MarketingOfficer>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditEvent()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand();
            SetupHappyPath(command);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e =>
                        e.ActionDetail == "Create" &&
                        e.ActionCode == "MARKETING_OFFICER_CREATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_EntityHasCorrectChildCount()
        {
            var command = MarketingOfficerBuilders.ValidCreateCommand(
                salesGroups: new List<CreateOfficerSalesGroupDto>
                {
                    new() { SalesGroupId = 1 },
                    new() { SalesGroupId = 2 }
                });

            SalesManagement.Domain.Entities.MarketingOfficer? capturedEntity = null;

            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.MarketingOfficer>(command))
                .Returns(new SalesManagement.Domain.Entities.MarketingOfficer
                {
                    EmployeeNo = "EMP001",
                    EmployeeName = "Test",
                    Unit = "U",
                    Department = "D",
                    Designation = "Mgr",
                    SalesOfficeId = 1
                });

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.MarketingOfficer>()))
                .Callback<SalesManagement.Domain.Entities.MarketingOfficer>(e => capturedEntity = e)
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            capturedEntity.Should().NotBeNull();
            capturedEntity!.OfficerSalesGroups.Should().HaveCount(2);
        }
    }
}
