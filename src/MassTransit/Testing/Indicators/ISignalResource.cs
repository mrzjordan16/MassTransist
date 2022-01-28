namespace MassTransit.Testing.Indicators
{
    /// <summary>
    /// Represents a resource which may be signaled.
    /// </summary>
    public interface ISignalResource
    {
        void Signal();
    }
}
