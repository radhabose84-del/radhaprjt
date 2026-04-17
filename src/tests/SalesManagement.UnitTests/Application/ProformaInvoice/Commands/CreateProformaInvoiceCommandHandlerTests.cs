using AutoMapper;
using Contracts.Common;
using Contracts.Interfaces;
using Contracts.Interfaces.Lookups.Finance;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IProformaInvoice;
using SalesManagement.Application.ProformaInvoice.Commands.CreateProformaInvoice;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.ProformaInvoice.Commands
{
    public sealed class CreateProformaInvoiceCommandHandlerTests
    {
        private readonly Mock<IProformaInvoiceCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IProformaInvoiceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IDocumentSequenceLookup> _mockDocSeq = new(MockBehavior.Strict);
        private readonly Mock<IIPAddressService> _mockIp = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);

        private CreateProformaInvoiceCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockDocSeq.Object,
                _mockIp.Object, _mockMediator.Object, _mockMapper.Object);

        private void SetupHappyPath(int newId = 1, int? typeId = 7, string proformaNo = "PI-001", decimal soBalance = 100m)
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.ProformaInvoice>(It.IsAny<CreateProformaInvoiceCommand>()))
                .Returns(new SalesManagement.Domain.Entities.ProformaInvoice());
            _mockQueryRepo.Setup(r => r.GetSalesOrderBalanceAsync(It.IsAny<int>())).ReturnsAsync(soBalance);
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(typeId);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>()))
                .ReturnsAsync(new List<string> { proformaNo });
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ProformaInvoice>(), It.IsAny<int>()))
                .ReturnsAsync(newId);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsSuccess()
        {
            SetupHappyPath();
            var result = await CreateSut().Handle(new CreateProformaInvoiceCommand { ProformaAmount = 50m, SalesOrderId = 1 }, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            SetupHappyPath(newId: 99);
            var result = await CreateSut().Handle(new CreateProformaInvoiceCommand { ProformaAmount = 50m, SalesOrderId = 1 }, CancellationToken.None);
            result.Data.Should().Be(99);
        }

        [Fact]
        public async Task Handle_Calculates_SOBalance_From_Query()
        {
            SetupHappyPath(soBalance: 200m);
            SalesManagement.Domain.Entities.ProformaInvoice? captured = null;
            _mockCommandRepo.Setup(r => r.CreateAsync(It.IsAny<SalesManagement.Domain.Entities.ProformaInvoice>(), It.IsAny<int>()))
                .Callback<SalesManagement.Domain.Entities.ProformaInvoice, int>((e, _) => captured = e)
                .ReturnsAsync(1);

            await CreateSut().Handle(new CreateProformaInvoiceCommand { ProformaAmount = 75m, SalesOrderId = 1 }, CancellationToken.None);

            captured!.SOBalance.Should().Be(125m); // 200 - 75
        }

        [Fact]
        public async Task Handle_TypeIdNotFound_ThrowsExceptionRules()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.ProformaInvoice>(It.IsAny<CreateProformaInvoiceCommand>()))
                .Returns(new SalesManagement.Domain.Entities.ProformaInvoice());
            _mockQueryRepo.Setup(r => r.GetSalesOrderBalanceAsync(It.IsAny<int>())).ReturnsAsync(100m);
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync((int?)null);

            Func<Task> act = async () => await CreateSut().Handle(new CreateProformaInvoiceCommand { ProformaAmount = 50m, SalesOrderId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Transaction Type*not found*");
        }

        [Fact]
        public async Task Handle_NoDocumentSequence_ThrowsExceptionRules()
        {
            _mockIp.Setup(s => s.GetUnitId()).Returns(1);
            _mockMapper.Setup(m => m.Map<SalesManagement.Domain.Entities.ProformaInvoice>(It.IsAny<CreateProformaInvoiceCommand>()))
                .Returns(new SalesManagement.Domain.Entities.ProformaInvoice());
            _mockQueryRepo.Setup(r => r.GetSalesOrderBalanceAsync(It.IsAny<int>())).ReturnsAsync(100m);
            _mockDocSeq.Setup(d => d.GetTransactionTypeIdAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .ReturnsAsync(7);
            _mockDocSeq.Setup(d => d.GenerateDocumentNumber(It.IsAny<int>())).ReturnsAsync(new List<string>());

            Func<Task> act = async () => await CreateSut().Handle(new CreateProformaInvoiceCommand { ProformaAmount = 50m, SalesOrderId = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*No document sequence configured*");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            SetupHappyPath();
            await CreateSut().Handle(new CreateProformaInvoiceCommand { ProformaAmount = 50m, SalesOrderId = 1 }, CancellationToken.None);
            _mockMediator.Verify(
                m => m.Publish(It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "PROFORMA_CREATE"), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
