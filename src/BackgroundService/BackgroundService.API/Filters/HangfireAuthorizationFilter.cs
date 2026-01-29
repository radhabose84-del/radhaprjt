using Hangfire.Dashboard;

namespace BackgroundService.API.Filters
{   

    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly string _username;
        private readonly string _password;

        public HangfireAuthorizationFilter(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetHttpContext();

            // Check if basic auth header is present
            string authHeader = httpContext.Request.Headers["Authorization"].ToString();
            if (!string.IsNullOrWhiteSpace(authHeader) && authHeader.StartsWith("Basic "))
            {
                // Decode the base64 encoded username:password
                var encodedCredentials = authHeader.Substring("Basic ".Length).Trim();
                var decodedCredentials = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(encodedCredentials));
                var credentials = decodedCredentials.Split(':');

                if (credentials.Length is 2)
                {
                    var username = credentials[0];
                    var password = credentials[1];

                    // Validate username and password
                    return username == _username && password == _password;
                }
            }

            // Prompt for credentials
            httpContext.Response.Headers["WWW-Authenticate"] = "Basic realm=\"Hangfire Dashboard\"";
            httpContext.Response.StatusCode = 401;
            return false;
        }
    }
}