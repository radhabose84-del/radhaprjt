using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IItemPriceMaster;
using SalesManagement.Application.ItemPriceMaster.Commands.UpdateItemPriceMaster;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ItemPriceMaster.Commands
{
    public class UpdateItemPriceMasterCommandHandlerTests
    {
        private readonly Mock<IItemPriceMasterCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Strict);

        private UpdateItemPriceMasterCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupMapper(UpdateItemPriceMasterCommand cmd)
        {
            _mockMapper
                .Setup(m => m.Map<SalesManagement.Domain.Entities.ItemPriceMaster>(cmd))
                .Returns(new SalesManagement.Domain.Entities.ItemPriceMaster
                {
                    Id = cmd.Id,
                    ItemId = cmd.ItemId,
                    SalesSegmentId = cmd.SalesSegmentId,
                    BaseRate = cmd.BaseRate,
                    CurrencyId = cmd.CurrencyId
                });
        }

        private void SetupUpdateAsync(int returnId = 1)
        {
            _mockCommandRepo
                .Setup(r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()))
                .ReturnsAsync(returnId);
        }

        private void SetupPublishAudit()
        {
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        private static UpdateItemPriceMasterCommand ValidCommand() => new()
        {
            Id = 1,
            ItemId = 1,
            SalesSegmentId = 2,
            BaseRate = 150m,
            CurrencyId = 1,
            ValidFrom = DateOnly.FromDateTime(DateTime.Today),
            ValidTo = DateOnly.FromDateTime(DateTime.Today.AddDays(30)),
            IsActive = 1
        };

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
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
                r => r.UpdateAsync(It.IsAny<SalesManagement.Domain.Entities.ItemPriceMaster>()),
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
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "ITEM_PRICE_UPDATE"),
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
