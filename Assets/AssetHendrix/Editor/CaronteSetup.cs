using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using Caronte;
public static class CaronteSetup {
 const string Root="Assets/AssetHendrix";
 static Material metal,gold,solar,glass,board;
 static Transform Node(string name,Transform parent,Vector3 position){var g=new GameObject(name);g.transform.SetParent(parent,false);g.transform.localPosition=position;return g.transform;}
 static GameObject Piece(string name,Transform parent,PrimitiveType shape,Vector3 pos,Vector3 scale,Material mat){
 var g=GameObject.CreatePrimitive(shape);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;return g;}
 static Material Mat(string name,Color c,float metallic){
 var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Metallic",metallic);m.SetFloat("_Smoothness",.55f);AssetDatabase.CreateAsset(m,Root+"/Materials/"+name+".mat");return m;}
 static GameObject Prefab(string name,System.Action<Transform> construct){var g=new GameObject(name);construct(g.transform);var p=PrefabUtility.SaveAsPrefabAsset(g,Root+"/Prefabs/"+name+".prefab");Object.DestroyImmediate(g);return p;}
 public static string Build(){
 if(GameObject.Find("CARONTE"))return "Already installed";
 foreach(string dir in new[]{"Materials","Prefabs","Documentation"})System.IO.Directory.CreateDirectory(Root+"/"+dir);
 metal=Mat("Titanium",new Color(.35f,.43f,.55f),.8f);gold=Mat("ThermalGold",new Color(.85f,.55f,.13f),.7f);solar=Mat("SolarCells",new Color(.025f,.10f,.33f),.45f);glass=Mat("OpticalGlass",new Color(.13f,.65f,.9f),.8f);board=Mat("Electronics",new Color(.05f,.32f,.2f),.1f);
 var root=new GameObject("CARONTE");var app=root.AddComponent<CarontePrototype>();app.metal=metal;app.gold=gold;app.blue=solar;app.green=board;app.glass=glass;
 var ship=Node("SpacecraftRoot",root.transform,Vector3.zero);app.spacecraft=ship;
 var body=Node("BodyAnchor",ship,Vector3.zero);
 Piece("ThermalFace",body,PrimitiveType.Cube,new Vector3(0,0,.44f),new Vector3(1.92f,2.9f,.12f),gold);
 Piece("BackPlate",body,PrimitiveType.Cube,new Vector3(0,0,-.44f),new Vector3(1.92f,2.9f,.12f),metal);
 for(int x=-1;x<=1;x+=2)for(int y=-1;y<=1;y+=2)Piece("Rail",ship,PrimitiveType.Cube,new Vector3(x,y*1.5f,0),new Vector3(.09f,.09f,1.1f),metal);
 for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Piece("FrameUpright",ship,PrimitiveType.Cube,new Vector3(x,0,z*.5f),new Vector3(.09f,3.08f,.09f),metal);
 for(int y=-1;y<=1;y+=2)for(int z=-1;z<=1;z+=2)Piece("CrossMember",ship,PrimitiveType.Cube,new Vector3(0,y*1.5f,z*.5f),new Vector3(2.1f,.09f,.09f),metal);
 Node("FrontPayloadAnchor",ship,new Vector3(0,.7f,-.58f));Node("RearPayloadAnchor",ship,new Vector3(0,.7f,.58f));
 Node("LeftSolarPanelAnchor",ship,new Vector3(-1.07f,0,0));var right=Node("RightSolarPanelAnchor",ship,new Vector3(1.07f,0,0));right.localRotation=Quaternion.Euler(0,180,0);
 var inside=Node("InternalModulesAnchor",ship,Vector3.zero);Node("BatteryAnchor",inside,new Vector3(0,-.8f,0));Node("ComputerAnchor",inside,new Vector3(0,.15f,0));Node("StorageAnchor",inside,new Vector3(.5f,-.1f,0));
 Node("TopAntennaAnchor",ship,new Vector3(0,1.6f,0));Node("BottomEquipmentAnchor",ship,new Vector3(0,-1.6f,0));
 Piece("OnboardComputer",inside,PrimitiveType.Cube,new Vector3(0,.2f,0),new Vector3(1.5f,.12f,.7f),board);
 for(int i=0;i<4;i++)Piece("Processor",inside,PrimitiveType.Cube,new Vector3(-.5f+i*.32f,.3f,0),new Vector3(.2f,.08f,.24f),metal);
 app.payloadPrefabs=new GameObject[3];app.solarPrefabs=new GameObject[3];app.batteryPrefabs=new GameObject[3];app.radioPrefabs=new GameObject[3];app.storagePrefabs=new GameObject[3];
 for(int i=0;i<3;i++){int v=i;
 app.payloadPrefabs[i]=Prefab("Payload_"+i,t=>{
 Piece("CameraHousing",t,PrimitiveType.Cube,Vector3.zero,new Vector3(.65f+v*.15f,.6f,.35f),gold);
 var lens=Piece("Lens",t,PrimitiveType.Cylinder,new Vector3(0,0,-.33f),new Vector3(.4f+v*.12f,.22f,.4f+v*.12f),metal);lens.transform.localRotation=Quaternion.Euler(90,0,0);
 var optic=Piece("Optics",t,PrimitiveType.Cylinder,new Vector3(0,0,-.57f),new Vector3(.32f+v*.12f,.015f,.32f+v*.12f),glass);optic.transform.localRotation=Quaternion.Euler(90,0,0);});
 app.solarPrefabs[i]=Prefab("Solar_"+i,t=>{
 var hinge=Node("Hinge",t,Vector3.zero);float w=1.3f+v*.48f;
 Piece("PanelFrame",hinge,PrimitiveType.Cube,new Vector3(-w/2,0,0),new Vector3(w,2.4f,.08f),metal);
 for(int a=0;a<4;a++)for(int b=0;b<6;b++)Piece("PhotovoltaicCell",hinge,PrimitiveType.Cube,new Vector3(-w+(a+.5f)*w/4,-1.2f+(b+.5f)*.4f,-.055f),new Vector3(w/4-.035f,.36f,.025f),solar);
 Piece("HingePin",t,PrimitiveType.Cylinder,Vector3.zero,new Vector3(.13f,.45f,.13f),gold);});
 app.batteryPrefabs[i]=Prefab("Battery_"+i,t=>{for(int a=0;a<=v;a++)Piece("BatteryCell",t,PrimitiveType.Cube,new Vector3((a-v*.5f)*.43f,0,0),new Vector3(.38f,.66f,.7f),gold);});
 app.radioPrefabs[i]=Prefab("Radio_"+i,t=>{
 Piece("RadioBase",t,PrimitiveType.Cube,Vector3.zero,new Vector3(.5f,.15f,.5f),board);
 Piece("AntennaMast",t,PrimitiveType.Cylinder,new Vector3(0,.35f,0),new Vector3(.06f,.35f,.06f),metal);
 if(v>0)Piece("PatchAntenna",t,PrimitiveType.Cube,new Vector3(0,.7f,0),new Vector3(.6f+v*.2f,.08f,.6f+v*.2f),gold);
 else for(int k=-1;k<=1;k+=2){var rod=Piece("UHFElement",t,PrimitiveType.Cylinder,new Vector3(k*.27f,.5f,0),new Vector3(.025f,.35f,.025f),metal);rod.transform.localRotation=Quaternion.Euler(0,0,k*55);}});
 app.storagePrefabs[i]=Prefab("Storage_"+i,t=>{Piece("MemoryBoard",t,PrimitiveType.Cube,Vector3.zero,new Vector3(.45f,.12f,.55f),board);for(int k=0;k<=v;k++)Piece("FlashChip",t,PrimitiveType.Cube,new Vector3(0,.1f,(k-v*.5f)*.16f),new Vector3(.3f,.08f,.12f),metal);});
 }
 app.viewer=Camera.main;app.viewer.clearFlags=CameraClearFlags.Skybox;app.viewer.transform.position=new Vector3(6,4,-9);app.viewer.transform.LookAt(Vector3.zero);app.viewer.fieldOfView=42;
 var light=Object.FindFirstObjectByType<Light>();light.transform.rotation=Quaternion.Euler(30,-25,0);light.intensity=2;
 var fill=Node("CaronteFillLight",root.transform,new Vector3(-4,2,-6)).gameObject.AddComponent<Light>();fill.type=LightType.Point;fill.color=new Color(.4f,.65f,1);fill.intensity=12;fill.range=20;
 RenderSettings.ambientLight=new Color(.28f,.35f,.5f);
 app.earth=Piece("Earth",root.transform,PrimitiveType.Sphere,Vector3.zero,Vector3.one*4.5f,solar);app.earth.SetActive(false);
 app.orbitLine=new GameObject("OrbitPath");app.orbitLine.transform.SetParent(root.transform);var line=app.orbitLine.AddComponent<LineRenderer>();line.sharedMaterial=glass;line.widthMultiplier=.015f;line.positionCount=129;
 for(int n=0;n<129;n++){float a=n/128f*Mathf.PI*2;line.SetPosition(n,new Vector3(Mathf.Cos(a)*4.1f,Mathf.Sin(a)*1.5f,Mathf.Sin(a)*3.5f));}app.orbitLine.SetActive(false);
 app.BuildInterface();
 // Create initial model instances for edit-mode preview.
 string[] keys={"FrontPayloadAnchor","LeftSolarPanelAnchor","RightSolarPanelAnchor","BatteryAnchor","TopAntennaAnchor","StorageAnchor"};
 GameObject[] pref={app.payloadPrefabs[0],app.solarPrefabs[1],app.solarPrefabs[1],app.batteryPrefabs[1],app.radioPrefabs[1],app.storagePrefabs[1]};
 for(int n=0;n<keys.Length;n++)foreach(Transform a in ship.GetComponentsInChildren<Transform>())if(a.name==keys[n]){var instance=(GameObject)PrefabUtility.InstantiatePrefab(pref[n],a);instance.name=pref[n].name;}
 System.IO.File.WriteAllText(Root+"/Documentation/Implementation.md","# CARONTE prototype\n\nPlay scene Assets/Scenes/Prototipo.unity. All created assets belong to Assets/AssetHendrix.\n\nVisual: X width 2, Y height 3, Z depth 1; payload faces -Z. Visual dimensions are enlarged didactic proportions; NASA structure reference 100 x 226.3 x 366 mm, https://www.nasa.gov/smallsat-institute/sst-soa/structures-materials-and-mechanisms/ .\n\nReplace the prefabs assigned to CarontePrototype arrays, keeping centered local origin and anchor orientation. Technical properties live only in MissionConfig, independent of meshes. Left panel extends along -X; right anchor rotates 180 degrees.\n\nSimulation: deterministic 30 s steps for 30 days; circular Kepler period with mu=398600.4418 km3/s2, R=6371 km. Eclipse fraction 36%, 4 or 5 eight-minute daily contacts are didactic assumptions, not propagated ground visibility. Solar generation, payload, mass, budget and communication values are didactic. MB decimal, Mbps / 8 for MB/s. Storage cannot become negative; conservation captured = sent + stored + lost. No NASA launch reliability claims.\n\nHTML retained: category catalog, immediate resource checks, configuration JSON, engineering console. Modified: 6U, 30-day discrete simulation, deterministic resource failures, native 3D prefabs. Deferred: interplanetary missions, VR, multiplayer. This is an initial implementation; orbital contact geometry, advanced ADCS/thermal models and polished transitions remain pending.\n");
 AssetDatabase.SaveAssets();EditorSceneManager.MarkSceneDirty(root.scene);EditorSceneManager.SaveScene(root.scene);
 return "CARONTE scene assembled with 15 component prefabs.";
 }
}