using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ICashOperations
    {
        Task<OperationResponse> CashInAsync(string brokerId, CashOperationModel model);

        Task<OperationResponse> CashOutAsync(string brokerId, CashOperationModel model);

        Task<OperationResponse> CashTransferAsync(string brokerId, CashTransferModel model);
    }
}
