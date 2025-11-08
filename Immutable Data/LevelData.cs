using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using NamPhuThuy.GamePlay;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
    public class LevelData : ScriptableObject
    {
        public LevelRecord[] allLevels;
        public int coinReward;
        
        
        private Dictionary<int, LevelRecord> _dictLevelData;

        public Dictionary<int, LevelRecord> DictLevelData
        {
            get
            {
                if (_dictLevelData == null)
                {
                    EnsureDict();
                }
                return _dictLevelData;
            }
        }

        #region Callbacks

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (allLevels == null || allLevels.Length == 0)
                return;

            bool needsUpdate = false;

            for (int i = 0; i < allLevels.Length; i++)
            {
                
                LevelRecord levelRecord = allLevels[i];
                if (levelRecord == null)
                    continue;

                // Update level ID
                if (levelRecord.levelID != i)
                {
                    levelRecord.levelID = i;
                    needsUpdate = true;
                }

                if (levelRecord.grillRecords == null)
                {
                    continue;
                }

                // Update grill IDs within each level
                for (int j = 0; j < levelRecord.grillRecords.Count; j++)
                {
                    if (levelRecord.grillRecords[j] == null)
                        continue;
                    
                    GrillRecord grillRecord = levelRecord.grillRecords[j];

                    if (grillRecord.grillId != j)
                    {
                        grillRecord.grillId = j;
                        needsUpdate = true;
                    }
                }
                
                // Count food
                int foodCnt = 0;
                for (int j = 0; j < levelRecord.grillRecords.Count; j++)
                {
                    if (levelRecord.grillRecords[j] == null)
                        continue;
                    GrillRecord grillRecord = levelRecord.grillRecords[j];
                    
                    if (grillRecord.foodList == null)
                        continue;
                    
                    foodCnt += grillRecord.foodList.Count;
                }
                
                if (levelRecord.foodAmount != foodCnt)
                {
                    levelRecord.foodAmount = foodCnt;
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                DebugLogger.Log(message:"Level and Grill IDs updated", context:this);
            }

            _dictLevelData = null;
        }
#endif

        #endregion

        #region Private Methods

        private void EnsureDict()
        {
            // DebugLogger.Log();
            if (_dictLevelData != null) return;
            _dictLevelData = new Dictionary<int, LevelRecord>(allLevels?.Length ?? 0);
            if (_dictLevelData == null) return;

            foreach (var level in allLevels)
            {
                if (level == null)
                {
                    DebugLogger.LogError(message: "Level record is null", context: this);
                    continue;
                }

                if (level.levelID < 0)
                {
                    DebugLogger.LogError(message: "Invalid level ID", context: this);
                    continue;
                }

                _dictLevelData[level.levelID] = level;
            }
        }
        
        public LevelRecord GetLevelData(int levelId)
        {
            // DebugLogger.Log(message:$"Get level data: {levelId}", context:this);
            EnsureDict();
            if (_dictLevelData == null) return null;
            return _dictLevelData.GetValueOrDefault(levelId);
        }

        /// <summary>
        /// The total number of levels
        /// </summary>
        /// <returns></returns>
        public int GetTotalLevels()
        {
            return allLevels?.Length ?? 0;
        }

        #endregion
    }

    [Serializable]
    public class LevelRecord
    {
        public int levelID;
        public int foodTypeNum; // number of different food types in this level
        public int foodAmount;
        public int grillNum;
        public int grillEmpty;
        
        
        public float duration;
        
        
        public ConceptRecord.ConceptType conceptType;
        public List<GrillRecord> grillRecords;
    }
    
    [Serializable]
    public class GrillRecord
    {
        public int grillId;
        public BaseGrill.GrillMechanic grillMechanic;
        public FoodType lockedFoodType;
        public List<FoodType> foodList;
        public List<FoodMechanicRecord> foodMechanics;
        public List<GrillMechanicRecord> grillMechanics;
        // public List<int> foodId;
    }

    [Serializable]
    public class FoodMechanicRecord
    {
        public FoodMechanic mechanic;
        public int amount;
    }
    
    [Serializable]
    public class GrillMechanicRecord
    {
        public BaseGrill.GrillMechanic mechanic;
        public FoodType lockedFoodType;
        public int amount;
    }
    

    [Serializable]
    public class ManualFoodRecord
    {
        /// <summary>
        /// grillIndex -> list of food unit IDs on that grill
        /// Example: {0: [0,1], 1: [2]} means units 0,1 on grill 0, unit 2 on grill 1
        /// </summary>
        public List<int> grillIndices;
        public List<List<int>> foodUnitsPerGrill;

        public Dictionary<int, List<int>> GetGrillToFoodMapping()
        {
            var mapping = new Dictionary<int, List<int>>();
            for (int i = 0; i < grillIndices.Count; i++)
            {
                mapping[grillIndices[i]] = foodUnitsPerGrill[i];
            }

            return mapping;
        }
    }
    
}