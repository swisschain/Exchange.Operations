using System.Reflection;
using Autofac;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Operations.Configuration;
using Swisschain.Sdk.Server.Common;

namespace Operations
{
    public sealed class Startup : SwisschainStartup<AppConfig>
    {
        public Startup(IConfiguration configuration) : base(configuration)
        {
            AddJwtAuth(Config.Jwt.Secret, "exchange.swisschain.io");
        }

        protected override void ConfigureServicesExt(IServiceCollection services)
        {
            services
                .AddControllersWithViews()
                .AddFluentValidation(options =>
            {
                ValidatorOptions.CascadeMode = CascadeMode.StopOnFirstFailure;
                options.RegisterValidatorsFromAssembly(Assembly.GetEntryAssembly());
            });
        }

        protected override void ConfigureContainerExt(ContainerBuilder builder)
        {
            builder.RegisterModule(new AutofacModule(Config));
        }
    }
}
