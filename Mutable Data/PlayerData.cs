using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;

namespace NamPhuThuy.Data
{

    [Serializable]
    public class PlayerData
    {
        #region LEVEL
        public int currentLevelId;
        public int CurrentLevelId
        {
            get => currentLevelId;
            set
            {
                currentLevelId = value;
                currentLevelId = Math.Max(0, value);
                DataManager.Ins.MarkDirty();
            }
        }

        public bool isLoopLevel;
        public bool IsLoopLevel
        {
            get => isLoopLevel;
            set => isLoopLevel = value;
        }

        public int numLevelLoop;
        public int NumLevelLoop
        {
            get => numLevelLoop;
            set => numLevelLoop = value;
        }

        public int currentLevelLoop;
        public int CurrentLevelLoop
        {
            get => currentLevelLoop;
            set => currentLevelLoop = value;
        }

        public bool isCurrentLevelLoopRandomized;
        public bool IsCurrentLevelLoopRandomized
        {
            get => isCurrentLevelLoopRandomized;
            set => isCurrentLevelLoopRandomized = value;
        }
        #endregion

        #region Consent

        public bool isAllowPersonalizedAds = true;
        public bool IsAllowPersonalizedAds
        {
            get => isAllowPersonalizedAds;
            set => isAllowPersonalizedAds = value;
        }

        #endregion

        public int currentBackgroundId;
        public int CurrentBackgroundId
        {
            get => currentBackgroundId;
            set
            {
                currentBackgroundId = value;
                currentBackgroundId = Math.Max(0, value);
                DataManager.Ins.MarkDirty();
            }
        }

        public int coin;
        public int Coin
        {
            get => coin;
            set
            {
                coin = value;
                coin = Math.Max(0, value);

                DataManager.Ins.MarkDirty();
                // MMEventManager.TriggerEvent(new EResourceUpdated(ResourceType.COIN));
            }
        }

        public bool isRemoveAds;
        public bool IsRemoveAds => isRemoveAds;

        

        public List<PlayerBoosterData> boosters = new List<PlayerBoosterData>();

        public PlayerData(int currentLevelId = 0, int coin = 0)
        {
            this.CurrentLevelId = currentLevelId;
            this.Coin = coin;
            this.isRemoveAds = false;
            // EnsureIndex(); 
        }

        #region Tutorial Rewards Tracking
        
        // Persisted list (JSON-friendly)
        public List<int> grantedLevelRewardIds = new List<int>();
        // Fast lookup at runtime (not serialized)
        [NonSerialized] private HashSet<int> _grantedLevelRewardSet;

        private void EnsureGrantedSet()
        {
            if (_grantedLevelRewardSet != null) return;
            _grantedLevelRewardSet = new HashSet<int>(grantedLevelRewardIds ?? new List<int>());
        }

        /// <summary>Has this level's one-time rewards already been granted?</summary>
        public bool HasGrantedLevelRewards(int levelId)
        {
            EnsureGrantedSet();
            return _grantedLevelRewardSet.Contains(levelId);
        }

        /// <summary>Mark a level's one-time rewards as granted (idempotent).</summary>
        public void MarkLevelRewardsGranted(int levelId)
        {
            EnsureGrantedSet();
            if (_grantedLevelRewardSet.Add(levelId))
            {
                // keep serialized list in sync
                if (grantedLevelRewardIds == null) grantedLevelRewardIds = new List<int>();
                grantedLevelRewardIds.Add(levelId);

                DataManager.Ins.MarkDirty();
            }
        }

        #endregion

        #region Fortune Wheel

        public long lastFreeFortuneSpinTs;

        public PlayerData()
        {
            throw new NotImplementedException();
        }

        public long LastFreeFortuneSpinTs
        {
            get => lastFreeFortuneSpinTs;
            set
            {
                lastFreeFortuneSpinTs = value;
                DataManager.Ins.MarkDirty();
            }
        }
        
        public bool CanSpinFortuneFree()
        {
            return (RemainTimeToNextFree() <= 0);
        }
        
