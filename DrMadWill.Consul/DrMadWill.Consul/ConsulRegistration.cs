using Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DrMadWill.Consul
{
    public static class ConsulRegistration
    {
        public static ConsulConfigModel Config;
        public static IServiceCollection ConfigureConsul(this IServiceCollection services, IConfiguration configuration)
        {
            Console.WriteLine("IConsulClient adding");
            Config = new ConsulConfigModel
            {
                Id = configuration["ConsulConfig:Id"],
                Name = configuration["ConsulConfig:Name"],
                ConsulAddress = configuration["ConsulConfig:ConsulAddress"],
                Address = configuration["ConsulConfig:Address"],
            };
            Console.WriteLine(Config.ToString());
            services.AddSingleton<IConsulClient, ConsulClient>(p => new ConsulClient(
                config => config.Address = new Uri(Config.ConsulAddress)));
            Console.WriteLine("IConsulClient added");
            return services;
        }

        public static IApplicationBuilder RegisterWithConsul(this IApplicationBuilder app, IHostApplicationLifetime lifetime)
        {
           
            lifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("Registering start...");
                var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

                var logFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

                var logger = logFactory.CreateLogger<IApplicationBuilder>();

                Console.WriteLine("Consul Client And Logger Created.");
                // Get Server IP Address

                var uri = new Uri(Config.Address);
                var registration = new AgentServiceRegistration()
                {
                    ID = Config.Id,
                    Name = Config.Name,
                    Address = $"{uri.Host}",
                    Port = uri.Port,
                    Tags = Config.Tags,
                };

                Console.WriteLine("Register data generated.");
                logger.LogInformation("Registering with Consul" + Config);
                consulClient.Agent.ServiceDeregister(registration.ID).Wait();
                consulClient.Agent.ServiceRegister(registration).Wait();
                Console.WriteLine("Registered Consul.");
                
            });
            
            lifetime.ApplicationStopped.Register(() =>
            {
                var consulClient = app.ApplicationServices.GetRequiredService<IConsulClient>();

                var logFactory = app.ApplicationServices.GetRequiredService<ILoggerFactory>();

                var logger = logFactory.CreateLogger<IApplicationBuilder>();
                Console.WriteLine("Deregister start" + Config);
                logger.LogInformation("Deregister service" + Config); 
                consulClient.Agent.ServiceDeregister(Config.Id).Wait();
                Console.WriteLine("Deregister end. " );
            });
            return app;
        }
    }
}