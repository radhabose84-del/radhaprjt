using AutoMapper;

namespace PurchaseManagement.Application.Common.Mappings
{
    public interface IMapFrom<T>
    {
         void  Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
         
    }
}