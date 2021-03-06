﻿using System;
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
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
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
            services.AddMemoryCache();
            services.AddSignalR(options =>
            {
                // Faster pings for testing
                options.KeepAliveInterval = TimeSpan.FromSeconds(5);
            }).AddJsonProtocol(options =>
            {
                //options.PayloadSerializerSettings.Converters.Add(JsonConver);
                //the next settings are important in order to serialize and deserialize date times as is and not convert time zones
                options.PayloadSerializerSettings.Converters.Add(new IsoDateTimeConverter());
                options.PayloadSerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Unspecified;
                options.PayloadSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
            });
            services.AddSingleton<IServiceProvider, ServiceProvider>();

            services.AddMvc(config=> {
                //config.Filters.Add(typeof(GlobalExceptionFilter));
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
                routes.MapHub<ComHub>("/remote");
            });
            app.UseStaticFiles();

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
            app.UseExceptionHandler(options =>
            {
                options.Run(
                    async context =>
                    {
                        var logger = loggerFactory.CreateLogger("errorLogger");
                        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                        context.Response.ContentType = "text/json";

                        var ex = context.Features.Get<IExceptionHandlerFeature>();
                        if (ex != null)
                        {
                            var err = $"<h1>Error: {ex.Error.Message}</h1>{ex.Error.StackTrace }";
                            logger.LogError(err);
                            var errObj =new  { error=ex.Error.Message, stack = ex.Error.StackTrace };
                            var errStr = JsonConvert.SerializeObject(errObj);
                            await context.Response.WriteAsync(errStr).ConfigureAwait(false);
                        }
                    });
            });

            //app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
