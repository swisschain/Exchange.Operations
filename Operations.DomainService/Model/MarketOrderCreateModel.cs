using System;

namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents market order creation information.
    /// </summary>
    public class MarketOrderCreateModel
    {
        /// <summary>
        /// (Optional) Unique ID of market order.
        /// In case if MarketOrderId is null then ID will auto-generated.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// Client's account.
        /// </summary>
        public ulong AccountId { get; set; }

        /// <summary>
        /// The wallet identifier.
        /// </summary>
        public ulong WalletId { get; set; }

        /// <summary>
        /// The asset pair symbol.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// The market order volume.
        /// </summary>
        public decimal Volume { get; set; }

        public MarketOrderCreateModel()
        {
        }

        public MarketOrderCreateModel(Guid? id, string assetPair, decimal volume, ulong accountId, ulong walletId)
        {
            Id = id;
            AssetPair = assetPair;
            Volume = volume;
            AccountId = accountId;
            WalletId = walletId;
        }
    }
}
