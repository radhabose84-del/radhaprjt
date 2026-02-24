#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Queries
{
    public class GetSalesOrganisationByIdQueryHandlerTests
    {
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetSalesOrganisationByIdQueryHandler CreateSut() =>
            new GetSalesOrganisationByIdQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccess()
        {
            // Arrange
            var query = new GetSalesOrganisationByIdQuery { Id = 1 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");

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
            var query = new GetSalesOrganisationByIdQuery { Id = 1 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001", name: "Test Organisation");

            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data.Id.Should().Be(1);
            result.Data.SalesOrganisationCode.Should().Be("ORG001");
            result.Data.SalesOrganisationName.Should().Be("Test Organisation");
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsSuccessMessage()
        {
            // Arrange
            var query = new GetSalesOrganisationByIdQuery { Id = 1 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 1);

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
            var query = new GetSalesOrganisationByIdQuery { Id = 7 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 7);

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
            var query = new GetSalesOrganisationByIdQuery { Id = 99 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesOrganisationDto)null);

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
            var query = new GetSalesOrganisationByIdQuery { Id = 99 };

            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesOrganisationDto)null);

            var sut = CreateSut();

            // Act
            Func<Task> act = async () => await sut.Handle(query, CancellationToken.None);

            // Assert — message should mention "Sales Organisation"
            await act.Should().ThrowAsync<EntityNotFoundException>()
                .WithMessage("*Sales Organisation*");
        }
    }
}
