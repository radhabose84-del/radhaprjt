using BudgetManagement.Application.Common.Interfaces;
using BudgetManagement.Infrastructure.Data;
using BudgetManagement.Presentation.Validation.Common;
using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BudgetManagement.UnitTests.TestHelpers
{
    internal static class TestMaxLengthProviderFactory
    {
        private static MaxLengthProvider? _instance;

        public static MaxLengthProvider Create()
        {
            if (_instance != null)
                return _instance;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("BudgetUnitTest_MaxLength_" + Guid.NewGuid().ToString("N"))
                .Options;

            var mockIp = new Mock<IIPAddressService>(MockBehavior.Loose);
            mockIp.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            mockIp.Setup(s => s.GetUserName()).Returns("test-user");
            mockIp.Setup(s => s.GetUserId()).Returns(1);
            mockIp.Setup(s => s.GetCompanyId()).Returns(1);
            mockIp.Setup(s => s.GetUnitId()).Returns(1);
            mockIp.Setup(s => s.GetGroupCode()).Returns("ADMIN");
            mockIp.Setup(s => s.GetEntityId()).Returns(1);

            var mockTz = new Mock<ITimeZoneService>(MockBehavior.Loose);
            mockTz.Setup(s => s.GetCurrentTime(It.IsAny<string>())).Returns(DateTimeOffset.UtcNow);
            mockTz.Setup(s => s.GetSystemTimeZone()).Returns("UTC");

            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            _instance = new MaxLengthProvider(ctx);
            return _instance;
        }
    }
}
