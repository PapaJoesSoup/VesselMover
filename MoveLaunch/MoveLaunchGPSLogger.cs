using KSP.UI.Screens;
using System;
using UnityEngine;

namespace VesselMover
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    public class MoveLaunchGPSLogger : MonoBehaviour
    {
        private const float WindowWidth = 220;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static bool GuiEnabledMLGPS;
        public static bool HasAddedButton;
        private readonly float _incrButtonWidth = 26;
        private readonly float contentWidth = WindowWidth - 2 * LeftIndent;
        private readonly float entryHeight = 20;
        private float _contentWidth;
        private bool _gameUiToggle;
        private float _windowHeight = 250;
        private Rect _windowRect;
        public bool guiOpen = false;
        private string GPSname = string.Empty;

        private void Start()
        {
            _windowRect = new Rect(Screen.width - 200 - 140, 100, WindowWidth, _windowHeight);
            GameEvents.onHideUI.Add(GameUiDisableMLGPS);
            GameEvents.onShowUI.Add(GameUiEnableMLGPS);
            AddToolbarButton();
            _gameUiToggle = true;
            GPSname = "Enter Name";
        }

        private void OnGUI()
        {
            if (GuiEnabledMLGPS)
            {
                _windowRect = GUI.Window(627252316, _windowRect, GuiWindowMLGPS, "");
            }
        }

        private void AddToolbarButton()
        {
            string textureDir = "VesselMover/Textures/";

            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture(textureDir + "GPS_icon", false); //texture to use for the button
                ApplicationLauncher.Instance.AddModApplication(EnableGuiMLGPS, DisableGuiMLGPS, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.FLIGHT, buttonTexture);
                HasAddedButton = true;
            }
        }

        private void GetGPS()
        {
            double lat = FlightGlobals.ActiveVessel.latitude;
            double lon = FlightGlobals.ActiveVessel.longitude;
            double alt = FlightGlobals.ActiveVessel.altitude;

            Debug.Log("[Move Launch] GPS Location Name: " + GPSname);
            Debug.Log("[Move Launch] GPS Latitude: " + lat);
            Debug.Log("[Move Launch] GPS Longitude: " + lon);
            Debug.Log("[Move Launch] GPS Altitude: " + alt);

            ScreenMsg("GPS Location Name: " + GPSname);
            ScreenMsg("GPS Latitude: " + lat);
            ScreenMsg("GPS Longitude: " + lon);
            ScreenMsg("GPS Altitude: " + alt);
        }

        #region GUI
        /// <summary>
        /// GUI
        /// </summary>

        private void GuiWindowMLGPS(int ML)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            float line = 0;
            _contentWidth = WindowWidth - 2 * LeftIndent;

            DrawTitle(line);
            line++;
            DrawNameEntry(line);
            line++;
            DrawGetGPS(line);

            _windowHeight = ContentTop + line * entryHeight + entryHeight + (entryHeight / 2);
            _windowRect.height = _windowHeight;
        }

        public void EnableGuiMLGPS()
        {
            guiOpen = true;
            GuiEnabledMLGPS = true;
            Debug.Log("[Move Launch]: Showing GUI");
        }

        public void DisableGuiMLGPS()
        {
            guiOpen = false;
            GuiEnabledMLGPS = false;
            Debug.Log("[Move Launch]: Hiding GUI");
        }

        private void GameUiEnableMLGPS()
        {
            _gameUiToggle = true;
        }

        private void GameUiDisableMLGPS()
        {
            _gameUiToggle = false;
        }

        private void DrawTitle(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "Move Launch GPS Logger", titleStyle);
        }

        private void DrawNameEntry(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Name",
                leftLabel);
            float textFieldWidth = 140;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            GPSname = GUI.TextField(fwdFieldRect, GPSname);
        }

        private void DrawGetGPS(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "LOG GPS COORDS"))
            {
                GetGPS();
            }
        }

        #endregion

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 5, ScreenMessageStyle.UPPER_LEFT));
        }

        private void Dummy()
        {
        }
    }
}
