#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ISalesGroup;
using SalesManagement.Application.SalesGroup.Dto;
using SalesManagement.Application.SalesGroup.Queries.GetSalesGroupById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesGroup.Queries
{
    public class GetSalesGroupByIdQueryHandlerTests
    {
        private readonly Mock<ISalesGroupQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesGroupByIdQueryHandler CreateSut() =>
            new GetSalesGroupByIdQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            // Arrange
            var query = new GetSalesGroupByIdQuery { Id = 1 };
            var dto = SalesGroupBuilders.ValidDto(id: 1, name: "Test Sales Group");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            // Arrange
            var query = new GetSalesGroupByIdQuery { Id = 1 };
            var dto = SalesGroupBuilders.ValidDto(id: 1, name: "North Region Group");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(1);
            result.Data.SalesGroupName.Should().Be("North Region Group");
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            // Arrange
            var query = new GetSalesGroupByIdQuery { Id = 1 };
            var dto = SalesGroupBuilders.ValidDto(id: 1);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Message.Should().Contain("retrieved");
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            // Arrange
            var query = new GetSalesGroupByIdQuery { Id = 7 };
            var dto = SalesGroupBuilders.ValidDto(id: 7);

            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);

            var sut = CreateSut();

            // Act
            await sut.Handle(query, CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ThrowsEntityNotFoundException()
        {
            // Arrange
            var query = new GetSalesGroupByIdQuery { Id = 99 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesGroupDto)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(query, CancellationToken.None);

            // Assert
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*99*");
        }

        [Fact]
        public async Task Handle_EntityNotFound_ExceptionMessage_ContainsEntityType()
        {
            // Arrange
            var query = new GetSalesGroupByIdQuery { Id = 99 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesGroupDto)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(query, CancellationToken.None);

            // Assert — message should mention "Sales Group"
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Sales Group*");
        }
    }
}
