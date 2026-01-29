using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MaintenanceManagement.Application.Common.HttpResponse;
using MediatR;

namespace MaintenanceManagement.Application.Power.Feeder.Command.UpdateFeeder
{
    public class UpdateFeederCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public string? FeederCode { get; set; }
        public string? FeederName { get; set; }
        public int? ParentFeederId { get; set; }
        public int FeederGroupId { get; set; }
        public int FeederTypeId { get; set; }
        public int UnitId { get; set; }
        public byte MeterAvailable { get; set; }
        public int? MeterTypeId { get; set; } 
        public int DepartmentId { get; set; }
        public string? Description { get; set; }
        public decimal? MultiplicationFactor { get; set; }
        public DateTimeOffset EffectiveDate { get; set; }
        public decimal? OpeningReading { get; set; }
        public byte   HighPriority { get; set; } 
        public decimal? Target { get; set; }
        public byte IsActive { get; set; }
    }
}