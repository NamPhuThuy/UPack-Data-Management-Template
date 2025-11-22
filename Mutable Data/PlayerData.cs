using System;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;

namespace NamPhuThuy.Data
{

    [Serializable]
    public partial class PlayerData
    {
        [SerializeField] private int currentLevelId;
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

        public int coin;
        public int Coin
        {
            get => coin;
            set
            {
                coin = value;
                coin = Math.Max(0, value);

                DataManager.Ins.MarkDirty();
            }
        }

        public int health;
        public int Health
        {
            get => health;
            set
            {
                health = value;
                health = Math.Max(0, value);

                DataManager.Ins.MarkDirty();
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
            SpendCoins(amount);
            return true;
        }
        
        public void SpendCoins(int amount)
        {
            Coin = coin - amount;
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

            DataManager.Ins.MarkDirty();
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.BOOSTER
            });
        }

        public void SetBoosterNum(BoosterType type, int count)
        {
            var entry = boosters.Find(b => b.boosterType == type);
            if (entry != null)
                entry.amount = count;
            else
                boosters.Add(new PlayerBoosterData { boosterType = type, amount = count });

            DataManager.Ins.MarkDirty();
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