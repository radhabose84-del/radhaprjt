using AutoMapper;
using MediatR;
using SalesManagement.Application.Common.Interfaces.ISalesOrganisation;
using SalesManagement.Application.SalesOrganisation.Dto;
using SalesManagement.Application.SalesOrganisation.Queries.GetSalesOrganisationById;
using SalesManagement.UnitTests.TestData;

namespace SalesManagement.UnitTests.Application.SalesOrganisation.Queries
{
    public class GetSalesOrganisationByIdQueryHandlerTests
    {
        private readonly Mock<ISalesOrganisationQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IMediator> _mockMediator = new();

        private GetSalesOrganisationByIdQueryHandler CreateSut()
        {
            _mockMapper.Setup(m => m.Map<SalesOrganisationDto>(It.IsAny<object>()))
                .Returns<object>(o => (o as SalesOrganisationDto)!);
            _mockMediator.Setup(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            return new GetSalesOrganisationByIdQueryHandler(_mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);
        }

        // â”€â”€ Tests â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        [Fact]
        public async Task Handle_EntityExists_ReturnsNotNull()
        {
            var query = new GetSalesOrganisationByIdQuery { Id = 1 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
        }

        [Fact]
        public async Task Handle_EntityExists_ReturnsCorrectDto()
        {
            var query = new GetSalesOrganisationByIdQuery { Id = 1 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 1, code: "ORG001", name: "Test Organisation");
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
            result!.SalesOrganisationCode.Should().Be("ORG001");
            result!.SalesOrganisationName.Should().Be("Test Organisation");
        }

        [Fact]
        public async Task Handle_EntityExists_PublishesAuditEvent()
        {
            var query = new GetSalesOrganisationByIdQuery { Id = 1 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 1);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(dto);
            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockMediator.Verify(m => m.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityExists_CallsGetByIdAsync_Once()
        {
            var query = new GetSalesOrganisationByIdQuery { Id = 7 };
            var dto = SalesOrganisationBuilders.ValidDto(id: 7);
            _mockQueryRepo.Setup(r => r.GetByIdAsync(7)).ReturnsAsync(dto);
            var sut = CreateSut();

            await sut.Handle(query, CancellationToken.None);

            _mockQueryRepo.Verify(r => r.GetByIdAsync(7), Times.Once);
        }

        [Fact]
        public async Task Handle_EntityNotFound_ReturnsNull()
        {
            var query = new GetSalesOrganisationByIdQuery { Id = 99 };
            _mockQueryRepo.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((SalesOrganisationDto?)null);
            var sut = CreateSut();

            var result = await sut.Handle(query, CancellationToken.None);

            result.Should().BeNull();
        }
    }
}