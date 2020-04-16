using System;
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

        public async Task<OperationResponse> CashInAsync(string brokerId, CashOperationModel model)
        {
            CashInOutOperation request = new CashInOutOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.ClientId,
                AssetId = model.Symbol,
                Volume = model.Amount.ToString(CultureInfo.InvariantCulture)
            };

            var result = await _matchingEngineClient.CashOperations.CashInOutAsync(request);

            return new OperationResponse(result);
        }

        public async Task<OperationResponse> CashOutAsync(string brokerId, CashOperationModel model)
        {
            var volume = model.Amount >= 0 ? -model.Amount : model.Amount;

            CashInOutOperation request = new CashInOutOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.ClientId,
                AssetId = model.Symbol,
                Volume = volume.ToString(CultureInfo.InvariantCulture)
            };

            var result = await _matchingEngineClient.CashOperations.CashInOutAsync(request);

            return new OperationResponse(result);
        }
    }
}
