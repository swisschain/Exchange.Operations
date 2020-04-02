using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface IMarketOrderOperations
    {
        Task<CreateMarketOrderResponse> CreateAsync(MarketOrderCreateModel model);
    }
}
