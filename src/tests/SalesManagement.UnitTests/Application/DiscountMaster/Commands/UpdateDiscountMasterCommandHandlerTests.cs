using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDiscountMaster;
using SalesManagement.Application.DiscountMaster.Commands.UpdateDiscountMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DiscountMaster.Commands
{
    public class UpdateDiscountMasterCommandHandlerTests
    {
        private readonly Mock<IDiscountMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IDiscountMasterQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateDiscountMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private static UpdateDiscountMasterCommand ValidCommand() => new()
        {
            Id = 1,
            DiscountName = "Updated Discount",
            DiscountTypeId = 1,
            ApplicableLevelId = 2,
            TriggerEventId = 3,
            ValueTypeId = 4,
            DiscountValue = 20m,
            IsActive = 1
        };

        private void SetupMapper()
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.DiscountMaster>(It.IsAny<UpdateDiscountMasterCommand>()))
                .Returns((UpdateDiscountMasterCommand cmd) => new SalesManagement.Domain.Entities.DiscountMaster
                {
                    Id = cmd.Id,
                    DiscountName = cmd.DiscountName
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DiscountMaster>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            var result = await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            result.Message.Should().Contain("updated");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsUpdateAsync_Once()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.DiscountMaster>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditLogEvent_Once()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DISCOUNT_MASTER_UPDATE"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_AuditEvent_ContainsEntityId()
        {
            SetupMapper();
            SetupUpdateAsync(1);
            SetupPublishAudit();

            await CreateSut().Handle(ValidCommand(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionName == "1"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
