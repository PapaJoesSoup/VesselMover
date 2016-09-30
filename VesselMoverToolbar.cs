using System;
using System.Collections;
using UnityEngine;
using KSP.UI.Screens;

namespace VesselMover
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class VesselMoverToolbar : MonoBehaviour
	{
		public static bool hasAddedButton = false;

		public static bool toolbarGuiEnabled = false;

		Rect toolbarRect;
		float toolbarWidth = 280;
		float toolbarHeight = 0;
		float toolbarMargin = 6;
		float toolbarLineHeight = 20;
		float contentWidth;
		Vector2 toolbarPosition;

		Rect svRectScreenSpace;

		bool showMoveHelp = false;
		float helpHeight;

		void Start()
		{
			toolbarPosition = new Vector2(Screen.width - toolbarWidth - 80, 39);
			toolbarRect = new Rect(toolbarPosition.x, toolbarPosition.y, toolbarWidth, toolbarHeight);
			contentWidth = toolbarWidth - (2 * toolbarMargin);

			AddToolbarButton();
		}

		void OnGUI()
		{
			if(toolbarGuiEnabled && VesselMove.instance && VesselSpawn.instance && !VesselSpawn.instance.openingCraftBrowser)
			{
				GUI.Window(401240, toolbarRect, ToolbarWindow, "Vessel Mover", HighLogic.Skin.window);

				if(!VesselMove.instance.isMovingVessel)
				{
					if(MouseIsInRect(svRectScreenSpace))
					{
						Vector2 mousePos = MouseGUIPos();
						Rect warningRect = new Rect(mousePos.x + 5, mousePos.y + 20, 200, 60);
						GUI.Label(warningRect, "WARNING: Experimental. Launch clamps may be broken.", HighLogic.Skin.box);
					}
				}
				else if(showMoveHelp)
				{
					GUI.Window(401241, new Rect(toolbarRect.x, toolbarRect.y + toolbarRect.height, toolbarRect.width, helpHeight), MoveHelp, "Controls", HighLogic.Skin.window);
				}
			}
		}

		void ToolbarWindow(int windowID)
		{
			float line = 0;
			line += 1.25f;

			if(FlightGlobals.ActiveVessel && (FlightGlobals.ActiveVessel.LandedOrSplashed || VesselMove.instance.isMovingVessel))
			{
				if(!VesselMove.instance.isMovingVessel)
				{
					if(GUI.Button(LineRect(ref line, 1.5f), "Move Vessel", HighLogic.Skin.button))
					{
						VesselMove.instance.StartMove(FlightGlobals.ActiveVessel, true);
					}
					line += 0.2f;

					Rect spawnVesselRect = LineRect(ref line);
					svRectScreenSpace = new Rect(spawnVesselRect);
					svRectScreenSpace.x += toolbarRect.x;
					svRectScreenSpace.y += toolbarRect.y;
					if(GUI.Button(spawnVesselRect, "Spawn Vessel", HighLogic.Skin.button))
					{
						VesselSpawn.instance.StartVesselSpawn();
					}
					showMoveHelp = false;
				}
				else
				{
					if(GUI.Button(LineRect(ref line, 2), "Place Vessel", HighLogic.Skin.button))
					{
						VesselMove.instance.EndMove();
					}

					line += 0.3f;

					if(GUI.Button(LineRect(ref line), "Help", HighLogic.Skin.button))
					{
						showMoveHelp = !showMoveHelp;
					}
				}
			}
			else
			{
				GUIStyle centerLabelStyle = new GUIStyle(HighLogic.Skin.label);
				centerLabelStyle.alignment = TextAnchor.UpperCenter;
				GUI.Label(LineRect(ref line), "You need to be landed to use this!", centerLabelStyle);
			}

			toolbarRect.height = (line * toolbarLineHeight) + (toolbarMargin * 2);
		}

		Rect LineRect(ref float currentLine, float heightFactor = 1)
		{
			Rect rect = new Rect(toolbarMargin, toolbarMargin + (currentLine * toolbarLineHeight), contentWidth, toolbarLineHeight*heightFactor);
			currentLine += heightFactor + 0.1f;
			return rect;
		}

		void MoveHelp(int windowID)
		{
			float line = 0;
			line += 1.25f;
			LineLabel("Movement: "+GameSettings.PITCH_DOWN.primary.ToString()+" "+
				GameSettings.PITCH_UP.primary.ToString()+" "+
				GameSettings.YAW_LEFT.primary.ToString()+" "+
				GameSettings.YAW_RIGHT.primary.ToString(), ref line);
			LineLabel("Roll: "+GameSettings.ROLL_LEFT.primary.ToString()+" "+
				GameSettings.ROLL_RIGHT.primary.ToString(), ref line);
			LineLabel("Pitch: "+GameSettings.TRANSLATE_DOWN.primary.ToString()+" "+
				GameSettings.TRANSLATE_UP.primary.ToString(), ref line);
			LineLabel("Yaw: "+GameSettings.TRANSLATE_LEFT.primary.ToString()+" "+
				GameSettings.TRANSLATE_RIGHT.primary.ToString(), ref line);
			LineLabel("Auto rotate rocket: " + GameSettings.TRANSLATE_BACK.primary.ToString(), ref line);
			LineLabel("Auto rotate plane: " + GameSettings.TRANSLATE_FWD.primary.ToString(), ref line);
			LineLabel("Change movement speed: Tab", ref line);
			//line++;
			//LineLabel("Hotkey: Alt + P", ref line);

			helpHeight = (line * toolbarLineHeight) + (toolbarMargin * 2);
		}

		void LineLabel(string label, ref float line)
		{
			GUI.Label(LineRect(ref line), label, HighLogic.Skin.label);
		}

		void AddToolbarButton()
		{
			if(HighLogic.LoadedSceneIsFlight)
			{
				if(!hasAddedButton)
				{
					Texture buttonTexture = GameDatabase.Instance.GetTexture("VesselMover/Textures/icon", false);
					ApplicationLauncher.Instance.AddModApplication(ShowToolbarGUI, HideToolbarGUI, Dummy, Dummy, Dummy, Dummy, ApplicationLauncher.AppScenes.FLIGHT, buttonTexture);
					hasAddedButton = true;
				}
			}
		}
		public void ShowToolbarGUI()
		{
			VesselMoverToolbar.toolbarGuiEnabled = true;	
		}

		public void HideToolbarGUI()
		{
			VesselMoverToolbar.toolbarGuiEnabled = false;	
		}
		void Dummy()
		{}

		public static bool MouseIsInRect(Rect rect)
		{
			return rect.Contains(MouseGUIPos());
		}

		public static Vector2 MouseGUIPos()
		{
			return new Vector3(Input.mousePosition.x, Screen.height-Input.mousePosition.y, 0);
		}
	}
}

