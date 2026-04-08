using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IQuotation.IRfqEntry;
using PurchaseManagement.Application.Quotation.RfqEntry.Commands.Update;
using PurchaseManagement.Domain.Entities.Quotation.RfqEntry;

namespace PurchaseManagement.UnitTests.Application.Rfqs.Commands
{
    public sealed class UpdateRfqCommandHandlerTests
    {
        private readonly Mock<IRfqCommandRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);

        private UpdateRfqCommandHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockIp.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsUnit()
        {
            _mockRepo
                .Setup(r => r.GetStatusIdByCodeAsync("SUBMIT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            _mockRepo
                .Setup(r => r.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<RfqMaster>(),
                    It.IsAny<List<RfqItem>>(),
                    It.IsAny<List<RfqSupplier>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateRfqCommand
            {
                Id = 1,
                InitiationTypeId = 1,
                IsActive = 1,
                Items = new List<RfqItemUpsertDto>(),
                Suppliers = new List<RfqSupplierUpsertDto>()
            };

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(Unit.Value);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsUpdateOnce()
        {
            _mockRepo
                .Setup(r => r.GetStatusIdByCodeAsync("SUBMIT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            _mockIp.Setup(i => i.GetUnitId()).Returns(1);

            _mockRepo
                .Setup(r => r.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<RfqMaster>(),
                    It.IsAny<List<RfqItem>>(),
                    It.IsAny<List<RfqSupplier>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var command = new UpdateRfqCommand
            {
                Id = 1,
                InitiationTypeId = 1,
                IsActive = 1,
                Items = new List<RfqItemUpsertDto>(),
                Suppliers = new List<RfqSupplierUpsertDto>()
            };

            await CreateSut().Handle(command, CancellationToken.None);

            _mockRepo.Verify(
                r => r.UpdateAsync(
                    It.IsAny<int>(),
                    It.IsAny<RfqMaster>(),
                    It.IsAny<List<RfqItem>>(),
                    It.IsAny<List<RfqSupplier>>(),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
