using System.Globalization;
using System.Threading.Tasks;
using MatchingEngine.Client;
using MatchingEngine.Client.Contracts.Incoming;
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
            CashInOutOperation request = new CashInOutOperation
            {
                WalletId = model.ClientId,
                AssetId = model.AssetId,
                Volume = model.Amount.ToString(CultureInfo.InvariantCulture)
            };

            await _matchingEngineClient.CashOperations.CashInOutAsync(request);
        }

        public async Task CashOutAsync(CashOperationModel model)
        {
            CashInOutOperation request = new CashInOutOperation
            {
                WalletId = model.ClientId,
                AssetId = model.AssetId,
                Volume = (-model.Amount).ToString(CultureInfo.InvariantCulture)
            };

            await _matchingEngineClient.CashOperations.CashInOutAsync(request);
        }
    }
}
