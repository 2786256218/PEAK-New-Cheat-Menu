using System;
using UnityEngine;
using PEAK.Cheat.GameData;

namespace PEAK.Cheat.Features
{
    public class ESP
    {
        private GUIStyle _labelStyle;
        private Texture2D _whiteTexture;

        public ESP()
        {
            // Initialization moved to Render to ensure it happens on the main thread during OnGUI
        }

        public void Render()
        {
            if (_labelStyle == null)
            {
                _labelStyle = new GUIStyle();
                _labelStyle.fontSize = 15;
                _labelStyle.fontStyle = FontStyle.Bold;
                _labelStyle.alignment = TextAnchor.MiddleCenter;
            }
            if (_whiteTexture == null)
            {
                _whiteTexture = new Texture2D(1, 1);
                _whiteTexture.SetPixel(0, 0, Color.white);
                _whiteTexture.Apply();
            }

            var config = ConfigManager.Config;
            var entities = MemoryReader.GetEntities();
            var localPos = MemoryReader.LocalPlayerPosition;

            Camera cam = Camera.main;
            if (cam == null) return;
            if (localPos == Vector3.zero)
            {
                localPos = cam.transform.position;
            }

            foreach (var entity in entities)
            {
                Vector3 screenPos3D = cam.WorldToScreenPoint(entity.Position);
                bool isOffScreen = screenPos3D.z < 0 || screenPos3D.x < 0 || screenPos3D.x > Screen.width || screenPos3D.y < 0 || screenPos3D.y > Screen.height;

                // Unity's screen origin is bottom-left. GUI origin is top-left.
                Vector2 screenPos = new Vector2(screenPos3D.x, Screen.height - screenPos3D.y);

                if (isOffScreen)
                {
                    DrawOffScreenArrow(entity, localPos, config, cam);
                    continue;
                }

                float distance = Vector3.Distance(localPos, entity.Position);

                switch (entity.Type)
                {
                    case EntityType.Player:
                        if (config.EnablePlayers)
                            DrawPlayer(entity, screenPos, distance, config);
                        break;
                    case EntityType.Monster:
                        if (config.EnableMonsters)
                            DrawMonster(entity, screenPos, distance, config);
                        break;
                    case EntityType.LootBox:
                        if (config.EnableLootBoxes)
                            DrawLootBox(entity, screenPos, distance, config);
                        break;
                    case EntityType.Food:
                        if (config.EnableFood)
                            DrawFood(entity, screenPos, distance, config);
                        break;
                    case EntityType.Campfire:
                        if (config.EnableCampfires)
                            DrawCampfire(entity, screenPos, distance, config);
                        break;
                }
            }
        }

