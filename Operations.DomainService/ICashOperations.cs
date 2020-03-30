using System.Threading.Tasks;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public interface ICashOperations
    {
        Task CashInAsync(CashOperationModel model);

        Task CashOutAsync(CashOperationModel model);
    }
}
