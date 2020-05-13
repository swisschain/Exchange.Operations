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
using Swisschain.Exchange.Fees.Client;
using Swisschain.Exchange.Fees.Client.Api;
using Swisschain.Exchange.Fees.Client.Models.CashOperationsFees;
using Swisschain.Exchange.Fees.Client.Models.Settings;
using Xunit;
using Fee = MatchingEngine.Client.Contracts.Incoming.Fee;
using FeeType = MatchingEngine.Client.Contracts.Incoming.FeeType;

namespace OperationsTests
{
    public class CashOperationsFeesTests : BaseTests
    {
        private const string BrokerId = "BrokerId";
        private const string Wallet = "Wallet";
        private const string FromWallet = "FromWallet";
        private const string ToWallet = "ToWallet";
        private const string Btc = "BTC";
        private const decimal CashInVolume = 1;
        private const decimal CashOutNegativeVolume = -1;
        private const decimal CashOutPositiveVolume = -1;
        private const decimal CashTransferVolume = 2;
        private const string Description = "Description";

        private IMatchingEngineClient InitializeMatchingEngineClient(
            Action<CashInOutOperation> cashInOutAssertMethod,
            Action<CashTransferOperation> cashTransferAssertMethod,
            AssertCalls assertCalls)
        {
            var matchingEngineClientMock = new Mock<IMatchingEngineClient>();
            var cashOperationsApiMock = new Mock<ICashOperationsApi>();
            if (cashInOutAssertMethod != null)
                cashOperationsApiMock.Setup(x => x.CashInOutAsync(It.IsAny<CashInOutOperation>(), default))
                    .Returns((CashInOutOperation cashInOutOperation, CancellationToken ct) =>
                    {
                        cashInOutAssertMethod(cashInOutOperation);

                        assertCalls.Count++;

                        return Task.FromResult(new Response());
                    });
            if (cashTransferAssertMethod != null)
                cashOperationsApiMock.Setup(x => x.CashTransferAsync(It.IsAny<CashTransferOperation>(), default))
                    .Returns((CashTransferOperation cashTransferOperation, CancellationToken ct) =>
                    {
                        cashTransferAssertMethod(cashTransferOperation);

                        assertCalls.Count++;

                        return Task.FromResult(new Response());
                    });
            matchingEngineClientMock.SetupGet(x => x.CashOperations).Returns(() => cashOperationsApiMock.Object);

            return matchingEngineClientMock.Object;
        }

        private IFeesClient InitializeFeesClient(CashOperationsFeeModel feeModel, SettingsModel settings)
        {
            var feesClientMock = new Mock<IFeesClient>();

            var cashOperationsFeesApiMock = new Mock<ICashOperationsFeesApi>();
            cashOperationsFeesApiMock.Setup(x => x.GetByBrokerIdAndAsset(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string brokerId, string asset) =>
                {
                    return Task.FromResult(feeModel);
                });
            feesClientMock.SetupGet(x => x.CashOperationsFees).Returns(() => cashOperationsFeesApiMock.Object);

            var settingsApiMock = new Mock<ISettingsApi>();
            settingsApiMock.Setup(x => x.GetByBrokerId(It.IsAny<string>()))
                .Returns((string brokerId) =>
                {
                    return Task.FromResult(settings);
                });
            feesClientMock.SetupGet(x => x.Settings).Returns(() => settingsApiMock.Object);

            return feesClientMock.Object;
        }

        private CashOperationsFeeModel GetCashOperationsFeeModel(decimal volume, CashType cashType)
        {
            var result = new CashOperationsFeeModel
            {
                BrokerId = BrokerId,
                Asset = Btc
            };

            switch (cashType)
            {
                case CashType.CashIn:
                    result.CashInValue = volume;
                    result.CashInFeeType = CashOperationsFeeTypeModel.Percentage;
                    break;
                case CashType.CashOut:
                    result.CashOutValue = volume;
                    result.CashOutFeeType = CashOperationsFeeTypeModel.Percentage;
                    break;
                case CashType.Transfer:
                    result.CashTransferValue = volume;
                    result.CashTransferFeeType = CashOperationsFeeTypeModel.Percentage;
                    break;
            }

            return result;
        }

        private SettingsModel GetSettingsModel()
        {
            var result = new SettingsModel
            {
                BrokerId = BrokerId,
                FeeWalletId = Wallet
            };

            return result;
        }

        private void AssertCashInOutOperation(CashInOutOperation cashInOutOperation, decimal volume)
        {
            Assert.NotNull(cashInOutOperation);
            Assert.NotEmpty(cashInOutOperation.Id);
            Assert.Equal(cashInOutOperation.BrokerId, BrokerId);
            Assert.Equal(cashInOutOperation.WalletId, Wallet);
            Assert.Equal(cashInOutOperation.AssetId, Btc);
            Assert.Equal(cashInOutOperation.Volume, volume.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(cashInOutOperation.Description, Description);
        }

        private void AssertCashTransferOperation(CashTransferOperation cashTransferOperation, decimal volume)
        {
            Assert.NotNull(cashTransferOperation);
            Assert.NotEmpty(cashTransferOperation.Id);
            Assert.Equal(cashTransferOperation.BrokerId, BrokerId);
            Assert.Equal(cashTransferOperation.FromWalletId, FromWallet);
            Assert.Equal(cashTransferOperation.ToWalletId, ToWallet);
            Assert.Equal(cashTransferOperation.AssetId, Btc);
            Assert.Equal(cashTransferOperation.Volume, volume.ToString(CultureInfo.InvariantCulture));
            Assert.Equal(cashTransferOperation.Description, Description);
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

        private enum CashType
        {
            CashIn = 0,
            CashOut = 1,
            Transfer = 2
        }

        [Fact]
        public async Task CashIn_NoFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var feesClient = InitializeFeesClient(null, null);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashInVolume, Wallet, Description);

            // act

            await cashOperations.CashInAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashInVolume);

