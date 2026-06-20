using System;
using System.IO;
using UnityEngine;

namespace PEAK.Cheat
{
    [Serializable]
    public class RgbaColor
    {
        public float R;
        public float G;
        public float B;
        public float A;

        public RgbaColor(float r, float g, float b, float a) { R = r; G = g; B = b; A = a; }
        
        public Color ToUnityColor() => new Color(R / 255f, G / 255f, B / 255f, A / 255f);
    }

    [Serializable]
    public class WallhackConfig
    {
        public bool EnablePlayers = false;
        public bool EnablePlayerDistance = false;
        public bool EnablePlayerTracers = false;
        public KeyCode EnablePlayersKey = KeyCode.None;
        public KeyCode EnablePlayerDistanceKey = KeyCode.None;
        public KeyCode EnablePlayerTracersKey = KeyCode.None;
        public RgbaColor PlayerColor = new RgbaColor(0, 150, 255, 200);

        public bool EnableMonsters = false;
        public bool EnableMonsterDistance = false;
        public bool EnableMonsterTracers = false;
        public KeyCode EnableMonstersKey = KeyCode.None;
        public KeyCode EnableMonsterDistanceKey = KeyCode.None;
        public KeyCode EnableMonsterTracersKey = KeyCode.None;
        public RgbaColor MonsterColor = new RgbaColor(200, 0, 50, 200);
        public float MonsterMaxDistance = 500.0f;

        public bool EnableLootBoxes = false;
        public bool EnableLootBoxDistance = false;
        public bool EnableLootBoxTracers = false;
        public KeyCode EnableLootBoxesKey = KeyCode.None;
        public KeyCode EnableLootBoxDistanceKey = KeyCode.None;
        public KeyCode EnableLootBoxTracersKey = KeyCode.None;
        public RgbaColor LootBoxColor = new RgbaColor(255, 140, 0, 200);

        public bool EnableFood = false;
        public bool EnableFoodDistance = false;
        public bool EnableFoodTracers = false;
        public KeyCode EnableFoodKey = KeyCode.None;
        public KeyCode EnableFoodDistanceKey = KeyCode.None;
        public KeyCode EnableFoodTracersKey = KeyCode.None;
        public RgbaColor FoodEdibleColor = new RgbaColor(50, 205, 50, 200);
        public RgbaColor FoodPoisonousColor = new RgbaColor(255, 110, 180, 200);

        public bool EnableCampfires = false;
        public bool EnableCampfireDistance = false;
        public bool EnableCampfireTracers = false;
        public KeyCode EnableCampfiresKey = KeyCode.None;
        public KeyCode EnableCampfireDistanceKey = KeyCode.None;
        public KeyCode EnableCampfireTracersKey = KeyCode.None;
        public RgbaColor CampfireColor = new RgbaColor(255, 255, 255, 200);
        public KeyCode ToggleEspKey = KeyCode.None;
        public KeyCode InfiniteStaminaKey = KeyCode.None;
        public KeyCode GodModeKey = KeyCode.None;
        public KeyCode NoAfflictionsKey = KeyCode.None;
        public KeyCode FlyModeKey = KeyCode.None;
        public KeyCode NoClipKey = KeyCode.None;
        public KeyCode SpeedBoostKey = KeyCode.None;
        public KeyCode JumpBoostKey = KeyCode.None;
        public EspItemConfigEntry[] ItemProfiles = Array.Empty<EspItemConfigEntry>();
    }

    [Serializable]
    public class EspItemConfigEntry
    {
        public string Key = string.Empty;
        public string Category = string.Empty;
        public string DisplayName = string.Empty;
        public string ColorCode = string.Empty;
        public bool Outline = true;
        public bool Box = true;
        public bool Skeleton = false;
    }

    public static class ConfigManager
    {
        private static WallhackConfig _currentConfig = new WallhackConfig();
        private static string _configPath;
        private static DateTime _lastRead = DateTime.MinValue;

