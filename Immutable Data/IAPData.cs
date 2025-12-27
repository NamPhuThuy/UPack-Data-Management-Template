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
        private Dictionary<string, IAPRecord> _dictRecordByID;

        public Dictionary<string, IAPRecord> Data
        {
            get
            {
                if (_dictRecordByID == null)
                {
                    TryInitDict();
                }

                return _dictRecordByID;
            }
        }
        
        #region Private Methods

        private void TryInitDict()
        {
            if (_dictRecordByID != null) return;
            _dictRecordByID = new Dictionary<string, IAPRecord>(records?.Length ?? 0);
            if (_dictRecordByID == null) return;
            
            foreach (var r in records)
            {
                if (r == null)
                {
                    DebugLogger.LogError(message:$"Record is null", context:this);
                    continue;
                }
                
                if (string.IsNullOrEmpty(r.BundleId))
                {
                    DebugLogger.LogError(message:$"Bundle is null", context:this);
                    continue;
                }
                _dictRecordByID[r.BundleId] = r; // last one wins if duplicates
            }
        }

        #endregion

        #region Public Methods

        public IAPRecord GetRecord(string bundleId)
        {
            TryInitDict();
            if (_dictRecordByID == null) return null;
            return _dictRecordByID.GetValueOrDefault(bundleId);
        }   

        #endregion
    }
    
    public enum IAPType
    {
        BUNDLE = 0,
        COIN = 1,
        NO_ADS = 2,
        BOOSTER = 3
    }
    
    [System.Serializable]
    public class IAPRecord
    {
        [SerializeField] private string bundleName;
        [SerializeField] private string bundleId;
        [SerializeField] private IAPType iapType;
        [SerializeField] private Sprite titleImage;
        [SerializeField] private Sprite subTitleImage;
        [SerializeField] private string description;
        [SerializeField] private string price;

        [SerializeField] private List<ResourceAmount> rewardList;
        
        public string BundleName => bundleName;
        public string BundleId => bundleId;
        public IAPType Type => iapType;
        public Sprite TitleImage => titleImage;
        public Sprite SubTitleImage => subTitleImage;
        public string Description => description;
        public string Price => price;
        public List<ResourceAmount> RewardList => rewardList;
    }
}