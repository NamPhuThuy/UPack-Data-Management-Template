using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
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
                if (allLevels[i] == null)
                    continue;

                // Update level ID
                if (allLevels[i].levelID != i)
                {
                    allLevels[i].levelID = i;
                    needsUpdate = true;
                }

                // Update grill IDs within each level
                if (allLevels[i].grillRecords != null)
                {
                    for (int j = 0; j < allLevels[i].grillRecords.Count; j++)
                    {
                        if (allLevels[i].grillRecords[j] == null)
                            continue;

                        if (allLevels[i].grillRecords[j].grillId != j)
                        {
                            allLevels[i].grillRecords[j].grillId = j;
                            needsUpdate = true;
                        }
                    }
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
            DebugLogger.Log();
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
            DebugLogger.Log(message:$"Get level data: {levelId}", context:this);
            EnsureDict();
            if (_dictLevelData == null) return null;
            return _dictLevelData.GetValueOrDefault(levelId);
        }

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
        public int grillNum;
        public int grillEmpty;
        
        
        public float duration;
        
        
        public ConceptRecord.ConceptType conceptType;
        public List<GrillRecord> grillRecords;
        public List<MechanicRecord> mechanicRecords;
    }
    
    [Serializable]
    public class GrillRecord
    {
        public int grillId;
        public GrillType type;
        public List<FoodType> foodList;
        // public List<int> foodId;
    }
    
    [Serializable]
    public class MechanicRecord
    {
        public MechanicType type;
        public int value;
    }

    public enum MechanicType
    {
        NONE = 0,
        
        /*Skewers*/
        SIMPLE_LOCKED_SKEWER = 1,
        ADS_LOCKED_SKEWER = 2,
        
        /*Food*/
        SILVER_PAPE_WRAPPED_FOOD = 20,
        ADD_TIME_FOOD = 21,
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