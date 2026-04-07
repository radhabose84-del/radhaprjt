using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.IDispatchAdvice;
using SalesManagement.Application.DispatchAdvice.Dto;
using SalesManagement.Application.DispatchAdvice.Queries.GetDispatchAdviceById;

namespace SalesManagement.UnitTests.Application.DispatchAdvice.Queries
{
    public sealed class GetDispatchAdviceByIdQueryHandlerTests
    {
        private readonly Mock<IDispatchAdviceQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetDispatchAdviceByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingId_ReturnsDto()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new DispatchAdviceHeaderDto { Id = 1 });

            var result = await CreateSut().Handle(new GetDispatchAdviceByIdQuery { Id = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_NonExistentId_ReturnsNull()
        {
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((DispatchAdviceHeaderDto?)null);

            var result = await CreateSut().Handle(new GetDispatchAdviceByIdQuery { Id = 99 }, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}
