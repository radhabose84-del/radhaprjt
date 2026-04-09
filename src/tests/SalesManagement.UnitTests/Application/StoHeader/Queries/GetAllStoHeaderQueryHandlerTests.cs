using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoHeader;
using SalesManagement.Application.StoHeader.Dto;
using SalesManagement.Application.StoHeader.Queries.GetAllStoHeader;

namespace SalesManagement.UnitTests.Application.StoHeader.Queries
{
    public sealed class GetAllStoHeaderQueryHandlerTests
    {
        private readonly Mock<IStoHeaderQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllStoHeaderQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var data = new List<StoHeaderDto> { new() };
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 1));

            _mockMapper
                .Setup(m => m.Map<List<StoHeaderDto>>(It.IsAny<List<StoHeaderDto>>()))
                .Returns(data);

            var result = await CreateSut().Handle(
                new GetAllStoHeaderQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            var data = new List<StoHeaderDto>();
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((data, 0));

            _mockMapper
                .Setup(m => m.Map<List<StoHeaderDto>>(It.IsAny<List<StoHeaderDto>>()))
                .Returns(data);

            var result = await CreateSut().Handle(
                new GetAllStoHeaderQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
