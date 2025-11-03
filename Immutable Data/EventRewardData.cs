using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using NamPhuThuy.Common;
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "EventRewardData", menuName = "Game/EventRewardData")]
    public class EventRewardData : ScriptableObject
    {
        public EventRewardRecord[] data;

        // internal cache for faster lookups
        private Dictionary<EventRewardType, EventRewardRecord> _dataDict;

        #region MonoBehaviour Callbacks

        private void OnValidate()
        {
#if UNITY_EDITOR
            if (data == null || data.Length == 0) return;

            var typesSeen = new HashSet<EventRewardType>();
            var duplicates = new List<EventRewardType>();

            foreach (var record in data)
            {
                if (record == null) continue;

                if (record.eventRewardType == EventRewardType.NONE) continue;

                if (!typesSeen.Add(record.eventRewardType))
                {
                    if (!duplicates.Contains(record.eventRewardType))
                    {
                        duplicates.Add(record.eventRewardType);
                    }
                }
            }

            if (duplicates.Count > 0)
            {
                DebugLogger.LogError(message: $"[EventRewardData] Duplicate EventRewardTypes found: {string.Join(", ", duplicates)}", context: this);
            }

            // Clear the cached dictionary to force rebuild on next lookup
            _dataDict = null;
#endif
        }


        #endregion
        
        private void EnsureDict()
        {
            if (_dataDict != null) return;

            _dataDict = new Dictionary<EventRewardType, EventRewardRecord>();
            if (data == null) return;

            foreach (var record in data)
            {
                if (record == null) continue;
                _dataDict[record.eventRewardType] = record; // last one wins if duplicates
            }
        }

        /// <summary>
        /// Finds event reward info by its type.
        /// Returns null if not found.
        /// </summary>
        public EventRewardRecord GetEventReward(EventRewardType type)
        {
            EnsureDict();
            return _dataDict.TryGetValue(type, out var record) ? record : null;
        }

        /// <summary>
        /// Gets the rewards list for a specific event reward type.
        /// Returns null if not found.
        /// </summary>
        public List<ResourceAmount> GetRewards(EventRewardType type)
        {
            var record = GetEventReward(type);
            return record?.rewards;
        }
    }
    
    [Serializable]
    public class EventRewardRecord
    {
        public EventRewardType eventRewardType;
        public List<ResourceAmount> rewards;
    }
    
    public enum EventRewardType
    {
        NONE = 0,
        WIN_LEVEL = 1,
        WATCH_ADS_WIN_LEVEL = 2,
    }
}