        public void MarkFortuneFreeSpun()
        {
            LastFreeFortuneSpinTs = (long)TimeHelper.ConvertToUnixTime(DateTime.UtcNow);
        }
        
        public double RemainTimeToNextFree()
        {
            double now = TimeHelper.ConvertToUnixTime(DateTime.UtcNow);
            double nextAllowed = lastFreeFortuneSpinTs + DataConst.SECONDS_PER_DAY;
            return (float)Math.Max(0, nextAllowed - now);
        }

        #endregion

        #region Player Resources Helpers

        #region Coin Helpers
        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            Coin = coin + amount;
        }

        public bool TrySpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (coin < amount) return false;
            Coin = coin - amount;
            return true;
        }

        public void ClearAllCoins()
        {
            Coin = 0;
        }
        #endregion
        
        /// <summary>
        /// Apply a list of rewards to the player. Returns true if anything was granted.
        /// </summary>
        public bool TryApplyRewards(IList<ResourceReward> rewards, int amountMultiplier = 1)
        {
            if (rewards == null || rewards.Count == 0) return false;

            bool anyGranted = false;

            for (int i = 0; i < rewards.Count; i++)
            {
                var item = rewards[i];
                if (item == null) continue; // if ResourceReward is a class

                int amount = Math.Max(0, item.amount * Math.Max(1, amountMultiplier));
                switch (item.resourceType)
                {
                    case ResourceType.COIN:
                        if (amount <= 0) break;
                            
                        AddCoins(amount);
                        anyGranted = true;
                            
                        break;

                    case ResourceType.BOOSTER:
                        if (amount <= 0) break;
                        
                        SetBoosterNum(item.boosterType, GetBoosterNum(item.boosterType) + amount);
                        anyGranted = true;
                        break;

                    case ResourceType.NO_ADS:

                        DataManager.Ins.PlayerData.ActiveNoAds();
                        break;
                }
            }

            if (anyGranted) DataManager.Ins.MarkDirty();
            if (anyGranted)
            {
                Debug.Log($"PlayerData.TryApplyRewards() - rewards are applied");
            }
            else
            {
                Debug.Log($"PlayerData.TryApplyRewards() - rewards haven't applied");
            }
            
            return anyGranted;
        }

        /// <summary>
        /// Apply a single reward item. Returns true if granted.
        /// </summary>
        public bool TryApplyReward(ResourceReward item, int amountMultiplier = 1)
        {
            if (item == null)
            {
                DebugLogger.Log($"PlayerData.TryApplyRewards() item is null");
                return false;
            }
            return TryApplyRewards(new List<ResourceReward> { item }, amountMultiplier);
        }

        #endregion

        #region Booster Helpers

        public int GetBoosterNum(BoosterType type)
        {
            /*EnsureIndex();
            return _boosterMap.TryGetValue(type, out var count) ? count : 0;*/

            var entry = boosters.Find(b => b.boosterType == type);
            return entry != null ? entry.amount : 0;
        }

        public void SetBoosterNum(BoosterType type, int count)
        {
            /*EnsureIndex();
            count = Math.Max(0, count);

            var old = GetBoosterNum(type);
            if (old == count) return;

            _boosterMap[type] = count;
            UpsertBoosterList(type, count);

            DataManager.Ins.MarkDirty();
            MMEventManager.TriggerEvent(new EBoosterDataUpdated(type));*/


            var entry = boosters.Find(b => b.boosterType == type);
            if (entry != null)
                entry.amount = count;
            else
                boosters.Add(new PlayerBoosterData { boosterType = type, amount = count });

            DataManager.Ins.MarkDirty();
        }

        public void ClearBoosters()
        {
            SetBoosterNum(BoosterType.UNDO, 0);
            SetBoosterNum(BoosterType.SHUFFLE, 0);
            SetBoosterNum(BoosterType.MAGIC_PICK, 0);
        }
        
        [Serializable]
        public class PlayerBoosterData
        {
            public BoosterType boosterType;
            public int amount;
        }

        #endregion

        #region NoAds Helpers

        public void ActiveNoAds()
        {
            isRemoveAds = true;
        }

        #endregion
    }
}