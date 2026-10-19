using System;
using NS.Core;
using UnityEngine;

namespace NS
{
    [DefaultExecutionOrder(-20)]
    public class GameplaySettingsManager : LocalSingleton<GameplaySettingsManager>
    {
        public enum Setting
        {
            StationPlacementAllAroundIsland,
            ScrollToRotateTile,
        }

        public void Set(Setting setting, bool value)
        {
            switch (setting)
            {
                case Setting.StationPlacementAllAroundIsland:
                    PlayerPrefs.SetInt("GameplaySettings.stationPlacementAllAround", value ? 1 : 0);
                    break;
                case Setting.ScrollToRotateTile:
                    PlayerPrefs.SetInt("GameplaySettings.scrollToRotateTile", value ? 1 : 0);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(setting), setting, null);
            }
        }

        public bool Get(Setting setting)
        {
            return setting switch
            {
                Setting.StationPlacementAllAroundIsland => PlayerPrefs.GetInt("GameplaySettings.stationPlacementAllAround", 1) == 1,
                Setting.ScrollToRotateTile => PlayerPrefs.GetInt("GameplaySettings.scrollToRotateTile", 1) == 1,
                _ => throw new ArgumentOutOfRangeException(nameof(setting), setting, null)
            };
        }
    }
}
