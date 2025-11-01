using System;
using System.Collections;
using System.IO;
using MoreMountains.Tools;
using NamPhuThuy.Common;
using NamPhuThuy.GamePlay;
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

    public class DataManagerChecked : Singleton<DataManagerChecked>, MMEventListener<EBoosterActivated>, MMEventListener<ELevelFinished>
    {
        #region Private Fields
        private Coroutine _saveDebounce;

        #endregion

        #region Mutable Datas

        [Header("Mutable Datas")]
        private string _savePlayerDataPath;
        private string _saveSettingsDataPath;
        private string _saveGalleryDataPath;

        private bool _isPlayerDataLoaded;
        private bool _isSettingsDataLoaded;

        [SerializeField] private PlayerData cachedPlayerData;

        public PlayerData PlayerData // Lazy loading - only load when needed
        {
            get
            {
                if (!_isPlayerDataLoaded)
                {
                    _isPlayerDataLoaded = true;
                    LoadPlayerData();
                }

                return cachedPlayerData;
            }
        }


        [SerializeField] private PlayerSettingsData cachedPlayerSettingsData;

        public PlayerSettingsData PlayerSettingsData
        {
            get
            {
                if (!_isSettingsDataLoaded)
                {
                    LoadSettingsData();
                    _isSettingsDataLoaded = true;
                }

                return cachedPlayerSettingsData;
            }
        }
        
        #endregion




        #region Immutable Datas

        [Header("Immutable Datas")]
        [SerializeField] private LevelData levelData;

        public LevelData LevelData
        {
            get
            {
                if (levelData == null)
                {
                    levelData = Resources.Load<LevelData>("LevelData");
                }

                return levelData;
            }
        }

        [SerializeField] private BoosterData boosterData;

        public BoosterData BoosterData
        {
            get
            {
                if (boosterData == null)
                {
                    boosterData = Resources.Load<BoosterData>("BoosterData");
                }

                return boosterData;
            }
        }

        [SerializeField] private IAPData iapData;

        public IAPData IAPData
        {
            get
            {
                if (iapData == null)
                {
                    iapData = Resources.Load<IAPData>("IAPData");
                }

                return iapData;
            }
        }
        
        [SerializeField] private ResourceData resourceData;

        public ResourceData ResourceData
        {
            get
            {
                if (resourceData == null)
                {
                    resourceData = Resources.Load<ResourceData>("ResourceData");
                }

                return resourceData;
            }
        }
        
        [SerializeField] private FoodData foodData;

        public FoodData FoodData
        {
            get
            {
                if (foodData == null)
                {
                    foodData = Resources.Load<FoodData>("FoodData");
                }

                return foodData;
            }
        }
        
        [SerializeField] private ConceptData conceptData;

        public ConceptData ConceptData
        {
            get
            {
                if (conceptData == null)
                {
                    conceptData = Resources.Load<ConceptData>("ConceptData");
                }

                return conceptData;
            }
        }

        #endregion


        #region MonoBehaviour Callbacks

        private void Start()
        {
            // Debug.Log($"path: {Application.persistentDataPath}");
            _savePlayerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";
            _saveSettingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            _saveGalleryDataPath = $"{Application.persistentDataPath}/gallery.{DataConst.DATA_FILES_EXTENSION}";

            LoadData();
        }

        private void OnEnable()
        {
            MMEventManager.RegisterAllCurrentEvents(this);
        }

        private void OnDisable()
        {
            MMEventManager.UnregisterAllCurrentEvents(this);
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

        #region Private Methods

        public void SavePlayerData()
        {
            _savePlayerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";

            //example: origin = "{"name":"NamTrinh","level":12,"currentExpPoint":31.0}"
            string origin = JsonUtility.ToJson(cachedPlayerData);
            string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);


            File.WriteAllText(_savePlayerDataPath, encrypted);
        }

        public void SaveSettingsData()
        {
            _saveSettingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            string origin = JsonUtility.ToJson(cachedPlayerSettingsData);
            string encrypted = EncryptHelper.XOROperator(origin, DataConst.DATA_ENCRYPT_KEY);
            File.WriteAllText(_saveSettingsDataPath, encrypted);
        }

        private void LoadPlayerData()
        {
            _savePlayerDataPath = $"{Application.persistentDataPath}/player.{DataConst.DATA_FILES_EXTENSION}";
            if (File.Exists(_savePlayerDataPath))
            {
                try
                {
                    /*
                 File.ReadAllText(_savePath) reads from disk
                 Disk operations are significantly slower than memory operations
                 Can cause frame drops if called during gameplay
                 */
                    string data = File.ReadAllText(_savePlayerDataPath);

                    //Large string operations can be memory and CPU intensive
                    string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPlayerData = JsonUtility.FromJson<PlayerData>(decrypted);
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetPlayerData();
                }
            }
            else
                ResetPlayerData();
        }

        private void LoadSettingsData()
        {
            _saveSettingsDataPath = $"{Application.persistentDataPath}/settings.{DataConst.DATA_FILES_EXTENSION}";
            if (File.Exists(_saveSettingsDataPath))
            {
                try
                {
                    string data = File.ReadAllText(_saveSettingsDataPath);
                    string decrypted = EncryptHelper.XOROperator(data, DataConst.DATA_ENCRYPT_KEY);
                    cachedPlayerSettingsData = JsonUtility.FromJson<PlayerSettingsData>(decrypted);
                }
                catch (Exception e)
                {
                    // Debug.Log(e.Message);
                    ResetSettingsData();
                }
            }
            else
                ResetSettingsData();
        }

        public void ResetPlayerData()
        {
            cachedPlayerData = new PlayerData();
            SavePlayerData();
        }

        public void ResetSettingsData()
        {
            cachedPlayerSettingsData = new PlayerSettingsData();
            SaveSettingsData();
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
            // Debug.Log($"DataManager.LoadData()");
            LoadPlayerData();
            LoadSettingsData();
        }

        #endregion

#if UNITY_EDITOR
        public bool LoadStaticDatas()
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
            ResourceReward reward = new ResourceReward()
            {
                resourceType = ResourceType.BOOSTER,
                boosterType = type,
                amount = -1
            };

            /*if (PlayerData.TryApplyReward(reward))
            {
                DebugLogger.Log($"DataManager.MinusBoosterAmount() done", Color.black);
            }

            else
            {
                DebugLogger.Log($"DataManager.MinusBoosterAmount() failed", Color.black);
            }*/
            var n = PlayerData.GetBoosterNum(type);
            PlayerData.SetBoosterNum(type, Mathf.Max(0, n - 1));
        }

        #endregion


        #region MMEvent Listeners
        
        public void OnMMEvent(EBoosterActivated eventData)
        {
            DebugLogger.Log(message:$"Data", context:this);
            MinusBoosterAmount(eventData.BoosterType);
            MMEventManager.TriggerEvent(new EResourceUpdated()
            {
                ResourceType = ResourceType.BOOSTER
            });
        }
        
        public void OnMMEvent(ELevelFinished eventData)
        {
            if (eventData.IsWin)
            {
                PlayerData.CurrentLevelId++;
            }
        }
        
        #endregion

        
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(DataManagerChecked), true)]
    public class DataManagerCheckedInspector : Editor
    {
        private DataManagerChecked dataManager;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            dataManager = (DataManagerChecked)target;

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
                if (!dataManager.LoadStaticDatas())
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

  

   

    public enum GrillType
    {
        NONE = 0,
        NORMAL = 1,
        FREEZE = 2,
        LOCKED = 3
    }
}