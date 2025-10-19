using System.Collections;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "IAPData", menuName = "Game/IAPData", order = 1)]
    public class IAPData : ScriptableObject
    {
        [Header("IAP Data")]
        [SerializeField] private IAPRecord[] records;
        private Dictionary<string, IAPRecord> _dictIAPData;

        public Dictionary<string, IAPRecord> DictIAPData
        {
            get
            {
                if (_dictIAPData == null)
                {
                    EnsureDict();
                }

                return _dictIAPData;
            }
        }
        
        #region Private Methods

        private void EnsureDict()
        {
            if (_dictIAPData != null) return;
            _dictIAPData = new Dictionary<string, IAPRecord>(records?.Length ?? 0);
            if (_dictIAPData == null) return;
            
            
            foreach (var r in records)
            {
                if (r == null)
                {
                    DebugLogger.LogError(message:$"Record is null", context:this);
                    continue;
                }
                
                if (string.IsNullOrEmpty(r.bundleId))
                {
                    DebugLogger.LogError(message:$"Bundle is null", context:this);
                    continue;
                }
                _dictIAPData[r.bundleId] = r; // last one wins if duplicates
            }
        }

        #endregion

        #region Public Methods

        public IAPRecord GetIAPData(string bundleId)
        {
            EnsureDict();
            if (_dictIAPData == null) return null;
            return _dictIAPData.GetValueOrDefault(bundleId);
        }   

        #endregion
    }
    
    [System.Serializable]
    public class IAPRecord
    {
        public enum IAPType
        {
            BUNDLE = 0,
            COIN = 1,
            NO_ADS = 2,
            BOOSTER = 3
        }
        
        public string bundleName;
        public string bundleId;
        public IAPType iapType;
        public Sprite titleImage;
        public Sprite subTitleImage;
        public string description;
        public string price;

        public List<ResourceReward> rewardList;
        
        public int GetResourceAmount(ResourceType type)
        {
            if (rewardList == null) return 0;

            foreach (var resource in rewardList)
            {
                if (resource != null && type == resource.resourceType)
                {
                    return resource.amount;
                }
            }

            return 0;
        }
        
        
    }
}