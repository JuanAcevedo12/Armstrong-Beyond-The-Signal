using System.Collections;using UnityEngine;
namespace Caronte{
public class CatalogVisuals:MonoBehaviour{
 public CarontePrototype app;public GameObject[] prefabs=new GameObject[33];
 public static readonly string[] Anchors={"PlatformAssembly","InstrumentAssembly","EnergyAssembly","BatteryAssembly","PropulsionAssembly","AntennaAssembly","AttitudeAssembly","ThermalAssembly","ComputerAssembly"};
 static readonly Vector3[] Positions={Vector3.zero,new Vector3(0,.6f,-.65f),new Vector3(0,0,.1f),new Vector3(-.4f,-.8f,0),new Vector3(0,-1.6f,0),new Vector3(0,1.6f,0),new Vector3(.4f,.1f,.3f),new Vector3(0,0,.65f),new Vector3(.4f,-.8f,0)};
 Transform root;int[] installed={-1,-1,-1,-1,-1,-1,-1,-1,-1};bool secondary;
 void Start(){Sync();}
 public void Sync(){
 if(!root){var previous=app.spacecraft.Find("CatalogRoot");if(previous)DestroyImmediate(previous.gameObject);foreach(var r in app.spacecraft.GetComponentsInChildren<Renderer>())r.enabled=false;root=new GameObject("CatalogRoot").transform;root.SetParent(app.spacecraft,false);for(int i=0;i<9;i++){var a=new GameObject(Anchors[i]).transform;a.SetParent(root,false);a.localPosition=Positions[i];}}
 for(int i=0;i<9;i++){int choice=app.Configuration.choices[i];if(installed[i]==choice)continue;var anchor=root.Find(Anchors[i]);for(int j=anchor.childCount-1;j>=0;j--)DestroyImmediate(anchor.GetChild(j).gameObject);var go=Instantiate(prefabs[i*3+choice],anchor);go.name=prefabs[i*3+choice].name;foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=8;installed[i]=choice;if(Application.isPlaying)StartCoroutine(Appear(go.transform));}
 bool need=app.Configuration.secondInstrument&&app.Configuration.choices[1]!=2;if(secondary!=need){var old=root.Find("SecondaryInstrument");if(old)DestroyImmediate(old.gameObject);if(need){var a=new GameObject("SecondaryInstrument").transform;a.SetParent(root,false);a.localPosition=new Vector3(.8f,.5f,-.6f);var go=Instantiate(prefabs[5],a);foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=8;}secondary=need;}
 FitHardware();
 }
 void FitHardware(){
 if(!root||!app)return;
 var choices=app.Configuration.choices;
 Bounds body=PrefabBounds(prefabs[choices[0]]);
 Bounds antenna=PrefabBounds(prefabs[5*3+choices[5]]);
 Bounds engine=PrefabBounds(prefabs[4*3+choices[4]]);
 var a=root.Find(Anchors[5]);var e=root.Find(Anchors[4]);
 var pos=a.localPosition;pos.y=body.max.y+.015f-antenna.min.y;a.localPosition=pos;
 pos=e.localPosition;pos.y=body.min.y-.015f-engine.max.y;e.localPosition=pos;
 }
 Bounds PrefabBounds(GameObject prefab){
 bool has=false;Bounds result=new Bounds(Vector3.zero,Vector3.zero);
 foreach(var mf in prefab.GetComponentsInChildren<MeshFilter>(true)){
 if(!mf.sharedMesh)continue;var b=mf.sharedMesh.bounds;
 var matrix=prefab.transform.worldToLocalMatrix*mf.transform.localToWorldMatrix;
 for(int i=0;i<8;i++){var point=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));point=matrix.MultiplyPoint3x4(point);if(!has){result=new Bounds(point,Vector3.zero);has=true;}else result.Encapsulate(point);}
 }return result;
 }

 IEnumerator Appear(Transform t){var end=t.localPosition;for(float p=0;p<1;p+=Time.deltaTime/.6f){if(!t)yield break;t.localPosition=end+Vector3.up*(1-Mathf.SmoothStep(0,1,p))*1.5f;t.localRotation=Quaternion.Euler(0,(1-p)*45,0);yield return null;}if(t){t.localPosition=end;t.localRotation=Quaternion.identity;}}
 public Transform Snapshot(Transform parent){Sync();var go=Instantiate(root.gameObject,parent);go.name="PhysicalCatalogCraft";go.transform.localPosition=Vector3.zero;go.transform.localRotation=Quaternion.Euler(0,20,0);go.transform.localScale=Vector3.one*.4f;foreach(var t in go.GetComponentsInChildren<Transform>())t.gameObject.layer=0;foreach(Transform anchor in go.transform)foreach(Transform module in anchor){module.localPosition=Vector3.zero;module.localRotation=Quaternion.identity;}return go.transform;}
}}
