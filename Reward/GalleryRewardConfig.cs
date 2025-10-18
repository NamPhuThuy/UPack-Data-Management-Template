using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NamPhuThuy
{
    [CreateAssetMenu(fileName = "GalleryRewardConfig", menuName = "ScriptableObjects/GalleryRewardConfig")]
    public class GalleryRewardConfig : ScriptableObject
    {
        [SerializeField] private GalleryRewardConfigItem[] items;

        public GalleryRewardConfigItem[] Items => items;

        public float GetCurrentProgress(int level)
        {
            int startLevel = 10;
            int requiredNumberOfLevel = 10;

            if (level == 0)
            {
                return 0;
            }

            for (int i = 0; i < items.Length; i++)
            {
                GalleryRewardConfigItem item = items[i];

                if (level >= item.StartLevel && level <= item.EndLevel)
                {
                    startLevel = item.StartLevel;
                    requiredNumberOfLevel = item.RequiredNumberOfLevel;

                    break;
                }
            }

            int relativeLevel = level - startLevel;
            int rewardCycleProgress = relativeLevel % requiredNumberOfLevel;
            float progressPerLevel = 1f / requiredNumberOfLevel;
            float progress = (float)rewardCycleProgress / requiredNumberOfLevel + progressPerLevel;

            return Mathf.Clamp01(progress);
        }

        public float GetPrevProgress(int level)
        {
            float progress = GetCurrentProgress(level - 1);

            return progress == 1 ? 0 : progress;
        }
    }

    [Serializable]
    public class GalleryRewardConfigItem
    {
        [SerializeField] private int startLevel;
        [SerializeField] private int endLevel;
        [SerializeField] private int requiredNumberOfLevel;

        public int StartLevel => startLevel;
        public int EndLevel => endLevel;
        public int RequiredNumberOfLevel => requiredNumberOfLevel;
    }
}
