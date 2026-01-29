using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UserManagement.UnitTests.Common.Helpers;

public static class ControllerContextHelper
{
    public static ControllerContext Create()
    {
        var http = new DefaultHttpContext();
        return new ControllerContext { HttpContext = http };
    }
}
