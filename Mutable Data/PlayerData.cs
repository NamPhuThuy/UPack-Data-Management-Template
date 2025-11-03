using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;

namespace NamPhuThuy.Data
{

    [Serializable]
    public class PlayerData
    {
        [SerializeField] private int currentLevelId;
        public int CurrentLevelId
        {
            get => currentLevelId;
            set
            {
                currentLevelId = value;
                currentLevelId = Math.Max(0, value);
                DataManagerChecked.Ins.MarkDirty();
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

                DataManagerChecked.Ins.MarkDirty();
                // MMEventManager.TriggerEvent(new EResourceUpdated(ResourceType.COIN));
            }
        }

        public bool isRemoveAds;
        
        public List<PlayerBoosterData> boosters = new List<PlayerBoosterData>();

        public PlayerData(int currentLevelId = 0, int coin = 0)
        {
            this.CurrentLevelId = currentLevelId;
            this.Coin = coin;
            this.isRemoveAds = false;
        }
        
        // For reflection serialization
        /*public PlayerData()
        {
            this.CurrentLevelId = 0;
            this.Coin = 0;
            this.isRemoveAds = false;
        }*/

        #region Tutorial Rewards Tracking
        
        public List<int> grantedLevelRewardIds = new List<int>();
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

                DataManagerChecked.Ins.MarkDirty();
            }
        }

        #endregion

        #region Fortune Wheel

        public long lastFreeFortuneSpinTs;

        

        public long LastFreeFortuneSpinTs
        {
            get => lastFreeFortuneSpinTs;
            set
            {
                lastFreeFortuneSpinTs = value;
                DataManagerChecked.Ins.MarkDirty();
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
        
        #region Booster Helpers

        public int GetBoosterNum(BoosterType type)
        {
            var entry = boosters.Find(b => b.boosterType == type);
            return entry?.amount ?? 0;
        }
        
        public void AddBooster(BoosterType type, int amount)
        {
            if (amount <= 0) return;

            var entry = boosters.Find(b => b.boosterType == type);
            if (entry != null)
            {
                entry.amount = Math.Max(0, entry.amount + amount);
            }
            else
            {
                boosters.Add(new PlayerBoosterData { boosterType = type, amount = Math.Max(0, amount) });
            }

            DataManagerChecked.Ins.MarkDirty();
        }

        public void SetBoosterNum(BoosterType type, int count)
        {
            var entry = boosters.Find(b => b.boosterType == type);
            if (entry != null)
                entry.amount = count;
            else
                boosters.Add(new PlayerBoosterData { boosterType = type, amount = count });

            DataManagerChecked.Ins.MarkDirty();
        }

        public void ClearBoosters()
        {
            SetBoosterNum(BoosterType.TIMER, 0);
            SetBoosterNum(BoosterType.SHUFFLE, 0);
            SetBoosterNum(BoosterType.CLEAR_A_FOOD_TYPE, 0);
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
        /// <summary>
        /// Apply a list of rewards to the player. Returns true if anything was granted.
        /// </summary>
        public bool TryApplyRewards(IList<ResourceAmount> rewards, int amountMultiplier = 1)
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

                        DataManagerChecked.Ins.PlayerData.ActiveNoAds();
                        break;
                }
            }

            if (anyGranted) DataManagerChecked.Ins.MarkDirty();
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
        public bool TryApplyReward(ResourceAmount item, int amountMultiplier = 1)
        {
            if (item == null)
            {
                return false;
            }
            return TryApplyRewards(new List<ResourceAmount> { item }, amountMultiplier);
        }
        
       

        #endregion

        

       
    }
}