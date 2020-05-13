using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MatchingEngine.Client;
using MatchingEngine.Client.Contracts.Incoming;
using Microsoft.Extensions.Logging;
using Operations.DomainService.Model;
using Swisschain.Exchange.Fees.Client;
using Swisschain.Exchange.Fees.Client.Models.Settings;
using Swisschain.Exchange.Fees.Client.Models.TradingFees;
using Fee = MatchingEngine.Client.Contracts.Incoming.Fee;
using FeeType = MatchingEngine.Client.Contracts.Incoming.FeeType;

namespace Operations.DomainService
{
    public class MarketOrderOperations : IMarketOrderOperations
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IFeesClient _feesClient;
        private readonly ILogger<MarketOrderOperations> _logger;

        public MarketOrderOperations(IMatchingEngineClient matchingEngineClient, IFeesClient feesClient, ILogger<MarketOrderOperations> logger)
        {
            _matchingEngineClient = matchingEngineClient;
            _feesClient = feesClient;
            _logger = logger;
        }

        public async Task<CreateMarketOrderResponse> CreateAsync(string brokerId, MarketOrderCreateModel model)
        {
            var request = new MarketOrder
            {
                Uid = model.Id.HasValue ? model.Id.Value.ToString() : Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.WalletId,
                AssetPairId = model.Symbol,
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            var fee = await GetFee(brokerId, model.Symbol);

            request.Fees.Add(fee);

            var response = await _matchingEngineClient.Trading.CreateMarketOrderAsync(request);

            var result = new CreateMarketOrderResponse(response);

            return result;
        }

        private async Task<Fee> GetFee(string brokerId, string assetPair)
        {
            var tradingFee = await GetTradingFeeAsync(brokerId, assetPair);

            var settings = await GetFeesSettingsAsync(brokerId);

            if (tradingFee == null)
            {
                _logger.LogWarning("TradingFee from Exchange.Fees is null. The fee is set to 0. " +
                                   "BrokerId={$BrokerId}; AssetPair={$AssetPair}", brokerId, assetPair);

                return NoFee();
            }

            if (settings == null)
            {
                _logger.LogWarning("Settings is null. The fee is set to 0." +
                                   "BrokerId={$BrokerId}", brokerId);

                return NoFee();
            }

            if (string.IsNullOrWhiteSpace(settings.FeeWalletId))
            {
                _logger.LogWarning("FeeWalletId is empty. The fee is set to 0. BrokerId={$BrokerId}", brokerId);

                return NoFee();
            }

            if (tradingFee.Levels == null || tradingFee.Levels.Count == 0)
            {
                _logger.LogWarning("TradingFee from Exchange.Fees has no levels. The fee is set to 0. " +
                                   "BrokerId={$BrokerId}; AssetPair={$AssetPair}", brokerId, assetPair);

                return NoFee();
            }

            var feeWallet = settings.FeeWalletId;

            // TODO: take first level as we don't have 30 days volume yet
            var level = tradingFee.Levels.OrderBy(x => x.Volume).First();

            var size = level.TakerFee / 100;

            var feeType = size == 0 ? (int)FeeType.NoFee : (int)FeeType.ClientFee;

            var result = new Fee
            {
                Size = size.ToString(CultureInfo.InvariantCulture),
                SizeType = (int)FeeSizeType.Percentage,
                TargetWalletId = feeWallet,
                Type = feeType
            };

            return result;
        }

        private async Task<TradingFeeModel> GetTradingFeeAsync(string brokerId, string assetPair)
        {
            try
            {
                var result = await _feesClient.TradingFees.GetByBrokerIdAndAssetPair(brokerId, assetPair);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Something went wrong while requesting TradingFee. The fee is set to 0. " +
                                      "BrokerId={$BrokerId}; AssetPair={$AssetPair}", brokerId, assetPair);

                return null;
            }
        }

        private async Task<SettingsModel> GetFeesSettingsAsync(string brokerId)
        {
            try
            {
                var result = await _feesClient.Settings.GetByBrokerId(brokerId);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Something went wrong while requesting broker FeesSettings. The fee is set to 0. " +
                                      "BrokerId={$BrokerId}", brokerId);

                return null;
            }
        }

        private Fee NoFee()
        {
            return new Fee
            {
                Size = 0.ToString(),
                Type = (int)FeeType.NoFee
            };
        }
    }
}
