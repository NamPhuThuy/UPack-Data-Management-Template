using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using NamPhuThuy.Common;
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    [Serializable]
    public class BoosterRecord
    {
        public BoosterType boosterType;
        public int unlockLevel;
        public int stackPerBuy;
        public List<ResourceAmount> price;
        
        public string boosterName;
        public Sprite gameplayImage;
        public Sprite inventoryImage;
    }
    
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Game/BoosterData")]
    public class BoosterData : ScriptableObject
    {
        [FormerlySerializedAs("allBoosters")] public BoosterRecord[] data;
        
        // internal cache for faster lookups
        private Dictionary<BoosterType, BoosterRecord> _dataDict;

        #region Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (data == null || data.Length == 0) return;

            var typesSeen = new HashSet<BoosterType>();
            var duplicates = new List<BoosterType>();

            foreach (var booster in data)
            {
                if (booster == null) continue;
        
                if (booster.boosterType == BoosterType.NONE) continue;

                if (!typesSeen.Add(booster.boosterType))
                {
                    if (!duplicates.Contains(booster.boosterType))
                    {
                        duplicates.Add(booster.boosterType);
                    }
                }
            }

            if (duplicates.Count > 0)
            {
                DebugLogger.LogError(message:$"[BoosterData] Duplicate BoosterTypes found: {string.Join(", ", duplicates)}", context: this);
            }

            // Clear the cached dictionary to force rebuild on next lookup
            _dataDict = null;
#endif
        }

        #endregion
        
        private void BuildIndex()
        {
            if (_dataDict != null) return;

            _dataDict = new Dictionary<BoosterType, BoosterRecord>();
            if (data == null) return;

            foreach (var booster in data)
            {
                if (booster == null) continue;
                _dataDict[booster.boosterType] = booster; // last one wins if duplicates
            }
        }
        
        public Sprite GetGamePlaySprite(BoosterType type)
        {
            var data = GetBoosterData(type);
            if (data == null) return null;
            return data.gameplayImage;  
        }

        public List<ResourceAmount> GetPrice(BoosterType type)
        {
            var boosterRecord = GetBoosterData(type);
            return boosterRecord?.price;
        }

        /// <summary>
        /// Finds booster info by its type.
        /// Returns null if not found.
        /// </summary>
        public BoosterRecord GetBoosterData(BoosterType type)
        {
            BuildIndex();
            return _dataDict.TryGetValue(type, out var data) ? data : null;
        }
    }
    
    public enum BoosterType
    {
        NONE = 0,
        TIMER = 1,
        CLEAR_A_FOOD_TYPE = 2,
        SHUFFLE = 3,
        MORE_GRILL = 7,
        EXTRA_HEART = 8

    }
}