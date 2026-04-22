using Contracts.Interfaces;
using SalesManagement.Infrastructure.Repositories.Common;
using SalesManagement.IntegrationTests.Common;

namespace SalesManagement.IntegrationTests.Repositories.Common
{
    /// <summary>
    /// Integration tests for <see cref="BaseQueryRepository"/>.
    /// BaseQueryRepository is a thin wrapper that exposes CompanyId and UnitId
    /// from IIPAddressService. These tests verify that the properties correctly
    /// delegate to the injected service and apply the ?? 0 null-coalescing pattern.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class BaseQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BaseQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Concrete subclass to expose protected members for testing.
        /// BaseQueryRepository is public but its properties are protected.
        /// </summary>
        private sealed class TestableBaseQueryRepository : BaseQueryRepository
        {
            public TestableBaseQueryRepository(IIPAddressService ipAddressService)
                : base(ipAddressService) { }

            public int ExposedCompanyId => CompanyId;
            public int ExposedUnitId => UnitId;
        }

        // -----------------------------------------------------------------------
        // CompanyId
        // -----------------------------------------------------------------------

        [Fact]
        public void CompanyId_Should_Return_Value_From_IPAddressService()
        {
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(x => x.GetCompanyId()).Returns(42);

            var repo = new TestableBaseQueryRepository(mockIp.Object);

            repo.ExposedCompanyId.Should().Be(42);
        }

        [Fact]
        public void CompanyId_Should_Return_Zero_When_IPAddressService_Returns_Null()
        {
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(x => x.GetCompanyId()).Returns((int?)null);

            var repo = new TestableBaseQueryRepository(mockIp.Object);

            repo.ExposedCompanyId.Should().Be(0);
        }

        // -----------------------------------------------------------------------
        // UnitId
        // -----------------------------------------------------------------------

        [Fact]
        public void UnitId_Should_Return_Value_From_IPAddressService()
        {
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(x => x.GetUnitId()).Returns(7);

            var repo = new TestableBaseQueryRepository(mockIp.Object);

            repo.ExposedUnitId.Should().Be(7);
        }

        [Fact]
        public void UnitId_Should_Return_Zero_When_IPAddressService_Returns_Null()
        {
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(x => x.GetUnitId()).Returns((int?)null);

            var repo = new TestableBaseQueryRepository(mockIp.Object);

            repo.ExposedUnitId.Should().Be(0);
        }

        // -----------------------------------------------------------------------
        // Constructor
        // -----------------------------------------------------------------------

        [Fact]
        public void Constructor_Should_Store_IPAddressService()
        {
            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);

            var repo = new TestableBaseQueryRepository(mockIp.Object);

            // Verify the instance was created successfully
            repo.Should().NotBeNull();
        }

        [Fact]
        public void DbFixture_IpMock_Should_Provide_Expected_Defaults()
        {
            // Verify the fixture's IpMock matches what BaseQueryRepository consumers expect
            var repo = new TestableBaseQueryRepository(_fixture.IpMock.Object);

            repo.ExposedCompanyId.Should().Be(1);
            repo.ExposedUnitId.Should().Be(1);
        }
    }
}
