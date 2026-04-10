using AutoMapper;
using Contracts.Interfaces.Lookups.Users;
using Contracts.Interfaces.Lookups.Warehouse;
using MediatR;
using PurchaseManagement.Application.Common.Interfaces.IMRS;
using PurchaseManagement.Application.MRS.Queries.GetMrsEntry;

namespace PurchaseManagement.UnitTests.Application.MRS.Queries
{
    public sealed class GetMrsEntryQueryHandlerTests
    {
        private readonly Mock<IMrsEntryQueryRepository> _mockRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<IDepartmentLookup> _mockDeptLookup = new(MockBehavior.Loose);
        private readonly Mock<IWarehouseLookup> _mockWarehouseLookup = new(MockBehavior.Loose);

        private GetMrsEntryQueryHandler CreateSut() =>
            new(_mockRepo.Object, _mockMapper.Object, _mockMediator.Object,
                _mockDeptLookup.Object, _mockWarehouseLookup.Object);

        [Fact]
        public void Constructor_CreatesHandler()
        {
            var sut = CreateSut();
            sut.Should().NotBeNull();
        }

        [Fact]
        public void QueryClass_Properties_ShouldBeAssignable()
        {
            var query = new GetMrsEntryQuery
            {
                PageNumber = 1,
                PageSize = 15,
                SearchTerm = "test",
                FromDate = DateTimeOffset.UtcNow.AddDays(-7),
                ToDate = DateTimeOffset.UtcNow
            };
            query.PageNumber.Should().Be(1);
            query.SearchTerm.Should().Be("test");
        }
    }
}
