using ComponentSpace.Saml2.Configuration;
using ExampleIdentityProvider.Data;
using ExampleIdentityProvider.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace ExampleIdentityProvider
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Account/Manage");
                    options.Conventions.AuthorizePage("/Account/Logout");
                });

            // Register no-op EmailSender used by account confirmation and password reset during development
            // For more information on how to enable account confirmation and password reset please visit https://go.microsoft.com/fwlink/?LinkID=532713
            services.AddSingleton<IEmailSender, EmailSender>();

            // Use a unique identity cookie name rather than sharing the cookie across applications in the domain.
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "ExampleIdentityProvider.Identity";
            });

            // Register the SAML configuration.
            services.Configure<SamlConfigurations>(Configuration.GetSection("SAML"));

            // Add SAML SSO services.
            services.AddSaml();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
        }

        // Demonstrates loading SAML configuration programmatically 
        // rather than through appsettings.json or another JSON configuration file.
        // This is useful if configuration is stored in a custom database, for example.
        // The SAML configuration is registered in ConfigureServices by calling:
        // services.Configure<SamlConfigurations>(config => ConfigureSaml(config));
        private void ConfigureSaml(SamlConfigurations samlConfigurations)
        {
            samlConfigurations.Configurations = new List<SamlConfiguration>()
            {
                new SamlConfiguration()
                {
                    ID = "Default",
                    LocalIdentityProviderConfiguration = new LocalIdentityProviderConfiguration()
                    {
                        Name = "https://ExampleIdentityProvider",
                        Description = "Example Identity Provider",
                        SingleSignOnServiceUrl = "https://localhost:44313/SAML/SingleSignOnService",
                        LocalCertificates = new List<Certificate>()
                        {
                            new Certificate()
                            {
                                FileName = "certificates/idp.pfx",
                                Password = "password"
                            }
                        }
                    },
                    PartnerServiceProviderConfigurations = new List<PartnerServiceProviderConfiguration>()
                    {
                        new PartnerServiceProviderConfiguration()
                        {
                            Name = "https://ExampleServiceProvider",
                            Description = "Example Service Provider",
                            WantAuthnRequestSigned = true,
                            SignSamlResponse = true,
                            AssertionConsumerServiceUrl = "https://localhost:44360/SAML/AssertionConsumerService",
                            SingleLogoutServiceUrl = "https://localhost:44360/SAML/SLOService",
                            PartnerCertificates = new List<Certificate>()
                            {
                                new Certificate()
                                {
                                    FileName = "certificates/sp.cer"
                                }
                            }
                        }
                    }
                }
            };
        }
    }
}
