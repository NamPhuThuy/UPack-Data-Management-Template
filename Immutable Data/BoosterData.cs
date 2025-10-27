using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    [Serializable]
    public class BoosterRecord
    {
        public BoosterType boosterType;
        public int unlockLevel;
        
        
        public string boosterName;
        public Sprite gameplayImage;
        public Sprite inventoryImage;
    }
    
    [CreateAssetMenu(fileName = "BoosterData", menuName = "Game/BoosterData")]
    public class BoosterData : ScriptableObject
    {
        public BoosterRecord[] allBoosters;
        
        // internal cache for faster lookups
        private Dictionary<BoosterType, BoosterRecord> _lookup;
        
        private void BuildIndex()
        {
            if (_lookup != null) return;

            _lookup = new Dictionary<BoosterType, BoosterRecord>();
            if (allBoosters == null) return;

            foreach (var booster in allBoosters)
            {
                if (booster == null) continue;
                _lookup[booster.boosterType] = booster; // last one wins if duplicates
            }
        }

        /// <summary>
        /// Finds booster info by its type.
        /// Returns null if not found.
        /// </summary>
        public BoosterRecord GetBoosterData(BoosterType type)
        {
            BuildIndex();
            return _lookup.TryGetValue(type, out var data) ? data : null;
        }
    }
}