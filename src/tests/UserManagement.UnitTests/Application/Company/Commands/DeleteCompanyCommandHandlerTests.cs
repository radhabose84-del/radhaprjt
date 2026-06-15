using AutoMapper;
using FluentValidation;
using MediatR;
using UserManagement.Application.Common.Interfaces.ICompany;
using UserManagement.Application.Companies.Commands.DeleteCompany;
using UserManagement.Domain.Events;

namespace UserManagement.UnitTests.Application.Company.Commands
{
    public sealed class DeleteCompanyCommandHandlerTests
    {
        private readonly Mock<ICompanyCommandRepository> _mockCommandRepo = new(MockBehavior.Strict);
        private readonly Mock<ICompanyQueryRepository> _mockQueryRepo = new(MockBehavior.Strict);
        private readonly Mock<IMapper> _mockMapper = new(MockBehavior.Loose);
        private readonly Mock<IMediator> _mockMediator = new(MockBehavior.Loose);

        private DeleteCompanyCommandHandler CreateSut() =>
            new(_mockCommandRepo.Object, _mockQueryRepo.Object, _mockMapper.Object, _mockMediator.Object);

        [Fact]
        public async Task Handle_ExistingCompany_ReturnsTrue()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new UserManagement.Domain.Entities.Company { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.IsCompanyUsedByAnyUserAsync(1))
                .ReturnsAsync(false);

            _mockMapper
                .Setup(m => m.Map<UserManagement.Domain.Entities.Company>(It.IsAny<DeleteCompanyCommand>()))
                .Returns(new UserManagement.Domain.Entities.Company());

            _mockCommandRepo
                .Setup(r => r.DeleteAsync(1, It.IsAny<UserManagement.Domain.Entities.Company>()))
                .ReturnsAsync(true);

            _mockMediator
                .Setup(m => m.Publish(It.IsAny<AuditLogsDomainEvent>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await CreateSut().Handle(
                new DeleteCompanyCommand { Id = 1 }, CancellationToken.None);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task Handle_NotFound_ReturnsFalse()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(999))
                .ReturnsAsync((UserManagement.Domain.Entities.Company?)null!);

            // Idempotent delete: a missing id is a no-op (returns false), not a thrown exception.
            var result = await CreateSut().Handle(
                new DeleteCompanyCommand { Id = 999 }, CancellationToken.None);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task Handle_CompanyInUse_ThrowsValidationException()
        {
            _mockQueryRepo
                .Setup(r => r.GetByIdAsync(1))
                .ReturnsAsync(new UserManagement.Domain.Entities.Company { Id = 1 });

            _mockQueryRepo
                .Setup(r => r.IsCompanyUsedByAnyUserAsync(1))
                .ReturnsAsync(true);

            Func<Task> act = () => CreateSut().Handle(
                new DeleteCompanyCommand { Id = 1 }, CancellationToken.None);

            await act.Should().ThrowAsync<ValidationException>();
        }
    }
}
