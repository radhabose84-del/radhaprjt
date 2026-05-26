using System.Data.Common;
using AutoMapper;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using PurchaseManagement.Application.Common.Interfaces;
using PurchaseManagement.Application.Common.Interfaces.IMiscMaster;
using PurchaseManagement.Application.Common.Interfaces.IOutbox;
using PurchaseManagement.Application.Common.Interfaces.IPurchaseOrder.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.Dtos.ImportPO;
using PurchaseManagement.Application.PurchaseOrder.ImportPO.Command.ImportPOAmendment;
using PurchaseManagement.Domain.Entities.PurchaseOrder;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.PurchaseOrder.ImportPO.Commands
{
    public sealed class ImportPOAmendmentCommandHandlerTests
    {
        private readonly Mock<IImportPOCommandRepository> _mockCmd = new(MockBehavior.Loose);
        private readonly Mock<IImportPOQueryRepository> _mockQry = new(MockBehavior.Loose);
        private readonly Mock<IMiscMasterQueryRepository> _mockMisc = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<ITimeZoneService> _mockTz = new(MockBehavior.Loose);
        private readonly Mock<ILogger<ImportPOAmendmentCommandHandler>> _mockLogger = new(MockBehavior.Loose);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Loose);
        private readonly Mock<IOutboxEventPublisher> _mockOutbox = new(MockBehavior.Loose);

        private ImportPOAmendmentCommandHandler CreateSut() =>
            new(_mockCmd.Object, _mockQry.Object, _mockMisc.Object, _mockMapper.Object,
                _mockIp.Object, _mockTz.Object, _mockLogger.Object, _mockDocSeq.Object, _mockOutbox.Object);

        [Fact]
        public async Task Handle_NullData_ThrowsInvalidOperationException()
        {
            var command = new ImportPOAmendmentCommand { Data = null! };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Handle_ZeroId_ThrowsInvalidOperationException()
        {
            var command = new ImportPOAmendmentCommand { Data = new ImportPOUpdateDto { Id = 0 } };

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Handle_PONotFound_ThrowsInvalidOperationException()
        {
            var command = new ImportPOAmendmentCommand { Data = new ImportPOUpdateDto { Id = 99 } };
            _mockCmd.Setup(r => r.GetAggregateAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PurchaseOrderHeader?)null);

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_NotApproved_ThrowsInvalidOperationException()
        {
            var command = new ImportPOAmendmentCommand { Data = new ImportPOUpdateDto { Id = 1 } };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 5, PONumber = "IPO001" };
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
            var command = new ImportPOAmendmentCommand { Data = new ImportPOUpdateDto { Id = 1 } };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 10, PONumber = "IPO001", RevisionNo = 0 };
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
            var command = new ImportPOAmendmentCommand { Data = new ImportPOUpdateDto { Id = 1 } };
            var existing = new PurchaseOrderHeader { Id = 1, StatusId = 10, PONumber = "IPO001", RevisionNo = 0 };
            _mockCmd.Setup(r => r.GetAggregateAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(existing);
            _mockMisc.Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new PurchaseManagement.Domain.Entities.MiscMaster { Id = 10 });
            _mockQry.Setup(r => r.HasAnyGrnAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);
            _mockTz.Setup(t => t.GetSystemTimeZone()).Returns("Asia/Kolkata");
            _mockIp.Setup(i => i.GetUnitId()).Returns(1);
            _mockIp.Setup(i => i.GetUserId()).Returns(1);
            _mockIp.Setup(i => i.GetUserName()).Returns("test-user");
            _mockIp.Setup(i => i.GetSystemIPAddress()).Returns("127.0.0.1");
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(1);
            _mockCmd.Setup(r => r.CreateExecutionStrategy()).Returns(new ImmediateExecutionStrategy());
            var mockEfTx = new Mock<IDbContextTransaction>(MockBehavior.Loose);
            var mockConn = new Mock<DbConnection>(MockBehavior.Loose);
            var mockDbTx = new Mock<DbTransaction>(MockBehavior.Loose);
            _mockCmd.Setup(r => r.BeginTransactionWithConnectionAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync((mockEfTx.Object, mockConn.Object, mockDbTx.Object));
            _mockCmd.Setup(r => r.AmendWithoutTransactionAsync(
                    It.IsAny<PurchaseOrderHeader>(), It.IsAny<ImportPOUpdateDto>(),
                    It.IsAny<PurchaseOrderHeader>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(55);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(55);
        }
    }
}
