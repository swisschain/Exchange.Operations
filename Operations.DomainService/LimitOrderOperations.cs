using System;
using System.Threading.Tasks;
using MatchingEngine.Client;
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

        public async Task<LimitOrderResponse> CreateAsync(LimitOrderCreateModel model)
        {


            var result = await _matchingEngineClient.Trading.CreateLimitOrderAsync(
                new MatchingEngine.Client.Models.Trading.LimitOrderRequestModel
                {
                    Id = Guid.NewGuid(),
                    AssetPairId = model.AssetPairId,
                    Price = model.Price,
                    Volume = model.Type == LimitOrderType.Sell
                        ? -model.Volume
                        : model.Volume,
                    WalletId = model.WalletId,
                    CancelAllPreviousLimitOrders = model.CancelPrevious,
                    Timestamp = DateTime.UtcNow
                });

            var response = new LimitOrderResponse { Id = result.Id, Status = result.Status, Reason = result.Reason };

            return response;
        }

        public async Task CancelAsync(Guid limitOrderId)
        {
            await _matchingEngineClient.Trading.CancelLimitOrderAsync(limitOrderId);
        }
    }
}
