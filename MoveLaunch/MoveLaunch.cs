using KSP.UI.Screens;
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

namespace VesselMover
{
    [KSPAddon(KSPAddon.Startup.FlightAndEditor, true)]
    public class MoveLaunch : MonoBehaviour
    {
        private const float WindowWidth = 220;
        private const float DraggableHeight = 40;
        private const float LeftIndent = 12;
        private const float ContentTop = 20;
        public static MoveLaunch instance;
        public static bool GuiEnabledML;
        public static bool GuiEnabledMLFlight;
        public static bool HasAddedButton;
        private readonly float _incrButtonWidth = 26;
        private readonly float contentWidth = WindowWidth - 2 * LeftIndent;
        private readonly float entryHeight = 20;
        private float _contentWidth;
        private bool _gameUiToggle;
        private float _windowHeight = 250;
        private float _windowHeight2 = 250;

        private Rect _windowRect;
        private Rect _windowRect2;
        private Vector3 _up;

        public bool guiOpen = false;
        public static bool runway;
        private bool launching = false;

        public string _guiX = String.Empty;
        public string _guiY = String.Empty;
        public string _guiZ = String.Empty;

        public string Name = String.Empty;
        public Guid playerVessel;

        private void Awake()
        {
            DontDestroyOnLoad(this);
            instance = this;
        }

        private void Start()
        {
            _windowRect = new Rect(Screen.width - 200 -140, 200, WindowWidth, _windowHeight);
            _windowRect2 = new Rect(Screen.width - 200 - 140, 200, WindowWidth, _windowHeight);

            GameEvents.onHideUI.Add(DisableGui);
            GameEvents.onShowUI.Add(EnableGui);
            AddToolbarButton();
            _gameUiToggle = true;
            altAdjust = 5;
            _guiX = "0";
            _guiY = "0";
            _guiZ = "0";
        }

        private void OnGUI()
        {
            if (_gameUiToggle)
            {
                if (GuiEnabledML && HighLogic.LoadedSceneIsEditor)
                {
                    _windowRect = GUI.Window(901019256, _windowRect, GuiWindowML, "");
                }

                if (GuiEnabledMLFlight && HighLogic.LoadedSceneIsFlight)
                {
                    GuiEnabledML = false;
                    _windowRect = GUI.Window(670129256, _windowRect2, GuiWindowMLFlight, "");
                }
            }
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsFlight && FlightGlobals.ready)
            {
                if (!FlightGlobals.ActiveVessel.HoldPhysics && !launching)
                {
                    launching = true;
                    StartLaunch();
                }
            }

            if (HighLogic.LoadedSceneIsEditor)
            {
                if (!_gameUiToggle)
                {
                    GameUiEnableML();
                }
            }
        }

        private void AddToolbarButton()
        {
            string textureDir = "VesselMover/Textures/";

            if (!HasAddedButton)
            {
                Texture buttonTexture = GameDatabase.Instance.GetTexture(textureDir + "ML_icon", false); //texture to use for the button
                ApplicationLauncher.Instance.AddModApplication(ToggleGUI, ToggleGUI, Dummy, Dummy, Dummy, Dummy,
                    ApplicationLauncher.AppScenes.SPH | ApplicationLauncher.AppScenes.VAB, buttonTexture);
                HasAddedButton = true;
            }
        }

        private void ScreenMsg(string msg)
        {
            ScreenMessages.PostScreenMessage(new ScreenMessage(msg, 4, ScreenMessageStyle.UPPER_CENTER));
        }

        #region Coords

        private static bool gps;

        private double altitude = 0.0f;
        private double longitude = 0.0f;
        private double latitude = 0.0f;
        public double alt = 1;
        public float altAdjust = 5;

        public double lat = 0;
        public double lon = 0;

        public double latRunway = -0.0485890418349364;
        public double lonRunway = 285.276094692895;
        public static bool launchSiteChanged;

        public static bool islandRunway;
        public double latislandRunway = -1.51590761884814;
        public double lonislandRunway = -71.904406638316;

        public static bool beach;
        public double latBeach = -0.039751;
        public double lonBeach = 285.639486;

        public static bool kscIslandBeach;
        public double latkscIslandBeach = -1.53556797173857;
        public double lonkscIslandBeach = 287.956960620886;

        public static bool baikerbanur;
        public double latBaikerbanur = 20.6635562459104;
        public double lonBaikerbanur = -146.420941786273;

        public static bool pyramids;
        public double latPyramids = -6.49869308429184;
        public double lonPyramids = -141.679184195229;

