using CryptoAnalyzer.CoinGecko;
using CryptoAnalyzer.Service;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using StackExchange.Exceptional;
using System;

namespace CryptoAnalyzer
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            InitializeExceptional(services);
            Context.Initialize(Configuration, Environment);

            services.AddHttpClient<IThrottledService, ThrottledService>(client =>
            {
                client.BaseAddress = new Uri(Context.CoinGeckoConfiguration.ApiBaseUrl);
            });
            services.AddSingleton<TelegramBot>();
            services.AddHostedService(provider => provider.GetService<TelegramBot>());
            services.AddHostedService<CoinGeckoApi>();
            
            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptional();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }

        private void InitializeExceptional(IServiceCollection services)
        {
            var exceptionalSettings = new ExceptionalSettings();
            Configuration.GetSection("Exceptional").Bind(exceptionalSettings);
            exceptionalSettings.UseExceptionalPageOnThrow = Environment.IsDevelopment();
            Exceptional.Configure(exceptionalSettings);
            services.Add(new ServiceDescriptor(typeof(IOptions<ExceptionalSettings>), new OptionsWrapper<ExceptionalSettings>(Exceptional.Settings)));
        }
    }
}
