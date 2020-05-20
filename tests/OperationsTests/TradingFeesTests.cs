using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MatchingEngine.Client;
using MatchingEngine.Client.Api;
using MatchingEngine.Client.Contracts.Incoming;
using Moq;
using Operations.DomainService;
using Operations.DomainService.Model;
using OperationsTests.Utils;
using Swisschain.Exchange.Accounts.Client;
using Swisschain.Exchange.Accounts.Client.Api;
using Swisschain.Exchange.Accounts.Client.Models.Wallet;
using Swisschain.Exchange.Fees.Client;
using Swisschain.Exchange.Fees.Client.Api;
using Swisschain.Exchange.Fees.Client.Models.Settings;
using Swisschain.Exchange.Fees.Client.Models.TradingFees;
using Xunit;
using Fee = MatchingEngine.Client.Contracts.Incoming.Fee;
using FeeType = MatchingEngine.Client.Contracts.Incoming.FeeType;

namespace OperationsTests
{
    public class TradingFeesTests : BaseTests
    {
        private const string BrokerId = "BrokerId";
        private const long Wallet = 7799;
        private const string Btc = "BTC";
        private const string BtcUsd = "BTCUSD";
        private const decimal MakerFee = 2;
        private const decimal TakerFee = 3;
        private const decimal Price = 99.77m;
        private const decimal Volume = 1;
        private const bool CancelPrevious = false;
        private const LimitOrderType LimitOrderTypeModel = LimitOrderType.StopLimit;
        private const LimitOrder.Types.LimitOrderType LimitOrderTypeValue = LimitOrder.Types.LimitOrderType.StopLimit;
        private readonly Guid Guid = Guid.NewGuid();

        private IMatchingEngineClient InitializeMatchingEngineClient(
            Action<MarketOrder> marketOrderAssertMethod,
            Action<LimitOrder> limitOrderAssertMethod,
            AssertCalls assertCalls)
        {
            var matchingEngineClientMock = new Mock<IMatchingEngineClient>();

            var tradingApiMock = new Mock<ITradingApi>();
            tradingApiMock.Setup(x => x.CreateMarketOrderAsync(It.IsAny<MarketOrder>(), default))
                .Returns((MarketOrder marketOrder, CancellationToken ct) =>
                {
                    marketOrderAssertMethod(marketOrder);

                    assertCalls.Count++;

                    return Task.FromResult(new MarketOrderResponse());
                });
            tradingApiMock.Setup(x => x.CreateLimitOrderAsync(It.IsAny<LimitOrder>(), default))
                .Returns((LimitOrder limitOrder, CancellationToken ct) =>
                {
                    limitOrderAssertMethod(limitOrder);

                    assertCalls.Count++;

                    return Task.FromResult(new Response());
                });
            matchingEngineClientMock.SetupGet(x => x.Trading).Returns(() => tradingApiMock.Object);

            return matchingEngineClientMock.Object;
        }

        private IFeesClient InitializeFeesClient(TradingFeeModel feeModel, SettingsModel settings)
        {
            var feesClientMock = new Mock<IFeesClient>();

            var tradingFeesApiMock = new Mock<ITradingFeesApi>();
            tradingFeesApiMock.Setup(x => x.GetByBrokerIdAndAssetPair(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string brokerId, string assetPair) =>
                {
                    return Task.FromResult(feeModel);
                });
            feesClientMock.SetupGet(x => x.TradingFees).Returns(() => tradingFeesApiMock.Object);

            var settingsApiMock = new Mock<ISettingsApi>();
            settingsApiMock.Setup(x => x.GetByBrokerId(It.IsAny<string>()))
                .Returns((string brokerId) =>
                {
                    return Task.FromResult(settings);
                });
            feesClientMock.SetupGet(x => x.Settings).Returns(() => settingsApiMock.Object);

            return feesClientMock.Object;
        }

        private IAccountsClient InitializeAccountsClient()
        {
            var accountsClientMock = new Mock<IAccountsClient>();

            var accountApiMock = new Mock<IAccountApi>();
            accountsClientMock.SetupGet(x => x.Account).Returns(() => accountApiMock.Object);

            var walletApiMock = new Mock<IWalletApi>();
            walletApiMock.Setup(x => x.GetAsync(It.IsAny<long>(), It.IsAny<string>()))
                .Returns((long id, string brokerId) =>
                {
                    var wallet = new WalletModel();

                    wallet.Id = id;
                    wallet.BrokerId = brokerId;
                    wallet.Type = WalletType.Main;
                    wallet.IsEnabled = true;

                    return Task.FromResult(wallet);
                });
            walletApiMock.Setup(x => x.GetAllAsync(It.IsAny<long[]>(), It.IsAny<string>()))
                .Returns((IEnumerable<long> ids, string brokerId) =>
                {
                    var wallets = new List<WalletModel>();

                    foreach (var id in ids)
                    {
                        var wallet = new WalletModel();

                        wallet.Id = id;
                        wallet.BrokerId = brokerId;
                        wallet.Type = WalletType.Main;
                        wallet.IsEnabled = true;

                        wallets.Add(wallet);
                    }

                    return Task.FromResult((IReadOnlyList<WalletModel>)wallets);
                });
            accountsClientMock.SetupGet(x => x.Wallet).Returns(() => walletApiMock.Object);

            return accountsClientMock.Object;
        }

