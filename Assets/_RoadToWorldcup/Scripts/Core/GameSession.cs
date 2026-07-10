using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace RoadToWorldcup
{
    public enum WalletCurrency { Coins, Gems }
    public enum CosmeticCategory { Hair, Jersey, Accessory }

    public struct Reward
    {
        public int coins;
        public int gems;
        public string label;

        public Reward(int coinAmount, int gemAmount, string rewardLabel)
        {
            coins = coinAmount;
            gems = gemAmount;
            label = rewardLabel;
        }

        public bool IsEmpty { get { return coins <= 0 && gems <= 0; } }
        public string ToDisplayString()
        {
            if (coins > 0 && gems > 0) return "+" + coins + " GOLD  +" + gems + " GEMS";
            if (gems > 0) return "+" + gems + " GEMS";
            return "+" + coins + " GOLD";
        }
    }

    public sealed class CosmeticItem
    {
        public string id;
        public CosmeticCategory category;
        public string displayName;
        public string icon;
        public WalletCurrency currency;
        public int price;
        public Color color;

        public CosmeticItem(string itemId, CosmeticCategory itemCategory, string name, string itemIcon, WalletCurrency itemCurrency, int itemPrice, Color itemColor)
        {
            id = itemId;
            category = itemCategory;
            displayName = name;
            icon = itemIcon;
            currency = itemCurrency;
            price = itemPrice;
            color = itemColor;
        }
    }

    /// <summary>Persistent, offline-first player profile. Store/IAP and rewarded-ad providers can credit this wallet later.</summary>
    public static class GameSession
    {
        public const int LevelCount = 30;

        private const string Prefix = "RoadToWorldcup.";
        private const string CurrentLevelPrefsKey = Prefix + "CurrentLevelIndex";
        private const string CoinsPrefsKey = Prefix + "Coins";
        private const string GemsPrefsKey = Prefix + "Gems";
        private const string CoinsSpentPrefsKey = Prefix + "CoinsSpent";
        private const string GemsSpentPrefsKey = Prefix + "GemsSpent";
        private const string LanguagePrefsKey = Prefix + "Language";
        private const string MusicPrefsKey = Prefix + "MusicEnabled";
        private const string SfxPrefsKey = Prefix + "SfxEnabled";
        private const string LastDailyRewardKey = Prefix + "LastDailyReward";
        private const string DailyRewardStreakKey = Prefix + "DailyRewardStreak";
        private const string LastSpinKey = Prefix + "LastSpin";
        private const string DailyMissionDateKey = Prefix + "DailyMissionDate";
        private const string DailyWinsKey = Prefix + "DailyWins";
        private const string DailyCoinSpendKey = Prefix + "DailyCoinSpend";

        private static readonly Reward[] DailyRewards =
        {
            new Reward(250, 0, "Kick-off Gold"), new Reward(350, 0, "Training Gold"),
            new Reward(450, 3, "Player Boost"), new Reward(600, 0, "Matchday Gold"),
            new Reward(700, 5, "Star Pack"), new Reward(900, 0, "Champion Gold"),
            new Reward(1200, 12, "Weekly Trophy")
        };

        private static readonly Reward[] SpinRewards =
        {
            new Reward(150, 0, "150 Gold"), new Reward(300, 0, "300 Gold"),
            new Reward(500, 0, "500 Gold"), new Reward(0, 3, "3 Gems"),
            new Reward(0, 8, "8 Gems"), new Reward(900, 2, "Jackpot")
        };

        private static readonly List<CosmeticItem> CosmeticCatalog = new List<CosmeticItem>
        {
            new CosmeticItem("hair_street", CosmeticCategory.Hair, "Street Fade", "HAIR", WalletCurrency.Coins, 0, new Color(0.10f, 0.05f, 0.02f)),
            new CosmeticItem("hair_blaze", CosmeticCategory.Hair, "Blaze Hawk", "FIRE", WalletCurrency.Coins, 650, new Color(0.85f, 0.18f, 0.06f)),
            new CosmeticItem("hair_ice", CosmeticCategory.Hair, "Ice Twist", "ICE", WalletCurrency.Gems, 12, new Color(0.42f, 0.9f, 1f)),
            new CosmeticItem("jersey_ocean", CosmeticCategory.Jersey, "Ocean 10", "10", WalletCurrency.Coins, 0, new Color(0.03f, 0.30f, 0.92f)),
            new CosmeticItem("jersey_sun", CosmeticCategory.Jersey, "Sunset Striker", "10", WalletCurrency.Coins, 900, new Color(1f, 0.42f, 0.06f)),
            new CosmeticItem("jersey_royal", CosmeticCategory.Jersey, "Royal Pulse", "10", WalletCurrency.Gems, 18, new Color(0.50f, 0.16f, 0.92f)),
            new CosmeticItem("accessory_captain", CosmeticCategory.Accessory, "Captain Band", "C", WalletCurrency.Coins, 0, new Color(1f, 0.78f, 0.10f)),
            new CosmeticItem("accessory_shades", CosmeticCategory.Accessory, "Goal Shades", "COOL", WalletCurrency.Coins, 1100, new Color(0.06f, 0.08f, 0.12f)),
            new CosmeticItem("accessory_crown", CosmeticCategory.Accessory, "Golden Crown", "KING", WalletCurrency.Gems, 25, new Color(1f, 0.74f, 0.08f))
        };

        private static int selectedLevelIndex;
        private static bool progressLoaded;
        public static event Action StateChanged;

        public static int SelectedLevelIndex { get { EnsureProgressLoaded(); return Mathf.Clamp(selectedLevelIndex, 0, LevelCount - 1); } }
        public static int SelectedLevelNumber { get { return SelectedLevelIndex + 1; } }
        public static int Coins { get { EnsureProgressLoaded(); return PlayerPrefs.GetInt(CoinsPrefsKey, 1500); } }
        public static int Gems { get { EnsureProgressLoaded(); return PlayerPrefs.GetInt(GemsPrefsKey, 20); } }
        public static int TotalCoinsSpent { get { return PlayerPrefs.GetInt(CoinsSpentPrefsKey, 0); } }
        public static int TotalGemsSpent { get { return PlayerPrefs.GetInt(GemsSpentPrefsKey, 0); } }
        public static int DailyCoinsSpent { get { EnsureDailyMissionsCurrent(); return PlayerPrefs.GetInt(DailyCoinSpendKey, 0); } }
        public static string Language { get { return PlayerPrefs.GetString(LanguagePrefsKey, "Tiếng Việt"); } }
        public static bool MusicEnabled { get { return PlayerPrefs.GetInt(MusicPrefsKey, 1) == 1; } }
        public static bool SfxEnabled { get { return PlayerPrefs.GetInt(SfxPrefsKey, 1) == 1; } }
        public static int DailyRewardDay { get { return Mathf.Clamp(PlayerPrefs.GetInt(DailyRewardStreakKey, 0), 0, DailyRewards.Length - 1); } }
        public static bool CanClaimDailyReward { get { return !IsToday(PlayerPrefs.GetString(LastDailyRewardKey, string.Empty)); } }
        public static bool CanSpinToday { get { return !IsToday(PlayerPrefs.GetString(LastSpinKey, string.Empty)); } }

        public static void SelectLevel(int levelIndex)
        {
            EnsureProgressLoaded();
            selectedLevelIndex = Mathf.Clamp(levelIndex, 0, LevelCount - 1);
            SaveProgress();
        }

        public static void SelectFirstLevel() { SelectLevel(0); }
        public static void LoadProgress() { progressLoaded = false; EnsureProgressLoaded(); EnsureDailyMissionsCurrent(); }

        public static bool SelectNextLevel()
        {
            EnsureProgressLoaded();
            if (selectedLevelIndex >= LevelCount - 1) { selectedLevelIndex = LevelCount - 1; SaveProgress(); return false; }
            selectedLevelIndex++;
            SaveProgress();
            return true;
        }

        public static int GetLevelReward(int levelNumber) { return 100 + Mathf.Max(0, levelNumber - 1) * 25; }

        /// <summary>Rewards a level only on its first clear to prevent replay farming.</summary>
        public static int CompleteLevel(int levelNumber)
        {
            string key = Prefix + "LevelRewardClaimed." + Mathf.Clamp(levelNumber, 1, LevelCount);
            EnsureDailyMissionsCurrent();
            PlayerPrefs.SetInt(DailyWinsKey, PlayerPrefs.GetInt(DailyWinsKey, 0) + 1);
            if (PlayerPrefs.GetInt(key, 0) == 1)
            {
                SaveAndNotify();
                return 0;
            }
            int reward = GetLevelReward(levelNumber);
            PlayerPrefs.SetInt(key, 1);
            AddCurrency(WalletCurrency.Coins, reward);
            SaveAndNotify();
            return reward;
        }

        public static Reward GetDailyRewardPreview() { return DailyRewards[DailyRewardDay]; }
        public static Reward ClaimDailyReward()
        {
            if (!CanClaimDailyReward) return new Reward(0, 0, "Come back tomorrow");
            DateTime last;
            string lastValue = PlayerPrefs.GetString(LastDailyRewardKey, string.Empty);
            int day = DailyRewardDay;
            if (TryReadDate(lastValue, out last) && (Today() - last.Date).TotalDays > 1) day = 0;
            Reward reward = DailyRewards[day];
            AddCurrency(WalletCurrency.Coins, reward.coins);
            AddCurrency(WalletCurrency.Gems, reward.gems);
            PlayerPrefs.SetString(LastDailyRewardKey, TodayString());
            PlayerPrefs.SetInt(DailyRewardStreakKey, (day + 1) % DailyRewards.Length);
            SaveAndNotify();
            return reward;
        }

        public static Reward Spin()
        {
            if (!CanSpinToday) return new Reward(0, 0, "Free spin used");
            int index = Mathf.Abs((TodayString() + SystemInfo.deviceUniqueIdentifier).GetHashCode()) % SpinRewards.Length;
            Reward reward = SpinRewards[index];
            AddCurrency(WalletCurrency.Coins, reward.coins);
            AddCurrency(WalletCurrency.Gems, reward.gems);
            PlayerPrefs.SetString(LastSpinKey, TodayString());
            SaveAndNotify();
            return reward;
        }

        public static int GetMissionProgress(int missionIndex)
        {
            EnsureDailyMissionsCurrent();
            if (missionIndex == 0) return Mathf.Min(1, PlayerPrefs.GetInt(DailyWinsKey, 0));
            if (missionIndex == 1) return Mathf.Min(3, PlayerPrefs.GetInt(DailyWinsKey, 0));
            return Mathf.Min(500, PlayerPrefs.GetInt(DailyCoinSpendKey, 0));
        }

        public static int GetMissionTarget(int missionIndex) { return missionIndex == 0 ? 1 : missionIndex == 1 ? 3 : 500; }
        public static int GetMissionReward(int missionIndex) { return missionIndex == 0 ? 150 : missionIndex == 1 ? 350 : 200; }
        public static bool IsMissionClaimed(int missionIndex) { EnsureDailyMissionsCurrent(); return PlayerPrefs.GetInt(Prefix + "MissionClaimed." + missionIndex, 0) == 1; }
        public static bool CanClaimMission(int missionIndex) { return !IsMissionClaimed(missionIndex) && GetMissionProgress(missionIndex) >= GetMissionTarget(missionIndex); }
        public static bool ClaimMission(int missionIndex)
        {
            if (!CanClaimMission(missionIndex)) return false;
            PlayerPrefs.SetInt(Prefix + "MissionClaimed." + missionIndex, 1);
            AddCurrency(WalletCurrency.Coins, GetMissionReward(missionIndex));
            SaveAndNotify();
            return true;
        }

        public static IList<CosmeticItem> GetCosmetics(CosmeticCategory category)
        {
            List<CosmeticItem> result = new List<CosmeticItem>();
            for (int i = 0; i < CosmeticCatalog.Count; i++) if (CosmeticCatalog[i].category == category) result.Add(CosmeticCatalog[i]);
            return result;
        }
        public static bool IsOwned(CosmeticItem item) { return item != null && (item.price == 0 || PlayerPrefs.GetInt(Prefix + "CosmeticOwned." + item.id, 0) == 1); }
        public static bool IsEquipped(CosmeticItem item) { return item != null && PlayerPrefs.GetString(Prefix + "Equipped." + item.category, DefaultItemId(item.category)) == item.id; }
        public static CosmeticItem GetEquipped(CosmeticCategory category) { return GetCosmetic(PlayerPrefs.GetString(Prefix + "Equipped." + category, DefaultItemId(category))); }

        public static bool PurchaseOrEquip(CosmeticItem item, out string result)
        {
            result = string.Empty;
            if (item == null) { result = "Item unavailable"; return false; }
            if (!IsOwned(item))
            {
                if (!TrySpend(item.currency, item.price)) { result = item.currency == WalletCurrency.Coins ? "Not enough gold" : "Not enough gems"; return false; }
                PlayerPrefs.SetInt(Prefix + "CosmeticOwned." + item.id, 1);
                result = "Unlocked " + item.displayName;
            }
            else result = "Equipped " + item.displayName;
            PlayerPrefs.SetString(Prefix + "Equipped." + item.category, item.id);
            SaveAndNotify();
            return true;
        }

        public static void SetLanguage(string value) { PlayerPrefs.SetString(LanguagePrefsKey, value); SaveAndNotify(); }
        public static void SetMusicEnabled(bool value) { PlayerPrefs.SetInt(MusicPrefsKey, value ? 1 : 0); SaveAndNotify(); }
        public static void SetSfxEnabled(bool value) { PlayerPrefs.SetInt(SfxPrefsKey, value ? 1 : 0); SaveAndNotify(); }

        private static bool TrySpend(WalletCurrency currency, int amount)
        {
            if (amount <= 0) return true;
            if (currency == WalletCurrency.Coins)
            {
                if (Coins < amount) return false;
                PlayerPrefs.SetInt(CoinsPrefsKey, Coins - amount);
                PlayerPrefs.SetInt(CoinsSpentPrefsKey, TotalCoinsSpent + amount);
                EnsureDailyMissionsCurrent();
                PlayerPrefs.SetInt(DailyCoinSpendKey, DailyCoinsSpent + amount);
            }
            else
            {
                if (Gems < amount) return false;
                PlayerPrefs.SetInt(GemsPrefsKey, Gems - amount);
                PlayerPrefs.SetInt(GemsSpentPrefsKey, TotalGemsSpent + amount);
            }
            return true;
        }

        private static void AddCurrency(WalletCurrency currency, int amount)
        {
            if (amount <= 0) return;
            string key = currency == WalletCurrency.Coins ? CoinsPrefsKey : GemsPrefsKey;
            PlayerPrefs.SetInt(key, PlayerPrefs.GetInt(key, currency == WalletCurrency.Coins ? 1500 : 20) + amount);
        }

        private static void EnsureProgressLoaded()
        {
            if (progressLoaded) return;
            selectedLevelIndex = Mathf.Clamp(PlayerPrefs.GetInt(CurrentLevelPrefsKey, 0), 0, LevelCount - 1);
            progressLoaded = true;
        }
        private static void EnsureDailyMissionsCurrent()
        {
            if (PlayerPrefs.GetString(DailyMissionDateKey, string.Empty) == TodayString()) return;
            PlayerPrefs.SetString(DailyMissionDateKey, TodayString());
            PlayerPrefs.SetInt(DailyWinsKey, 0);
            PlayerPrefs.SetInt(DailyCoinSpendKey, 0);
            for (int i = 0; i < 3; i++) PlayerPrefs.DeleteKey(Prefix + "MissionClaimed." + i);
            PlayerPrefs.Save();
        }
        private static CosmeticItem GetCosmetic(string id) { for (int i = 0; i < CosmeticCatalog.Count; i++) if (CosmeticCatalog[i].id == id) return CosmeticCatalog[i]; return null; }
        private static string DefaultItemId(CosmeticCategory category) { return category == CosmeticCategory.Hair ? "hair_street" : category == CosmeticCategory.Jersey ? "jersey_ocean" : "accessory_captain"; }
        private static DateTime Today() { return DateTime.Today; }
        private static string TodayString() { return Today().ToString("yyyyMMdd", CultureInfo.InvariantCulture); }
        private static bool IsToday(string value) { return value == TodayString(); }
        private static bool TryReadDate(string value, out DateTime date) { return DateTime.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date); }
        private static void SaveProgress() { PlayerPrefs.SetInt(CurrentLevelPrefsKey, selectedLevelIndex); PlayerPrefs.Save(); }
        private static void SaveAndNotify() { PlayerPrefs.Save(); if (StateChanged != null) StateChanged(); }
    }
}
