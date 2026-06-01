namespace QCManagement.Domain.Common
{
    /// <summary>
    /// Marker interface. Entities implementing this are captured by
    /// ActivityLogSaveChangesInterceptor for field-level Old → New value auditing.
    /// </summary>
    public interface IActivityTracked { }
}
