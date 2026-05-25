using Contracts.Interfaces;
using Microsoft.EntityFrameworkCore;
using QCManagement.Application.Common.Interfaces;
using QCManagement.Infrastructure.Data;
using QCManagement.Presentation.Validation.Common;

namespace QCManagement.UnitTests.TestHelpers
{
    internal static class TestMaxLengthProviderFactory
    {
        private static MaxLengthProvider? _instance;

        public static MaxLengthProvider Create()
        {
            if (_instance != null)
                return _instance;

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("UnitTest_MaxLength_" + Guid.NewGuid().ToString("N"))
                .Options;

            var mockIp = new Mock<IIPAddressService>();
            mockIp.Setup(s => s.GetSystemIPAddress()).Returns("127.0.0.1");
            mockIp.Setup(s => s.GetUserIPAddress()).Returns("127.0.0.1");
            mockIp.Setup(s => s.GetUserName()).Returns("test-user");
            mockIp.Setup(s => s.GetUserId()).Returns(1);

            var mockTz = new Mock<ITimeZoneService>();
            mockTz.Setup(s => s.GetCurrentTime(It.IsAny<string?>())).Returns(DateTimeOffset.UtcNow);

            var ctx = new ApplicationDbContext(options, mockIp.Object, mockTz.Object);
            _instance = new MaxLengthProvider(ctx);
            return _instance;
        }
    }
}
