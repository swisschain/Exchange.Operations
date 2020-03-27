using Swisschain.Exchange.Operations.ApiContract;

namespace Swisschain.Exchange.Operations.ApiClient
{
    public interface IOperationsClient
    {
        Monitoring.MonitoringClient Monitoring { get; }
    }
}
