namespace ActiveMqConsoleListener
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using MassTransit.ActiveMqTransport;
    using Microsoft.Extensions.DependencyInjection;

    public class Program
    {
        public static async Task Main()
        {
            var services = new ServiceCollection();
            services.AddMassTransit(x =>
            {
                x.UsingActiveMq((context, cfg) =>
                {
                    cfg.Host("localhost", h =>
                    {
                        h.UseSsl();

                        h.Username("admin");
                        h.Password("admin");
                    });
                });
            });
        }
    }
}
