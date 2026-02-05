using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using PartyManagement.Application.Common.Interfaces.IPartyMaster;
using MediatR;

namespace PartyManagement.Application.PartyMaster.Queries.GetPartyActivityLog
{
    public class GetPartyActivityLogQueryHandler : IRequestHandler<GetPartyActivityLogQuery, List<PartyActivityDto>>
    {
        private readonly IPartyActivityLogCommandRepository _partyActivityLogCommandRepository;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public GetPartyActivityLogQueryHandler(IPartyActivityLogCommandRepository partyActivityLogCommandRepository, IMapper mapper, IMediator mediator)
        {
            _partyActivityLogCommandRepository = partyActivityLogCommandRepository;
            _mapper = mapper;
            _mediator = mediator;
        }

        public async Task<List<PartyActivityDto>> Handle(GetPartyActivityLogQuery request, CancellationToken cancellationToken)
        {
            var logs = await _partyActivityLogCommandRepository.GetActivityLogsByPartyIdAsync(request.PartyId, cancellationToken);

            var response = logs.Select(log => new PartyActivityDto
            {
                // ActionType = log.ActionType,
                // ColumnName = log.ColumnName ?? "",
                // OldValue = log.OldValue ?? "",
                // NewValue = log.NewValue ?? "",
                // ChangedOn = log.ChangedOn,
                // ChangedByName = log.ChangedByName,
                Message = BuildMessage(log)
            }).ToList();

            return response;
        }
        private string BuildMessage(PartyManagement.Domain.Entities.PartyActivityLog log)
        {
            var timeAgo = DateTime.Now - log.ChangedOn;
            string ago = FormatTimeAgo(timeAgo);

            // Handle null/empty values gracefully → show as ""
          string oldValue = string.IsNullOrWhiteSpace(log.OldValue) ? "Empty" : log.OldValue;
          string newValue = string.IsNullOrWhiteSpace(log.NewValue) ? "Empty" : log.NewValue;

            if (log.ActionType == "Insert")
                return $"{log.ChangedByName} Created New {log.ColumnName} {newValue} - {ago}";

            else if (log.ActionType == "Update")
                return $"{log.ChangedByName} Changed {log.ColumnName} from {oldValue} to {newValue} - {ago}";

            else if (log.ActionType == "Delete")
                return $"{log.ChangedByName} Deleted {log.ColumnName} from {oldValue} to {newValue} - {ago}";

                else if (log.ActionType == "Approved" || log.ActionType == "Rejected")
                    return $"{log.ChangedByName} {log.ActionType} PartyId {log.PartyId} - {ago}";

            return $"{log.ChangedByName} Performed {log.ActionType} on {log.ColumnName} - {ago}";
        }


        private string FormatTimeAgo(TimeSpan timeAgo)
        {
            if (timeAgo.TotalMinutes < 1)
                return "just now";
            if (timeAgo.TotalMinutes < 60)
                return $"{(int)timeAgo.TotalMinutes} minute{(timeAgo.TotalMinutes >= 2 ? "s" : "")} ago";
            if (timeAgo.TotalHours < 24)
                return $"{(int)timeAgo.TotalHours} hour{(timeAgo.TotalHours >= 2 ? "s" : "")} ago";
            if (timeAgo.TotalDays < 7)
                return $"{(int)timeAgo.TotalDays} day{(timeAgo.TotalDays >= 2 ? "s" : "")} ago";
            if (timeAgo.TotalDays < 30)
                return $"{(int)(timeAgo.TotalDays / 7)} week{(timeAgo.TotalDays / 7 >= 2 ? "s" : "")} ago";
            if (timeAgo.TotalDays < 365)
                return $"{(int)(timeAgo.TotalDays / 30)} month{(timeAgo.TotalDays / 30 >= 2 ? "s" : "")} ago";

            return $"{(int)(timeAgo.TotalDays / 365)} year{(timeAgo.TotalDays / 365 >= 2 ? "s" : "")} ago";
        }

            
    }
}