using Contracts.Common;
using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.Common.Interfaces.IMiscMaster;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceStock;


namespace SalesManagement.UnitTests.Application.DispatchAdvice.Queries
{
    public sealed class GetDispatchAdviceStockQueryHandlerTests
    {
        private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMiscMasterQueryRepository> _mockMiscRepo = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDispatchAdviceStockQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMiscRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsStockList()
        {
            _mockMiscRepo
                .Setup(r => r.GetMiscMasterByName(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new SalesManagement.Domain.Entities.MiscMaster { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.GetStockAsync(1, 1, 1))
                .ReturnsAsync(new List<DispatchAdviceStockDto> { new() });

            var result = await CreateSut().Handle(
                new GetDispatchAdviceStockQuery { ItemId = 1, LotId = 1 }, CancellationToken.None);

            result.Should().HaveCount(1);
        }
    }
}
