namespace FinanceManagement.Domain.Common
{
    // Marker — entities implementing this get an immutable, field-level change trail written to the
    // hardened Finance.AccountAuditTrail table by AccountAuditTrailSaveChangesInterceptor on every
    // SaveChanges (Insert/Update/Delete), in the same transaction as the change itself (US-GL02-09).
    //
    // Kept separate from IActivityTracked so statutory COA audit data (8-year retention, DB-enforced
    // immutability) does not mix with the operational Finance.ActivityLog trail.
    public interface IAuditTrailed { }
}
