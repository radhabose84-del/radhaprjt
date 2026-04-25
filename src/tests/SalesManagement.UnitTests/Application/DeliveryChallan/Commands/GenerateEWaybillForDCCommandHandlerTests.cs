using Contracts.Common;
using Contracts.Dtos.Lookups.Finance;
using Contracts.Dtos.Lookups.Inventory;
using Contracts.Dtos.Lookups.Party;
using Contracts.Dtos.Lookups.Users;
using Contracts.Interfaces.Lookups.Finance;
using Contracts.Interfaces.Lookups.Inventory;
using Contracts.Interfaces.Lookups.Party;
using Contracts.Interfaces.Lookups.Users;
using FinanceManagement.Application.EWaybillHeader.Commands.CreateEWaybillHeader;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDeliveryChallan;
using SalesManagement.Application.DeliveryChallan.Commands.GenerateEWaybillForDC;
using SalesManagement.Application.DeliveryChallan.Dto;
using SalesManagement.Domain.Common;
using SalesManagement.Domain.Events;

namespace SalesManagement.UnitTests.Application.DeliveryChallan.Commands
{
    public sealed class GenerateEWaybillForDCCommandHandlerTests
    {
        private readonly Mock<IDeliveryChallanQueryRepository> _mockDcRepo = new(MockBehavior.Strict);
        private readonly Mock<IEWaybillLookup> _mockEwbLookup = new(MockBehavior.Strict);
        private readonly Mock<IUnitLookup> _mockUnitLookup = new(MockBehavior.Loose);
        private readonly Mock<ICompanyLookup> _mockCompanyLookup = new(MockBehavior.Loose);
        private readonly Mock<IPartyLookup> _mockPartyLookup = new(MockBehavior.Loose);
        private readonly Mock<IItemLookup> _mockItemLookup = new(MockBehavior.Loose);
        private readonly Mock<IUOMLookup> _mockUomLookup = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Strict);

        private GenerateEWaybillForDCCommandHandler CreateSut() =>
            new(_mockDcRepo.Object, _mockEwbLookup.Object, _mockUnitLookup.Object,
                _mockCompanyLookup.Object, _mockPartyLookup.Object,
                _mockItemLookup.Object, _mockUomLookup.Object, _mockMediator.Object);

        private static DeliveryChallanHeaderDto BuildDc(int id = 1) => new()
        {
            Id = id,
            DeliveryNumber = "DC-2026-0001",
            DeliveryDate = new DateOnly(2026, 4, 24),
            FromPlantId = 10,
            ToPlantId = 20,
            TransporterId = 99,
            VehicleNumber = "TN01-AB-1234",
            TransportDistance = 150.5m,
            ConsignmentValue = 45000m,
            DeliveryValue = 45000m
        };

        private void SetupLookups()
        {
            _mockUnitLookup
                .Setup(u => u.GetByIdAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 10, CompanyId = 1 });
            _mockUnitLookup
                .Setup(u => u.GetByIdAsync(20, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new UnitLookupDto { UnitId = 20, CompanyId = 2 });

            _mockCompanyLookup
                .Setup(c => c.GetAllCompanyAsync())
                .ReturnsAsync(new List<CompanyLookupDto>
                {
                    new() { CompanyId = 1, GstNumber = "33AACCA8432H1ZX", LegalName = "Consignor Ltd" },
                    new() { CompanyId = 2, GstNumber = "29AACCA8432H1ZX", LegalName = "Consignee Ltd" }
                });

            _mockPartyLookup
                .Setup(p => p.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PartyLookupDto { Id = 99, PartyName = "ABC Transports", GstNumber = "33AAAAA0000A1Z9" });
        }

        [Fact]
        public async Task Handle_DCNotFound_ThrowsExceptionRules()
        {
            _mockDcRepo.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((DeliveryChallanHeaderDto?)null);

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(new GenerateEWaybillForDCCommand(999), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*not found*");
        }

        [Fact]
        public async Task Handle_EWaybillAlreadyExists_ReturnsExistingWithAlreadyExistedTrue()
        {
            var dc = BuildDc();
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new EWaybillLookupDto { Id = 77, EWBNumber = "EWB-9999", EwbStatus = "Generated" });

            var result = await CreateSut().Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.AlreadyExisted.Should().BeTrue();
            result.Data.EWaybillHeaderId.Should().Be(77);
            result.Data.EwbNumber.Should().Be("EWB-9999");
        }

