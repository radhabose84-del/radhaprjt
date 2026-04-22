using Contracts.Interfaces;
using ProjectManagement.Infrastructure.Repositories.Common;

namespace ProjectManagement.IntegrationTests.Repositories.Common
{
    /// <summary>
    /// Tests for BaseQueryRepository — verifies the protected CompanyId / UnitId
    /// properties delegate correctly to IIPAddressService.
    /// Uses a concrete test-only subclass to expose the protected members.
    /// </summary>
    [Collection("DatabaseCollection")]
    public sealed class BaseQueryRepositoryTests
    {
        private readonly DbFixture _fixture;

        public BaseQueryRepositoryTests(DbFixture fixture) => _fixture = fixture;

        /// <summary>
        /// Concrete subclass that exposes protected members for testing.
        /// </summary>
        private sealed class TestableBaseQueryRepository : BaseQueryRepository
        {
            public TestableBaseQueryRepository(IIPAddressService ipAddressService)
                : base(ipAddressService) { }

            public int ExposedCompanyId => CompanyId;
            public int ExposedUnitId => UnitId;
            public IIPAddressService ExposedIpService => _ipAddressService;
        }

        // --- CompanyId ---

        [Fact]
        public void CompanyId_Should_Return_Value_From_IIPAddressService()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(42);

            var repo = new TestableBaseQueryRepository(ipMock.Object);

            repo.ExposedCompanyId.Should().Be(42);
        }

        [Fact]
        public void CompanyId_Should_Return_Zero_When_Null()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns((int?)null);

            var repo = new TestableBaseQueryRepository(ipMock.Object);

            repo.ExposedCompanyId.Should().Be(0);
        }

        // --- UnitId ---

        [Fact]
        public void UnitId_Should_Return_Value_From_IIPAddressService()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns(7);

            var repo = new TestableBaseQueryRepository(ipMock.Object);

            repo.ExposedUnitId.Should().Be(7);
        }

        [Fact]
        public void UnitId_Should_Return_Zero_When_Null()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetUnitId()).Returns((int?)null);

            var repo = new TestableBaseQueryRepository(ipMock.Object);

            repo.ExposedUnitId.Should().Be(0);
        }

        // --- IIPAddressService injection ---

        [Fact]
        public void Constructor_Should_Store_IIPAddressService()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);

            var repo = new TestableBaseQueryRepository(ipMock.Object);

            repo.ExposedIpService.Should().BeSameAs(ipMock.Object);
        }

        [Fact]
        public void CompanyId_And_UnitId_Should_Be_Independently_Configured()
        {
            var ipMock = new Mock<IIPAddressService>(MockBehavior.Loose);
            ipMock.Setup(x => x.GetCompanyId()).Returns(100);
            ipMock.Setup(x => x.GetUnitId()).Returns(200);

            var repo = new TestableBaseQueryRepository(ipMock.Object);

            repo.ExposedCompanyId.Should().Be(100);
            repo.ExposedUnitId.Should().Be(200);
        }
    }
}
