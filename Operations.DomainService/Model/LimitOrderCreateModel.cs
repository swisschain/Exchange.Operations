using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Operations.DomainService.Model
{
    /// <summary>
    /// Represents limit order creation information.
    /// </summary>
    public class LimitOrderCreateModel
    {
        /// <summary>
        /// (Optional) Unique ID of limit order.
        /// In case if LimitOrderId is null then ID will auto-generated.
        /// </summary>
        public Guid? Id { get; set; }

        /// <summary>
        /// The asset pair symbol.
        /// </summary>
        public string AssetPair { get; set; }

        /// <summary>
        /// The limit order price.
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// The limit order volume.
        /// </summary>
        public decimal Volume { get; set; }

        /// <summary>
        /// The wallet identifier.
        /// </summary>
        public long WalletId { get; set; }

        /// <summary>
        /// The limit order type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LimitOrderType Type { get; set; }

        /// <summary>
        /// If <c>true</c> and LimitOrderID is set then previously created limit orders will be closed.
        /// </summary>
        public bool CancelPrevious { get; set; }

        public LimitOrderCreateModel()
        {
        }

        public LimitOrderCreateModel(Guid? id, string assetPair, decimal price, decimal volume,
            long walletId, LimitOrderType type, bool cancelPrevious)
        {
            Id = id;
            AssetPair = assetPair;
            Price = price;
            Volume = volume;
            WalletId = walletId;
            Type = type;
            CancelPrevious = cancelPrevious;
        }
    }
}
