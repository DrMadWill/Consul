using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DrMadWill.Consul
{
    public static class ConsulRegistration
    {
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, string address)
        {
            Console.WriteLine("IConsulClient adding");
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(
                config => config.Address = new Uri(address)));
            Console.WriteLine("IConsulClient added");
            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime, ConsulConfigModel model)
        {
            Console.WriteLine("Registering start...");
            var consulCilent = app.ApplicationServices.GetRequiredService<IConsulClient>();

            var logFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

            var logger = logFactory.CreateLogger<IApplicationBuilder>();

            Console.WriteLine("Consul Client And Logger Created.");

            // Get Server IP Address

            var features = app.Properties["server:Features"] as FeatureCollection;
            var addresses = features.Get<IServerAddressesFeature>();
            var address = addresses.Addresses.First();
            var uri = new Uri(address);
            var registration = new AgentServiceRegistration()
            {
                ID = model.Id,
                Name = model.Name,
                Address = $"{uri.Host}",
                Port = uri.Port,
                Tags = model.Tags,
            };

            Console.WriteLine("Register data generated.");
            logger.LogInformation("Registering with Consul");
            consulCilent.Agent.ServiceDeregister(registration.ID);
            consulCilent.Agent.ServiceRegister(registration).Wait();
            Console.WriteLine("Registered Consul.");
            return app;
        }
    }
}