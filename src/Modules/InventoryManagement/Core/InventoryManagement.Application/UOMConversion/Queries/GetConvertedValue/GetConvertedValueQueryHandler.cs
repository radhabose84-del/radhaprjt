using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using InventoryManagement.Application.Common.Interfaces.IUOMConversion;
using MediatR;

namespace InventoryManagement.Application.UOMConversion.Queries.GetConvertedValue
{
    public class GetConvertedValueQueryHandler : IRequestHandler<GetConvertedValueQuery, ApiResponseDTO<decimal>>
    {
        private readonly IUOMConversionQueryRepository  _uOMConversionQueryRepository;
        
        public GetConvertedValueQueryHandler( IUOMConversionQueryRepository queryRepository)
        {
            _uOMConversionQueryRepository = queryRepository;
        }
         public async Task<ApiResponseDTO<decimal>> Handle(GetConvertedValueQuery request, CancellationToken cancellationToken)
        {
            // Fetch direct conversion
            var conversion = await _uOMConversionQueryRepository.GetConversionFactorAsync(request.FromUOMId, request.ToUOMId);

            decimal result;

            if (conversion == null)
            {
                // Try reverse conversion
                var reverse = await _uOMConversionQueryRepository.GetConversionFactorAsync(request.ToUOMId, request.FromUOMId);
                if (reverse == null)
                {
                    return new ApiResponseDTO<decimal>
                    {
                        IsSuccess = false,
                        Message = "No conversion factor found for the given UOMs.",
                        Data = 0
                    };
                }

                result = request.Quantity / reverse.Value;
            }
            else
            {
                result = request.Quantity * conversion.Value;
            }

            return new ApiResponseDTO<decimal>
            {
                IsSuccess = true,
                Message = "Conversion calculated successfully.",
                Data = Math.Round(result, 6)
            };
        }
    }
}