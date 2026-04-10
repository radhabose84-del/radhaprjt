using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using SalesManagement.Application.Common.Interfaces.ICustomerVisit;
using SalesManagement.Application.CustomerVisit.Commands.UpdateCustomerVisit;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.CustomerVisit.Commands
{
    public class UpdateCustomerVisitCommandHandlerTests
    {
        private readonly Mock<ICustomerVisitCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICustomerVisitQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);
        private readonly Mock<ILogger<UpdateCustomerVisitCommandHandler>> _mockLogger = new();

        private UpdateCustomerVisitCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object, _mockLogger.Object);

        private static UpdateCustomerVisitCommand ValidCommand() => new()
        {
            Id = 1,
            CustomerId = 1,
            VisitTypeId = 2,
            VisitDateTime = DateTimeOffset.UtcNow,
            Latitude = 12.97m,
            Longitude = 77.59m,
            Remarks = "Updated visit",
            MarketingOfficerId = 3,
            IsActive = 1
        };

        private void SetupMapper(UpdateCustomerVisitCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.CustomerVisit>(cmd))
                .Returns(new SalesManagement.Domain.Entities.CustomerVisit
                {
                    Id = cmd.Id,
                    CustomerId = cmd.CustomerId,
                    VisitTypeId = cmd.VisitTypeId,
                    MarketingOfficerId = cmd.MarketingOfficerId
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.CustomerVisit>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_NoImage_ReturnsSuccess()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUpdatedId()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Data.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.CustomerVisit>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_PublishesAuditLogEvent_Once()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(command, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "CUSTOMER_VISIT_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsCorrectMessage()
        {
            var command = ValidCommand();
            SetupMapper(command);
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Message.Should().Contain("updated successfully");
        }
    }
}
