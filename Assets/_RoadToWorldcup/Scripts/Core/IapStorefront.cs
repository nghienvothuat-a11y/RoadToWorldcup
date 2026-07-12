using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Purchasing;

namespace RoadToWorldcup
{
    public sealed class IapPack
    {
        public readonly string productId;
        public readonly string displayName;
        public readonly int coins;
        public readonly int gems;
        public readonly string fallbackPrice;

        public IapPack(string id, string name, int coinAmount, int gemAmount, string price)
        {
            productId = id;
            displayName = name;
            coins = coinAmount;
            gems = gemAmount;
            fallbackPrice = price;
        }

        public string RewardText
        {
            get
            {
                return gems > 0 ? coins.ToString("N0") + " GOLD  + " + gems + " GEMS" : coins.ToString("N0") + " GOLD";
            }
        }
    }

    /// <summary>Apple App Store consumables. Product IDs must exactly match App Store Connect.</summary>
    public sealed class IapStorefront : MonoBehaviour
    {
        public const string StarterKickId = "com.phamduckien.roadtowc.gold.starter";
        public const string StrikerChestId = "com.phamduckien.roadtowc.gold.striker";
        public const string LegendVaultId = "com.phamduckien.roadtowc.gold.legend";

        private const string FulfilledTransactionPrefix = "RoadToWorldcup.IapFulfilled.";
        private static readonly IapPack[] packs =
        {
            new IapPack(StarterKickId, "STARTER KICK", 1200, 0, "$0.99"),
            new IapPack(StrikerChestId, "STRIKER CHEST", 4000, 30, "$2.99"),
            new IapPack(LegendVaultId, "LEGEND VAULT", 12000, 120, "$7.99")
        };

        private static IapStorefront instance;
        private readonly Dictionary<string, string> localizedPrices = new Dictionary<string, string>();
        private StoreController storeController;
        private bool isReady;
        private bool isPurchasing;
        private string status = "CONNECTING TO STORE";

        public static event Action StateChanged;
        public static IList<IapPack> Packs { get { return packs; } }
        public static bool IsReady { get { return instance != null && instance.isReady; } }
        public static bool IsPurchasing { get { return instance != null && instance.isPurchasing; } }
        public static string Status { get { return instance != null ? instance.status : "CONNECTING TO STORE"; } }

        public static void EnsureInitialized()
        {
            if (instance != null)
            {
                return;
            }

            GameObject root = new GameObject("IapStorefront");
            instance = root.AddComponent<IapStorefront>();
            DontDestroyOnLoad(root);
        }

        public static string GetLocalizedPrice(IapPack pack)
        {
            if (pack == null)
            {
                return string.Empty;
            }

#if UNITY_EDITOR
            // The Unity Editor fake store reports a placeholder $0.01 for every product.
            return pack.fallbackPrice;
#else
            string price;
            return instance != null && instance.localizedPrices.TryGetValue(pack.productId, out price) ? price : pack.fallbackPrice;
#endif
        }

        public static void Purchase(IapPack pack)
        {
            EnsureInitialized();
            if (pack == null || instance == null || instance.isPurchasing)
            {
                return;
            }

            if (!instance.isReady || instance.storeController == null)
            {
                instance.SetStatus("STORE NOT READY");
                return;
            }

            Product product = instance.storeController.GetProductById(pack.productId);
            if (product == null || !product.availableToPurchase)
            {
                instance.SetStatus("PACK UNAVAILABLE");
                return;
            }

            instance.isPurchasing = true;
            instance.SetStatus("OPENING APP STORE");
            instance.storeController.PurchaseProduct(product);
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            InitializeIap();
        }

        private async void InitializeIap()
        {
            storeController = UnityIAPServices.StoreController();
            storeController.OnPurchasePending += OnPurchasePending;
            storeController.OnPurchaseConfirmed += OnPurchaseConfirmed;
            storeController.OnPurchaseFailed += OnPurchaseFailed;
            storeController.OnStoreDisconnected += OnStoreDisconnected;
            storeController.OnProductsFetched += OnProductsFetched;
            storeController.OnProductsFetchFailed += OnProductsFetchFailed;

            try
            {
                await storeController.Connect();
                List<ProductDefinition> products = new List<ProductDefinition>();
                for (int i = 0; i < packs.Length; i++)
                {
                    products.Add(new ProductDefinition(packs[i].productId, ProductType.Consumable));
                }
                storeController.FetchProducts(products);
            }
            catch (Exception exception)
            {
                SetStatus("STORE CONNECTION FAILED");
                Debug.LogWarning("The King: Road to Champion IAP initialization failed: " + exception.Message);
            }
        }

        private void OnProductsFetched(List<Product> products)
        {
            localizedPrices.Clear();
            for (int i = 0; i < products.Count; i++)
            {
                Product product = products[i];
                if (product != null && product.metadata != null && !string.IsNullOrEmpty(product.metadata.localizedPriceString))
                {
                    localizedPrices[product.definition.id] = product.metadata.localizedPriceString;
                }
            }

            isReady = true;
            SetStatus("STORE READY");
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            SetStatus("PACKS UNAVAILABLE");
                Debug.LogWarning("The King: Road to Champion IAP products failed to load: " + failure);
        }

        private void OnPurchasePending(PendingOrder order)
        {
            IapPack pack = GetPack(GetProductId(order));
            if (pack == null)
            {
                isPurchasing = false;
                SetStatus("UNKNOWN PACK");
                return;
            }

            string transactionId = order.Info.TransactionID;
            string transactionKey = FulfilledTransactionPrefix + transactionId;
            if (string.IsNullOrEmpty(transactionId) || PlayerPrefs.GetInt(transactionKey, 0) == 0)
            {
                GameSession.GrantIapReward(pack.coins, pack.gems);
                if (!string.IsNullOrEmpty(transactionId))
                {
                    PlayerPrefs.SetInt(transactionKey, 1);
                    PlayerPrefs.Save();
                }
            }

            storeController.ConfirmPurchase(order);
        }

        private void OnPurchaseConfirmed(Order order)
        {
            isPurchasing = false;
            if (order is FailedOrder)
            {
                SetStatus("PURCHASE NOT CONFIRMED");
                return;
            }

            SetStatus("PACK ADDED TO WALLET");
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            isPurchasing = false;
            SetStatus("PURCHASE CANCELLED");
                Debug.LogWarning("The King: Road to Champion IAP purchase failed: " + order.FailureReason + " " + order.Details);
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            isReady = false;
            isPurchasing = false;
            SetStatus("STORE OFFLINE");
                Debug.LogWarning("The King: Road to Champion IAP store disconnected: " + description);
        }

        private static string GetProductId(Order order)
        {
            if (order == null || order.CartOrdered == null || order.CartOrdered.Items() == null)
            {
                return string.Empty;
            }

            CartItem item = order.CartOrdered.Items().FirstOrDefault();
            return item != null && item.Product != null ? item.Product.definition.id : string.Empty;
        }

        private static IapPack GetPack(string productId)
        {
            for (int i = 0; i < packs.Length; i++)
            {
                if (packs[i].productId == productId)
                {
                    return packs[i];
                }
            }

            return null;
        }

        private void SetStatus(string value)
        {
            status = value;
            if (StateChanged != null)
            {
                StateChanged();
            }
        }
    }
}
