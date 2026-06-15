namespace FinanceManagement.Domain.Common
{
    // Marker — entities implementing this get property-level change rows written to Finance.ActivityLog
    // by ActivityLogSaveChangesInterceptor on every SaveChanges.
    public interface IActivityTracked { }
}