        public static bool kscHarborEast;
        public double latkscHarborEast = 0.375130657119614;
        public double lonkscHarborEast = -74.8013601760569;

        public static bool kscIslandNewHarbor;
        public double latkscIslandNewHarbor = -1.40773488976674;
        public double lonkscIslandNewHarbor = -71.7228026295601;

        public static bool kscIsandChannel;
        public double latkscIsandChannel = -2.10429648237059;
        public double lonkscIsandChannel = -72.3596956502614;

        public static bool MissileRange200Island;
        public double latMissileRange200Island = 5.90869555576486;
        public double lonMissileRange200Island = -63.1593788751818;

        public static bool TirpitzBay;
        public double latTirpitzBay = 22.845958677363;
        public double lonTirpitzBay = -39.1337875955089;

        public static bool KerbiniAtol;
        public double latKerbiniAtol = 8.71077999875324;
        public double lonKerbiniAtol = -176.41545362997;

        public static bool KerbiniIsland;
        public double latKerbiniIsland = 8.32394058875283;
        public double lonKerbiniIsland = -179.721583163672;

        public static bool NorthPole;
        public double latNorthPole = 1.53556797173857;
        public double lonNorthPole = 287.956960620886;

        public static bool SouthPole;
        public double latSouthPole = -89.9997071680853;
        public double lonSouthPole = -130.854184613173;

        public static bool MidwayIsland;
        public double latMidwayIsland = -89.9997071680853;
        public double lonMidwayIsland = -130.854184613173;

        public static bool TrunkPeninsula;
        public double latTrunkPeninsula = -89.9997071680853;
        public double lonTrunkPeninsula = -130.854184613173;

        #endregion

        #region Launch

        public Vector3d LaunchPosition()
        {
            return FlightGlobals.ActiveVessel.mainBody.GetWorldSurfacePosition((double)latitude, (double)longitude, (double)altitude);
        }

        private void StartLaunch()
        {
            EnableGuiF();

            if (beach)
            {
                LaunchToBeach();
            }

            if (kscIslandBeach)
            {
                LaunchToIslandBeach();
            }

            if (baikerbanur)
            {
                LaunchToBaikerbanur();
            }

            if (pyramids)
            {
                LaunchToPyramids();
            }

            if (kscHarborEast)
            {
                LaunchTokscHarborEast();
            }

            if (kscIsandChannel)
            {
                LaunchTokscIsandChannel();
            }

            if (kscIslandNewHarbor)
            {
                LaunchTokscIslandNewHarbor();
            }

            if (MissileRange200Island)
            {
                LaunchToMissileRange200Island();
            }

            if (TirpitzBay)
            {
                LaunchToTirpitzBay();
            }

            if (KerbiniAtol)
            {
                LaunchToKerbiniAtol();
            }

            if (islandRunway)
            {
                LaunchToIslandRunway();
            }

            if (MidwayIsland)
            {
                LaunchToMidwayIsland();
            }

            if (TrunkPeninsula)
            {
                LaunchToTrunkPeninsula();
            }

            if (NorthPole)
            {
                LaunchToNorthPole();
            }

            if (SouthPole)
            {
                LaunchToSouthPole();
            }

            if (KerbiniIsland)
            {
                LaunchToKerbiniIsland();
            }

            if (gps)
            {
                LaunchToGPS();
            }
        }

        public void ResetLaunch()
        {
            StartCoroutine(ResetToggle());
        }

        IEnumerator ResetToggle()
        {
            yield return new WaitForSeconds(1);
            ResetLaunchCoords();
        }

        public void ResetLaunchCoords()
        {
            gps = false;
            launching = false;
            islandRunway = false;
            TrunkPeninsula = false;
            KerbiniIsland = false;
            MidwayIsland = false;
            NorthPole = false;
            SouthPole = false;
            launchSiteChanged = false;
            kscIsandChannel = false;
            kscHarborEast = false;
            MissileRange200Island = false;
            kscIslandNewHarbor = false;
            TirpitzBay = false;
            KerbiniAtol = false;
            beach = false;
            kscIslandBeach = false;
            baikerbanur = false;
            pyramids = false;
            runway = false;
            lat = 0;
            lon = 0;
        }

        private void LaunchToGPS()
        {
            lat = double.Parse(_guiX);
            lon = double.Parse(_guiY);
            Debug.Log("[Move Launch] LaunchToGPS");
            StartCoroutine(Launch());
        }

