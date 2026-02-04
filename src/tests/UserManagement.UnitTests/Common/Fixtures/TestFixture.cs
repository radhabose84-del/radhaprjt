using AutoMapper;
using UserManagement.Application.Users.Queries.GetUserById;
using Microsoft.Extensions.Logging;

namespace UserManagement.UnitTests.Common.Fixtures
{
    public class TestFixture
    {
        public IMapper Mapper { get; }
        public ILoggerFactory LoggerFactory { get; } = Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance;

        public TestFixture()
        {
            var cfg = new MapperConfiguration(c =>
            {
                // pick a type from your Application assembly
                 c.AddMaps(typeof(GetUserByIdQueryHandler).Assembly);
            });
            Mapper = cfg.CreateMapper();
        }

    }
}