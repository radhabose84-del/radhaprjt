using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Application.Companies.Queries.GetCompanyById;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Company.Queries
{
    public sealed class GetCompanyByIdQueryHandlerTests
    {
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private GetCompanyByIdQueryHandler CreateSut() =>
            new(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingCompany_ReturnsMappedDto()
        {
            var entity = new UserManagement.Domain.Entities.Company { Id = 1, CompanyName = "Test" };
            var mappedDto = new GetByIdDTO { Id = 1, CompanyName = "Test" };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetByIdDTO>(entity))
                .Returns(mappedDto);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new GetCompanyByIdQuery { CompanyId = 1 }, CancellationToken.None);

            result.Should().NotBeNull();
            result.Id.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ExistingCompany_PublishesAuditEvent()
        {
            var entity = new UserManagement.Domain.Entities.Company { Id = 1 };

            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(entity);

            _mockMapper
                .Setup(m => m.Map<GetByIdDTO>(entity))
                .Returns(new GetByIdDTO { Id = 1 });

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(new GetCompanyByIdQuery { CompanyId = 1 }, CancellationToken.None);

            _mockMediator.Verify(
                m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }
    }
}
