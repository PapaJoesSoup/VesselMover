using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;


namespace VesselMover
{
	[KSPAddon(KSPAddon.Startup.Flight, false)]
	public class VesselSpawn : MonoBehaviour
	{
		public static VesselSpawn instance;

		void Awake()
		{
			if(instance) Destroy(instance);
			instance = this;
		}

		private class CrewData
		{
			public string name = null;
			public ProtoCrewMember.Gender? gender = null;
			public bool addToRoster = true;

			public CrewData() { }
			public CrewData(CrewData cd)
			{
				name = cd.name;
				gender = cd.gender;
				addToRoster = cd.addToRoster;
			}
		}

		private class VesselData
		{
			public string name = null;
			public Guid? id = null;
			public string craftURL = null;
			public AvailablePart craftPart = null;
			public string flagURL = null;
			public VesselType vesselType = VesselType.Ship;
			public CelestialBody body = null;
			public Orbit orbit = null;
			public double latitude = 0.0;
			public double longitude = 0.0;
			public double? altitude = null;
			public float height = 0.0f;
			public bool orbiting = false;
			public bool owned = false;
			public List<CrewData> crew = new List<CrewData>();
			public PQSCity pqsCity = null;
			public Vector3d pqsOffset;
			public float heading;
			public float pitch;
			public float roll;

			public VesselData() { }
			public VesselData(VesselData vd)
			{
				name = vd.name;
				id = vd.id;
				craftURL = vd.craftURL;
				craftPart = vd.craftPart;
				flagURL = vd.flagURL;
				vesselType = vd.vesselType;
				body = vd.body;
				orbit = vd.orbit;
				latitude = vd.latitude;
				longitude = vd.longitude;
				altitude = vd.altitude;
				height = vd.height;
				orbiting = vd.orbiting;
				owned = vd.owned;
				pqsCity = vd.pqsCity;
				pqsOffset = vd.pqsOffset;
				heading = vd.heading;
				pitch = vd.pitch;
				roll = vd.roll;

				foreach (CrewData cd in vd.crew)
				{
					crew.Add(new CrewData(cd));
				}
			}
		}

		public bool openingCraftBrowser = false;
		bool loadingCraft = false;
		bool choosingPosition = false;
		CraftBrowserDialog craftBrowser;
		public void StartVesselSpawn()
		{
			if(FlightGlobals.ActiveVessel && FlightGlobals.ActiveVessel.LandedOrSplashed)
			{
				StartCoroutine(StartVesselSpawnRoutine());
			}
		}

		IEnumerator StartVesselSpawnRoutine()
		{
			openingCraftBrowser = true;

			float width = 450;
			float height = Screen.height * 0.7f;
			yield return null;

            //craftBrowser = new CraftBrowserDialog(new Rect((Screen.width - width) / 2, (Screen.height - height) / 2, width, height), EditorFacility.SPH, HighLogic.CurrentGame.Title.Split(new string[] { " (" }, StringSplitOptions.None)[0], "Spawn Vessel", OnSelected, OnCancelled, HighLogic.Skin, Texture2D.whiteTexture, false, false);
            craftBrowser = CraftBrowserDialog.Spawn(EditorFacility.SPH, HighLogic.CurrentGame.Title.Split(new string[] { " (" }, StringSplitOptions.None)[0], OnSelected, OnCancelled, false);
		}

		void OnSelected(string fullPath, CraftBrowserDialog.LoadType loadType)
		{
			StartCoroutine(SpawnCraftRoutine(fullPath));
			craftBrowser = null;
			openingCraftBrowser = false;
			choosingPosition = true;
		}

		void OnCancelled()
		{
			craftBrowser = null;
			openingCraftBrowser = false;
		}

		void OnGUI()
		{
			if(craftBrowser != null)
			{
			  craftBrowser.enabled = true;
        //craftBrowser.Start();
			}

			if(openingCraftBrowser)
			{
				DrawShadowedMessage("Opening Craft Browser...");
			}

			if(choosingPosition)
			{
				DrawShadowedMessage("Click somewhere to spawn!");
			}

			if(loadingCraft)
			{
				DrawShadowedMessage("Loading Craft...");
			}
		}

