using System;
using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ILimitOrderOperations
    {
        Task<OperationResponse> CreateAsync(string brokerId, LimitOrderCreateModel model);

        Task<OperationResponse> CancelAsync(string brokerId, Guid limitOrderId);
    }
}