        public void LaunchToRunway(double lat, double lon)
        {
            Debug.Log("[Move Launch] LaunchToRunway");
            lat = latRunway;
            lon = lonRunway;
            StartCoroutine(Launch());
        }

        public void LaunchToIslandRunway()
        {
            Debug.Log("[Move Launch] LaunchToIslandRunway");
            lat = latislandRunway;
            lon = lonislandRunway;
            StartCoroutine(Launch());
        }

        public void LaunchToTrunkPeninsula()
        {
            Debug.Log("[Move Launch] LaunchToTrunkPeninsula");
            lat = latTrunkPeninsula;
            lon = lonTrunkPeninsula;
            StartCoroutine(Launch());
        }

        public void LaunchToMidwayIsland()
        {
            Debug.Log("[Move Launch] LaunchToRunway");
            lat = latMidwayIsland;
            lon = lonMidwayIsland;
            StartCoroutine(Launch());
        }

        public void LaunchToSouthPole()
        {
            Debug.Log("[Move Launch] LaunchToSouthPole");
            lat = latSouthPole;
            lon = lonSouthPole;
            StartCoroutine(Launch());
        }

        public void LaunchToNorthPole()
        {
            Debug.Log("[Move Launch] LaunchToNorthPole");
            lat = latNorthPole;
            lon = lonNorthPole;
            StartCoroutine(Launch());
        }

        public void LaunchToKerbiniIsland()
        {
            Debug.Log("[Move Launch] LaunchToKerbiniIsland");
            lat = latKerbiniIsland;
            lon = lonKerbiniIsland;
            StartCoroutine(Launch());
        }

        public void LaunchToBeach()
        {
            Debug.Log("[Move Launch] LaunchToBeach");
            lat = latBeach;
            lon = lonBeach;
            StartCoroutine(Launch());
        }

        public void LaunchToIslandBeach()
        {
            Debug.Log("[Move Launch] LaunchToIslandBeach");
            lat = latkscIslandBeach;
            lon = lonkscIslandBeach;
            StartCoroutine(Launch());
        }

        public void LaunchToBaikerbanur()
        {
            Debug.Log("[Move Launch] LaunchToBaikerbanur");
            lat = latBaikerbanur;
            lon = lonBaikerbanur;
            StartCoroutine(Launch());
        }

        public void LaunchToPyramids()
        {
            Debug.Log("[Move Launch] LaunchToPyramids");
            lat = latPyramids;
            lon = lonPyramids;
            StartCoroutine(Launch());
        }

        public void LaunchTokscHarborEast()
        {
            Debug.Log("[Move Launch] LaunchTokscHarborEast");
            lat = latkscHarborEast;
            lon = lonkscHarborEast;
            StartCoroutine(Launch());
        }

        public void LaunchTokscIslandNewHarbor()
        {
            Debug.Log("[Move Launch] LaunchTokscIslandNewHarbor");
            lat = latkscIslandNewHarbor;
            lon = lonkscIslandNewHarbor;
            StartCoroutine(Launch());
        }

        public void LaunchTokscIsandChannel()
        {
            Debug.Log("[Move Launch] LaunchTokscIsandChannel");
            lat = latkscIsandChannel;
            lon = lonkscIsandChannel;
            StartCoroutine(Launch());
        }

        public void LaunchToMissileRange200Island()
        {
            Debug.Log("[Move Launch] LaunchToMissileRange200Island");
            lat = latMissileRange200Island;
            lon = lonMissileRange200Island;
            StartCoroutine(Launch());
        }

        public void LaunchToTirpitzBay()
        {
            Debug.Log("[Move Launch] LaunchToTirpitzBay");
            lat = latTirpitzBay;
            lon = lonTirpitzBay;
            StartCoroutine(Launch());
        }

        public void LaunchToKerbiniAtol()
        {
            Debug.Log("[Move Launch] LaunchToKerbiniAtol");
            lat = latKerbiniAtol;
            lon = lonKerbiniAtol;
            StartCoroutine(Launch());
        }

