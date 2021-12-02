using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using MyCompany.MyProduct.EntityFrameworkCore;
using MyCompany.MyProduct.MultiTenancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Volo.Abp;
using Volo.Abp.Account;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Authentication.JwtBearer;
using Volo.Abp.AspNetCore.MultiTenancy;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Basic.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.Swashbuckle;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;

namespace MyCompany.MyProduct
{
    [DependsOn(
        typeof(AbpAccountWebIdentityServerModule),
        typeof(AbpAspNetCoreAuthenticationJwtBearerModule),
        typeof(AbpAspNetCoreMultiTenancyModule),
        typeof(AbpAspNetCoreMvcUiBasicThemeModule),
        typeof(AbpAspNetCoreSerilogModule),
        typeof(AbpAutofacModule),
        typeof(AbpSwashbuckleModule),
        typeof(MyProductApplicationModule),
        typeof(MyProductEntityFrameworkCoreModule),
        typeof(MyProductHttpApiModule))]
    public class MyProductHttpApiHostModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            IConfiguration configuration = context.Services.GetConfiguration();
            IWebHostEnvironment hostingEnvironment = context.Services.GetHostingEnvironment();

            ConfigureBundles();
            ConfigureUrls(configuration);
            ConfigureConventionalControllers();
            ConfigureAuthentication(context, configuration);
            ConfigureLocalization();
            ConfigureVirtualFileSystem(context);
            ConfigureCors(context, configuration);
            ConfigureSwaggerServices(context, configuration);
        }

        public override void OnApplicationInitialization(ApplicationInitializationContext context)
        {
            IApplicationBuilder app = context.GetApplicationBuilder();
            IWebHostEnvironment env = context.GetEnvironment();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseAbpRequestLocalization();

            if (!env.IsDevelopment())
            {
                app.UseErrorPage();
            }

            app.UseCorrelationId();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseJwtTokenMiddleware();

            if (MultiTenancyConsts.IsEnabled)
            {
                app.UseMultiTenancy();
            }

            app.UseUnitOfWork();
            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseAbpSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "MyProduct API");

                IConfiguration configuration = context.ServiceProvider.GetRequiredService<IConfiguration>();
                c.OAuthClientId(configuration["AuthServer:SwaggerClientId"]);
                c.OAuthClientSecret(configuration["AuthServer:SwaggerClientSecret"]);
                c.OAuthScopes("MyProduct");
            });

            app.UseAuditing();
            app.UseAbpSerilogEnrichers();
            app.UseConfiguredEndpoints();
        }

        private static void ConfigureSwaggerServices(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddAbpSwaggerGenWithOAuth(
                configuration["AuthServer:Authority"],
                new Dictionary<string, string>
                {
                    { "MyProduct", "MyProduct API" }
                },
                options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyProduct API", Version = "v1" });

                    options.DocInclusionPredicate((docName, description) => true);
                    options.CustomSchemaIds(type => type.FullName);

                    // Set the comments path for the Swagger JSON and UI.
                    List<string> xmlFileNames = new List<string>()
                    {
                        "MyCompany.MyProduct.HttpApi.Host.xml",
                        "MyCompany.MyProduct.HttpApi.xml"
                    };
                    foreach (string xmlFileName in xmlFileNames)
                    {
                        string xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
                        options.IncludeXmlComments(xmlFilePath);
                    }
                });
        }

        private void ConfigureBundles()
        {
            Configure<AbpBundlingOptions>(options =>
            {
                options.StyleBundles.Configure(
                    BasicThemeBundles.Styles.Global,
                    bundle => { bundle.AddFiles("/global-styles.css"); });
            });
        }

        private void ConfigureUrls(IConfiguration configuration)
        {
            Configure<AppUrlOptions>(options =>
            {
                options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
                options.RedirectAllowedUrls.AddRange(configuration["App:RedirectAllowedUrls"].Split(','));

                options.Applications["Angular"].RootUrl = configuration["App:ClientUrl"];
                options.Applications["Angular"].Urls[AccountUrlNames.PasswordReset] = "account/reset-password";
            });
        }

        private void ConfigureVirtualFileSystem(ServiceConfigurationContext context)
        {
            IWebHostEnvironment hostingEnvironment = context.Services.GetHostingEnvironment();

            if (hostingEnvironment.IsDevelopment())
            {
                Configure<AbpVirtualFileSystemOptions>(options =>
                {
                    options.FileSets.ReplaceEmbeddedByPhysical<MyProductDomainSharedModule>(
                        Path.Combine(
                            hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompany.MyProduct.Domain.Shared"));
                    options.FileSets.ReplaceEmbeddedByPhysical<MyProductDomainModule>(
                        Path.Combine(
                            hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompany.MyProduct.Domain"));
                    options.FileSets.ReplaceEmbeddedByPhysical<MyProductApplicationContractsModule>(
                        Path.Combine(
                            hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompany.MyProduct.Application.Contracts"));
                    options.FileSets.ReplaceEmbeddedByPhysical<MyProductApplicationModule>(
                        Path.Combine(
                            hostingEnvironment.ContentRootPath,
                            $"..{Path.DirectorySeparatorChar}MyCompany.MyProduct.Application"));
                });
            }
        }

        private void ConfigureConventionalControllers()
        {
            Configure<AbpAspNetCoreMvcOptions>(options =>
            {
                options.ConventionalControllers.Create(typeof(MyProductApplicationModule).Assembly);
            });
        }

        private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.Authority = configuration["AuthServer:Authority"];
                    options.RequireHttpsMetadata = Convert.ToBoolean(configuration["AuthServer:RequireHttpsMetadata"]);
                    options.Audience = "MyProduct";
                    options.BackchannelHttpHandler = new HttpClientHandler
                    {
                        ServerCertificateCustomValidationCallback =
                            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                });
        }

        private void ConfigureLocalization()
        {
            Configure<AbpLocalizationOptions>(options =>
            {
                options.Languages.Add(new LanguageInfo("en", "en", "English", "gb"));
                options.Languages.Add(new LanguageInfo("nl", "nl", "Nederlands", "nl"));
            });
        }

        private void ConfigureCors(ServiceConfigurationContext context, IConfiguration configuration)
        {
            context.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
               {
                   builder
                       .WithOrigins(
                           configuration["App:CorsOrigins"]
                               .Split(",", StringSplitOptions.RemoveEmptyEntries)
                               .Select(o => o.RemovePostFix("/"))
                               .ToArray())
                       .WithAbpExposedHeaders()
                       .SetIsOriginAllowedToAllowWildcardSubdomains()
                       .AllowAnyHeader()
                       .AllowAnyMethod()
                       .AllowCredentials();
               });
            });
        }
    }
}