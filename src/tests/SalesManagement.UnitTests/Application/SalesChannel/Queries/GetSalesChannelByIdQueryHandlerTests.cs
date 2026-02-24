#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ISalesChannel;
using SalesManagement.Application.SalesChannel.Dto;
using SalesManagement.Application.SalesChannel.Queries.GetSalesChannelById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesChannel.Queries
{
    public class GetSalesChannelByIdQueryHandlerTests
    {
        private readonly Mock<ISalesChannelQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesChannelByIdQueryHandler CreateSut() =>
            new GetSalesChannelByIdQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            var query = new GetSalesChannelByIdQuery { Id = 1 };
            var dto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetSalesChannelByIdQuery { Id = 1 };
            var dto = SalesChannelBuilders.ValidDto(id: 1, code: "CH001", name: "Test Channel");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(1);
            result.Data.SalesChannelCode.Should().Be("CH001");
            result.Data.SalesChannelName.Should().Be("Test Channel");
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            var query = new GetSalesChannelByIdQuery { Id = 1 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(SalesChannelBuilders.ValidDto(id: 1));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.Message.Should().Contain("retrieved");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var query = new GetSalesChannelByIdQuery { Id = 7 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(SalesChannelBuilders.ValidDto(id: 7));

            await CreateSut().Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            var query = new GetSalesChannelByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesChannelDto)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ExceptionMessage_ContainsEntityType()
        {
            var query = new GetSalesChannelByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((SalesChannelDto)null);

            Func<Task> act = async () => await CreateSut().Handle(query, CancellationToken.None);

            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Sales Channel*");
        }
    }
}
