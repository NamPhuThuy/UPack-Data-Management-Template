using System;
using System.Collections.Generic;
using System.IO;
using NamPhuThuy.Common;
using NamPhuThuy.GamePlay;
using UnityEditor;
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

                int grillRequire = levelRecord.foodAmount / GamePlayConst.SKEWER_FOOD_CAPACITY;
                if (levelRecord.grillRequireCleared != grillRequire)
                {
                    levelRecord.grillRequireCleared = grillRequire;
                    needsUpdate = true;
                }

                int grillWithMechanics = 0;
                for (var i1 = 0; i1 < allLevels[i].grillMechanics.Count; i1++)
                {
                    if (allLevels[i].grillMechanics[i1].mechanic == BaseGrill.GrillMechanic.FOOD_LOCKED) continue;
                    grillWithMechanics += allLevels[i].grillMechanics[i1].amount;
                }

                int tempGrillTotal = levelRecord.grillRequireCleared + levelRecord.grillEmpty + grillWithMechanics;


                if (levelRecord.grillTotal != tempGrillTotal)
                {
                    levelRecord.grillTotal = tempGrillTotal;
                    needsUpdate = true;
                }
            }

            if (needsUpdate)
            {
                UnityEditor.EditorUtility.SetDirty(this);
                DebugLogger.Log(message: "Level and Grill IDs updated", context: this);
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

        #region Public Methods
        
        // Call this at runtime with JSON text from Resources/StreamingAssets/RemoteConfig, etc.
        /*public void ImportFromJsonRuntime(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
                return;

            try
            {
                var wrapper = JsonUtility.FromJson<LevelRecordArrayWrapper>(
                    "{\"items\":" + jsonText + "}"
                );

                allLevels = wrapper.items;
                _dictLevelData = null;
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to import LevelData from JSON at runtime: {e}");
            }
        }*/

#if UNITY_EDITOR
        /// <summary>
        /// Import `allLevels` from a JSON file that contains an array of LevelRecord.
        /// </summary>
        public void ImportFromJson(string jsonText)
        {
            if (string.IsNullOrEmpty(jsonText))
            {
                DebugLogger.LogError(message: "JSON text is null or empty", context: this);
                return;
            }

            try
            {
                var wrapper = new LevelRecordArrayWrapper();
                wrapper.items = JsonUtility.FromJson<LevelRecordArrayWrapper>(
                    "{\"items\":" + jsonText + "}"
                ).items;

                allLevels = wrapper.items;
                _dictLevelData = null;

                UnityEditor.EditorUtility.SetDirty(this);
                DebugLogger.Log(message: $"Imported {allLevels?.Length ?? 0} levels from JSON", context: this);
            }
            catch (Exception e)
            {
                DebugLogger.LogError(message: $"Failed to import LevelData from JSON: {e}", context: this);
            }
        }

        [Serializable]
        private class LevelRecordArrayWrapper
        {
            public LevelRecord[] items;
        }
#endif

        #endregion
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(LevelData))]
    public class LevelDataEditor : Editor
    {
        // Adjust this to your actual JSON path if needed
        private const string DefaultJsonPath =
            "Assets/_Project/UPack-Data-Management-Template/Immutable Data/LevelData.json";

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("JSON Import", EditorStyles.boldLabel);

            if (GUILayout.Button("Import From JSON"))
            {
                var levelData = (LevelData)target;

                string path = DefaultJsonPath;

                // Optionally let user pick a file
                if (Event.current.shift) // hold Shift to choose file
                {
                    string picked = EditorUtility.OpenFilePanel("Select LevelData JSON", Application.dataPath, "json");
                    if (!string.IsNullOrEmpty(picked))
                    {
                        path = picked;
                    }
                }

                string jsonText;
                try
                {
                    if (!Path.IsPathRooted(path))
                    {
                        // make relative to project
                        jsonText = File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), path));
                    }
                    else
                    {
                        jsonText = File.ReadAllText(path);
                    }
                }
                catch (IOException e)
                {
                    Debug.LogError($"Failed to read JSON file at {path}: {e}");
                    return;
                }

                levelData.ImportFromJson(jsonText);
                EditorUtility.SetDirty(levelData);
            }

            EditorGUILayout.HelpBox(
                "Click `Import From JSON` to load level list from `LevelData.json`.\nHold Shift while clicking to choose a custom JSON file.",
                MessageType.Info);
        }
    }
#endif

    [Serializable]
    public class LevelRecord
    {
        public int levelID;
        public ConceptRecord.ConceptType conceptType;

        public float duration;

        public int foodTypeNum; // number of different food types in this level
        public int foodAmount;
        public int grillEmpty;
        
        [Tooltip("Food amount / 4")]
        public int grillRequireCleared;

        [Tooltip("grillRequireCleared + grillEmpty + grills with mechanics-not locked food")]
        public int grillTotal;

        public List<FoodMechanicRecord> foodMechanics;
        public List<GrillMechanicRecord> grillMechanics;
        public List<ManualFoodRecord> manualFoods;
    }

    [Serializable]
    public class GrillRecord
    {
        public int grillId;
        public BaseGrill.GrillMechanic grillMechanic;
        public FoodType lockedFoodType;
        public List<FoodType> foodList;


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
        public int amount;
    }


    [Serializable]
    public class ManualFoodRecord
    {
        public List<ListIntValue> foodIdPerGrill;
    }

    [Serializable]
    public class ListIntValue
    {
        public List<int> values;
    }
}