        [Fact]
        public async Task Handle_ValidDC_CreatesEWaybillAndReturnsId()
        {
            var dc = BuildDc();
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EWaybillLookupDto?)null);
            SetupLookups();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 42 });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data!.AlreadyExisted.Should().BeFalse();
            result.Data.EWaybillHeaderId.Should().Be(42);
            result.Data.EwbStatus.Should().Be("Pending");
            result.Data.DeliveryNumber.Should().Be("DC-2026-0001");
        }

        [Fact]
        public async Task Handle_ValidDC_MapsDcFieldsIntoCreateCommand()
        {
            var dc = BuildDc();
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EWaybillLookupDto?)null);
            SetupLookups();

            CreateEWaybillHeaderCommand? captured = null;
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ApiResponseDTO<int>>, CancellationToken>((cmd, _) => captured = (CreateEWaybillHeaderCommand)cmd)
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 42 });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            captured.Should().NotBeNull();
            captured!.UnitId.Should().Be(10);
            captured.InvoiceNo.Should().Be("DC-2026-0001");
            captured.InvoiceValue.Should().Be(45000m);
            captured.TotalValue.Should().Be(45000m);
            captured.SupplyType.Should().Be("Outward");
            captured.SubSupplyType.Should().Be("For Own Use");
            captured.DocumentType.Should().Be("Delivery Challan");
            captured.TransactionType.Should().Be(1);
            captured.FromGSTIN.Should().Be("33AACCA8432H1ZX");
            captured.FromTradeName.Should().Be("Consignor Ltd");
            captured.ToGSTIN.Should().Be("29AACCA8432H1ZX");
            captured.ToTradeName.Should().Be("Consignee Ltd");
            captured.CGST.Should().Be(0);
            captured.SGST.Should().Be(0);
            captured.IGST.Should().Be(0);
            captured.TransporterId.Should().Be(99);
            captured.TransporterGSTIN.Should().Be("33AAAAA0000A1Z9");
            captured.TransporterName.Should().Be("ABC Transports");
            captured.VehicleNo.Should().Be("TN01-AB-1234");
            captured.Distance.Should().Be(151);
            captured.EwbStatus.Should().Be("Pending");
        }

        [Fact]
        public async Task Handle_ValidDC_PublishesAuditEvent()
        {
            var dc = BuildDc();
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EWaybillLookupDto?)null);
            SetupLookups();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 42 });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "DELIVERYCHALLAN_GENERATE_EWAYBILL"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_CreateCommandFails_ThrowsExceptionRules()
        {
            var dc = BuildDc();
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EWaybillLookupDto?)null);
            SetupLookups();
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = false, Message = "DB down" });

            var sut = CreateSut();
            Func<Task> act = async () => await sut.Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            await act.Should().ThrowAsync<ExceptionRules>().WithMessage("*Failed to create e-waybill*");
        }

        [Fact]
        public async Task Handle_DcWithDetails_MapsLinesIntoCommandDetails()
        {
            // Arrange — DC with two line items
            var dc = BuildDc();
            dc.DeliveryChallanDetails = new List<DeliveryChallanDetailDto>
            {
                new()
                {
                    Id = 10, ItemId = 2222, UOMId = 2,
                    DispatchQuantity = 3m, LineMovementValue = 4500m, ItemName = "Yarn"
                },
                new()
                {
                    Id = 11, ItemId = 3333, UOMId = 5,
                    DispatchQuantity = 12m, LineMovementValue = 8000m, ItemName = "Cotton"
                }
            };
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EWaybillLookupDto?)null);
            SetupLookups();

            _mockItemLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<ItemLookupDto>)new List<ItemLookupDto>
                {
                    new() { Id = 2222, ItemName = "Yarn",   HSNCode = "5205" },
                    new() { Id = 3333, ItemName = "Cotton", HSNCode = "5201" }
                });
            _mockUomLookup
                .Setup(l => l.GetByIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<UOMLookupDto>)new List<UOMLookupDto>
                {
                    new() { Id = 2, Code = "KGS", UOMName = "Kilograms" },
                    new() { Id = 5, Code = "NOS", UOMName = "Numbers"   }
                });

            CreateEWaybillHeaderCommand? captured = null;
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ApiResponseDTO<int>>, CancellationToken>(
                    (cmd, _) => captured = (CreateEWaybillHeaderCommand)cmd)
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 99 });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // Act
            await CreateSut().Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            // Assert — two details in the captured command, lookups applied
            captured.Should().NotBeNull();
            captured!.Details.Should().HaveCount(2);

            var line1 = captured.Details[0];
            line1.ItemSno.Should().Be(1);
            line1.ItemId.Should().Be(2222);
            line1.ItemName.Should().Be("Yarn");
            line1.HsnNo.Should().Be("5205");
            line1.IsService.Should().Be("N");
            line1.Qty.Should().Be(3m);
            line1.UOM.Should().Be("KGS");
            line1.TaxableValue.Should().Be(4500m);
            line1.CGST.Should().Be(0);
            line1.SGST.Should().Be(0);

            var line2 = captured.Details[1];
            line2.ItemSno.Should().Be(2);
            line2.ItemId.Should().Be(3333);
            line2.HsnNo.Should().Be("5201");
            line2.UOM.Should().Be("NOS");
            line2.Qty.Should().Be(12m);
        }

        [Fact]
        public async Task Handle_DcWithNoDetails_MapsEmptyDetailsList()
        {
            var dc = BuildDc();
            dc.DeliveryChallanDetails = null;  // simulates "DC loaded without details"
            _mockDcRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dc);
            _mockEwbLookup
                .Setup(l => l.GetByDCAsync("DC-2026-0001", 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync((EWaybillLookupDto?)null);
            SetupLookups();

            CreateEWaybillHeaderCommand? captured = null;
            _mockMediator
                .Setup(m => m.Send(It.IsAny<CreateEWaybillHeaderCommand>(), It.IsAny<CancellationToken>()))
                .Callback<IRequest<ApiResponseDTO<int>>, CancellationToken>(
                    (cmd, _) => captured = (CreateEWaybillHeaderCommand)cmd)
                .ReturnsAsync(new ApiResponseDTO<int> { IsSuccess = true, Data = 42 });
            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GenerateEWaybillForDCCommand(1), CancellationToken.None);

            captured!.Details.Should().BeEmpty();
        }
    }
}
