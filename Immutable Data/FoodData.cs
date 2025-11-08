using System;
using System.Collections.Generic;
using UnityEngine;
using NamPhuThuy.Common;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "FoodData", menuName = "Game/FoodData")]
    public class FoodData : ScriptableObject
    {
        [SerializeField] private FoodRecord[] allFoods;

        private Dictionary<FoodType, FoodRecord> _dictFoodData;

        public Dictionary<FoodType, FoodRecord> DictFoodData
        {
            get
            {
                if (_dictFoodData == null)
                {
                    EnsureDict();
                }
                return _dictFoodData;
            }
        }

        #region Callbacks

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (allFoods == null || allFoods.Length == 0)
                return;

            HashSet<FoodType> seenTypes = new HashSet<FoodType>();
            bool foundDuplicate = false;

            for (int i = 0; i < allFoods.Length; i++)
            {
                if (allFoods[i] == null)
                    continue;

                FoodType currentType = allFoods[i].type;

                if (seenTypes.Contains(currentType))
                {
                    DebugLogger.LogError(message:$"Duplicate FoodType '{currentType}', index {i}", context: this);
                    foundDuplicate = true;
                }
                else
                {
                    seenTypes.Add(currentType);
                }
            }

            if (foundDuplicate)
            {
                UnityEditor.EditorUtility.DisplayDialog(
                    "Duplicate FoodType Detected",
                    "One or more records have duplicate FoodType values. Please ensure each record has a unique FoodType.",
                    "OK"
                );
            }

            _dictFoodData = null;
        }
#endif

        #endregion

        #region Public Methods

        public FoodRecord GetFoodData(FoodType foodType)
        {
            EnsureDict();
            if (_dictFoodData == null) return null;
            return _dictFoodData.GetValueOrDefault(foodType);
        }

        public Sprite GetFoodSprite(FoodType foodType)
        {
            EnsureDict();
            Sprite sprite = null;
            
            if (_dictFoodData == null) return null;
            if (_dictFoodData.TryGetValue(foodType, out var record))
            {
                sprite = record.sprite;
            }

            return sprite;
        }

        public int GetTotalFoods()
        {
            return allFoods?.Length ?? 0;
        }

        #endregion

        #region Private Methods

        private void EnsureDict()
        {
            if (_dictFoodData != null) return;
            _dictFoodData = new Dictionary<FoodType, FoodRecord>(allFoods?.Length ?? 0);
            if (_dictFoodData == null) return;

            foreach (var food in allFoods)
            {
                if (food == null)
                {
                    DebugLogger.LogError(message: "Food record is null", context: this);
                    continue;
                }

                if (_dictFoodData.ContainsKey(food.type))
                {
                    DebugLogger.LogError(message: $"Duplicate FoodType: {food.type}", context: this);
                    continue;
                }

                _dictFoodData[food.type] = food;
            }
        }

        #endregion
    }

    [Serializable]
    public class FoodRecord
    {
        public FoodType type;
        public Sprite sprite;
        public string displayName;
        public int baseScore;
    }
    
    public enum FoodMechanic
    {
        NONE = 0,
        NORMAL = 1,
        ADD_TIME = 2,
        UNKNOW = 3,
    }
    
    public enum FoodType
    {
        NONE = 0,
        MEET_SCROLL = 1,
        CAKE = 2,
        ICE_CREAM = 3,
        SHRIMP = 4,
        HOTDOG = 5,
        SALAD = 6,
    }
    
    // generate FoodMechanicPair
    [Serializable]
    public class FoodMechanicPair
    {
        public FoodMechanic mechanic;
        public int value;
    }
}