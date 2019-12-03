﻿using System;
using System.Collections.Generic;

/// <summary>
/// A class to hold save data.
/// </summary>
[System.Serializable]
public class SaveData
{
    /// <summary>
    /// A class to hold save data for a level.
    /// </summary>
    public class LevelSaveData
    {
        public float lowestTime = 0.0f;
        public int stars = 0;
        public bool hasCollectedTreasures = false;
        public bool hasAchievedTargetTime = false;
        public bool hasFoundRelic = false;
        public bool hasAchievedThreeStars = false;

        public LevelSaveData()
        {
            lowestTime = 0.0f;
            stars = 0;
            hasCollectedTreasures = false;
            hasAchievedTargetTime = false;
            hasFoundRelic = false;
            hasAchievedThreeStars = false;
        }
    }
    
    public int Coins { get; set; }
    public int LastLevelNumber { get; set; }
    public DateTime Timestamp { get; private set; }
    public List<LevelSaveData> levelSaveData;

    public SaveData()
    {
        Coins = 0;
        LastLevelNumber = 1;
        levelSaveData = new List<LevelSaveData>();
        levelSaveData.Add(new LevelSaveData());
        UpdateTimestamp();
    }

    /// <summary>
    /// Updates the timestamp.
    /// </summary>
    public void UpdateTimestamp()
    {
        Timestamp = DateTime.Now;
    }

    /// <summary>
    /// Resets level progress data.
    /// </summary>
    public void ResetProgress()
    {
        LastLevelNumber = 1;
        levelSaveData = new List<LevelSaveData>();
        UpdateTimestamp();
    }
}