        IEnumerator Launch()
        {
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().isKinematic = true;

            List<Part>.Enumerator p = FlightGlobals.ActiveVessel.parts.GetEnumerator();
            while (p.MoveNext())
            {
                if (p.Current == null) continue;
                p.Current.AddModule("MoveLaunchMassModifier", true);
            }
            p.Dispose();

            latitude = FlightGlobals.ActiveVessel.latitude;
            longitude = FlightGlobals.ActiveVessel.longitude;
            altitude = 65000;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            FlightGlobals.ActiveVessel.SetPosition(LaunchPosition(), true);
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity = Vector3.zero;
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            yield return new WaitForFixedUpdate();

            latitude = lat / 2 * -1;
            longitude = lon / 2 * -1;
            altitude = 65000;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            FlightGlobals.ActiveVessel.SetPosition(LaunchPosition(), true);
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity = Vector3.zero;
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            yield return new WaitForFixedUpdate();

            latitude = 0;
            longitude = 0;
            altitude = 65000;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            FlightGlobals.ActiveVessel.SetPosition(LaunchPosition(), true);
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity = Vector3.zero;
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            yield return new WaitForFixedUpdate();

            latitude = lat / 2;
            longitude = lon / 2;
            altitude = 65000;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            FlightGlobals.ActiveVessel.SetPosition(LaunchPosition(), true);
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity = Vector3.zero;
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            yield return new WaitForFixedUpdate();

            latitude = lat;
            longitude = lon;
            altitude = altAdjust;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            FlightGlobals.ActiveVessel.SetPosition(LaunchPosition(), true);
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().velocity = Vector3.zero;
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().isKinematic = false;
            yield return new WaitForFixedUpdate();

            StartCoroutine(ResetToggle());
            VesselMove.Instance.StartMove(FlightGlobals.ActiveVessel, true);
        }

        public void DropVessel()
        {
            GameUiDisableML();


            Debug.Log("[Move Launch] Removing Mass Modifier Module .......");

            foreach (Part p in FlightGlobals.ActiveVessel.parts)
            {
                var mass = p.FindModuleImplementing<MoveLaunchMassModifier>();
                if (mass != null)
                {
                    mass.modify = false;
                }
            }

            VesselMove.Instance.DropMove();
        }

        public void PlaceVessel()
        {
            GameUiDisableML();

            var altitude_ = FlightGlobals.ActiveVessel.altitude - FlightGlobals.ActiveVessel.radarAltitude;
            if (altitude_ >= 0)
            {
                altitude = altitude_ + altAdjust;
            }
            else
            {
                altitude = altAdjust;
            }
            VesselMove.Instance.EndMove();
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().isKinematic = true;
            FlightGlobals.ActiveVessel.geeForce = 0;
            FlightGlobals.ActiveVessel.geeForce_immediate = 0;
            latitude = FlightGlobals.ActiveVessel.latitude;
            longitude = FlightGlobals.ActiveVessel.longitude;
            FlightGlobals.ActiveVessel.SetPosition(LaunchPosition(), true);
            FlightGlobals.ActiveVessel.GetComponent<Rigidbody>().isKinematic = false;

            Debug.Log("[Move Launch] Removing Mass Modifier Module .......");

            foreach (Part p in FlightGlobals.ActiveVessel.parts)
            {
                var mass = p.FindModuleImplementing<MoveLaunchMassModifier>();
                if (mass != null)
                {
                    mass.modify = false;
                }
            }
        }

        #endregion

        #region GUI

        private void GuiWindowMLFlight(int MLFlight)
        {
            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            float line = 0;
            _contentWidth = WindowWidth - 2 * LeftIndent;

            DrawTitle(line);
            line++;
            DrawAltitudeText(line);
            line++;
            DrawAltitudeSlider(line);
            line++;
            line++;
            DrawPlaceVessel(line);
            line++;
            DrawDropVessel(line);

            _windowHeight2 = ContentTop + line * entryHeight + entryHeight + (entryHeight / 2);
            _windowRect2.height = _windowHeight2;

        }

        private void GuiWindowML(int ML)
        {

            GUI.DragWindow(new Rect(0, 0, WindowWidth, DraggableHeight));
            float line = 0;
            _contentWidth = WindowWidth - 2 * LeftIndent;

            DrawTitle(line);
            line++;
            DrawIslandRunway(line);
            line++;
            DrawIslandBeach(line);
            line++;
            DrawkscHarborEast(line);
            line++;
            DrawkscIsandChannel(line);
            line++;
            DrawkscIslandNewHarbor(line);
            line++;
            DrawTirpitzBay(line);
            line++;
            DrawKerbiniAtol(line);
            line++;
            DrawKerbiniIsland(line);
            line++;
            DrawMissileRange200Island(line);
            line++;
            DrawBaikerbanur(line);
            line++;
            DrawPyramids(line);
            line++;
            DrawNorthPole(line);
            line++;
            DrawSouthPole(line);
            //            DrawMidwayIsland(line);
            //            line++;
            //            DrawTrunkPeninsula(line);
            //            line++;
            line++;
            line++;
            DrawText(line);
            line++;
            DrawX(line);
            line++;
            DrawY(line);
            line++;
            DrawZ(line);
            line++;
            DrawLaunchToGPS(line);


            _windowHeight = ContentTop + line * entryHeight + entryHeight + (entryHeight / 2);
            _windowRect.height = _windowHeight;
        }
        
