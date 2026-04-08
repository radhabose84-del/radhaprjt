using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IStoReceipt;
using SalesManagement.Application.StoReceipt.Dto;
using SalesManagement.Application.StoReceipt.Queries.GetAllStoReceipt;

namespace SalesManagement.UnitTests.Application.StoReceipt.Queries
{
    public sealed class GetAllStoReceiptQueryHandlerTests
    {
        private readonly Mock<IStoReceiptQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllStoReceiptQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<StoReceiptHeaderDto> { new() }, 1));

            var result = await CreateSut().Handle(
                new GetAllStoReceiptQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsSuccess()
        {
            _mockQueryRepo
                .Setup(r => r.GetAllAsync(1, 10, null))
                .ReturnsAsync((new List<StoReceiptHeaderDto>(), 0));

            var result = await CreateSut().Handle(
                new GetAllStoReceiptQuery { PageNumber = 1, PageSize = 10 }, CancellationToken.None);

            result.Data.Should().BeEmpty();
        }
    }
}
