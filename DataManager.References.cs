using System.Collections;
using UnityEngine;


namespace NamPhuThuy.Data
{
    
    public partial class DataManager
    {
        #region Components

        [Header("Components")]
        [SerializeField] private LevelDataLoader levelDataLoader;

        #endregion
        
        #region Mutable Datas

        [Header("Mutable Datas")] 
        private string _playerDataPath;
        private string _settingsDataPath;

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
        [Header("Level Data")] 
        [SerializeField] private bool isUseRemoteConfig;
      
      

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

      

        #endregion

    }
}