        public void ToggleGUI()
        {
            if (HighLogic.LoadedSceneIsEditor)
            {
                if (GuiEnabledML)
                {
                    DisableGui();
                }
                else
                {
                    EnableGui();
                }
            }

            if (HighLogic.LoadedSceneIsFlight)
            {
                if (GuiEnabledMLFlight)
                {
                    DisableGuiF();
                }
                else
                {
                    EnableGuiF();
                }
            }
        }

        public void EnableGui()
        {
            ResetLaunchCoords();
            _gameUiToggle = true;
            GuiEnabledMLFlight = false;
            GuiEnabledML = true;
            guiOpen = true;
            Debug.Log("[Move Launch Controller]: Showing Editor GUI");
        }

        public void DisableGui()
        {
            guiOpen = false;
            GuiEnabledML = false;
            Debug.Log("[Move Launch Controller]: Hiding Editor GUI");
        }

        public void EnableGuiF()
        {
            _gameUiToggle = true;
            GuiEnabledMLFlight = true;
            GuiEnabledML = false;
            guiOpen = true;
            Debug.Log("[Move Launch Controller]: Showing Flight GUI");
        }

        public void DisableGuiF()
        {
            guiOpen = false;
            GuiEnabledMLFlight = false;
            Debug.Log("[Move Launch Controller]: Hiding Flight GUI");
        }

        private void GameUiEnableML()
        {
            _gameUiToggle = true;
        }

