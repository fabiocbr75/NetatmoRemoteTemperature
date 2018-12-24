using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TemperatureHub.NetatmoData;
using TemperatureHub.Process;
using TemperatureHub.Repository;

namespace TemperatureHub
{
    public class Startup
    {
        //public Startup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}

        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                    .SetBasePath(env.ContentRootPath)
                    .AddJsonFile("appsettings.json",
                                 optional: false,
                                 reloadOnChange: true)
                    .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            services.AddMemoryCache();
            services.AddSingleton<ISQLiteFileRepository, SQLiteFileRepository>();
            services.AddSingleton<INetatmoDataHandler, NetatmoDataHandler>();
            services.AddSingleton<IProcessData, ProcessData>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var appSettingsSection = Configuration.GetSection("AppSettings");
            var sett = appSettingsSection.Get<AppSettings>();

            SQLiteFileRepository.CreateOrUpdateDb(sett.DbFullPath);

            app.UseMvc();

            var lifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
            lifeTime.ApplicationStopping.Register(() =>
            {
                SQLiteFileRepository.CleanUp();
            });

        }
    }
}
