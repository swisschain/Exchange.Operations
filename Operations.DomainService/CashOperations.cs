using System;
using System.Globalization;
using System.Threading.Tasks;
using MatchingEngine.Client;
using MatchingEngine.Client.Contracts.Incoming;
using Microsoft.Extensions.Logging;
using Operations.DomainService.Model;
using Swisschain.Exchange.Fees.Client;
using Swisschain.Exchange.Fees.Client.Models.CashOperationsFees;
using Swisschain.Exchange.Fees.Client.Models.Settings;
using Fee = MatchingEngine.Client.Contracts.Incoming.Fee;
using FeeType = MatchingEngine.Client.Contracts.Incoming.FeeType;

namespace Operations.DomainService
{
    public class CashOperations : ICashOperations
    {
        private readonly IMatchingEngineClient _matchingEngineClient;
        private readonly IFeesClient _feesClient;
        private readonly ILogger<CashOperations> _logger;

        public CashOperations(IMatchingEngineClient matchingEngineClient, IFeesClient feesClient, ILogger<CashOperations> logger)
        {
            _matchingEngineClient = matchingEngineClient;
            _feesClient = feesClient;
            _logger = logger;
        }

        public async Task<OperationResponse> CashInAsync(string brokerId, CashInOutModel model)
        {
            var request = new CashInOutOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.Wallet,
                AssetId = model.Asset,
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                Description = model.Description
            };

            var fee = await GetFeeAsync(brokerId, model.Asset, RequestType.CashIn);

            request.Fees.Add(fee);

            var result = await _matchingEngineClient.CashOperations.CashInOutAsync(request);

            return new OperationResponse(result);
        }

        public async Task<OperationResponse> CashOutAsync(string brokerId, CashInOutModel model)
        {
            var volume = model.Volume >= 0 ? -model.Volume : model.Volume;

            var request = new CashInOutOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                WalletId = model.Wallet,
                AssetId = model.Asset,
                Volume = volume.ToString(CultureInfo.InvariantCulture),
                Description = model.Description
            };

            var fee = await GetFeeAsync(brokerId, model.Asset, RequestType.CashOut);

            request.Fees.Add(fee);

            var result = await _matchingEngineClient.CashOperations.CashInOutAsync(request);

            return new OperationResponse(result);
        }

        public async Task<OperationResponse> CashTransferAsync(string brokerId, CashTransferModel model)
        {
            var request = new CashTransferOperation
            {
                Id = Guid.NewGuid().ToString(),
                BrokerId = brokerId,
                AssetId = model.Asset,
                Volume = model.Volume.ToString(CultureInfo.InvariantCulture),
                FromWalletId = model.FromWallet,
                ToWalletId = model.ToWallet,
                Description = model.Description
            };

            var fee = await GetFeeAsync(brokerId, model.Asset, RequestType.Transfer);

            request.Fees.Add(fee);

            var result = await _matchingEngineClient.CashOperations.CashTransferAsync(request);

            return new OperationResponse(result);
        }

        private async Task<Fee> GetFeeAsync(string brokerId, string asset, RequestType requestType)
        {
            var cashOperationsFee = await GetCashOperationsFeeAsync(brokerId, asset);
            var settings = await GetFeesSettingsAsync(brokerId);

            if (cashOperationsFee == null)
            {
                _logger.LogWarning("CashOperationsFee from Exchange.Fees is null. The fee is set to 0. " +
                    "BrokerId={$BrokerId}; Asset={$Asset}", brokerId, asset);

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

            decimal value = 0;
            var feeType = CashOperationsFeeTypeModel.Absolute;

            switch (requestType)
            {
                case RequestType.CashIn:
                    value = cashOperationsFee.CashInValue;
                    feeType = cashOperationsFee.CashInFeeType;
                    break;
                case RequestType.CashOut:
                    value = cashOperationsFee.CashOutValue;
                    feeType = cashOperationsFee.CashOutFeeType;
                    break;
                case RequestType.Transfer:
                    value = cashOperationsFee.CashTransferValue;
                    feeType = cashOperationsFee.CashTransferFeeType;
                    break;
            }

            var result = new Fee
            {
                Size = Map(value, feeType).ToString(CultureInfo.InvariantCulture),
                SizeType = (int)Map(feeType),
                TargetWalletId = settings.FeeWalletId,
                Type = (int)Map(value)
            };

            return result;
        }

        private async Task<CashOperationsFeeModel> GetCashOperationsFeeAsync(string brokerId, string asset)
        {
            try
            {
                var result = await _feesClient.CashOperationsFees.GetByBrokerIdAndAsset(brokerId, asset);

                return result;
            }
            catch (Exception e)
            {
                _logger.LogWarning(e, "Something went wrong while requesting CashOperationsFee. The fee is set to 0. " +
                                      "BrokerId={$BrokerId}; Asset={$Asset}", brokerId, asset);

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

        private FeeSizeType Map(CashOperationsFeeTypeModel feeType)
        {
            if (feeType == CashOperationsFeeTypeModel.Absolute
                || feeType == CashOperationsFeeTypeModel.None)
                return FeeSizeType.Absolute;

            return FeeSizeType.Percentage;
        }

        private FeeType Map(decimal size)
        {
            return size == 0 ? FeeType.NoFee : FeeType.ClientFee;
        }

        private decimal Map(decimal size, CashOperationsFeeTypeModel feeType)
        {
            var sizeType = Map(feeType);

            if (sizeType == FeeSizeType.Percentage)
                return size / 100;

            return size;
        }

        private enum RequestType
        {
            CashIn = 0,
            CashOut = 1,
            Transfer = 2
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
