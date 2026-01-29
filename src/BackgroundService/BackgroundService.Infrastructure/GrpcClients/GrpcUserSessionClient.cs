using Contracts.Dtos.Users;
using Contracts.Interfaces.External.IUser;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcServices.UserManagement;

namespace BackgroundService.Infrastructure.GrpcClients
 {
   public class GrpcUserSessionClient : IUserSessionGrpcClient
     {
         private readonly SessionService.SessionServiceClient _client;

         public GrpcUserSessionClient(SessionService.SessionServiceClient client)
         {
             _client = client;
         }

         public async Task<UserSessionDto?> GetSessionByJwtIdAsync(string jwtId, string token)
         {
             var metadata = new Metadata
             {
                 { "Authorization", token.StartsWith("Bearer ") ? token : $"Bearer {token}" }
             };
             var callOptions = new CallOptions(metadata);

             var request = new SessionRequest { JwtId = jwtId };
             var response = await _client.GetSessionByJwtIdAsync(request, callOptions); 

             return new UserSessionDto
             {
                 Id = response.Id,
                 UserId = response.UserId,
                 JwtId = response.JwtId,
                 BrowserInfo = response.BrowserInfo,
                 CreatedAt = response.CreatedAt.ToDateTimeOffset(),
                 ExpiresAt = response.ExpiresAt.ToDateTimeOffset(),
                 IsActive = response.IsActive,
                 LastActivity = response.LastActivity.ToDateTimeOffset()
             };
         }

         public async Task<bool> UpdateSessionAsync(string jwtId, DateTimeOffset lastActivity, string token)
         {
             var metadata = new Metadata
             {
                 { "Authorization", token.StartsWith("Bearer ") ? token : $"Bearer {token}" }
             };
             var callOptions = new CallOptions(metadata);

             var request = new UpdateSessionRequest
             {
                 JwtId = jwtId,
                 LastActivity = Timestamp.FromDateTimeOffset(lastActivity)
             };

             await _client.UpdateSessionAsync(request, callOptions);  
             return true;
         }
     }
     }