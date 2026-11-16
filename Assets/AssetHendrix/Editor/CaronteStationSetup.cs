using UnityEngine;using UnityEditor;using UnityEditor.SceneManagement;using Caronte;
public static class CaronteStationSetup {
 const string Root="Assets/AssetHendrix";
 static Material hull,dark,white,cyan,amber,planet;
 static GameObject P(string name,Transform parent,PrimitiveType shape,Vector3 pos,Vector3 scale,Material mat,bool collider=true){var g=GameObject.CreatePrimitive(shape);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collider)Object.DestroyImmediate(g.GetComponent<Collider>());return g;}
 static Transform N(string name,Transform parent,Vector3 pos){var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=pos;return g.transform;}
 static Material M(string name,Color col,bool glow=false){string path=Root+"/Materials/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);if(mat)return mat;mat=new Material(Shader.Find(glow?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));mat.color=col;if(!glow){mat.SetFloat("_Metallic",.65f);mat.SetFloat("_Smoothness",.65f);}AssetDatabase.CreateAsset(mat,path);return mat;}
 static void Layer(Transform t,int layer){t.gameObject.layer=layer;foreach(Transform c in t)Layer(c,layer);}
 static void Ring(string name,Transform parent,Vector3 center,float radius,Material mat,float width){var g=N(name,parent,Vector3.zero).gameObject;var lr=g.AddComponent<LineRenderer>();lr.useWorldSpace=false;lr.positionCount=97;lr.loop=true;lr.sharedMaterial=mat;lr.widthMultiplier=width;for(int i=0;i<97;i++){float a=i/96f*2*Mathf.PI;lr.SetPosition(i,center+new Vector3(Mathf.Cos(a)*radius,0,Mathf.Sin(a)*radius));}}
 public static string Build(){
 var app=Object.FindFirstObjectByType<CarontePrototype>();if(GameObject.Find("StationInterior"))return "Station already exists";
 hull=M("StationHull",new Color(.15f,.21f,.28f));dark=M("StationGraphite",new Color(.035f,.06f,.09f));white=M("RobotCeramic",new Color(.65f,.73f,.8f));cyan=M("StationCyanLight",new Color(.12f,.7f,1),true);amber=M("StationAmberLight",new Color(1,.57f,.18f),true);
 var room=new GameObject("StationInterior");var station=room.AddComponent<CaronteStation>();station.app=app;station.lightMaterial=cyan;app.station=station;
 P("Floor",room.transform,PrimitiveType.Cube,new Vector3(0,-.15f,3),new Vector3(12,.3f,16),dark);
 for(int x=-5;x<=5;x+=2)for(int z=-3;z<=9;z+=2)P("FloorPanel",room.transform,PrimitiveType.Cube,new Vector3(x,.012f,z),new Vector3(1.94f,.025f,1.94f),hull,false);
 for(int x=-1;x<=1;x+=2)P("FloorNavigationStrip",room.transform,PrimitiveType.Cube,new Vector3(x*4,.035f,3),new Vector3(.035f,.015f,14),cyan,false);
 P("RearBulkhead",room.transform,PrimitiveType.Cube,new Vector3(0,3,11),new Vector3(12,6,.3f),hull);
 P("RightBulkhead",room.transform,PrimitiveType.Cube,new Vector3(6,3,3),new Vector3(.3f,6,16),dark);
 P("FrontBulkhead",room.transform,PrimitiveType.Cube,new Vector3(0,3,-5),new Vector3(12,6,.3f),dark);
 P("Ceiling",room.transform,PrimitiveType.Cube,new Vector3(0,6,3),new Vector3(12,.3f,16),dark);
 for(int z=-4;z<=10;z+=3){P("CeilingRib",room.transform,PrimitiveType.Cube,new Vector3(0,5.7f,z),new Vector3(12,.25f,.25f),hull);P("CeilingLight",room.transform,PrimitiveType.Cube,new Vector3(0,5.5f,z),new Vector3(5,.07f,.12f),cyan,false);}
 // Window on the port side: the planet is real 3D scenery outside this opening.
 for(int z=-4;z<=11;z+=3){P("WindowMullion",room.transform,PrimitiveType.Cube,new Vector3(-6,3,z),new Vector3(.25f,6,.23f),hull);P("WindowEdgeLight",room.transform,PrimitiveType.Cube,new Vector3(-5.85f,3,z+.14f),new Vector3(.03f,5,.045f),cyan,false);}
 P("WindowLowerFrame",room.transform,PrimitiveType.Cube,new Vector3(-6,.5f,3),new Vector3(.4f,1,16),hull);
 P("WindowUpperFrame",room.transform,PrimitiveType.Cube,new Vector3(-6,5.7f,3),new Vector3(.4f,.6f,16),hull);
 var boundary=N("WindowSafetyCollider",room.transform,new Vector3(-6,3,3)).gameObject.AddComponent<BoxCollider>();boundary.size=new Vector3(.2f,6,16);
 // Procedural, reusable map; no external dependencies.
 var texture=new Texture2D(512,256,TextureFormat.RGBA32,false);texture.name="ProceduralEarth";
 for(int y=0;y<256;y++)for(int x=0;x<512;x++){float lat=(float)y/255;float n=Mathf.PerlinNoise(x*.015f,y*.02f)+.3f*Mathf.PerlinNoise(x*.043f+6,y*.05f);Color c=n>.68f?new Color(.12f,.35f,.26f):new Color(.015f,.12f,.31f);float cloud=Mathf.PerlinNoise(x*.029f+20,y*.034f);if(cloud>.67f)c=Color.Lerp(c,new Color(.8f,.91f,1),(cloud-.67f)*2);if(lat<.08f||lat>.92f)c=new Color(.65f,.83f,.92f);texture.SetPixel(x,y,c);}texture.Apply();AssetDatabase.CreateAsset(texture,Root+"/Materials/ProceduralEarth.asset");
 planet=M("WindowPlanet",Color.white);planet.mainTexture=texture;EditorUtility.SetDirty(planet);
 P("DestinationEarth",room.transform,PrimitiveType.Sphere,new Vector3(-22,-3,10),Vector3.one*25,planet,false);
 var stars=N("Starfield",room.transform,Vector3.zero);var rng=new System.Random(42);for(int i=0;i<130;i++)P("Star",stars,PrimitiveType.Sphere,new Vector3(-38-(float)rng.NextDouble()*15,(float)rng.NextDouble()*35-5,(float)rng.NextDouble()*75-30),Vector3.one*(.04f+(float)rng.NextDouble()*.08f),white,false);
 // Desk and monitor bodies.
 var desk=N("MissionDesk",room.transform,new Vector3(0,.8f,.2f));P("DeskSurface",desk,PrimitiveType.Cube,Vector3.zero,new Vector3(5.7f,.16f,1.6f),hull);P("DeskFrontLight",desk,PrimitiveType.Cube,new Vector3(0,0,-.81f),new Vector3(5.65f,.035f,.03f),cyan,false);
 for(int x=-1;x<=1;x+=2)P("DeskLeg",room.transform,PrimitiveType.Cube,new Vector3(x*2.3f,.4f,.4f),new Vector3(.22f,.8f,.7f),dark);
 station.dataScreen=N("LeftMonitorSurface",room.transform,new Vector3(-1.25f,1.8f,.45f));station.dataScreen.rotation=Quaternion.Euler(0,7,0);
 station.selectionScreen=N("RightMonitorSurface",room.transform,new Vector3(1.35f,1.96f,.46f));station.selectionScreen.rotation=Quaternion.Euler(0,-9,0);
 foreach(Transform screen in new[]{station.dataScreen,station.selectionScreen}){bool left=screen==station.dataScreen;float w=left?2.4f:1.8f,h=left?1.6f:2.1f;P("MonitorBezel",screen,PrimitiveType.Cube,new Vector3(0,0,.06f),new Vector3(w+.10f,h+.10f,.1f),dark);P("MonitorTopEdge",screen,PrimitiveType.Cube,new Vector3(0,h*.5f+.035f,-.001f),new Vector3(w,.015f,.015f),cyan,false);P("MonitorStand",room.transform,PrimitiveType.Cube,new Vector3(screen.position.x,1.05f,.63f),new Vector3(.13f,.5f,.13f),hull);}
 for(int a=0;a<4;a++)for(int b=0;b<12;b++)P("KeyboardKey",desk,PrimitiveType.Cube,new Vector3(-1.5f+b*.115f,.12f,-.45f+a*.09f),new Vector3(.09f,.025f,.065f),dark,false);
 P("DeskTablet",desk,PrimitiveType.Cube,new Vector3(.5f,.1f,-.3f),new Vector3(.65f,.04f,.4f),cyan,false);
 // Assembly stage, transfer trays and gantry.
 var bay=N("AssemblyBay",room.transform,new Vector3(0,0,6.2f));
 P("AssemblyPedestal",bay,PrimitiveType.Cylinder,new Vector3(0,.65f,0),new Vector3(3,.65f,3),hull);
 P("AssemblyTop",bay,PrimitiveType.Cylinder,new Vector3(0,1.32f,0),new Vector3(3.2f,.045f,3.2f),dark);
 Ring("StageIllumination",bay,new Vector3(0,1.38f,0),1.48f,cyan,.055f);
 Ring("OverheadGantry",bay,new Vector3(0,4.9f,0),2.7f,hull,.24f);Ring("GantryLighting",bay,new Vector3(0,4.75f,0),2.65f,cyan,.06f);
 station.assemblyOrigin=N("AssemblyMount",bay,new Vector3(0,2.15f,0));
 station.leftSupply=N("LeftSupply",room.transform,new Vector3(-2.65f,1.2f,4.2f));station.rightSupply=N("RightSupply",room.transform,new Vector3(2.65f,1.2f,4.2f));
 foreach(var t in new[]{station.leftSupply,station.rightSupply}){P("SupplyBench",room.transform,PrimitiveType.Cube,t.position+new Vector3(0,-.13f,.65f),new Vector3(1.25f,.16f,2.2f),hull);P("SupplySupport",room.transform,PrimitiveType.Cube,t.position+new Vector3(0,-.65f,.65f),new Vector3(.7f,1.1f,1.6f),dark);}
 station.armBases=new Transform[2];station.armUpper=new Transform[2];station.armLower=new Transform[2];station.armElbow=new Transform[2];station.armWrist=new Transform[2];station.armGrip=new Transform[2];
 for(int i=0;i<2;i++){float sign=i==0?-1:1;var arm=N("RobotArm_"+i,room.transform,new Vector3(sign*2.3f,.4f,6.3f));
 P("RobotBase",arm,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.75f,.4f,.75f),dark);station.armBases[i]=arm;
 station.armUpper[i]=P("UpperActuator",room.transform,PrimitiveType.Cylinder,new Vector3(sign*2.3f,1.3f,6.3f),new Vector3(.2f,.7f,.2f),white,false).transform;
 station.armLower[i]=P("ForearmActuator",room.transform,PrimitiveType.Cylinder,new Vector3(sign*1.8f,2,6.3f),new Vector3(.16f,.65f,.16f),white,false).transform;
 station.armElbow[i]=P("ElbowServo",room.transform,PrimitiveType.Sphere,new Vector3(sign*2.3f,2,6.3f),Vector3.one*.35f,hull,false).transform;
 station.armWrist[i]=P("WristServo",room.transform,PrimitiveType.Sphere,new Vector3(sign*1.6f,2.3f,6.3f),Vector3.one*.25f,hull,false).transform;
 station.armGrip[i]=N("Gripper",room.transform,new Vector3(sign*1.6f,2.3f,6.3f));
 for(int j=-1;j<=1;j+=2)P("GripperFinger",station.armGrip[i],PrimitiveType.Cube,new Vector3(j*.1f,-.13f,0),new Vector3(.055f,.25f,.08f),white,false);
 }
 // Player. Separate from the isolated spacecraft preview camera.
 var rig=N("FirstPersonPlayer",room.transform,new Vector3(0,.05f,-3.6f));station.player=rig.gameObject.AddComponent<CharacterController>();station.player.height=1.7f;station.player.radius=.24f;station.player.center=new Vector3(0,.85f,0);station.player.stepOffset=.25f;
 var cam=N("FirstPersonCamera",rig,new Vector3(0,1.65f,0));station.playerCamera=cam.gameObject.AddComponent<Camera>();cam.gameObject.tag="MainCamera";station.playerCamera.fieldOfView=68;station.playerCamera.nearClipPlane=.05f;station.playerCamera.farClipPlane=180;station.playerCamera.cullingMask=~(1<<8);cam.gameObject.AddComponent<AudioListener>();
 app.viewer.gameObject.tag="Untagged";var listener=app.viewer.GetComponent<AudioListener>();if(listener)Object.DestroyImmediate(listener);
 var rt=new RenderTexture(1200,800,24);rt.name="SatelliteMonitor";AssetDatabase.CreateAsset(rt,Root+"/Materials/SatelliteMonitor.renderTexture");station.previewTexture=rt;app.viewer.targetTexture=rt;app.viewer.cullingMask=1<<8;app.viewer.aspect=1.5f;
 Layer(app.spacecraft,8);Layer(app.earth.transform,8);Layer(app.orbitLine.transform,8);
 // Prevent preview lighting from washing out the room.
 var fill=app.transform.Find("CaronteFillLight");if(fill)fill.GetComponent<Light>().cullingMask=1<<8;
 for(int z=0;z<=9;z+=4){var li=N("RoomAreaLight",room.transform,new Vector3(0,4.5f,z)).gameObject.AddComponent<Light>();li.type=LightType.Point;li.intensity=13;li.range=11;li.color=new Color(.55f,.76f,1);li.cullingMask=~(1<<8);}
 RenderSettings.ambientLight=new Color(.17f,.22f,.3f);
 var old=app.transform.Find("MissionConsole");if(old)Object.DestroyImmediate(old.gameObject);old=app.transform.Find("CaronteEventSystem");if(old)Object.DestroyImmediate(old.gameObject);
 app.BuildInterface();EditorUtility.SetDirty(app);EditorUtility.SetDirty(station);AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(room.scene);EditorSceneManager.SaveScene(room.scene);
 return "Station, first person rig, twin monitors, Earth window and robotic bay saved in Prototipo.";
 }
}