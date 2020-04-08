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

        public async Task CashInAsync(string brokerId, CashOperationModel model)
        {
            CashInOutOperation request = new CashInOutOperation
            {
                BrokerId = brokerId,
                WalletId = model.ClientId,
                AssetId = model.AssetId,
                Volume = model.Amount.ToString(CultureInfo.InvariantCulture)
            };

            await _matchingEngineClient.CashOperations.CashInOutAsync(request);
        }

        public async Task CashOutAsync(string brokerId, CashOperationModel model)
        {
            var volume = model.Amount >= 0 ? -model.Amount : model.Amount;

            CashInOutOperation request = new CashInOutOperation
            {
                BrokerId = brokerId,
                WalletId = model.ClientId,
                AssetId = model.AssetId,
                Volume = volume.ToString(CultureInfo.InvariantCulture)
            };

            await _matchingEngineClient.CashOperations.CashInOutAsync(request);
        }
    }
}
