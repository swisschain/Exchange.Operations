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
        /// The wallet identifier.
        /// </summary>
        public string WalletId { get; set; }

        /// <summary>
        /// The asset pair symbol.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The market order volume.
        /// </summary>
        public decimal Volume { get; set; }

        public MarketOrderCreateModel()
        {
        }

        public MarketOrderCreateModel(Guid? id, string symbol, decimal volume, string walletId)
        {
            Id = id;
            Symbol = symbol;
            Volume = volume;
            WalletId = walletId;
        }
    }
}
