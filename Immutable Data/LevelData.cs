using System;
using UnityEngine;

namespace NamPhuThuy.Data
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "Game/LevelData")]
    public class LevelData : ScriptableObject
    {
        public LevelRecord[] allLevels;
        public int coinReward;
    }

    [Serializable]
    public class LevelRecord
    {
        public GameObject prefab;
    }
}