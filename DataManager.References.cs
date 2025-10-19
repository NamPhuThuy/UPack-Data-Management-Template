using System.Collections;
using UnityEngine;


namespace NamPhuThuy.Data
{
    
    public partial class DataManager
    {
        [Header("Mutable Datas")]
        #region Mutable Data References

        private bool _isPlayerDataLoaded;
        private bool _isSettingsDataLoaded;
        
        [SerializeField] private PlayerData cachedPlayerData;

        public PlayerData PlayerData
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

        #region Immutable Data References

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

    }
}