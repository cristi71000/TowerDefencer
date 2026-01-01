using UnityEngine;

namespace TowerDefense.UI
{
    /// <summary>
    /// ScriptableObject containing references to all UI icons used in the game.
    /// This centralizes icon management for easy access and modification.
    /// </summary>
    [CreateAssetMenu(fileName = "UIIconsData", menuName = "TD/UI Icons Data")]
    public class UIIconsData : ScriptableObject
    {
        [Header("HUD Icons")]
        [Tooltip("Currency/gold icon displayed in the economy HUD")]
        public Sprite CurrencyIcon;

        [Tooltip("Lives/hearts icon displayed in the player stats HUD")]
        public Sprite LivesIcon;

        [Tooltip("Wave indicator icon displayed in the wave counter")]
        public Sprite WaveIcon;

        [Header("Button Icons")]
        [Tooltip("Play button icon for wave start")]
        public Sprite PlayIcon;

        [Tooltip("Pause button icon for game pause")]
        public Sprite PauseIcon;

        [Tooltip("Fast forward icon for speed control")]
        public Sprite FastForwardIcon;

        [Tooltip("Settings/gear icon for options menu")]
        public Sprite SettingsIcon;

        [Header("Targeting Priority Icons")]
        [Tooltip("Icon for 'First' targeting priority (closest to exit)")]
        public Sprite TargetFirstIcon;

        [Tooltip("Icon for 'Nearest' targeting priority (closest to tower)")]
        public Sprite TargetNearestIcon;

        [Tooltip("Icon for 'Strongest' targeting priority (highest health)")]
        public Sprite TargetStrongestIcon;

        [Tooltip("Icon for 'Weakest' targeting priority (lowest health)")]
        public Sprite TargetWeakestIcon;

        [Tooltip("Icon for 'Fastest' targeting priority (highest speed)")]
        public Sprite TargetFastestIcon;

        [Header("Health Bar Sprites")]
        [Tooltip("Background sprite for health bars")]
        public Sprite HealthBarBackground;

        [Tooltip("Green fill for high health")]
        public Sprite HealthBarFillGreen;

        [Tooltip("Yellow fill for medium health")]
        public Sprite HealthBarFillYellow;

        [Tooltip("Red fill for low health")]
        public Sprite HealthBarFillRed;

        [Header("Panel Sprites")]
        [Tooltip("Dark semi-transparent panel background")]
        public Sprite PanelDark;

        [Tooltip("Light semi-transparent panel background")]
        public Sprite PanelLight;

        [Tooltip("Normal state button background")]
        public Sprite ButtonNormal;

        [Tooltip("Hover state button background")]
        public Sprite ButtonHover;

        [Tooltip("Pressed state button background")]
        public Sprite ButtonPressed;
    }
}
