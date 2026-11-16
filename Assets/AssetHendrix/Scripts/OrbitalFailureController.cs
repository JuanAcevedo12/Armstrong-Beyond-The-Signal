using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;
namespace Caronte{
public class OrbitalFailureController:MonoBehaviour{
 public CarontePrototype app;public CaronteStation station;public bool Destroyed{get;private set;}public string Alert{get;private set;}="";
 public int FragmentCount{get{return fragments.Count;}}List<GameObject> fragments=new List<GameObject>();List<GameObject> effects=new List<GameObject>();
 Text alertText;GameObject banner;bool started;float visualPhase;bool warnedEnergy;bool permanentLinkLoss;public bool DesignFault {get;private set;}
 void EnsureBanner(){var hud=app.transform.Find("StationHUD");if(!hud||banner)return;
 banner=new GameObject("OrbitAlertBanner",typeof(RectTransform),typeof(Image));banner.transform.SetParent(hud,false);var rt=banner.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=new Vector2(.5f,1);rt.pivot=new Vector2(.5f,1);rt.anchoredPosition=new Vector2(0,-24);rt.sizeDelta=new Vector2(1000,68);banner.GetComponent<Image>().color=new Color(.23f,.015f,.025f,.94f);banner.GetComponent<Image>().raycastTarget=false;
 var label=new GameObject("AlertMessage",typeof(RectTransform),typeof(Caronte.LocalizedText));label.transform.SetParent(banner.transform,false);var r=label.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(12,5);r.offsetMax=new Vector2(-12,-5);alertText=label.GetComponent<Text>();alertText.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");alertText.fontSize=22;alertText.alignment=TextAnchor.MiddleCenter;alertText.color=new Color(1,.65f,.55f);alertText.raycastTarget=false;banner.SetActive(false);
 }
 public void OnLaunch(){ResetFailure();started=true;DesignFault=HasFault(app.Configuration);if(station.ConsoleActive)station.SetConsole(false);Alert=DesignFault?"LANZAMIENTO EXPERIMENTAL · Diseño con restricciones. Observa la telemetría.":"";}
 bool HasFault(MissionConfig c){return c.OperationalRisk;}
 void LateUpdate(){
 var room=GetComponent<StationRoomController>();if(room&&room.IsPaused)return;EnsureBanner();if(!started)return;var e=app.CurrentEngine;
 if(e!=null&&app.Stage==3&&!Destroyed){
 bool low=!e.Powered||e.Energy<app.Configuration.Capacity*.05f;if(!low&&e.Energy>app.Configuration.Capacity*.15f)warnedEnergy=false;
 if(low){warnedEnergy=true;Alert="ALERTA · ENERGÍA AGOTADA / PÉRDIDA DE CONEXIÓN · El satélite continúa a la deriva orbital.";}
 else if(!e.ElectronicsAlive){Alert="ALERTA · COMPUTADORA FUERA DE SERVICIO / PÉRDIDA DE CONEXIÓN POR RADIACIÓN";}else if(app.Configuration.LinkQuality<.12f){permanentLinkLoss=true;Alert="ALERTA · PÉRDIDA DE CONEXIÓN · Antena o estación insuficiente para esta distancia.";}
 else if(app.Configuration.ScienceQuality==0)Alert="ALERTA · DATOS CIENTÍFICOS NO VÁLIDOS · Revisa instrumento, actitud y control térmico.";
 else if(!app.Configuration.CanLaunch)Alert="ALERTA · OPERACIÓN FUERA DE DISEÑO · "+app.Configuration.Review;
 else if(!warnedEnergy&&!permanentLinkLoss)Alert="";
 var orbitVisual=station.GetComponent<StationPolish>();visualPhase=orbitVisual?orbitVisual.VisualPhase:(float)e.VisualAngle;
 app.spacecraft.position=new Vector3(Mathf.Cos(visualPhase)*4.1f,Mathf.Sin(visualPhase)*1.5f,Mathf.Sin(visualPhase)*3.5f);
 var tint=new MaterialPropertyBlock();foreach(var r in app.spacecraft.GetComponentsInChildren<Renderer>()){if(!r.sharedMaterial)continue;r.GetPropertyBlock(tint);tint.SetColor("_BaseColor",r.sharedMaterial.color*(e.Eclipse?.25f:1));r.SetPropertyBlock(tint);}if(warnedEnergy)app.spacecraft.rotation=Quaternion.Euler(Time.time*7,Time.time*10,Time.time*4);else app.spacecraft.LookAt(Vector3.zero);
 }
 if(banner){banner.SetActive(!string.IsNullOrEmpty(Alert)&&!(app.Stage==4&&!Destroyed&&GetComponent<MissionControlDirector>()));alertText.text=Alert;alertText.color=Color.Lerp(new Color(1,.5f,.4f),new Color(1,.85f,.7f),.5f+.5f*Mathf.Sin(Time.unscaledTime*2));}
 if(!string.IsNullOrEmpty(Alert)){app.status.text=Alert;app.status.color=new Color(1,.5f,.4f);}
 }
 public void OnMissionEnd(MissionResult r){if(r.missionLost){r.missionLost=true;r.failureReason=app.Configuration.Review;DesignFault=true;BreakApart();}else{Alert="MISIÓN COMPLETADA · Nave estable y conexión operativa.";if(banner)banner.GetComponent<Image>().color=new Color(.015f,.19f,.12f,.94f);}}
 public void BreakApart(){
 if(Destroyed)return;Destroyed=true;Alert="ALERTA CRÍTICA · EXPLOSIÓN / PÉRDIDA TOTAL DE CONEXIÓN · Fragmentos sin gravedad.";
 ExplodeRepresentation(app.spacecraft,8);
 var fx=station.GetComponent<StationPolish>();if(fx&&fx.WindowCraft)ExplodeRepresentation(fx.WindowCraft,0);
 if(station.physicalCraft)station.physicalCraft.gameObject.SetActive(false);
 }
 void ExplodeRepresentation(Transform craft,int layer){
 var meshes=craft.GetComponentsInChildren<MeshFilter>(true);var rng=new System.Random(731+layer);Vector3 center=craft.position;int count=0;
 foreach(var mf in meshes){var renderer=mf.GetComponent<Renderer>();if(!renderer||!renderer.enabled||!mf.gameObject.activeInHierarchy)continue;
 var go=new GameObject("OrbitalDebris_"+count);go.layer=layer;go.transform.SetParent(transform,false);go.transform.position=mf.transform.position;go.transform.rotation=mf.transform.rotation;go.transform.localScale=mf.transform.lossyScale;go.AddComponent<MeshFilter>().sharedMesh=mf.sharedMesh;go.AddComponent<MeshRenderer>().sharedMaterials=renderer.sharedMaterials;
 var d=go.AddComponent<OrbitalFragment>();Vector3 outward=(go.transform.position-center).normalized;if(outward.sqrMagnitude<.1f)outward=Vector3.up;
 d.velocity=(outward+new Vector3((float)rng.NextDouble()-.5f,(float)rng.NextDouble()-.5f,(float)rng.NextDouble()-.5f))*(layer==8?.1f:.18f);
 d.spin=new Vector3((float)rng.NextDouble()*30-15,(float)rng.NextDouble()*30-15,(float)rng.NextDouble()*30-15);fragments.Add(go);if(++count>110)break;
 }
 craft.gameObject.SetActive(false);Explosion(center,layer);
 }
 void Explosion(Vector3 center,int layer){
 var go=new GameObject("MissionExplosion");go.transform.SetParent(transform,false);go.transform.position=center;go.layer=layer;effects.Add(go);var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
 var main=ps.main;main.loop=false;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(.35f,1.4f);main.startSpeed=new ParticleSystem.MinMaxCurve(.3f,1.1f);main.startSize=new ParticleSystem.MinMaxCurve(.035f,.16f);main.gravityModifier=0;main.startColor=new ParticleSystem.MinMaxGradient(new Color(1,.15f,.01f),new Color(1,.9f,.4f));main.simulationSpace=ParticleSystemSimulationSpace.World;
 var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.18f;var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
 ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=station.GetComponent<StationPolish>().particleMaterial;ps.Emit(100);StartCoroutine(ExplosionFlash(center,layer));
 }
 IEnumerator ExplosionFlash(Vector3 p,int layer){var go=new GameObject("ExplosionFlash");go.transform.SetParent(transform,false);go.transform.position=p;effects.Add(go);var l=go.AddComponent<Light>();l.type=LightType.Point;l.cullingMask=1<<layer;l.range=4;l.color=new Color(1,.4f,.05f);for(float t=0;t<1;t+=Time.deltaTime/.7f){l.intensity=(1-t)*14;yield return null;}Destroy(go);}
 public void ResetFailure(){StopAllCoroutines();foreach(var p in fragments)if(p)Destroy(p);fragments.Clear();foreach(var p in effects)if(p)Destroy(p);effects.Clear();Destroyed=false;DesignFault=false;started=false;warnedEnergy=false;permanentLinkLoss=false;visualPhase=0;Alert="";app.spacecraft.gameObject.SetActive(true);var fx=station?station.GetComponent<StationPolish>():null;if(fx&&fx.WindowCraft)fx.WindowCraft.gameObject.SetActive(true);if(app.status)app.status.color=Color.white;if(banner){banner.GetComponent<Image>().color=new Color(.23f,.015f,.025f,.94f);banner.SetActive(false);}}
 }
}