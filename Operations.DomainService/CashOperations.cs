using System.Threading.Tasks;
using MatchingEngine.Client;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public class CashOperations : ICashOperations
    {
        private readonly IMatchingEngineClient _matchingEngineClient;

        public CashOperations(IMatchingEngineClient matchingEngineClient)
        {
            _matchingEngineClient = matchingEngineClient;
        }

        public async Task CashInAsync(CashOperationModel model)
        {
            await _matchingEngineClient.CashOperations.CashInAsync(model.WalletId, model.AssetId, model.Amount);
        }

        public async Task CashOutAsync(CashOperationModel model)
        {
            await _matchingEngineClient.CashOperations.CashOutAsync(model.WalletId, model.AssetId, model.Amount);
        }
    }
}