		void DrawShadowedMessage(string message)
		{
			GUIStyle style = new GUIStyle(HighLogic.Skin.label);
			style.fontSize = 22;
			style.alignment = TextAnchor.UpperCenter;

			Rect labelRect = new Rect(0, (Screen.height * 0.25f) + (Mathf.Sin(2*Time.time)*5), Screen.width, 200);

			//shadow
			GUIStyle shadowStyle = new GUIStyle(style);
			shadowStyle.normal.textColor = new Color(0, 0, 0, 0.15f);
			Vector3 shadowOffset = 2*Vector3.up;
			for(int i = 0; i < 16; i++)
			{
				Rect shadowRect = new Rect(labelRect);
				shadowRect.x += shadowOffset.x;
				shadowRect.y += shadowOffset.y;
				GUI.Label(shadowRect, message, shadowStyle);
				shadowOffset = Quaternion.AngleAxis(22.5f, Vector3.forward) * shadowOffset;
			}



			GUI.Label(labelRect, message, style);
		}


		IEnumerator SpawnCraftRoutine(string craftUrl)
		{
			yield return null;
			yield return null;
			GameObject indicatorObject = new GameObject();
			LineRenderer lr = indicatorObject.AddComponent<LineRenderer>();
			lr.material = new Material(Shader.Find("KSP/Particles/Alpha Blended"));
			lr.material.SetColor("_TintColor", Color.green);
			lr.material.mainTexture = Texture2D.whiteTexture;
			lr.useWorldSpace = false;
			lr.SetVertexCount(2);
			lr.SetPosition(0, Vector3.zero);
			lr.SetPosition(1, 10 * Vector3.forward);
			lr.SetWidth(0.1f, 1f);

			while(true)
			{
				Vector3 worldPos;
				if(GetMouseWorldPoint(out worldPos))
				{
					indicatorObject.transform.position = worldPos;
					indicatorObject.transform.rotation = Quaternion.LookRotation(worldPos - FlightGlobals.currentMainBody.transform.position);

					if(Input.GetMouseButtonDown(0))
					{
						Vector3 gpsPos = WorldPositionToGeoCoords(worldPos, FlightGlobals.currentMainBody);
						SpawnVesselFromCraftFile(craftUrl, gpsPos, 90, 0);
						break;
					}
				}

				yield return null;
			}
				
			choosingPosition = false;
			Destroy(indicatorObject);
		}

		public static Vector3d WorldPositionToGeoCoords(Vector3d worldPosition, CelestialBody body)
		{
			if(!body)
			{
				return Vector3d.zero;
			}
            
			double lat = body.GetLatitude(worldPosition);
			double longi = body.GetLongitude(worldPosition);
			double alt = body.GetAltitude(worldPosition);
			return new Vector3d(lat,longi,alt);
		}

		bool GetMouseWorldPoint(out Vector3 worldPos)
		{
			worldPos = Vector3.zero;
			float maxTargetingRange = 5000;
			//MouseControl
			Vector3 mouseAim = new Vector3(Input.mousePosition.x/Screen.width, Input.mousePosition.y/Screen.height, 0);
			Ray ray = FlightCamera.fetch.mainCamera.ViewportPointToRay(mouseAim);
			bool raycastFailed = false;
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, maxTargetingRange, 557057))
			{
				if(FlightGlobals.getAltitudeAtPos(hit.point) < 0)
				{
					raycastFailed = true;
				}
				else
				{
					worldPos =  hit.point;
				}

			}
			else
			{
				raycastFailed = true;
			}

			if(raycastFailed)
			{
				double dist;
				if(VMUtils.SphereRayIntersect(ray, FlightGlobals.currentMainBody.transform.position, FlightGlobals.currentMainBody.Radius, out dist))
				{
					worldPos = ray.GetPoint((float)dist);
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				return true;
			}

		}

		void SpawnVesselFromCraftFile(string craftURL, Vector3d gpsCoords, float heading, float pitch)
		{
			VesselData newData = new VesselData();

			newData.craftURL = craftURL;
			newData.latitude = gpsCoords.x;
			newData.longitude = gpsCoords.y;
			//newData.altitude = gpsCoords.z+35;            
            newData.altitude = gpsCoords.z = 35;            

			newData.body = FlightGlobals.currentMainBody;
			newData.heading = heading;
			newData.pitch = pitch;
			newData.orbiting = false;
			newData.flagURL = HighLogic.CurrentGame.flagURL;
			newData.owned = true;
			newData.vesselType = VesselType.Ship;
			newData.crew = new List<CrewData>();

			SpawnVessel(newData);
		}

