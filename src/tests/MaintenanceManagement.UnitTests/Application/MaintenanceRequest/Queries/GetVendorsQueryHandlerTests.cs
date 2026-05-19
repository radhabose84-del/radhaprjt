using Contracts.Dtos.Lookups.Party;
using Contracts.Interfaces.Lookups.Party;
using MaintenanceManagement.Application.MaintenanceRequest.Queries.GetVendors;
using MaintenanceManagement.Domain.Events;
using MediatR;

namespace MaintenanceManagement.UnitTests.Application.MaintenanceRequest.Queries
{
    public sealed class GetVendorsQueryHandlerTests
    {
        private readonly Mock<ISupplierLookup> _mockSupplierLookup = new(MockBehavior.Strict);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVendorsQueryHandler CreateSut() =>
            new(_mockSupplierLookup.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_WithResults_ReturnsSuccessAndData()
        {
            var suppliers = new List<SupplierLookupDto>
            {
                new() { Id = 1, VendorCode = "V001", VendorName = "Acme Supplies" }
            };
            _mockSupplierLookup
                .Setup(s => s.SearchSuppliersAsync("Acme", It.IsAny<CancellationToken>()))
                .ReturnsAsync(suppliers);

            var result = await CreateSut().Handle(
                new GetVendorsQuery { Term = "Acme" }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
            result.Data![0].VendorName.Should().Be("Acme Supplies");
        }

        [Fact]
        public async Task Handle_NoResults_ReturnsSuccessWithEmptyData()
        {
            _mockSupplierLookup
                .Setup(s => s.SearchSuppliersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SupplierLookupDto>());

            var result = await CreateSut().Handle(new GetVendorsQuery(), CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.Message.Should().Be("No vendors found.");
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            _mockSupplierLookup
                .Setup(s => s.SearchSuppliersAsync(It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<SupplierLookupDto>());

            await CreateSut().Handle(new GetVendorsQuery(), CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(
                    It.Is<AuditLogsDomainEvent>(e => e.ActionCode == "VENDOR_SEARCH"),
                    It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