        public static WallhackConfig Config => _currentConfig;
        public static string ConfigPath => _configPath;
        public static string ConfigDirectoryPath => string.IsNullOrEmpty(_configPath) ? null : Path.GetDirectoryName(_configPath);

        public static void Initialize()
        {
            _configPath = Path.Combine(Application.dataPath, "..", "Config", "Wallhack.json");
            
            var dir = Path.GetDirectoryName(_configPath);
            if (dir != null && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            if (!File.Exists(_configPath))
            {
                SaveDefaultConfig();
            }

            LoadConfig();
        }

        public static void Update()
        {
            if (string.IsNullOrEmpty(_configPath)) return;

            if (File.Exists(_configPath))
            {
                var lastWrite = File.GetLastWriteTime(_configPath);
                if (lastWrite > _lastRead)
                {
                    LoadConfig();
                }
            }
        }

        public static void Save()
        {
            try
            {
                if (string.IsNullOrEmpty(_configPath))
                {
                    Initialize();
                }

                string json = JsonUtility.ToJson(_currentConfig, true);
                File.WriteAllText(_configPath, json);
                _lastRead = File.GetLastWriteTime(_configPath);
            }
            catch (Exception ex)
            {
                Debug.Log($"[Wallhack] Error saving config: {ex.Message}");
            }
        }

        public static void Reload()
        {
            if (string.IsNullOrEmpty(_configPath))
            {
                Initialize();
                return;
            }

            LoadConfig();
        }

        public static void ClearSavedConfig()
        {
            try
            {
                if (string.IsNullOrEmpty(_configPath))
                {
                    Initialize();
                }

                _currentConfig = CreateDefaultConfig();
                NormalizeConfig(_currentConfig);
                string json = JsonUtility.ToJson(_currentConfig, true);
                File.WriteAllText(_configPath, json);
                _lastRead = File.GetLastWriteTime(_configPath);
            }
            catch (Exception ex)
            {
                Debug.Log($"[Wallhack] Error clearing config: {ex.Message}");
            }
        }

        private static void LoadConfig()
        {
            try
            {
                string json = File.ReadAllText(_configPath);
                var newConfig = JsonUtility.FromJson<WallhackConfig>(json);
                if (newConfig != null)
                {
                    NormalizeConfig(newConfig);
                    _currentConfig = newConfig;
                    _lastRead = File.GetLastWriteTime(_configPath);
                }
            }
            catch (Exception ex)
            {
                Debug.Log($"[Wallhack] Error loading config: {ex.Message}");
            }
        }

        private static void SaveDefaultConfig()
        {
            try
            {
                string json = JsonUtility.ToJson(CreateDefaultConfig(), true);
                File.WriteAllText(_configPath, json);
            }
            catch (Exception ex)
            {
                Debug.Log($"[Wallhack] Error saving default config: {ex.Message}");
            }
        }

        private static WallhackConfig CreateDefaultConfig()
        {
            return new WallhackConfig();
        }

        private static void NormalizeConfig(WallhackConfig config)
        {
            if (config.ItemProfiles == null)
            {
                config.ItemProfiles = Array.Empty<EspItemConfigEntry>();
            }

            if (config.PlayerColor == null) config.PlayerColor = new RgbaColor(0, 150, 255, 200);
            if (config.MonsterColor == null) config.MonsterColor = new RgbaColor(200, 0, 50, 200);
            if (config.LootBoxColor == null) config.LootBoxColor = new RgbaColor(255, 140, 0, 200);
            if (config.FoodEdibleColor == null) config.FoodEdibleColor = new RgbaColor(50, 205, 50, 200);
            if (config.FoodPoisonousColor == null) config.FoodPoisonousColor = new RgbaColor(255, 110, 180, 200);
            if (config.CampfireColor == null) config.CampfireColor = new RgbaColor(255, 255, 255, 200);
        }
    }
}
