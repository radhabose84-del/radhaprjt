using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IBillEntry;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Commands.Update;
using PurchaseManagement.Application.PurchaseOrder.BillEntry.Dto;
using PurchaseManagement.Domain.Entities.PurchaseOrder.BillEntry;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.BillEntry.Commands
{
    public sealed class UpdatePurchaseBillEntryCommandHandlerTests
    {
        private readonly Mock<IPurchaseBillEntryCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseBillEntryQueryRepository> _mockQryRepo = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdatePurchaseBillEntryCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockQryRepo.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_NotFound_ThrowsKeyNotFoundException()
        {
            var dto = new PurchaseBillEntryHeaderDto
            {
                Id = 999,
                PartyId = 1,
                BillNumber = "BILL001",
                Lines = new List<PurchaseBillEntryDetailDto>()
            };

            _mockQryRepo
                .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseBillEntryHeader?)null);

            var command = new UpdatePurchaseBillEntryCommand { Data = dto };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("*Bill entry 999 not found*");
        }

        [Fact]
        public async Task Handle_DuplicateBill_ThrowsInvalidOperationException()
        {
            var dto = new PurchaseBillEntryHeaderDto
            {
                Id = 1,
                PartyId = 1,
                BillNumber = "BILL001",
                Lines = new List<PurchaseBillEntryDetailDto>()
            };

            _mockQryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PurchaseBillEntryHeader { Lines = new List<PurchaseBillEntryDetail>() });

            _mockQryRepo
                .Setup(r => r.BillNumberExistsAsync(1, "BILL001", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var command = new UpdatePurchaseBillEntryCommand { Data = dto };

            Func<Task> act = () => CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Bill number already exists*");
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateAsync()
        {
            var dto = new PurchaseBillEntryHeaderDto
            {
                Id = 1,
                PartyId = 1,
                BillNumber = "BILL001",
                Lines = new List<PurchaseBillEntryDetailDto>
                {
                    new PurchaseBillEntryDetailDto { ItemId = 1, BilledQty = 5 }
                }
            };

            var existingHeader = new PurchaseBillEntryHeader { Lines = new List<PurchaseBillEntryDetail>() };

            _mockQryRepo
                .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existingHeader);

            _mockQryRepo
                .Setup(r => r.BillNumberExistsAsync(1, "BILL001", 1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<PurchaseBillEntryDetail>(It.IsAny<object>()))
                .Returns(new PurchaseBillEntryDetail());

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test");
            _mockIp.Setup(i => i.GetUserIPAddress()).Returns("127.0.0.1");

            var command = new UpdatePurchaseBillEntryCommand { Data = dto };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
            _mockRepo.Verify(r => r.UpdateAsync(It.IsAny<PurchaseBillEntryHeader>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
