using System;
using System.Collections;
using System.IO;
using MoreMountains.Tools;
using NamPhuThuy.Common;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    /*
ScriptableObjects for:
- Static game configuration
- Level design data
- Item/weapon/character definitions
- Anything that needs Unity asset references

JSON for:
- Player progress/saves
- User settings
- Dynamic content updates
- Anything that needs to change after build
*/

    public partial class DataManager : Singleton<DataManager>, MMEventListener<EBoosterActivated>
    {
        #region Private Fields

        private Coroutine _saveDebounce;

        #endregion

        #region Mutable Datas
       
        public void SavePlayerData()
        {
            SaveMutableData(cachedPlayerData, "player");
        }

        public void SaveSettingsData()
        {
            SaveMutableData(cachedPlayerSettingsData, "settings");
        }

        private void LoadPlayerData()
        {
            cachedPlayerData = LoadMutableData<PlayerData>("player", () => {
                ResetPlayerData();
                return cachedPlayerData;
            });
        }

        private void LoadSettingsData()
        {
            cachedPlayerSettingsData = LoadMutableData<PlayerSettingsData>("settings", () => {
                ResetSettingsData();
                return cachedPlayerSettingsData;
            });
        }


        public void ResetPlayerData()
        {
            ResetMutableData(ref cachedPlayerData, ref _isPlayerDataLoaded, "player");
        }

        public void ResetSettingsData()
        {
            ResetMutableData(ref cachedPlayerSettingsData, ref _isSettingsDataLoaded, "settings");
        }


        #region Mutable Data Management

        public void ResetData()
        {
            ResetPlayerData();
            ResetSettingsData();

            // gameData.dataResourcesInGame = JsonUtility.FromJson<DataResources>(Resources.Load<TextAsset>("Data/data_resources").text).dataResources;
            // SaveDataAsync().ConfigureAwait(false);
        }

        public void SaveData(bool postData = true)
        {
            SavePlayerData();
            SaveSettingsData();
        }

        public void LoadData()
        {
            LoadPlayerData();
            LoadSettingsData();
        }

        #endregion

        #region Generic Save/Load/Reset Methods
        
        private void SaveMutableData<T>(T data, string fileName)
        {
            string path = $"{Application.persistentDataPath}/{fileName}.{DataConst.DATA_FILES_EXTENSION}";
            
            //example: origin = "{"name":"NamTrinh","level":12,"currentExpPoint":31.0}"
            string origin = JsonUtility.ToJson(data);
            // string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            // File.WriteAllText(path, encrypted);
            
            File.WriteAllText(path, origin);
        }

        private T LoadMutableData<T>(string fileName, Func<T> resetAction) where T : class
        {
            string path = $"{Application.persistentDataPath}/{fileName}.{DataConst.DATA_FILES_EXTENSION}";
    
            if (File.Exists(path))
            {
                try
                {
                    /*
                 File.ReadAllText(_savePath) reads from disk
                 Disk operations are significantly slower than memory operations
                 Can cause frame drops if called during gameplay
                 */
                    string data = File.ReadAllText(path);
                    
                    //Large string operations can be memory and CPU intensive
                    // string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    return JsonUtility.FromJson<T>(data);
                }
                catch (Exception e)
                {
                    DebugLogger.LogWarning($"DataManager.LoadMutableData() {e}");
                    return resetAction();
                }
            }
            
            DebugLogger.LogWarning($"DataManager.LoadMutableData() - file not exist: {path}");
            return resetAction();
        }

        private void ResetMutableData<T>(ref T cachedData, ref bool isLoaded, string fileName, Action<T> postResetAction = null) where T : new()
        {
            isLoaded = false;
            cachedData = new T();
            isLoaded = true;
    
            postResetAction?.Invoke(cachedData);
            SaveMutableData(cachedData, fileName);
        }
        #endregion
        
        #endregion


        #region MonoBehaviour Callbacks

        private void Start()
        {
            LoadData();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) SaveData();
        }

        private void OnApplicationQuit()
        {
            SaveData();
        }

        #endregion

        #region Data Management

        public void MarkDirty()
        {
            if (_saveDebounce != null) StopCoroutine(_saveDebounce);
            _saveDebounce = StartCoroutine(SaveAfterDelay(DataConst.SAVE_INTERVAL));
        }

        private IEnumerator SaveAfterDelay(float delay)
        {
            yield return YieldHelper.GetRealtime(delay);
            SaveData();
            _saveDebounce = null;
        }

        #endregion

#if UNITY_EDITOR
        public bool LoadImmutableData()
        {
            levelData = Resources.Load<LevelData>("LevelData");
            boosterData = Resources.Load<BoosterData>("BoosterData");
            iapData = Resources.Load<IAPData>("IAPData");
            resourceData = Resources.Load<ResourceData>("ResourceData");


            if (levelData == null)
                return false;


            /*// Print all Resources folders to help debug
        Object[] resourcesFolders = Resources.FindObjectsOfTypeAll<DefaultAsset>();
        foreach (Object folder in resourcesFolders) {
            if (AssetDatabase.IsValidFolder(AssetDatabase.GetAssetPath(folder))) {
                Debug.Log("Resources Folder: " + AssetDatabase.GetAssetPath(folder));
            }
        }*/

            return true;
        }
#endif

        #region Event Listen

        private void MinusBoosterAmount(BoosterType type)
        {
            try
            {
                var n = PlayerData.GetBoosterNum(type);
                PlayerData.SetBoosterNum(type, Mathf.Max(0, n - 1));
            }
            catch (Exception e)
            {
                DebugLogger.LogError(message:$"Error: {e}", context:this);
            }
        }

        #endregion

        #region MMEvent Listen

        public void OnMMEvent(EBoosterActivated eventData)
        {
            DebugLogger.Log(message:$"Data", context:this);
            MinusBoosterAmount(eventData.BoosterType);
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.BOOSTER
            });
        }

        #endregion
        
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataManager), true)]
    public class DataManagerInspector : Editor
    {
        private DataManager dataManager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            dataManager = (DataManager)target;

            //Interact with dynamic datas
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Data")) dataManager.SaveData();
            if (GUILayout.Button("Load Data")) dataManager.LoadData();
            if (GUILayout.Button("Reset Data")) dataManager.ResetData();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Player Data")) dataManager.SavePlayerData();
            if (GUILayout.Button("Save Settings Data")) dataManager.SaveSettingsData();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset Player Data")) dataManager.ResetPlayerData();
            if (GUILayout.Button("Reset Settings Data")) dataManager.ResetSettingsData();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Static Datas"))
            {
                if (!dataManager.LoadImmutableData())
                    Debug.LogError($"Load statics datas failed");
                else
                    Debug.LogError($"Load statics successfully");
            }

            GUILayout.EndHorizontal();
        }
    }
#endif

    public enum ResourceType
    {
        NONE = 0,
        COIN = 1,
        BOOSTER = 2,
        HEART = 3,
        NO_ADS = 5,
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

   

    public enum GrillType
    {
        NONE = 0,
        NORMAL = 1,
        FREEZE = 2,
        LOCKED = 3
    }
    
}