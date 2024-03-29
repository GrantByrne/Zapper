using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zapper.Core;
using Zapper.Core.Bluetooth;
using Zapper.Core.Devices;
using Zapper.Core.Devices.Abstract;
using Zapper.Core.KeyboardMouse;
using Zapper.Core.KeyboardMouse.Abstract;
using Zapper.Core.Linux;
using Zapper.Core.Remote;
using Zapper.Core.Repository;
using Zapper.Core.WebOs;
using Zapper.Core.WebOs.Abstract;
using Zapper.Web.Data;
using Zapper.Web.Data.Abstract;
using Zapper.WebOs;

namespace Zapper.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();
            services.AddServerSideBlazor();
            
            services.AddSingleton<IAggregateInputReader, AggregateInputReader>();
            services.AddSingleton<IDeviceManager, DeviceManager>();
            services.AddSingleton<IRemoteManager, RemoteManager>();
            services.AddSingleton<IService, Service>();
            services.AddSingleton<IRemoteEventHandler, RemoteEventHandler>();
            services.AddSingleton<IWebOsConnectionFactory, WebOsConnectionFactory>();
            services.AddSingleton<IWebOsStatusManager, WebOsStatusManager>();
            services.AddSingleton<IWakeOnLanManager, WakeOnLanManager>();
            services.AddSingleton<IWebOsActionFactory, WebOsActionFactory>();
            services.AddSingleton<IBluetoothConnection, BluetoothConnection>();
            services.AddSingleton<ILinuxGroupManager, LinuxGroupManager>();
            services.AddSingleton<IIpAddressManager, IpAddressManager>();
            services.AddDbContext<ZapperDbContext>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapBlazorHub();
                endpoints.MapFallbackToPage("/_Host");
            });

            var remoteManager = app.ApplicationServices.GetService<IRemoteManager>();
            remoteManager?.Initialize();
        }
    }
}