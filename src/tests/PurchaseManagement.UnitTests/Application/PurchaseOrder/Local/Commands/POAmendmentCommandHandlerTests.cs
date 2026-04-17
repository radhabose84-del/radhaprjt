using AutoMapper;
using Contracts.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.IPurchaseDocument;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.Local;
using PurchaseManagement.Application.PurchaseOrder.Dtos.Local;
using PurchaseManagement.Application.PurchaseOrder.Local.Commands.Amend;
using PurchaseManagement.Application.PurchaseOrder.POAmendment;
using PurchaseManagement.Domain.Entities.PurchaseOrder;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.Local.Commands
{
    public sealed class POAmendmentCommandHandlerTests
    {
        private readonly Mock<IPurchaseOrderCommandRepository> _mockCmd = new(MockBehavior.Loose);
        private readonly Mock<IPurchaseOrderQueryRepository> _mockQry = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<ILogger<POAmendmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);
        private readonly Mock<IPODocumentQueryRepository> _mockPoDocs = new(MockBehavior.Loose);

        private POAmendmentCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockQry.Object, _mockMisc.Object, _mockMapper.Object,
                _mockIp.Object, _mockTz.Object, _mockLogger.Object, _mockOutbox.Object, _mockPoDocs.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsValidationException()
        {
            var command = new POAmendmentCommand { Data = null! };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsValidationException()
        {
            var command = new POAmendmentCommand { Data = new PurchaseOrderUpdateDto { Id = 0 } };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<System.ComponentModel.DataAnnotations.ValidationException>();
        }

        [Fact]
        public async Task Handle_PONotFound_ThrowsInvalidOperationException()
        {
            var dto = new PurchaseOrderUpdateDto { Id = 99 };
            var command = new POAmendmentCommand { Data = dto };
            _mockCmd.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseOrderHeader?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NotApproved_ThrowsInvalidOperationException()
        {
            var dto = new PurchaseOrderUpdateDto { Id = 1 };
            var command = new POAmendmentCommand { Data = dto };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 5, PONumber = "PO001" };
            _mockCmd.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _mockMisc.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("Asia/Kolkata");

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not approved*");
        }

        [Fact]
        public async Task Handle_GrnExists_ThrowsInvalidOperationException()
        {
            var dto = new PurchaseOrderUpdateDto { Id = 1 };
            var command = new POAmendmentCommand { Data = dto };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 10, PONumber = "PO001", RevisionNo = 0 };
            _mockCmd.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _mockMisc.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
            _mockQry.Setup(r => r.HasAnyGrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("Asia/Kolkata");

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*GRN*");
        }

        [Fact]
        public async Task Handle_ValidAmendment_ReturnsNewId()
        {
            var dto = new PurchaseOrderUpdateDto { Id = 1 };
            var command = new POAmendmentCommand { Data = dto };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 10, PONumber = "PO001", RevisionNo = 0 };
            _mockCmd.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _mockMisc.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
            _mockQry.Setup(r => r.HasAnyGrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("Asia/Kolkata");
            _mockCmd.Setup(r => r.AmendAsync(It.IsAny<PurchaseOrderHeader>(), It.IsAny<PurchaseOrderUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(42);
        }

        [Fact]
        public async Task Handle_ValidAmendment_CallsAmendAsyncOnce()
        {
            var dto = new PurchaseOrderUpdateDto { Id = 1 };
            var command = new POAmendmentCommand { Data = dto };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 10, PONumber = "PO001", RevisionNo = 0 };
            _mockCmd.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _mockMisc.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
            _mockQry.Setup(r => r.HasAnyGrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("Asia/Kolkata");
            _mockCmd.Setup(r => r.AmendAsync(It.IsAny<PurchaseOrderHeader>(), It.IsAny<PurchaseOrderUpdateDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(42);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCmd.Verify(r => r.AmendAsync(It.IsAny<PurchaseOrderHeader>(), It.IsAny<PurchaseOrderUpdateDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
