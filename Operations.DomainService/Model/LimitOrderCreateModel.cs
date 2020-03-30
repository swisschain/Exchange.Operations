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
        /// The asset pair identifier.
        /// </summary>
        public string AssetPairId { get; set; }

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
        public string WalletId { get; set; }

        /// <summary>
        /// The limit order type.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public LimitOrderType Type { get; set; }

        /// <summary>
        /// If <c>true</c> previously created limit orders will be closed.
        /// </summary>
        public bool CancelPrevious { get; set; }
    }
}
