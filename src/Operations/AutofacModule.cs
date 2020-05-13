using Autofac;
using MatchingEngine.Client;
using MatchingEngine.Client.Extensions;
using Operations.Configuration;
using Operations.DomainService;
using Swisschain.Exchange.Fees.Client;
using Swisschain.Exchange.Fees.Client.Extensions;

namespace Operations.Modules
{
    public class AutofacModule : Module
    {
        private readonly AppConfig _config;

        public AutofacModule(AppConfig config)
        {
            _config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterMatchingEngineClient(new MatchingEngineClientSettings
            {
                CashOperationsServiceAddress = _config.MatchingEngine.CashOperationsServiceGrpcUrl,
                TradingServiceAddress = _config.MatchingEngine.TradingServiceGrpcUrl,
                BalancesServiceAddress = "http://172.16.0.4:4003"
            });

            builder.RegisterFeesClient(new FeesClientSettings{ ServiceAddress = _config.ExchangeFeesService.GrpcUrl });

            builder.RegisterType<LimitOrderOperations>()
                .As<ILimitOrderOperations>()
                .SingleInstance();

            builder.RegisterType<MarketOrderOperations>()
                .As<IMarketOrderOperations>()
                .SingleInstance();

            builder.RegisterType<CashOperations>()
                .As<ICashOperations>()
                .SingleInstance();
        }
    }
}
