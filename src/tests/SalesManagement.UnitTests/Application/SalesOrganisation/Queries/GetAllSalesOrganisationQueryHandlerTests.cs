#nullable disable
using Contracts.Common;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Application.SalesOrganisation.Queries.GetAllSalesOrganisation;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Queries
{
    public class GetAllSalesOrganisationQueryHandlerTests
    {
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);

        private GetAllSalesOrganisationQueryHandler CreateSut() =>
            new GetAllSalesOrganisationQueryHandler(_mockQueryRepo.Object);

        // ── Tests ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Handle_ReturnsSuccess_WhenRepositoryReturnsData()
        {
            // Arrange
            var query = new GetAllSalesOrganisationQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<SalesOrganisationDto>
            {
                SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001"),
                SalesOrganisationBuilders.ValidDto(id: 2, code: "ORG002")
            };

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.IsSuccess.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_ReturnsCorrectData_FromRepository()
        {
            // Arrange
            var query = new GetAllSalesOrganisationQuery { PageNumber = 1, PageSize = 10, SearchTerm = null };
            var dtoList = new List<SalesOrganisationDto>
            {
                SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001"),
                SalesOrganisationBuilders.ValidDto(id: 2, code: "ORG002")
            };

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 2));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Data.Should().NotBeNull();
            result.Data.Should().HaveCount(2);
            result.Data[0].SalesOrganisationCode.Should().Be("ORG001");
            result.Data[1].SalesOrganisationCode.Should().Be("ORG002");
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPaginationMetadata()
        {
            // Arrange
            var query = new GetAllSalesOrganisationQuery { PageNumber = 2, PageSize = 5, SearchTerm = "ORG" };
            var dtoList = new List<SalesOrganisationDto>
            {
                SalesOrganisationBuilders.ValidDto(id: 6, code: "ORG006")
            };

            _mockQueryRepo.Setup(r => r.GetAllAsync(2, 5, "ORG")).ReturnsAsync((dtoList, 6));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.TotalCount.Should().Be(6);
            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsEmptyListWithZeroCount()
        {
            // Arrange
            var query = new GetAllSalesOrganisationQuery { PageNumber = 1, PageSize = 10, SearchTerm = "NOTFOUND" };
            var emptyList = new List<SalesOrganisationDto>();

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, "NOTFOUND")).ReturnsAsync((emptyList, 0));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.IsSuccess.Should().BeTrue();
            result.Data.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
        }

        [Fact]
        public async Task Handle_CallsGetAllAsync_Once_WithCorrectParameters()
        {
            // Arrange
            var query = new GetAllSalesOrganisationQuery { PageNumber = 3, PageSize = 20, SearchTerm = "test" };
            var dtoList = new List<SalesOrganisationDto>();

            _mockQueryRepo.Setup(r => r.GetAllAsync(3, 20, "test")).ReturnsAsync((dtoList, 0));

            var sut = CreateSut();

            // Act
            await sut.Handle(query, CancellationToken.None);

            // Assert
            _mockQueryRepo.Verify(r => r.GetAllAsync(3, 20, "test"), Times.Once);
        }

        [Fact]
        public async Task Handle_ReturnsSuccessMessage()
        {
            // Arrange
            var query = new GetAllSalesOrganisationQuery { PageNumber = 1, PageSize = 10 };
            var dtoList = new List<SalesOrganisationDto>();

            _mockQueryRepo.Setup(r => r.GetAllAsync(1, 10, null)).ReturnsAsync((dtoList, 0));

            var sut = CreateSut();

            // Act
            var result = await sut.Handle(query, CancellationToken.None);

            // Assert
            result.Message.Should().Contain("retrieved");
        }
    }
}
