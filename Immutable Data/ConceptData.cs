using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
using Random = UnityEngine.Random;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "ConceptData", menuName = "Game/ConceptData")]
    public class ConceptData : ScriptableObject
    {
        #region Private Serializable Fields
        
        [Header("Concept Data")]
        [SerializeField] private ConceptRecord[] records;

        #endregion

        #region Private Fields

        private Dictionary<ConceptRecord.ConceptType, ConceptRecord> _dictConceptData;

        #endregion

        public Dictionary<ConceptRecord.ConceptType, ConceptRecord> DictConceptData
        {
            get
            {
                if (_dictConceptData == null)
                {
                    EnsureDict();
                }
                return _dictConceptData;
            }
        }

        #region Callbacks

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (records == null || records.Length == 0)
                return;

            HashSet<ConceptRecord.ConceptType> seenTypes = new HashSet<ConceptRecord.ConceptType>();
            bool foundDuplicate = false;

            for (int i = 0; i < records.Length; i++)
            {
                if (records[i] == null)
                    continue;

                ConceptRecord.ConceptType currentType = records[i].type;

                if (seenTypes.Contains(currentType))
                {
                    DebugLogger.LogError(message:$"Duplicate ConceptType '{currentType}' found at index {i}. Each record must have a unique ConceptType.", context:this);
                    foundDuplicate = true;
                }
                else
                {
                    seenTypes.Add(currentType);
                }
            }

            if (foundDuplicate)
            {
                EditorUtility.DisplayDialog(
                    "Duplicate ConceptType Detected",
                    "One or more records have duplicate ConceptType values. Please ensure each record has a unique ConceptType.",
                    "OK"
                );
            }

            // Clear cached dictionary to force rebuild
            _dictConceptData = null;
        }
#endif

        #endregion
        
        #region Private Methods

        private void EnsureDict()
        {
            if (_dictConceptData != null) return;
            _dictConceptData = new Dictionary<ConceptRecord.ConceptType, ConceptRecord>(records?.Length ?? 0);
            if (_dictConceptData == null) return;

            foreach (var record in records)
            {
                if (record == null)
                {
                    DebugLogger.LogError(message:$"ConceptData is null", context:this);
                    continue;
                }
                _dictConceptData[record.type] = record;
            }
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Get a random concept from ConceptData
        /// </summary>
        public ConceptRecord GetRandomConcept()
        {
            ConceptRecord result = null;
            
            int ranId = Random.Range(0, records.Length);
            result = GetConceptData(records[ranId].type);

            return result;
        }

        public ConceptRecord GetConceptData(ConceptRecord.ConceptType conceptType)
        {
            EnsureDict();
            if (_dictConceptData == null) return null;
            return _dictConceptData.GetValueOrDefault(conceptType);
        }

        #endregion
    }
    
    [Serializable]
    public class ConceptRecord
    {
        public enum ConceptType
        {
            NONE = 0,
            DEFAULT = 1,
            MEAT = 2,
            SEA_FOOD = 3,
            VEGETABLE = 4,
            FRUIT = 5,
            STREET_FOOD = 6,
            NATION_THEME = 7
        }
        
        [Header("Concept Settings")]
        public ConceptType type;
        
        [Header("Level Visuals")]
        public Sprite levelHeader;
        public Sprite levelBackground;
        // public Sprite levelFooter;
        
        [Header("Gameplay Elements")]
        public Sprite plateSprite;
        public Sprite skewerSprite;
        public Sprite grillLidSprite;
        public Sprite grillBodySprite;
        
        [Header("Food Data")]
        public FoodType[] foodTypes;
    }

 
}