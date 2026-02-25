using AutoMapper;
using PartyManagement.Application.BankAccount;


namespace PartyManagement.Application.Common.Mappings
{
    public class BankAccountProfile : Profile
    {
        public BankAccountProfile()
        {
            CreateMap<PartyManagement.Domain.Entities.BankAccount, BankAccountDto>().ReverseMap();
        }
    }
}
