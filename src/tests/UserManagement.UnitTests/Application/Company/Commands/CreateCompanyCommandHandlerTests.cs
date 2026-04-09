using AutoMapper;
using MediatR;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Commands.CreateCompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Company.Commands
{
    public sealed class CreateCompanyCommandHandlerTests
    {
        private readonly Mock<ICompanyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Loose);

        private CreateCompanyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockFileUpload.Object, _mockMediator.Object, _mockQueryRepo.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsNewId()
        {
            var command = new CreateCompanyCommand
            {
                Company = new CompanyDTO { CompanyName = "TestCompany" }
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Company>(command.Company))
                .Returns(new UserManagement.Domain.Entities.Company());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Company>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().Be(1);
        }

        [Fact]
        public async Task Handle_ValidCommand_CallsCreateOnce()
        {
            var command = new CreateCompanyCommand
            {
                Company = new CompanyDTO { CompanyName = "TestCompany" }
            };

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Company>(command.Company))
                .Returns(new UserManagement.Domain.Entities.Company());

            _mockCommandRepo
                .Setup(r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Company>()))
                .ReturnsAsync(1);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            await CreateSut().Handle(command, CancellationToken.None);

            _mockCommandRepo.Verify(
                r => r.CreateAsync(It.IsAny<UserManagement.Domain.Entities.Company>()),
                Times.Once);
        }
    }
}
