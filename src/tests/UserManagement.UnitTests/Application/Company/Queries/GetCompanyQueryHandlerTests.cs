using AutoMapper;
using Contracts.Common;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Company.Queries
{
    public sealed class GetCompanyQueryHandlerTests
    {
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCompanyQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ReturnsSuccess()
        {
            var entities = new List<UserManagement.Domain.Entities.Company> { new() { Id = 1 } };
            var dtoList = new List<GetCompanyDTO> { new() { Id = 1 } };

            _mockQueryRepo
                .Setup(r => r.GetAllCompaniesAsync(1, 10, null))
                .ReturnsAsync((entities, 1));

            _mockMapper
                .Setup(m => m.Map<List<GetCompanyDTO>>(entities))
                .Returns(dtoList);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCompanyQuery { PageNumber = 1, PageSize = 10 },
                CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Data.Should().HaveCount(1);
        }

        [Fact]
        public async Task Handle_ReturnsPaginationMetadata()
        {
            var entities = new List<UserManagement.Domain.Entities.Company>();

            _mockQueryRepo
                .Setup(r => r.GetAllCompaniesAsync(2, 5, "test"))
                .ReturnsAsync((entities, 10));

            _mockMapper
                .Setup(m => m.Map<List<GetCompanyDTO>>(entities))
                .Returns(new List<GetCompanyDTO>());

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCompanyQuery { PageNumber = 2, PageSize = 5, SearchTerm = "test" },
                CancellationToken.None);

            result.PageNumber.Should().Be(2);
            result.PageSize.Should().Be(5);
            result.TotalCount.Should().Be(10);
        }
    }
}
