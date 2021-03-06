using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EnergySupervisor.Domain;
using EnergySupervisor.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SignalRChat.Hubs;

namespace EnergySupervisor
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Console.WriteLine(configuration["AllowedHosts"]);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            var iotHubConfiguration = new IoTHubConfiguration
            {
                IotHubConnectionString = Configuration["IotHubConnectionString"],
                EventHubsCompatibleEndpoint = Configuration["EventHubsCompatibleEndpoint"],
                EventHubsCompatiblePath = Configuration["EventHubsCompatiblePath"],
                IotHubSasKey = Configuration["IotHubSasKey"],
                IotHubSasKeyName = Configuration["IotHubSasKeyName"]
            };
            services.AddSingleton(iotHubConfiguration);
            services.AddSingleton(new DeviceController(iotHubConfiguration));
            services.AddHostedService<TelemetryIngesterHostedService>();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
            services.AddSignalR();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseSignalR(routes =>
            {
                routes.MapHub<PageUpdateHub>("/pageUpdateHub");
            });
            app.UseMvc();
        }
    }
}
