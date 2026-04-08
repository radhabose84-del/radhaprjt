using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Commands.UpdateCompany;
using UserManagement.Application.Companies.Queries.GetCompanies;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Company.Commands
{
    public sealed class UpdateCompanyCommandHandlerTests
    {
        private readonly Mock<ICompanyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IFileUploadService> _mockFileUpload = new(MockBehavior.Loose);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private UpdateCompanyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockMapper.Object, _mockFileUpload.Object, _mockQueryRepo.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ValidCommand_ReturnsTrue()
        {
            var command = new UpdateCompanyCommand { Company = new UpdateCompanyDTO { Id = 1, CompanyName = "Updated" } };
            _mockQueryRepo.Setup(r => r.GetByCompanynameAsync("Updated", 1)).ReturnsAsync((UserManagement.Domain.Entities.Company?)null);
            _mockMapper.Setup(m => m.Map<UserManagement.Domain.Entities.Company>(command.Company)).Returns(new UserManagement.Domain.Entities.Company());
            _mockCommandRepo.Setup(r => r.UpdateAsync(1, It.IsAny<UserManagement.Domain.Entities.Company>())).ReturnsAsync(true);
            _mockMediator.Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(command, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_DuplicateName_ThrowsValidationException()
        {
            var command = new UpdateCompanyCommand { Company = new UpdateCompanyDTO { Id = 1, CompanyName = "Existing" } };
            _mockQueryRepo.Setup(r => r.GetByCompanynameAsync("Existing", 1)).ReturnsAsync(new UserManagement.Domain.Entities.Company());

            Func<Task> act = async () => await CreateSut().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