        private TradingFeeModel GetTradingFeeModel()
        {
            var result = new TradingFeeModel
            {
                BrokerId = BrokerId,
                Asset = Btc,
                AssetPair = BtcUsd,
                Levels = new List<TradingFeeLevelModel>
                {
                    new TradingFeeLevelModel { Id = Guid.NewGuid(), MakerFee = MakerFee*2, TakerFee = TakerFee*2, Volume = 1 },
                    new TradingFeeLevelModel { Id = Guid.NewGuid(), MakerFee = MakerFee, TakerFee = TakerFee, Volume = 0.5m }
                }
            };

            return result;
        }

        private SettingsModel GetSettingsModel()
        {
            var result = new SettingsModel
            {
                BrokerId = BrokerId,
                FeeWalletId = Wallet.ToString()
            };

            return result;
        }

        private void AssertMarketOrder(MarketOrder marketOrder, decimal volume)
        {
            Assert.NotNull(marketOrder);
            Assert.NotEmpty(marketOrder.Uid);
            Assert.Equal(marketOrder.Uid, Guid.ToString());
            Assert.Equal(marketOrder.BrokerId, BrokerId);
            Assert.Equal(marketOrder.WalletId, Wallet.ToString());
            Assert.Equal(marketOrder.AssetPairId, BtcUsd);
            Assert.Equal(marketOrder.Volume, volume.ToString(CultureInfo.InvariantCulture));
            Assert.NotEqual(default, marketOrder.Timestamp);
        }

