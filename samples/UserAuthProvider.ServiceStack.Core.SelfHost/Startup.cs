// // This Source Code Form is subject to the terms of the Mozilla Public
// // License, v. 2.0. If a copy of the MPL was not distributed with this
// // file, You can obtain one at http://mozilla.org/MPL/2.0/.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UserAuthProvider.ServiceStack.Core.SelfHost
{
    using global::ServiceStack;
    using global::ServiceStack.Host.Handlers;
    using global::ServiceStack.Mvc;
    using Serilog;

    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            Log.Logger = new LoggerConfiguration()
               .MinimumLevel.Verbose()
               .Enrich.FromLogContext()
               .WriteTo.Seq("http://localhost:5341")
               .CreateLogger();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole();

            loggerFactory.AddSerilog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseServiceStack(new AppHost("http://localhost:5001"));

            app.Use(new RequestInfoHandler());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            app.Use(new RazorHandler("/notfound"));
        }
    }
}
