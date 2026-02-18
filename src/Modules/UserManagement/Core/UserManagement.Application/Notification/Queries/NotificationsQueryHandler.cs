#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts.Common;
using UserManagement.Application.Common.Interfaces.INotifications;
using UserManagement.Domain.Events;
using FluentValidation;
using Hangfire;
using MediatR;
using Microsoft.Extensions.Logging;

namespace UserManagement.Application.Notification.Queries
{
    public class NotificationsQueryHandler
    {
        private readonly INotificationsQueryRepository _INotificationsQueryRepository;

        private readonly ILogger<NotificationsQueryHandler> _logger;

        private readonly IMediator _Imediator;

        public NotificationsQueryHandler(INotificationsQueryRepository INotificationsQueryRepository,ILogger<NotificationsQueryHandler> logger,IMediator Imediator)
        {
            _INotificationsQueryRepository = INotificationsQueryRepository;
           _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _Imediator = Imediator;
           
        }

       public async Task<NotificationResponse> Handle(NotificationRequest request, CancellationToken cancellationToken)
        {
        _logger.LogInformation($"Handling user login request for Username: {request.Username}");
        var username = request.Username;
        if (string.IsNullOrWhiteSpace(request.Username))
            {
                _logger.LogWarning("Invalid Username.");
                throw new ValidationException("Username required.");
              
            }
         _logger.LogInformation($"User {request.Username} found. Retrieving Password expiry details...");

           

        // Get password last change date from PasswordLastChange table
        var lastPasswordChangeDate = await _INotificationsQueryRepository.GetLastPasswordChangeDate(username.Trim());
        if (lastPasswordChangeDate == null)
        {
            _logger.LogWarning($"Password last change date not found for Username: {request.Username}");
            throw new ValidationException("Password last change date not found /Invalid Username.");
         
        }
        _logger.LogInformation($"Getting Last Password Change Date for Username: {lastPasswordChangeDate}");

         //Domain Event
                  var domainEvent = new AuditLogsDomainEvent(
                      actionDetail: "GetPasswordExpiryDetails",
                      actionCode: username,
                      actionName: "GET",
                      details: $"Password expiry details for user '{username}' successfully fetched.",
                      module:"Password Expiry Notifications"
                  );
                  await _Imediator.Publish(domainEvent, cancellationToken);
       
        //Check if password has expired
        var (pwdExpiryDays, pwdExpiryAlertDays) = await _INotificationsQueryRepository.GetPasswordExpiryDays();
        _logger.LogInformation($"Fetching Password Expiry Details and Alert Days Details pwdExpiryDays: {pwdExpiryDays}. PwdExpiryAlertDays: {pwdExpiryAlertDays}");
        
        var passwordAge = (DateTime.Now - lastPasswordChangeDate).Value.Days;

            // handle case where password has expired
            if (passwordAge >= pwdExpiryDays)
            {

                _logger.LogWarning($"Password has expired for Username: {request.Username}");
            throw new ValidationException("Your password has expired. Please update your password to regain access..");
         
        }

            // handle case where password is near expiry
            else if (passwordAge >= pwdExpiryDays - pwdExpiryAlertDays)
            {
                int daysLeft = pwdExpiryDays - passwordAge;
                _logger.LogWarning($"Your password will expire in {daysLeft} days. Please update your password to avoid any disruptions.", request.Username);
                throw new ValidationException($"Your password will expire in {daysLeft} days. Please update your password to avoid any disruptions.");
            

            }
            // handle case where password is still valid
            else
            {
                _logger.LogInformation($"Password is still valid for Username: {request.Username}");
                throw new ValidationException("Your password is still valid.");
               
            }
    }
       
    }
}
   