        private void GameUiDisableML()
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
            GUI.Label(new Rect(0, 0, WindowWidth, 20), "Move Launch", titleStyle);
        }

        private void DrawBeach(float line)
        {
            GUIStyle guardStyle = beach ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!beach)
            {
                if (GUI.Button(saveRect, "KSC Beach", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        beach = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "KSC Beach", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawkscHarborEast(float line)
        {
            GUIStyle guardStyle = kscHarborEast ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!kscHarborEast)
            {
                if (GUI.Button(saveRect, "KSC Harbor East", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        kscHarborEast = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "KSC Harbor East", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawkscIslandNewHarbor(float line)
        {
            GUIStyle guardStyle = kscHarborEast ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!kscIslandNewHarbor)
            {
                if (GUI.Button(saveRect, "KSC Island Harbor", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        kscIslandNewHarbor = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "KSC Island Harbor", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawkscIsandChannel(float line)
        {
            GUIStyle guardStyle = kscIsandChannel ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!kscIsandChannel)
            {
                if (GUI.Button(saveRect, "KSC Island Channel", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        kscIsandChannel = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "KSC Island Channel", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawMissileRange200Island(float line)
        {
            GUIStyle guardStyle = MissileRange200Island ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!MissileRange200Island)
            {
                if (GUI.Button(saveRect, "Missile Range 200", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        MissileRange200Island = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Missile Range 200", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawTirpitzBay(float line)
        {
            GUIStyle guardStyle = TirpitzBay ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!TirpitzBay)
            {
                if (GUI.Button(saveRect, "Tirpitz Bay", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        TirpitzBay = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Tirpitz Bay", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawKerbiniAtol(float line)
        {
            GUIStyle guardStyle = KerbiniAtol ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!KerbiniAtol)
            {
                if (GUI.Button(saveRect, "Kerbini Atol", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        KerbiniAtol = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Kerbini Atol", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawIslandBeach(float line)
        {
            GUIStyle guardStyle = kscIslandBeach ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!kscIslandBeach)
            {
                if (GUI.Button(saveRect, "KSC Island Beach", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        kscIslandBeach = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "KSC Island Beach", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawBaikerbanur(float line)
        {
            GUIStyle guardStyle = baikerbanur ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!baikerbanur)
            {
                if (GUI.Button(saveRect, "Baikerbanur", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        baikerbanur = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Baikerbanur", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawPyramids(float line)
        {
            GUIStyle guardStyle = pyramids ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!pyramids)
            {
                if (GUI.Button(saveRect, "Pyramids", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        pyramids = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Pyramids", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawKerbiniIsland(float line)
        {
            GUIStyle guardStyle = KerbiniIsland ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!KerbiniIsland)
            {
                if (GUI.Button(saveRect, "Kerbini Island", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        KerbiniIsland = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Kerbini Island", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawTrunkPeninsula(float line)
        {
            GUIStyle guardStyle = TrunkPeninsula ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!TrunkPeninsula)
            {
                if (GUI.Button(saveRect, "Trunk Peninsula", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        TrunkPeninsula = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Trunk Peninsula", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawMidwayIsland(float line)
        {
            GUIStyle guardStyle = MidwayIsland ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!MidwayIsland)
            {
                if (GUI.Button(saveRect, "Midway Island", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        MidwayIsland = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Midway Island", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawNorthPole(float line)
        {
            GUIStyle guardStyle = NorthPole ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!NorthPole)
            {
                if (GUI.Button(saveRect, "North Pole", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        NorthPole = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "North Pole", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawSouthPole(float line)
        {
            GUIStyle guardStyle = SouthPole ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!SouthPole)
            {
                if (GUI.Button(saveRect, "South Pole", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        SouthPole = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "South Pole", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawIslandRunway(float line)
        {
            GUIStyle guardStyle = islandRunway ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!islandRunway)
            {
                if (GUI.Button(saveRect, "KSC Island Runway", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        islandRunway = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "KSC Island Runway", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        private void DrawAltitudeText(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, ContentTop + line * entryHeight, WindowWidth, 18),
                "ATTITUDE ADJUSTMENT",
                titleStyle);
        }

        private void DrawAltitudeSlider(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            GUI.Label(new Rect(10, ContentTop + line * entryHeight, contentWidth * 0.9f, 20), "0");
            GUI.Label(new Rect(95, ContentTop + line * entryHeight, contentWidth * 0.9f, 20), "|");
            GUI.Label(new Rect(178, ContentTop + line * entryHeight, contentWidth * 0.9f, 20), "20");
            altAdjust = GUI.HorizontalSlider(saveRect, altAdjust, 0, 20);
        }

        private void DrawPlaceVessel(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Place Vessel"))
            {
                PlaceVessel();
                DisableGuiF();
            }
        }

        private void DrawDropVessel(float line)
        {
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);
            if (GUI.Button(saveRect, "Drop Vessel"))
            {
                DropVessel();
                DisableGuiF();
            }
        }

        private void DrawText(float line)
        {
            var centerLabel = new GUIStyle
            {
                alignment = TextAnchor.UpperCenter,
                normal = { textColor = Color.white }
            };
            var titleStyle = new GUIStyle(centerLabel)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            GUI.Label(new Rect(0, ContentTop + line * entryHeight, WindowWidth, 20),
                "Enter GPS Coords Below",
                titleStyle);
        }

        private void DrawX(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Latitude",
                leftLabel);
            float textFieldWidth = 80;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiX = GUI.TextField(fwdFieldRect, _guiX);
        }

        private void DrawY(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Longitude",
                leftLabel);
            float textFieldWidth = 80;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiY = GUI.TextField(fwdFieldRect, _guiY);
        }

        private void DrawZ(float line)
        {
            var leftLabel = new GUIStyle();
            leftLabel.alignment = TextAnchor.UpperLeft;
            leftLabel.normal.textColor = Color.white;

            GUI.Label(new Rect(LeftIndent, ContentTop + line * entryHeight, 60, entryHeight), "Altitude",
                leftLabel);
            float textFieldWidth = 80;
            var fwdFieldRect = new Rect(LeftIndent + contentWidth - textFieldWidth,
                ContentTop + line * entryHeight, textFieldWidth, entryHeight);
            _guiZ = GUI.TextField(fwdFieldRect, _guiZ);
        }

        private void DrawLaunchToGPS(float line)
        {
            GUIStyle guardStyle = islandRunway ? HighLogic.Skin.box : HighLogic.Skin.button;
            var saveRect = new Rect(LeftIndent * 1.5f, ContentTop + line * entryHeight, contentWidth * 0.9f, entryHeight);

            if (!gps)
            {
                if (GUI.Button(saveRect, "Launch to GPS", guardStyle))
                {
                    if (!launchSiteChanged)
                    {
                        launchSiteChanged = true;
                        gps = true;
                        EditorLogic.fetch.launchVessel();
                    }
                }
            }
            else
            {
                if (GUI.Button(saveRect, "Launch to GPS", guardStyle))
                {
                    ResetLaunchCoords();
                }
            }
        }

        #endregion

        private void Dummy()
        {
        }
    }
}