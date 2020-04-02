using System;
using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ILimitOrderOperations
    {
        Task<OperationResponse> CreateAsync(LimitOrderCreateModel model);

        Task<OperationResponse> CancelAsync(Guid limitOrderId);
    }
}
