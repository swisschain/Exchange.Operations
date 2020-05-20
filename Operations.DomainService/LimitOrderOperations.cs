using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using MatchingEngine.Client;
using MatchingEngine.Client.Contracts.Incoming;
using Microsoft.Extensions.Logging;
using Operations.DomainService.Model;
using Swisschain.Exchange.Accounts.Client;
using Swisschain.Exchange.Fees.Client;
using Swisschain.Exchange.Fees.Client.Models.Settings;
using Swisschain.Exchange.Fees.Client.Models.TradingFees;
using FeeType = MatchingEngine.Client.Contracts.Incoming.FeeType;

namespace Operations.DomainService
{
    public class LimitOrderOperations : ILimitOrderOperations
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IFeesClient _feesClient;
        private readonly IAccountsClient _accountsClient;
        private readonly ILogger<LimitOrderOperations> _logger;

        public LimitOrderOperations(IMatchingEngineClient matchingEngineClient,
            IFeesClient feesClient, IAccountsClient accountsClient, ILogger<LimitOrderOperations> logger)
        {
            _matchingEngineClient = matchingEngineClient;
            _feesClient = feesClient;
            _accountsClient = accountsClient;
            _logger = logger;
        }

        public async Task<OperationResponse> CreateAsync(string brokerId, LimitOrderCreateModel model)
        {
            var wallet = await _accountsClient.Wallet.GetAsync(model.WalletId, brokerId);

            if (wallet == null)
                throw new ArgumentException($"Wallet '{model.WalletId}' does not exist.");

            if (!wallet.IsEnabled)
                throw new ArgumentException($"Wallet '{model.WalletId}' is disabled.");

            var request = new LimitOrder
            {
                Uid = model.Id.HasValue ? model.Id.Value.ToString() : Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.WalletId.ToString(CultureInfo.InvariantCulture),
                AssetPairId = model.AssetPair,
                CancelAllPreviousLimitOrders = model.CancelPrevious,
                Price = model.Price.ToString(CultureInfo.InvariantCulture),
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                Type = model.Type == LimitOrderType.StopLimit ? LimitOrder.Types.LimitOrderType.StopLimit : LimitOrder.Types.LimitOrderType.Limit,
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            var limitOrderFee = await GetFee(brokerId, model.AssetPair);

            request.Fees.Add(limitOrderFee);

            var response = await _matchingEngineClient.Trading.CreateLimitOrderAsync(request);

            var result = new OperationResponse(response);

            return result;
        }

        public async Task<OperationResponse> CancelAsync(string brokerId, Guid limitOrderId)
        {
            LimitOrderCancel request = new LimitOrderCancel
            {
                Uid = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                LimitOrderId = { limitOrderId.ToString() }
            };

            var response = await _matchingEngineClient.Trading.CancelLimitOrderAsync(request);

            var result = new OperationResponse(response);

            return result;
        }

        private async Task<LimitOrderFee> GetFee(string brokerId, string assetPair)
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

            var makerSize = level.MakerFee / 100;
            var takerSize = level.TakerFee / 100;

            var feeType = makerSize == 0 && takerSize == 0 ? (int)FeeType.NoFee : (int)FeeType.ClientFee;

            var result = new LimitOrderFee
            {
                MakerSize = makerSize.ToString(CultureInfo.InvariantCulture),
                MakerSizeType = (int)FeeSizeType.Percentage,
                TakerSize = takerSize.ToString(CultureInfo.InvariantCulture),
                TakerSizeType = (int)FeeSizeType.Percentage,
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

        private LimitOrderFee NoFee()
        {
            return new LimitOrderFee
            {
                MakerSize = 0.ToString(),
                TakerSize = 0.ToString(),
                Type = (int)FeeType.NoFee
            };
        }
    }
}
