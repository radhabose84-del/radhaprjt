using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Application.StoHeader.Queries.GetPendingStoHeader;

namespace SalesManagement.UnitTests.Application.StoHeader.Queries
{
    public sealed class GetPendingStoHeaderQueryHandlerTests
    {
        private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetPendingStoHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetPendingAsync(1, 10, null))
                .ReturnsAsync((new List<StoHeaderDto>(), 0));

            var result = await CreateSut().Handle(
                new GetPendingStoHeaderQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }
    }
}
