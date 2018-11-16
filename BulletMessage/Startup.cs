using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using BulletMessage.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Extensions.Logging;

namespace BulletMessage
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
            services.AddCors(options =>
            {
                options.AddPolicy("SignalrCore",
                    policy => policy.AllowAnyOrigin()
                                    .AllowAnyHeader()
                                    .AllowAnyMethod());
            });
            services.AddSignalR();
            services.AddSingleton<IServiceProvider, ServiceProvider>();

            services.AddMvc(config=> {
                config.Filters.Add(typeof(GlobalExceptionFilter));
            }).SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddNLog();
            
            app.UseCors("SignalrCore");
            app.UseSignalR(routes =>
            {
                routes.MapHub<BulletHub>("/chat");
                routes.MapHub<RaceHub>("/race");
                routes.MapHub<ClientHub>("/client");
            });
            app.UseStaticFiles();
            //app.UseExceptionHandler(options => {
            //    options.Run(
            //        async context => {
            //            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            //            context.Response.ContentType = "text/html";
            //            var ex = context.Features.Get<IExceptionHandlerFeature>();
            //            if (ex != null)
            //            {
            //                var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";
            //                await context.Response.WriteAsync(err).ConfigureAwait(false);
            //            }
            //        })
            //});

            //app.UseMvc();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }
            app.UseWebSockets();

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
