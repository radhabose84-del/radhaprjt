// using AutoMapper;
// using Contracts.Interfaces.External.IUser;
// using PurchaseManagement.Application.Common.Interfaces.IPortMaster;
// using PurchaseManagement.Application.Port.Dto;
// using MediatR;

// namespace PurchaseManagement.Application.Port.Queries.GetById;

// public sealed class GetPortByIdQueryHandler : IRequestHandler<GetPortByIdQuery, PortMasterDto?>
// {
//     private readonly IPortMasterQueryRepository _repo;
//     private readonly ICountryGrpcClient _countryGrpcClient;

//     public GetPortByIdQueryHandler(IPortMasterQueryRepository repo, ICountryGrpcClient countryGrpcClient)
//     {
//         _repo = repo;
//         _countryGrpcClient = countryGrpcClient;
//     }

//     public async Task<PortMasterDto?> Handle(GetPortByIdQuery request, CancellationToken ct)
//     {
//         var dto = await _repo.GetByIdAsync(request.Id, ct);
//         if (dto is null) return null;

//         // pull countries once (or use a GetById if your gRPC has it)
//         var countries = await _countryGrpcClient.GetAllCountryAsync();
//         var countryDict = countries
//             .GroupBy(x => x.CountryId)
//             .ToDictionary(g => g.Key, g => g.First().CountryName);

//         if (countryDict.TryGetValue(dto.CountryId, out var cname))
//             dto.Country = cname;

//         return dto;
//     }
// }
