using System;
using System.Globalization;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MatchingEngine.Client;
using MatchingEngine.Client.Contracts.Incoming;
using Operations.DomainService.Model;

namespace Operations.DomainService
{
    public class MarketOrderOperations : IMarketOrderOperations
    {
        private readonly IMatchingEngineClient _matchingEngineClient;

        public MarketOrderOperations(IMatchingEngineClient matchingEngineClient)
        {
            _matchingEngineClient = matchingEngineClient;
        }

        public async Task<CreateMarketOrderResponse> CreateAsync(MarketOrderCreateModel model)
        {
            MarketOrder request = new MarketOrder
            {
                Uid = model.Id?.ToString(),
                WalletId = model.WalletId,
                AssetPairId = model.AssetPairId,
                MessageId = model.MessageId,
                ReservedLimitVolume = model.ReservedLimitVolume.ToString(CultureInfo.InvariantCulture),
                Straight = model.Type != OrderType.Sell,
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            var response = await _matchingEngineClient.Trading.CreateMarketOrderAsync(request);

            var result = new CreateMarketOrderResponse(response);

            return result;
        }
    }
}
