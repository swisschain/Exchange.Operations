﻿using System;

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
        /// Message identifier
        /// </summary>
        public string MessageId { get; set; }

        /// <summary>
        /// The asset pair identifier.
        /// </summary>
        public string AssetPairId { get; set; }

        /// <summary>
        /// The market order volume.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// Reserved limit orders
        /// </summary>
        public decimal ReservedLimitVolume { get; set; }

        /// <summary>
        /// Buy or Sell
        /// </summary>
        public OrderType Type { get; set; }
    }
}
