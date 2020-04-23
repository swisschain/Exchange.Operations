using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ICashOperations
    {
        Task<OperationResponse> CashInAsync(string brokerId, CashInOutModel model);

        Task<OperationResponse> CashOutAsync(string brokerId, CashInOutModel model);

        Task<OperationResponse> CashTransferAsync(string brokerId, CashTransferModel model);
    }
}
