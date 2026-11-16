using UnityEngine;using UnityEngine.Rendering;
namespace Caronte{
public class MissionDestination:MonoBehaviour{
 public CarontePrototype app;public Transform windowPlanet;public Material[] materials=new Material[6];public Material europaMaterial;
 public GameObject[] bodyModels=new GameObject[6];public GameObject europaModel;
 public string DestinationName{get;private set;}="TIERRA";public int ActiveMission{get;private set;}=-1;Transform europa,previewEuropa;
 public void ApplyMission(int mission){
  int m=Mathf.Clamp(mission,0,5);ActiveMission=m;DestinationName=new[]{"TIERRA / ÓRBITA BAJA","TIERRA / REFERENCIA DEL PUNTO L2","LUNA / ÓRBITA LUNAR","MARTE / ÓRBITA MARCIANA","JÚPITER Y EUROPA","SOL / HELIOSFERA INTERIOR"}[m];
  if(!windowPlanet)windowPlanet=transform.Find("DestinationEarth");if(!windowPlanet||!app)return;
  if(m<materials.Length&&materials[m]){windowPlanet.GetComponent<Renderer>().sharedMaterial=materials[m];app.earth.GetComponent<Renderer>().sharedMaterial=materials[m];}
  var model=bodyModels!=null&&m<bodyModels.Length?bodyModels[m]:null;
  ReplaceBody(windowPlanet,model,"DestinationBody_JX",29);ReplaceBody(app.earth.transform,model,"PreviewBody_JX",8);
  if(!europa)europa=Moon(windowPlanet,"DestinationEuropa",29);if(!previewEuropa)previewEuropa=Moon(app.earth.transform,"PreviewEuropa",8);
  foreach(var moon in new[]{europa,previewEuropa}){
   moon.localPosition=new Vector3(.76f,-.21f,.3f);moon.localScale=Vector3.one*.13f;moon.localRotation=Quaternion.Euler(10,40,0);
   moon.GetComponent<Renderer>().sharedMaterial=europaMaterial?europaMaterial:materials[2];moon.gameObject.SetActive(m==4);
   if(m==4)ReplaceBody(moon,europaModel,"EuropaBody_JX",moon.gameObject.layer);
  }
 }
 void ReplaceBody(Transform host,GameObject model,string name,int layer){
  var old=host.Find(name);if(old){old.gameObject.SetActive(false);if(Application.isPlaying)Destroy(old.gameObject);else DestroyImmediate(old.gameObject);}
  var fallback=host.GetComponent<Renderer>();if(fallback)fallback.enabled=model==null;
  if(!model)return;var instance=Instantiate(model,host,false);instance.name=name;instance.transform.localPosition=Vector3.zero;instance.transform.localRotation=Quaternion.identity;instance.transform.localScale=Vector3.one;
  foreach(var t in instance.GetComponentsInChildren<Transform>(true))t.gameObject.layer=layer;
  foreach(var r in instance.GetComponentsInChildren<Renderer>(true)){r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;}
 }
 Transform Moon(Transform parent,string name,int layer){var t=parent.Find(name);if(!t){t=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer)).transform;t.SetParent(parent,false);t.GetComponent<MeshFilter>().sharedMesh=windowPlanet.GetComponent<MeshFilter>().sharedMesh;}t.gameObject.layer=layer;return t;}
 void Update(){if(app&&app.Configuration.mission!=ActiveMission)ApplyMission(app.Configuration.mission);}
}
}