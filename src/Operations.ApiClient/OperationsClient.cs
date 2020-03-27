using Swisschain.Exchange.Operations.ApiClient.Common;
using Swisschain.Exchange.Operations.ApiContract;

namespace Swisschain.Exchange.Operations.ApiClient
{
    public class OperationsClient : BaseGrpcClient, IOperationsClient
    {
        public OperationsClient(string serverGrpcUrl) : base(serverGrpcUrl)
        {
            Monitoring = new Monitoring.MonitoringClient(Channel);
        }

        public Monitoring.MonitoringClient Monitoring { get; }
    }
}
