using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FAM.Infrastructure.Services
{
    public class AuthTokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthTokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var accessToken = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

            if (!string.IsNullOrWhiteSpace(accessToken) && accessToken.StartsWith("Bearer "))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken.Substring("Bearer ".Length));
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}