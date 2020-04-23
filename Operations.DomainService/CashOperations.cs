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

        public async Task<OperationResponse> CashInAsync(string brokerId, CashInOutModel model)
        {
            CashInOutOperation request = new CashInOutOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.Wallet,
                AssetId = model.Asset,
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                Description = model.Description
            };

            var result = await _matchingEngineClient.CashOperations.CashInOutAsync(request);

            return new OperationResponse(result);
        }

        public async Task<OperationResponse> CashOutAsync(string brokerId, CashInOutModel model)
        {
            var volume = model.Volume >= 0 ? -model.Volume : model.Volume;

            CashInOutOperation request = new CashInOutOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.Wallet,
                AssetId = model.Asset,
                Volume = volume.ToString(CultureInfo.InvariantCulture),
                Description = model.Description
            };

            var result = await _matchingEngineClient.CashOperations.CashInOutAsync(request);

            return new OperationResponse(result);
        }

        public async Task<OperationResponse> CashTransferAsync(string brokerId, CashTransferModel model)
        {
            CashTransferOperation request = new CashTransferOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                AssetId = model.Asset,
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                FromWalletId = model.FromWallet,
                ToWalletId = model.ToWallet
            };

            var result = await _matchingEngineClient.CashOperations.CashTransferAsync(request);

            return new OperationResponse(result);
        }
    }
}
