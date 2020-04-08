using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ICashOperations
    {
        Task CashInAsync(string brokerId, CashOperationModel model);

        Task CashOutAsync(string brokerId, CashOperationModel model);
    }
}
