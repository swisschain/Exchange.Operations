using System;
using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ILimitOrderOperations
    {
        Task<LimitOrderResponse> CreateAsync(LimitOrderCreateModel model);

        Task CancelAsync(Guid limitOrderId);
    }
}
