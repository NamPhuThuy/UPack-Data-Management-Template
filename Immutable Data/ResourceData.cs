using System;
using System.Collections.Generic;
using NamPhuThuy.Data;
using Sirenix.OdinInspector;
using UnityEngine;

namespace NamPhuThuy.Data
{
    /// <summary>
    /// Testing, not being used yet
    /// </summary>
    [CreateAssetMenu(fileName = "ResourceData", menuName = "Game/ResourceData")]
    
    public class ResourceData : ScriptableObject
    {
        [Header("Resource Data")]
        [SerializeField] private ResourceRecord[] resourceRecords;
        private Dictionary<ResourceType, ResourceRecord> _dictResourceData;

        public Dictionary<ResourceType, ResourceRecord> DictResourceData
        {
            get
            {
                if (_dictResourceData == null)
                {
                    EnsureIndex();
                }
                return _dictResourceData;
            }
        }

        private void EnsureIndex()
        {
            if (_dictResourceData != null) return;
            _dictResourceData = new Dictionary<ResourceType, 
                ResourceRecord>(resourceRecords?.Length ?? 0);
            if (resourceRecords == null) return;
            foreach (var r in resourceRecords)
            {
                if (r == null) continue;
                _dictResourceData[r.resourceType] = r;
            }
        }

        public ResourceRecord GetResourceRecord(ResourceType resourceType)
        {
            EnsureIndex();
            return _dictResourceData != null && _dictResourceData.TryGetValue(resourceType, out var record) ? record : null;
        }
        
        public Sprite GetResourceGameplayImage(ResourceType resourceType)
        {
            EnsureIndex();
            if (_dictResourceData != null && _dictResourceData.TryGetValue(resourceType, out var record))
            {
                return record.gameplayImage;
            }
            return null;
        }
        
        public int GetBoosterPrice(BoosterType boosterType)
        {
            EnsureIndex();
            foreach (var record in _dictResourceData.Values)
            {
                if (record.resourceType == ResourceType.BOOSTER && record.boosterType == boosterType)
                {
                    int price = 0;
                    foreach (var priceItem in record.priceInResources)
                    {
                        if (priceItem.resourceType == ResourceType.COIN)
                            price += priceItem.amount;
                    }
                    return price;
                }
            }
            return 150; // Not found
        }
    }

    [Serializable]
    public class ResourceRecord
    {
        public ResourceType resourceType;
        
        [ShowIf(nameof(resourceType), ResourceType.BOOSTER)]
        public BoosterType boosterType;
        
        public Sprite gameplayImage;
        public Sprite inventoryImage;
        
        public string description;
        public List<ResourceAmount> priceInResources;
    }
    
    [Serializable]
    public class ResourceAmount
    {
        public ResourceType resourceType;
        
        [ShowIf(nameof(resourceType), ResourceType.BOOSTER)]
        public BoosterType boosterType;

        public int amount;

        public ResourceAmount(ResourceType resourceType = ResourceType.COIN, BoosterType boosterType = BoosterType.NONE, int pictureId = -1, int amount = 0)
        {
            this.resourceType = resourceType;
            this.boosterType = boosterType;
            this.amount = amount;
        }
    }
    
    public enum ResourceType
    {
        NONE = 0,
        COIN = 1,
        BOOSTER = 2,
        HEART = 3,
        NO_ADS = 5,
        TIME = 6,
    }
    
}