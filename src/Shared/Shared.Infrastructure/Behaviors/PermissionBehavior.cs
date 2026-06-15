using Contracts.Common;
using Contracts.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Shared.Infrastructure.Behaviors;

public class PermissionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IIPAddressService      _ipAddressService;
    private readonly IPermissionService     _permissionService;
    private readonly IHttpContextAccessor   _httpContextAccessor;

    public PermissionBehavior(
        IIPAddressService    ipAddressService,
        IPermissionService   permissionService,
        IHttpContextAccessor httpContextAccessor)
    {
        _ipAddressService    = ipAddressService;
        _permissionService   = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TResponse> Handle(
        TRequest                request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken       cancellationToken)
    {
        if (request is not IRequirePermission permRequest)
            return await next();

        // Read menuId from the X-Menu-Id HTTP header (set by FE axios interceptor)
        var menuId = 0;
        if (_httpContextAccessor.HttpContext?.Request.Headers
                .TryGetValue("X-Menu-Id", out var headerVal) == true)
            int.TryParse(headerVal, out menuId);

        // No menuId sent — bypass (backward compatible with live modules)
        if (menuId <= 0)
            return await next();

        var userId = _ipAddressService.GetUserId();
        if (userId <= 0)
            throw new ForbiddenException("Authentication is required.");

        var allowed = await _permissionService.HasPermissionAsync(
            userId,
            menuId,
            permRequest.RequiredPermission,
            cancellationToken);

        if (!allowed)
            throw new ForbiddenException(
                $"You do not have {permRequest.RequiredPermission} permission for this operation.");

        return await next();
    }
}