                AssertFees(cashInOutOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashIn_ExistedFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashInVolume, CashType.CashIn);
            var feesClient = InitializeFeesClient(feeModel, null);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashInVolume, Wallet, Description);

            // act

            await cashOperations.CashInAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashInVolume);

                AssertFees(cashInOutOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashIn_NoFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(null, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashInVolume, Wallet, Description);

            // act

            await cashOperations.CashInAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashInVolume);

                AssertFees(cashInOutOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashIn_ExistedFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashInVolume, CashType.CashIn);
            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(feeModel, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashInVolume, Wallet, Description);

            // act

            await cashOperations.CashInAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashInVolume);

                AssertFees(cashInOutOperation.Fees, CashInVolume / 100);
            }

            Assert.Equal(1, assertCalls.Count);
        }


        [Fact]
        public async Task CashOut_NoFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var feesClient = InitializeFeesClient(null, null);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashOutNegativeVolume, Wallet, Description);

            // act

            await cashOperations.CashOutAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashOutNegativeVolume);

                AssertFees(cashInOutOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashOut_ExistedFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashOutNegativeVolume, CashType.CashOut);
            var feesClient = InitializeFeesClient(feeModel, null);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashOutNegativeVolume, Wallet, Description);

            // act

            await cashOperations.CashOutAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashOutNegativeVolume);

                AssertFees(cashInOutOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashOut_NoFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(null, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashOutNegativeVolume, Wallet, Description);

            // act

            await cashOperations.CashOutAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashOutNegativeVolume);

                AssertFees(cashInOutOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashOut_ExistedFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashOutNegativeVolume, CashType.CashOut);
            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(feeModel, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashInOutModel(Btc, CashOutNegativeVolume, Wallet, Description);

            // act

            await cashOperations.CashOutAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashOutNegativeVolume);

                AssertFees(cashInOutOperation.Fees, CashOutNegativeVolume / 100);
            }

            Assert.Equal(1, assertCalls.Count);
        }

        [Fact]
        public async Task CashOut_ExistedFee_ExistedSettings_PositiveVolumeToNegative_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashOutNegativeVolume, CashType.CashOut);
            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(feeModel, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(AssertMatchingEngineClientInput, null, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            // Volume is positive here and will 'converted' to negative
            var model = new CashInOutModel(Btc, CashOutPositiveVolume, Wallet, Description);

            // act

            await cashOperations.CashOutAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashInOutOperation cashInOutOperation)
            {
                AssertCashInOutOperation(cashInOutOperation, CashOutNegativeVolume);

                AssertFees(cashInOutOperation.Fees, CashOutNegativeVolume / 100);
            }

            Assert.Equal(1, assertCalls.Count);
        }


        [Fact]
        public async Task CashTransfer_NoFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var feesClient = InitializeFeesClient(null, null);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashTransferModel(Btc, CashTransferVolume, FromWallet, ToWallet, Description);

            // act

            await cashOperations.CashTransferAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashTransferOperation cashTransferOperation)
            {
                AssertCashTransferOperation(cashTransferOperation, CashTransferVolume);

                AssertFees(cashTransferOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashTransfer_ExistedFee_NoSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashTransferVolume, CashType.Transfer);
            var feesClient = InitializeFeesClient(feeModel, null);
            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashTransferModel(Btc, CashTransferVolume, FromWallet, ToWallet, Description);

            // act

            await cashOperations.CashTransferAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashTransferOperation cashTransferOperation)
            {
                AssertCashTransferOperation(cashTransferOperation, CashTransferVolume);

                AssertFees(cashTransferOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashTransfer_NoFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var settings = GetSettingsModel();
            var feesClient = InitializeFeesClient(null, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashTransferModel(Btc, CashTransferVolume, FromWallet, ToWallet, Description);

            // act

            await cashOperations.CashTransferAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashTransferOperation cashTransferOperation)
            {
                AssertCashTransferOperation(cashTransferOperation, CashTransferVolume);

                AssertFees(cashTransferOperation.Fees, 0);
            }

            Assert.Equal(2, assertCalls.Count);
        }

        [Fact]
        public async Task CashTransfer_ExistedFee_ExistedSettings_Test()
        {
            // arrange

            var assertCalls = new AssertCalls();

            var feeModel = GetCashOperationsFeeModel(CashTransferVolume, CashType.Transfer);
            var settings = new SettingsModel
            {
                BrokerId = BrokerId,
                FeeWalletId = Wallet
            };
            var feesClient = InitializeFeesClient(feeModel, settings);
            var matchingEngineClient = InitializeMatchingEngineClient(null, AssertMatchingEngineClientInput, assertCalls);
            var logger = InitializeLogger<CashOperations>(AssertLoggerWarning, assertCalls);

            var cashOperations = new CashOperations(matchingEngineClient, feesClient, logger);

            var model = new CashTransferModel(Btc, CashTransferVolume, FromWallet, ToWallet, Description);

            // act

            await cashOperations.CashTransferAsync(BrokerId, model);

            // assert

            void AssertMatchingEngineClientInput(CashTransferOperation cashTransferOperation)
            {
                AssertCashTransferOperation(cashTransferOperation, CashTransferVolume);

                AssertFees(cashTransferOperation.Fees, CashTransferVolume / 100);
            }

            Assert.Equal(1, assertCalls.Count);
        }

        
    }
}
