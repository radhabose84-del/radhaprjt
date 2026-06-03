using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ILocation;
using UserManagement.Application.Location.Queries.GetAllLocation;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Location.Queries
{
    public sealed class GetAllLocationQueryHandlerTests
    {
        private readonly Mock<ILocationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetAllLocationQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        private static List<UserManagement.Domain.Entities.Location> ValidEntityList() =>
            new()
            {
                new() { Id = 1, Code = "LOC-0001", LocationName = "Loc One" },
                new() { Id = 2, Code = "LOC-0002", LocationName = "Loc Two" }
            };

        private static List<GetAllLocationDto> ValidDtoList() =>
            new()
            {
                new() { Id = 1, Code = "LOC-0001", LocationName = "Loc One" },
                new() { Id = 2, Code = "LOC-0002", LocationName = "Loc Two" }
            };

        [Fact]
        public async Task Handle_WithData_ReturnsSuccess()
        {
            var entities = ValidEntityList();
            var query = new GetAllLocationQuery { PageNumber = 1, PageSize = 10 };

            _mockQueryRepo.Setup(r => r.GetAllLocationAsync(query.PageNumber, query.PageSize, query.SearchTerm)).ReturnsAsync((entities, 2));
            _mockMapper.Setup(m => m.Map<List<GetAllLocationDto>>(entities)).Returns(ValidDtoList());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(2);
            result.TotalCount.Should().Be(2);
        }

        [Fact]
        public async Task Handle_EmptyResult_ReturnsFailure()
        {
            var query = new GetAllLocationQuery { PageNumber = 1, PageSize = 10 };
            _mockQueryRepo.Setup(r => r.GetAllLocationAsync(query.PageNumber, query.PageSize, query.SearchTerm))
                .ReturnsAsync((new List<UserManagement.Domain.Entities.Location>(), 0));

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.IsSuccess.Should().BeFalse();
            result.Message.Should().Contain("No Record Found");
        }

        [Fact]
        public async Task Handle_WithData_ReturnsPaginationMetadata()
        {
            var entities = ValidEntityList();
            var query = new GetAllLocationQuery { PageNumber = 2, PageSize = 5 };

            _mockQueryRepo.Setup(r => r.GetAllLocationAsync(query.PageNumber, query.PageSize, query.SearchTerm)).ReturnsAsync((entities, 12));
            _mockMapper.Setup(m => m.Map<List<GetAllLocationDto>>(entities)).Returns(ValidDtoList());
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(query, CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(12);
        }
    }
}
