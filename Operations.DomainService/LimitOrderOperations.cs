using System;
using System.Globalization;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MatchingEngine.Client;
using MatchingEngine.Client.Contracts.Incoming;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public class LimitOrderOperations : ILimitOrderOperations
    {
        private readonly IMatchingEngineClient _matchingEngineClient;

        public LimitOrderOperations(IMatchingEngineClient matchingEngineClient)
        {
            _matchingEngineClient = matchingEngineClient;
        }

        public async Task<OperationResponse> CreateAsync(string brokerId, LimitOrderCreateModel model)
        {
            LimitOrder request = new LimitOrder
            {
                BrokerId = brokerId,
                WalletId = model.WalletId,
                AssetPairId = model.AssetPairId,
                CancelAllPreviousLimitOrders = model.CancelPrevious,
                Price = model.Price.ToString(CultureInfo.InvariantCulture),
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            var response = await _matchingEngineClient.Trading.CreateLimitOrderAsync(request);

            var result = new OperationResponse(response);

            return result;
        }

        public async Task<OperationResponse> CancelAsync(string brokerId, Guid limitOrderId)
        {
            LimitOrderCancel request = new LimitOrderCancel
            {
                BrokerId = brokerId,
                Uid = limitOrderId.ToString()
            };

            var response = await _matchingEngineClient.Trading.CancelLimitOrderAsync(request);

            var result = new OperationResponse(response);

            return result;
        }
    }
}
