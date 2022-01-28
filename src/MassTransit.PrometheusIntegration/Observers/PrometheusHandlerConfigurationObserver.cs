namespace MassTransit.PrometheusIntegration.Observers
{
    using ConsumeConfigurators;


    public class PrometheusHandlerConfigurationObserver :
        IHandlerConfigurationObserver
    {
        void IHandlerConfigurationObserver.HandlerConfigured<T>(IHandlerConfigurator<T> configurator)
        {
            var specification = new PrometheusHandlerSpecification<T>();

            configurator.AddPipeSpecification(specification);
        }
    }
}
