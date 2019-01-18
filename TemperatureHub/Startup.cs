﻿using System;
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
using TemperatureHub.Helpers;
using TemperatureHub.Models;
using TemperatureHub.NetatmoData;
using TemperatureHub.Process;
using TemperatureHub.Repository;
using System.Threading;
using System.Net.Mail;
using System.Net;

namespace TemperatureHub
{
    public class Startup
    {
        //public Startup(IConfiguration configuration)
        //{
        //    Configuration = configuration;
        //}

        private static Timer timer;

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
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                 builder => builder.AllowAnyOrigin()
                 .AllowAnyMethod()
                 .AllowAnyHeader()
                 .AllowCredentials());
            });

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
 
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            services.AddMemoryCache();
            services.AddSingleton<ISQLiteFileRepository, SQLiteFileRepository>();
            services.AddSingleton<INetatmoDataHandler, NetatmoDataHandler>();
            services.AddSingleton<IProcessData, ProcessData>();
            services.AddSingleton<ISharedData, SharedData>();

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

            Logger.Info("Startup", $"HomeId:{sett.HomeId}");
            Logger.Info("Startup", $"Username:{sett.Username}");

            Logger.Info("Startup", $"ClientId(last 3 char):{sett.ClientId.Substring(sett.ClientId.Length - 3)}");
            Logger.Info("Startup", $"ClientSecret(last 3 char):{sett.ClientSecret.Substring(sett.ClientSecret.Length - 3)}");
            Logger.Info("Startup", $"Password(last 3 char):{sett.Password.Substring(sett.Password.Length - 3)}");

            var sharedData = app.ApplicationServices.GetService<ISharedData>();
            var repo = app.ApplicationServices.GetService<ISQLiteFileRepository>();
            var allSensor = repo.LoadSensorMasterData().Where(x => x.Enabled == true);
            foreach (var item in allSensor)
            {
                sharedData.LastSensorData[item.SenderMAC] = (Temperature: 0, IngestionTime: DateTime.MinValue, BatteryLevel: 0);
            }

            timer = new Timer(
                callback: new TimerCallback(TimerTask),
                state: sharedData,
                dueTime: 600000,
                period: 600000);


            app.UseCors("CorsPolicy"); //Only 4 test. 
            app.UseMvc();

            var lifeTime = app.ApplicationServices.GetService<IApplicationLifetime>();
            lifeTime.ApplicationStopping.Register(() =>
            {
                SQLiteFileRepository.CleanUp();
            });

        }

        private static void TimerTask(object sharedData)
        {
            List<string> sensorNotReceived = new List<string>();
            var sensorData = sharedData as ISharedData;
            foreach (var item in sensorData.LastSensorData)
            {
                var delta = DateTime.UtcNow - item.Value.IngestionTime.ToUniversalTime();
                if (delta.TotalMinutes > 60) // Not seen for at least 1 hour
                {
                    sensorNotReceived.Add($"Sensor not received = {item.Key}. Last seens: {item.Value.IngestionTime}");
                }
            }

            if (sensorNotReceived.Count > 0)
            {
                SendMail(sensorNotReceived);
            }
        }

        private static void SendMail(List<string> sensorNotReceived)
        {
            // TODO Read Data from configuration or database

            /*
            SmtpClient client = new SmtpClient("");
            client.UseDefaultCredentials = false;
            client.Credentials = new NetworkCredential("", "");

            MailMessage mailMessage = new MailMessage();
            mailMessage.From = new MailAddress("");
            mailMessage.To.Add("");
            mailMessage.Body = string.Join("\n", sensorNotReceived.ToArray()); 
            mailMessage.Subject = "SensorNotReceived";
            client.Send(mailMessage);
            */
        }
    }
}
