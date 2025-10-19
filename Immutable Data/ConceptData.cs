using System;
using System.Collections.Generic;
using NamPhuThuy.Common;
using UnityEngine;
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
                _dictConceptData[record.conceptType] = record;
            }
        }

        #endregion

        #region Public Methods

        public ConceptRecord GetConceptData(ConceptRecord.ConceptType conceptType)
        {
            EnsureDict();
            if (_dictConceptData == null) return null;
            return _dictConceptData.GetValueOrDefault(conceptType);
        }

        public FoodRecord GetFoodData(ConceptRecord.ConceptType conceptType, FoodType foodType)
        {
            var concept = GetConceptData(conceptType);
            if (concept?.foodData == null) return null;

            foreach (var food in concept.foodData)
            {
                if (food.type == foodType)
                    return food;
            }
            return null;
        }

        #endregion
    }
    
    [Serializable]
    public class ConceptRecord
    {
        public enum ConceptType
        {
            NONE = 0,
            CLASSIC = 1,
            BEACH = 2,
            FARM = 3,
        }
        
        [Header("Concept Settings")]
        public ConceptType conceptType;
        
        [Header("Level Visuals")]
        public Sprite levelHeader;
        public Sprite levelBackground;
        public Sprite levelFooter;
        
        [Header("Gameplay Elements")]
        public Sprite plateSprite;
        public Sprite skewerSprite;
        public Sprite grillSprite;
        
        [Header("Food Data")]
        public FoodRecord[] foodData;

        public FoodRecord GetFoodRecord(FoodType type)
        {
            if (foodData == null) return null;
            
            foreach (var food in foodData)
            {
                if (food.type == type)
                    return food;
            }
            return null;
        }
    }

    [Serializable]
    public class FoodRecord
    {
        public FoodType type;
        public Sprite sprite;
    }
}