        private void AssertLimitOrder(LimitOrder limitOrder, decimal volume)
        {
            Assert.NotNull(limitOrder);
            Assert.NotEmpty(limitOrder.Uid);
            Assert.Equal(limitOrder.Uid, Guid.ToString());
            Assert.Equal(limitOrder.BrokerId, BrokerId);
            Assert.Equal(limitOrder.AssetPairId, BtcUsd);
            Assert.Equal(limitOrder.Price, Price.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(limitOrder.Volume, volume.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(limitOrder.WalletId, Wallet.ToString());
            Assert.Equal(limitOrder.Type, LimitOrderTypeValue);
            Assert.Equal(limitOrder.CancelAllPreviousLimitOrders, CancelPrevious);
            Assert.NotEqual(default, limitOrder.Timestamp);
        }

        private void AssertFees(ICollection<Fee> fees, decimal feeSize)
        {
            Assert.NotEmpty(fees);
            Assert.Single(fees);
            var fee = fees.Single();
            Assert.Equal(fee.Size, feeSize.ToString(CultureInfo.InvariantCulture));

            if (feeSize == 0)
            {
                Assert.Null(fee.SizeType);
                Assert.Equal(fee.Type, (int)FeeType.NoFee);
            }
            else
            {
                Assert.Equal(fee.SizeType, (int)FeeSizeType.Percentage);
                Assert.Equal(fee.Type, (int)FeeType.ClientFee);
            }
        }

        private void AssertFees(ICollection<LimitOrderFee> fees, decimal makerSize, decimal takerSize)
        {
            Assert.NotEmpty(fees);
            Assert.Single(fees);
            var fee = fees.Single();
            Assert.Equal(fee.MakerSize, makerSize.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(fee.TakerSize, takerSize.ToString(CultureInfo.InvariantCulture));

            if (fee.MakerSize == 0.ToString() && fee.TakerSize == 0.ToString())
            {
                Assert.Null(fee.MakerSizeType);
                Assert.Null(fee.TakerSizeType);
                Assert.Equal(fee.Type, (int)FeeType.NoFee);
            }
            else
            {
                Assert.Equal(fee.MakerSizeType, (int)FeeSizeType.Percentage);
                Assert.Equal(fee.TakerSizeType, (int)FeeSizeType.Percentage);
                Assert.Equal(fee.Type, (int)FeeType.ClientFee);
            }
        }

        [Fact]
        public async Task MarketOrder_NoFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var feesClient = InitializeFeesClient(null, null);
            var accountsClient = InitializeAccountsClient();
            var logger = InitializeLogger<MarketOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new MarketOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new MarketOrderCreateModel(Guid, BtcUsd, Volume, Wallet);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(MarketOrder marketOrder)
            {
                AssertMarketOrder(marketOrder, Volume);

                AssertFees(marketOrder.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task MarketOrder_ExistedFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = new TradingFeeModel
            {
                BrokerId = BrokerId,
                Asset = Btc,
                AssetPair = BtcUsd,
                Levels = new List<TradingFeeLevelModel>
                {
                    new TradingFeeLevelModel { Id = Guid.NewGuid(), MakerFee = MakerFee*2, TakerFee = TakerFee*2, Volume = 1 },
                    new TradingFeeLevelModel { Id = Guid.NewGuid(), MakerFee = MakerFee, TakerFee = TakerFee, Volume = 0.5m }
                }
            };
            var feesClient = InitializeFeesClient(feeModel, null);
            var accountsClient = InitializeAccountsClient();
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<MarketOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new MarketOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new MarketOrderCreateModel(Guid, BtcUsd, Volume, Wallet);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(MarketOrder marketOrder)
            {
                AssertMarketOrder(marketOrder, Volume);

                AssertFees(marketOrder.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task MarketOrder_NoFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var settings = new SettingsModel
            {
                BrokerId = BrokerId,
                FeeWalletId = Wallet.ToString()
            };
            var feesClient = InitializeFeesClient(null, settings);
            var accountsClient = InitializeAccountsClient();
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<MarketOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new MarketOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new MarketOrderCreateModel(Guid, BtcUsd, Volume, Wallet);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(MarketOrder marketOrder)
            {
                AssertMarketOrder(marketOrder, Volume);

                AssertFees(marketOrder.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task MarketOrder_ExistedFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = new TradingFeeModel
            {
                BrokerId = BrokerId,
                Asset = Btc,
                AssetPair = BtcUsd,
                Levels = new List<TradingFeeLevelModel>
                {
                    new TradingFeeLevelModel { Id = Guid.NewGuid(), MakerFee = MakerFee*2, TakerFee = TakerFee*2, Volume = 1 },
                    new TradingFeeLevelModel { Id = Guid.NewGuid(), MakerFee = MakerFee, TakerFee = TakerFee, Volume = 0.5m }
                }
            };
            var settings = new SettingsModel
            {
                BrokerId = BrokerId,
                FeeWalletId = Wallet.ToString()
            };
            var feesClient = InitializeFeesClient(feeModel, settings);
            var accountsClient = InitializeAccountsClient();
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<MarketOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new MarketOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new MarketOrderCreateModel(Guid, BtcUsd, Volume, Wallet);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(MarketOrder marketOrder)
            {
                AssertMarketOrder(marketOrder, Volume);

                AssertFees(marketOrder.Fees, TakerFee / 100);
            }

            Assert.Equal(1, assertCalls.Count);
        }


        [Fact]
        public async Task LimitOrder_NoFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var feesClient = InitializeFeesClient(null, null);
            var accountsClient = InitializeAccountsClient();
            var logger = InitializeLogger<LimitOrderOperations>(AssertLoggerWarning, assertCalls);

            var limitOrderOperations = new LimitOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new LimitOrderCreateModel(Guid, BtcUsd, Price, Volume, Wallet, LimitOrderTypeModel, CancelPrevious);
            
            // act

            await limitOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(LimitOrder limitOrder)
            {
                AssertLimitOrder(limitOrder, Volume);

                AssertFees(limitOrder.Fees, 0, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task LimitOrder_ExistedFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetTradingFeeModel();
            var feesClient = InitializeFeesClient(feeModel, null);
            var accountsClient = InitializeAccountsClient();
            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var logger = InitializeLogger<LimitOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new LimitOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new LimitOrderCreateModel(Guid, BtcUsd, Price, Volume, Wallet, LimitOrderTypeModel, CancelPrevious);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(LimitOrder limitOrder)
            {
                AssertLimitOrder(limitOrder, Volume);

                AssertFees(limitOrder.Fees, 0, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task LimitOrder_NoFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(null, settings);
            var accountsClient = InitializeAccountsClient();
            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var logger = InitializeLogger<LimitOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new LimitOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new LimitOrderCreateModel(Guid, BtcUsd, Price, Volume, Wallet, LimitOrderTypeModel, CancelPrevious);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(LimitOrder limitOrder)
            {
                AssertLimitOrder(limitOrder, Volume);

                AssertFees(limitOrder.Fees, 0, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task LimitOrder_ExistedFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetTradingFeeModel();
            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(feeModel, settings);
            var accountsClient = InitializeAccountsClient();
            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var logger = InitializeLogger<LimitOrderOperations>(AssertLoggerWarning, assertCalls);

            var marketOrderOperations = new LimitOrderOperations(matchingEngineClient, feesClient, accountsClient, logger);

            var model = new LimitOrderCreateModel(Guid, BtcUsd, Price, Volume, Wallet, LimitOrderTypeModel, CancelPrevious);

            // act

            await marketOrderOperations.CreateAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(LimitOrder limitOrder)
            {
                AssertLimitOrder(limitOrder, Volume);

                AssertFees(limitOrder.Fees, MakerFee / 100, TakerFee / 100);
            }

            Assert.Equal(1, assertCalls.Count);
        }
    }
}