        private void DrawRect(Rect rect, Color color, float thickness = 1.5f)
        {
            GUI.color = color;
            GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, thickness), _whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y + rect.height - thickness, rect.width, thickness), _whiteTexture);
            GUI.DrawTexture(new Rect(rect.x, rect.y, thickness, rect.height), _whiteTexture);
            GUI.DrawTexture(new Rect(rect.x + rect.width - thickness, rect.y, thickness, rect.height), _whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawFilledRect(Rect rect, Color color)
        {
            GUI.color = color;
            GUI.DrawTexture(rect, _whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawPlayer(GameEntity player, Vector2 screenPos, float distance, WallhackConfig config)
        {
            Color color = config.PlayerColor.ToUnityColor();
            Rect rect = new Rect(screenPos.x - 20, screenPos.y - 40, 40, 80);

            DrawRect(rect, color, 1.5f);

            if (config.EnablePlayerTracers)
            {
                DrawLine(new Vector2(Screen.width / 2f, Screen.height - 18f), new Vector2(screenPos.x, rect.yMax), color, 1.25f);
            }

            DrawTextWithOutline(player.Name, new Vector2(screenPos.x, screenPos.y - 50), Color.white);

            if (config.EnablePlayerDistance)
            {
                DrawTextWithOutline(string.Format("{0:F0}m", distance), new Vector2(screenPos.x, screenPos.y - 32), color);
            }

            DrawHpBar(player, new Vector2(screenPos.x, rect.yMax + 4));
        }

        private void DrawMonster(GameEntity monster, Vector2 screenPos, float distance, WallhackConfig config)
        {
            if (distance > config.MonsterMaxDistance) return;
            if (!monster.IsHostile) return;
            if (monster.IsEnvironmentDamage()) return;

            Color color = config.MonsterColor.ToUnityColor();
            string text = config.EnableMonsterDistance ? $"[敌对] {monster.Name} [{distance:F0}m]" : $"[敌对] {monster.Name}";

            Rect rect = new Rect(screenPos.x - 18, screenPos.y - 36, 36, 72);
            DrawRect(rect, color, 1.25f);

            if (config.EnableMonsterTracers)
            {
                DrawLine(new Vector2(Screen.width / 2f, Screen.height - 18f), new Vector2(screenPos.x, rect.yMax), color, 1.25f);
            }

            DrawTextWithOutline(text, new Vector2(screenPos.x, screenPos.y - 18), color);
        }

        private void DrawLootBox(GameEntity box, Vector2 screenPos, float distance, WallhackConfig config)
        {
            Color color = config.LootBoxColor.ToUnityColor();
            Rect rect = new Rect(screenPos.x - 12, screenPos.y - 12, 24, 24);
            DrawRect(rect, color, 2.0f);

            if (config.EnableLootBoxTracers)
            {
                DrawLine(new Vector2(Screen.width / 2f, Screen.height - 18f), new Vector2(screenPos.x, rect.yMax), color, 1.25f);
            }
            string label = string.IsNullOrEmpty(box.Name) ? "物资箱" : box.Name;
            if (config.EnableLootBoxDistance)
            {
                label = string.Format("{0} [{1:F0}m]", label, distance);
            }
            DrawTextWithOutline(label, new Vector2(screenPos.x, screenPos.y - 28), color);
        }

        private void DrawFood(GameEntity food, Vector2 screenPos, float distance, WallhackConfig config)
        {
            bool isPoisonous = food.IsPoisonous();
            Color fallbackColor = isPoisonous ? config.FoodPoisonousColor.ToUnityColor() : config.FoodEdibleColor.ToUnityColor();
            Color color;
            if (!EspItemRegistry.TryParseColorCode(food.ColorCode, fallbackColor, out color))
            {
                color = fallbackColor;
            }

            string displayName = string.IsNullOrEmpty(food.DisplayName) ? food.Name : food.DisplayName;
            string text = isPoisonous && displayName.IndexOf("有毒", StringComparison.Ordinal) < 0 ? $"有毒 {displayName}" : displayName;
            if (config.EnableFoodDistance)
            {
                text = string.Format("{0} [{1:F0}m]", text, distance);
            }

            Rect rect = new Rect(screenPos.x - 9, screenPos.y - 9, 18, 18);
            if (food.DrawBox)
            {
                DrawRect(rect, color, 1.5f);
            }

            if (config.EnableFoodTracers)
            {
                DrawLine(new Vector2(Screen.width / 2f, Screen.height - 18f), new Vector2(screenPos.x, rect.yMax), color, 1.25f);
            }

            DrawTextWithOutline(text, new Vector2(screenPos.x, screenPos.y - 18), color);
        }

        private void DrawCampfire(GameEntity campfire, Vector2 screenPos, float distance, WallhackConfig config)
        {
            Color color = config.CampfireColor.ToUnityColor();
            Rect rect = new Rect(screenPos.x - 23, screenPos.y - 23, 46, 46);
            DrawRect(rect, color, 2.0f);

            if (config.EnableCampfireTracers)
            {
                DrawLine(new Vector2(Screen.width / 2f, Screen.height - 18f), new Vector2(screenPos.x, rect.yMax), color, 1.25f);
            }
            string label = string.IsNullOrEmpty(campfire.Name) ? "营火存档点" : campfire.Name;
            if (config.EnableCampfireDistance)
            {
                label = string.Format("{0} [{1:F0}m]", label, distance);
            }
            DrawTextWithOutline(label, new Vector2(screenPos.x, screenPos.y - 38), Color.white);
        }

        private void DrawHpBar(GameEntity entity, Vector2 position)
        {
            if (entity.MaxHealth <= 0) return;
            float hpPercent = Mathf.Clamp01((float)entity.Health / entity.MaxHealth);

            float width = 60;
            float height = 6;
            Rect bgRect = new Rect(position.x - width / 2, position.y, width, height);
            DrawFilledRect(bgRect, new Color(0, 0, 0, 150f / 255f));

            Color fgColor;
            if (hpPercent <= 0.10f) fgColor = new Color(1, 0, 0, 200f / 255f);
            else if (hpPercent <= 0.30f) fgColor = new Color(1, 1, 0, 200f / 255f);
            else fgColor = new Color(0, 1, 0, 200f / 255f);

            Rect fgRect = new Rect(bgRect.x, bgRect.y, width * hpPercent, height);
            DrawFilledRect(fgRect, fgColor);

            string hpText = $"{entity.Health}/{entity.MaxHealth}";
            DrawTextWithOutline(hpText, new Vector2(position.x, position.y + height + 8), Color.white);
        }

        private void DrawTextWithOutline(string text, Vector2 pos, Color color)
        {
            _labelStyle.normal.textColor = Color.black;
            Rect rect = new Rect(pos.x - 110, pos.y - 12, 220, 26);
            
            // Outline
            GUI.Label(new Rect(rect.x - 1, rect.y - 1, rect.width, rect.height), text, _labelStyle);
            GUI.Label(new Rect(rect.x + 1, rect.y - 1, rect.width, rect.height), text, _labelStyle);
            GUI.Label(new Rect(rect.x - 1, rect.y + 1, rect.width, rect.height), text, _labelStyle);
            GUI.Label(new Rect(rect.x + 1, rect.y + 1, rect.width, rect.height), text, _labelStyle);

            _labelStyle.normal.textColor = color;
            GUI.Label(rect, text, _labelStyle);
        }

        private void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        {
            Vector2 delta = end - start;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;
            float length = delta.magnitude;
            Matrix4x4 oldMatrix = GUI.matrix;
            Color oldColor = GUI.color;

            GUI.color = color;
            GUIUtility.RotateAroundPivot(angle, start);
            GUI.DrawTexture(new Rect(start.x, start.y, length, thickness), _whiteTexture);

            GUI.matrix = oldMatrix;
            GUI.color = oldColor;
        }

        private void DrawOffScreenArrow(GameEntity entity, Vector3 localPos, WallhackConfig config, Camera cam)
        {
            // Simple offscreen indicator logic can be implemented here if needed.
        }
    }
}
