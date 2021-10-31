using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ScreenDraw.Classes;
using ScreenDraw.Hubs;
using ScreenDraw.Interfaces;
using System.Collections.Concurrent;

namespace ScreenDraw
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRazorPages();

            services.AddServerSideBlazor();
            //services.AddSignalR();

            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
                o.MaximumReceiveMessageSize = null; // bytes
            });

            services.AddControllersWithViews();

            // Inject a colour list for the colour drop down
            // For the moment add this as a singleton as its currently never changing
            services.Add(new ServiceDescriptor(typeof(IList<IColourListItem>), ColourListFactory.GetColourListObject()));

            //Inject a SketchRooms object as a singleton. This will hold all the information about
            //the rooms and the artists inside those rooms, and will persist for as long as there are users,
            //or until it expires, (where no one has used the site for a while).
            //TODO Replace the SketchRooms singeton with a permanent form of storage, database, flat files etc.
            services.Add(new ServiceDescriptor(typeof(ISketchRooms), new SketchRooms()));

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

            app.UseAuthorization();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapBlazorHub();
                endpoints.MapHub<DrawHub>("/drawHub");
            });
        }
    }
}
