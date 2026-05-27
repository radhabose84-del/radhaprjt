using AutoMapper;
using Contracts.Dtos.Lookups.Purchase;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IVendorRatingGrade;
using PurchaseManagement.Application.VendorRatingGrade.Queries.GetVendorRatingGradeAutoComplete;
using PurchaseManagement.Domain.Events;
using PurchaseManagement.UnitTests.TestData;

namespace PurchaseManagement.UnitTests.Application.VendorRatingGrade.Queries
{
    public sealed class GetVendorRatingGradeAutoCompleteQueryHandlerTests
    {
        private readonly Mock<IVendorRatingGradeQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetVendorRatingGradeAutoCompleteQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsLookupList()
        {
            var lookups = VendorRatingGradeBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Exc", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<VendorRatingGradeLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());
            var result = await CreateSut().Handle(new GetVendorRatingGradeAutoCompleteQuery("Exc"), CancellationToken.None);
            result.Should().NotBeEmpty();
            result.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_PublishesAuditEvent()
        {
            var lookups = VendorRatingGradeBuilders.ValidLookupList();
            _mockQueryRepo.Setup(r => r.AutocompleteAsync("Exc", It.IsAny<CancellationToken>())).ReturnsAsync(lookups);
            _mockMapper.Setup(m => m.Map<List<VendorRatingGradeLookupDto>>(It.IsAny<object>())).Returns(lookups.ToList());
            await CreateSut().Handle(new GetVendorRatingGradeAutoCompleteQuery("Exc"), CancellationToken.None);
            _mockMediator.Verify(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