		void SpawnVessel(VesselData vesselData)
		{
			string gameDataDir = KSPUtil.ApplicationRootPath;
			Debug.Log("Spawning a vessel named '" + vesselData.name + "'");

			// Set additional info for landed vessels
			bool landed = false;
			if (!vesselData.orbiting)
			{
				landed = true;
                if (vesselData.altitude == null || vesselData.altitude < 0)
				{
                    vesselData.altitude = 35; //LocationUtil.TerrainHeight(vesselData.latitude, vesselData.longitude, vesselData.body);
				}

				//Vector3d pos = vesselData.body.GetWorldSurfacePosition(vesselData.latitude, vesselData.longitude, vesselData.altitude.Value);
                 Vector3d pos = vesselData.body.GetRelSurfacePosition(vesselData.latitude, vesselData.longitude, vesselData.altitude.Value);

				vesselData.orbit = new Orbit(0.0, 0.0, 0.0, 0.0, 0.0, 0.0, 0.0, vesselData.body);
				vesselData.orbit.UpdateFromStateVectors(pos, vesselData.body.getRFrmVel(pos), vesselData.body, Planetarium.GetUniversalTime());
			}
			else
			{
				vesselData.orbit.referenceBody = vesselData.body;
			}

			ConfigNode[] partNodes;
			UntrackedObjectClass sizeClass;
			ShipConstruct shipConstruct = null;
			bool hasClamp = false;
			float lcHeight = 0;
			ConfigNode craftNode;
			Quaternion craftRotation = Quaternion.identity;
			if (!string.IsNullOrEmpty(vesselData.craftURL))
			{
				// Save the current ShipConstruction ship, otherwise the player will see the spawned ship next time they enter the VAB!
				ConfigNode currentShip = ShipConstruction.ShipConfig;

				shipConstruct = ShipConstruction.LoadShip(vesselData.craftURL);
				if (shipConstruct == null)
				{
					Debug.Log("ShipConstruct was null when tried to load '" + vesselData.craftURL +
						"' (usually this means the file could not be found).");
					return;//continue;
				}

				craftNode = ConfigNode.Load(vesselData.craftURL);
				lcHeight = ConfigNode.ParseVector3(craftNode.GetNode("PART").GetValue("pos")).y;
				craftRotation = ConfigNode.ParseQuaternion(craftNode.GetNode("PART").GetValue("rot"));

				// Restore ShipConstruction ship
				ShipConstruction.ShipConfig = currentShip;

				// Set the name
				if (string.IsNullOrEmpty(vesselData.name))
				{
					vesselData.name = shipConstruct.shipName;
				}

				// Set some parameters that need to be at the part level
				uint missionID = (uint)Guid.NewGuid().GetHashCode();
				uint launchID = HighLogic.CurrentGame.launchID++;
				foreach (Part p in shipConstruct.parts)
				{
					p.flightID = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
					p.missionID = missionID;
					p.launchID = launchID;
					p.flagURL = vesselData.flagURL ?? HighLogic.CurrentGame.flagURL;

					// Had some issues with this being set to -1 for some ships - can't figure out
					// why.  End result is the vessel exploding, so let's just set it to a positive
					// value.
					p.temperature = 1.0;
				}

				//add minimal crew
				bool success = false;
				Part part = shipConstruct.parts.Find(p => p.protoModuleCrew.Count < p.CrewCapacity);

				// Add the crew member
				if (part != null)
				{
					// Create the ProtoCrewMember
					ProtoCrewMember crewMember = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
					crewMember.gender = UnityEngine.Random.Range(0,100) > 50 ? ProtoCrewMember.Gender.Female : ProtoCrewMember.Gender.Male;
					//crewMember.trait = "Pilot";

					// Add them to the part
					part.AddCrewmemberAt(crewMember, part.protoModuleCrew.Count);
				}

				// Create a dummy ProtoVessel, we will use this to dump the parts to a config node.
				// We can't use the config nodes from the .craft file, because they are in a
				// slightly different format than those required for a ProtoVessel (seriously
				// Squad?!?).
				ConfigNode empty = new ConfigNode();
				ProtoVessel dummyProto = new ProtoVessel(empty, null);
				Vessel dummyVessel = new Vessel();
				dummyVessel.parts = shipConstruct.parts;
				dummyProto.vesselRef = dummyVessel;

				// Create the ProtoPartSnapshot objects and then initialize them
				foreach (Part p in shipConstruct.parts)
				{
					dummyProto.protoPartSnapshots.Add(new ProtoPartSnapshot(p, dummyProto));
				}
				foreach (ProtoPartSnapshot p in dummyProto.protoPartSnapshots)
				{
					p.storePartRefs();
				}



				// Create the ship's parts

				List<ConfigNode> partNodesL = new List<ConfigNode>();
				foreach(ProtoPartSnapshot snapShot in dummyProto.protoPartSnapshots)
				{
					ConfigNode node = new ConfigNode("PART");
					snapShot.Save(node);
					partNodesL.Add(node);
				}
				partNodes = partNodesL.ToArray();




				// Estimate an object class, numbers are based on the in game description of the
				// size classes.
				float size = shipConstruct.shipSize.magnitude / 2.0f;
				if (size < 4.0f)
				{
					sizeClass = UntrackedObjectClass.A;
				}
				else if (size < 7.0f)
				{
					sizeClass = UntrackedObjectClass.B;
				}
				else if (size < 12.0f)
				{
					sizeClass = UntrackedObjectClass.C;
				}
				else if (size < 18.0f)
				{
					sizeClass = UntrackedObjectClass.D;
				}
				else
				{
					sizeClass = UntrackedObjectClass.E;
				}
			}
			else
			{
				// Create crew member array
				ProtoCrewMember[] crewArray = new ProtoCrewMember[vesselData.crew.Count];
				int i = 0;
				foreach (CrewData cd in vesselData.crew)
				{
					// Create the ProtoCrewMember
					ProtoCrewMember crewMember = HighLogic.CurrentGame.CrewRoster.GetNewKerbal(ProtoCrewMember.KerbalType.Crew);
					if (cd.name != null)
					{
						crewMember.KerbalRef.name = cd.name;
					}

					crewArray[i++] = crewMember;
				}

				// Create part nodes
				uint flightId = ShipConstruction.GetUniqueFlightID(HighLogic.CurrentGame.flightState);
				partNodes = new ConfigNode[1];
				partNodes[0] = ProtoVessel.CreatePartNode(vesselData.craftPart.name, flightId, crewArray);

				// Default the size class
				sizeClass = UntrackedObjectClass.A;

				// Set the name
				if (string.IsNullOrEmpty(vesselData.name))
				{
					vesselData.name = vesselData.craftPart.name;
				}
			}

			// Create additional nodes
			ConfigNode[] additionalNodes = new ConfigNode[0];
			//DiscoveryLevels discoveryLevel = vesselData.owned ? DiscoveryLevels.Owned : DiscoveryLevels.Unowned;
			//additionalNodes[0] = ProtoVessel.CreateDiscoveryNode(discoveryLevel, sizeClass, contract.TimeDeadline, contract.TimeDeadline);

			// Create the config node representation of the ProtoVessel
			ConfigNode protoVesselNode = ProtoVessel.CreateVesselNode(vesselData.name, vesselData.vesselType, vesselData.orbit, 0, partNodes, additionalNodes);

			// Additional seetings for a landed vessel
			if (!vesselData.orbiting)
			{
				Vector3d norm = vesselData.body.GetRelSurfaceNVector(vesselData.latitude, vesselData.longitude);

				double terrainHeight = 0.0;
				if (vesselData.body.pqsController != null)
				{
					terrainHeight = vesselData.body.pqsController.GetSurfaceHeight(norm) - vesselData.body.pqsController.radius;
				}
				bool splashed = false;// = landed && terrainHeight < 0.001;

				// Create the config node representation of the ProtoVessel
				// Note - flying is experimental, and so far doesn't work
				protoVesselNode.SetValue("sit", (splashed ? Vessel.Situations.SPLASHED : landed ?
					Vessel.Situations.LANDED : Vessel.Situations.FLYING).ToString());
				protoVesselNode.SetValue("landed", (landed && !splashed).ToString());
				protoVesselNode.SetValue("splashed", splashed.ToString());
				protoVesselNode.SetValue("lat", vesselData.latitude.ToString());
				protoVesselNode.SetValue("lon", vesselData.longitude.ToString());
				protoVesselNode.SetValue("alt", vesselData.altitude.ToString());
				protoVesselNode.SetValue("landedAt", vesselData.body.name);

				// Figure out the additional height to subtract
				float lowest = float.MaxValue;
				if (shipConstruct != null)
				{
					foreach (Part p in shipConstruct.parts)
					{
						foreach (Collider collider in p.GetComponentsInChildren<Collider>())
						{
							if (collider.gameObject.layer != 21 && collider.enabled)
							{
								lowest = Mathf.Min(lowest, collider.bounds.min.y);
							}
						}
					}
				}
				else
				{
					foreach (Collider collider in vesselData.craftPart.partPrefab.GetComponentsInChildren<Collider>())
					{
						if (collider.gameObject.layer != 21 && collider.enabled)
						{
							lowest = Mathf.Min(lowest, collider.bounds.min.y);
						}
					}
				}

				if (lowest == float.MaxValue)
				{
					lowest = 0;
				}

				// Figure out the surface height and rotation
				Quaternion normal = Quaternion.LookRotation((Vector3)norm);// new Vector3((float)norm.x, (float)norm.y, (float)norm.z));
				Quaternion rotation = Quaternion.identity;
				float heading = vesselData.heading;
				if (shipConstruct == null)
				{
					rotation = rotation * Quaternion.FromToRotation(Vector3.up, Vector3.back);
				}
				else if (shipConstruct.shipFacility == EditorFacility.SPH)
				{
					rotation = rotation * Quaternion.FromToRotation(Vector3.forward, -Vector3.forward);
					heading += 180.0f;
				}
				else
				{
					rotation = rotation * Quaternion.FromToRotation(Vector3.up, Vector3.forward);
					rotation = Quaternion.FromToRotation(Vector3.up, -Vector3.up) * rotation;

					//rotation = craftRotation;


					vesselData.heading = 0;
					vesselData.pitch = 0;
				}

				rotation = rotation * Quaternion.AngleAxis(heading, Vector3.back);
				rotation = rotation * Quaternion.AngleAxis(vesselData.roll, Vector3.down);
				rotation = rotation * Quaternion.AngleAxis(vesselData.pitch, Vector3.left);

				// Set the height and rotation
				if (landed || splashed)
				{
					float hgt = (shipConstruct != null ? shipConstruct.parts[0] : vesselData.craftPart.partPrefab).localRoot.attPos0.y - lowest;
					hgt += vesselData.height;

					foreach(Part p in shipConstruct.Parts)
					{
						LaunchClamp lc = p.FindModuleImplementing<LaunchClamp>();
						if(lc)
						{
							hasClamp = true;
							break;
						}
					}

					if(!hasClamp)
					{
						hgt += 35;
					}
					else
					{
						hgt += lcHeight;
					}
					protoVesselNode.SetValue("hgt", hgt.ToString(), true);
				}
				protoVesselNode.SetValue("rot", KSPUtil.WriteQuaternion(normal * rotation), true);

				// Set the normal vector relative to the surface
				Vector3 nrm = (rotation * Vector3.forward);
				protoVesselNode.SetValue("nrm", nrm.x + "," + nrm.y + "," + nrm.z, true);

				protoVesselNode.SetValue("prst", false.ToString(), true);
			}

			// Add vessel to the game
            ProtoVessel protoVessel = HighLogic.CurrentGame.AddVessel(protoVesselNode);
			//protoVessel.vesselRef.transform.rotation = protoVessel.rotation;           

			// Store the id for later use
			vesselData.id = protoVessel.vesselRef.id;

			//protoVessel.vesselRef.currentStage = 0;
            
			StartCoroutine(PlaceSpawnedVessel(protoVessel.vesselRef, !hasClamp));

			// Associate it so that it can be used in contract parameters
			//ContractVesselTracker.Instance.AssociateVessel(vesselData.name, protoVessel.vesselRef);

			//destroy prefabs
			foreach(Part p in FindObjectsOfType<Part>())
			{
				if(!p.vessel)
				{
					Destroy(p.gameObject);
				}
			}
		}

		IEnumerator PlaceSpawnedVessel(Vessel v, bool moveVessel)
		{
			loadingCraft = true;
			v.isPersistent = true;

            while(v.packed)
			{
				yield return null;
			}
            
            v.IgnoreGForces(240);
            v.SetWorldVelocity(Vector3d.zero);  

			yield return null;
			FlightGlobals.ForceSetActiveVessel(v);
			yield return null;
			v.Landed = true;
			v.situation = Vessel.Situations.PRELAUNCH;
			v.GoOffRails();

			//Staging.beginFlight();
			StageManager.BeginFlight();
		 
			if(moveVessel)
			{
				VesselMove.instance.StartMove(v, false);
                v.IgnoreGForces(240);
                VesselMove.instance.moveHeight = 35;
				yield return null;
				if(VesselMove.instance.movingVessel == v)
				{
					v.Landed = false;
				}
			}
			loadingCraft = false;
		}

		public static class LocationUtil
		{
			public static float TerrainHeight(double lat, double lon, CelestialBody body)
			{
				return 0;
			}
		}
	}
}

