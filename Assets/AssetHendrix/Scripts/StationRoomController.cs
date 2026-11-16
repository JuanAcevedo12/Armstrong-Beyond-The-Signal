using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.UI;using UnityEngine.InputSystem;using UnityEngine.SceneManagement;
namespace Caronte{
[DefaultExecutionOrder(-250)]
public class StationRoomController:MonoBehaviour{
 public CaronteStation station;public bool IsPaused{get;private set;}public bool RoomDestroyed{get;private set;}public int FloatingCount{get;private set;}
 GameObject overlay,pausePanel,debris;Button continueButton;Text pauseHint,pauseTitle;public bool RestartRequired{get;private set;}bool altWasHeld;float previousScale=1;bool previousAudioPause;
 InputAction escapeAction; void OnEnable(){if(!Application.isPlaying)return;escapeAction=new InputAction("StationPause",InputActionType.Button,"<Keyboard>/escape");escapeAction.performed+=OnEscapeAction;escapeAction.Enable();} void OnDisable(){if(escapeAction!=null){escapeAction.performed-=OnEscapeAction;escapeAction.Dispose();escapeAction=null;}} void OnEscapeAction(InputAction.CallbackContext context){HandleEscape();} public void HandleEscape(){if(IsPaused){if(!RestartRequired)SetPaused(false);}else if(RestartRequired)SetPaused(true);else if(station.ConsoleActive)station.SetConsole(false);else SetPaused(true);} void Start(){BuildUI();}
 RectTransform Rect(Transform parent,string name,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(parent,false);var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=anchor;r.pivot=pivot;r.anchoredPosition=pos;r.sizeDelta=size;return r;}
 Text Text(Transform p,string value,Vector2 pos,Vector2 size,int font){var r=Rect(p,"Label",new Vector2(.5f,1),new Vector2(.5f,1),pos,size);var t=r.gameObject.AddComponent<Caronte.LocalizedText>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=font;t.alignment=TextAnchor.MiddleCenter;t.color=new Color(.85f,.94f,1);t.raycastTarget=false;return t;}
 Button MenuButton(Transform p,string label,float y,System.Action action){var r=Rect(p,label,new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,y),new Vector2(360,62));r.gameObject.AddComponent<Image>().color=new Color(.07f,.2f,.29f);var b=r.gameObject.AddComponent<Button>();b.onClick.AddListener(()=>action());Text(r,label,new Vector2(0,-4),new Vector2(340,52),26);return b;}
 public void BuildUI(){
 if(overlay)Destroy(overlay);overlay=new GameObject("StationGlobalUI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var cv=overlay.GetComponent<Canvas>();cv.renderMode=RenderMode.ScreenSpaceOverlay;cv.sortingOrder=4000;
 var scale=overlay.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1920,1080);scale.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
 pausePanel=new GameObject("PauseMenu",typeof(RectTransform),typeof(Image));pausePanel.transform.SetParent(overlay.transform,false);var pr=pausePanel.GetComponent<RectTransform>();pr.anchorMin=Vector2.zero;pr.anchorMax=Vector2.one;pr.offsetMin=pr.offsetMax=Vector2.zero;pausePanel.GetComponent<Image>().color=new Color(.005f,.015f,.03f,.9f);
 var card=Rect(pausePanel.transform,"PauseCard",new Vector2(.5f,.5f),new Vector2(.5f,.5f),Vector2.zero,new Vector2(480,450));card.gameObject.AddComponent<Image>().color=new Color(.035f,.08f,.13f,.98f);pauseTitle=Text(card,"PAUSA",new Vector2(0,-28),new Vector2(420,55),34);continueButton=MenuButton(card,"Continuar",-112,()=>SetPaused(false));MenuButton(card,"Reiniciar",-193,Restart);MenuButton(card,"Salir",-274,Exit);pauseHint=Text(card,"Esc para continuar",new Vector2(0,-370),new Vector2(420,40),20);pausePanel.SetActive(false);
 }
 void Update(){
 var k=Keyboard.current;if(k==null)return;
 
 if(IsPaused)return;
 bool alt=k.leftAltKey.isPressed||k.rightAltKey.isPressed;
 if(!station.ConsoleActive){if(alt){Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}else if(altWasHeld){Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}}
 altWasHeld=alt;
 }
 public void SetPaused(bool pause){
 if(!pause&&RestartRequired)return;if(IsPaused==pause)return;IsPaused=pause;if(pause){previousScale=Time.timeScale;previousAudioPause=AudioListener.pause;Time.timeScale=0;AudioListener.pause=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}else{Time.timeScale=previousScale>0?previousScale:1;AudioListener.pause=previousAudioPause;Cursor.lockState=MobileRuntime.Active||station.ConsoleActive?CursorLockMode.None:CursorLockMode.Locked;Cursor.visible=MobileRuntime.Active||station.ConsoleActive;}
 if(pausePanel)pausePanel.SetActive(pause);var hud=station.app.transform.Find("StationHUD");if(hud)hud.gameObject.SetActive(!pause);
 }
 public void RequireRestart(){RestartRequired=true;if(continueButton)continueButton.gameObject.SetActive(false);if(pauseHint)pauseHint.text="Reinicie o salga de la operación.";if(pauseTitle){pauseTitle.text="MISIÓN FALLIDA";pauseTitle.color=new Color(1,.3f,.2f);}if(pausePanel){var restart=pausePanel.transform.Find("PauseCard/Reiniciar");if(restart)restart.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,MobileRuntime.Active?-160:-140);var exit=pausePanel.transform.Find("PauseCard/Salir");if(exit)exit.GetComponent<RectTransform>().anchoredPosition=new Vector2(0,MobileRuntime.Active?-280:-220);}} public void Restart(){Time.timeScale=1;AudioListener.pause=false;string path=SceneManager.GetActiveScene().path;
 #if UNITY_EDITOR
 UnityEditor.SceneManagement.EditorSceneManager.LoadSceneInPlayMode(path,new LoadSceneParameters(LoadSceneMode.Single));
 #else
 SceneManager.LoadScene(SceneManager.GetActiveScene().name);
 #endif
 }
 public void Exit(){Time.timeScale=1;AudioListener.pause=false;if(MobileRuntime.Active){SceneManager.LoadScene("Home");return;}
 #if UNITY_EDITOR
 UnityEditor.EditorApplication.isPlaying=false;
 #else
 Application.Quit();
 #endif
 }
 bool Excluded(Transform t){for(var p=t;p!=null;p=p.parent){if(p==station.player.transform||p==station.app.transform)return true;string n=p.name;if(n=="DestinationEarth"||n=="Starfield"||(n.StartsWith("WindowOrbit")||n=="WindowSatelliteOrbit")||n.StartsWith("OrbitalDebris")||n.StartsWith("MissionExplosion"))return true;}return false;}
 public void ExplodeRoom(){
 if(RoomDestroyed)return;if(IsPaused)SetPaused(false);RoomDestroyed=true;station.EnterRoomCataclysm();station.app.enabled=false;
 var polish=station.GetComponent<StationPolish>();if(polish)polish.enabled=false;
 var fail=station.GetComponent<OrbitalFailureController>();if(fail)fail.enabled=false;
 var live=station.GetComponent<LiveTelemetryPanel>();if(live)live.enabled=false;
 var design=station.GetComponent<MissionDesignPanel>();if(design)design.enabled=false;
 var dest=station.GetComponent<MissionDestination>();if(dest)dest.enabled=false;
 debris=new GameObject("ZeroGravityRoomDebris");var list=new HashSet<Transform>();
 foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None)){if(r is ParticleSystemRenderer)continue;if(r.gameObject.scene!=gameObject.scene||r.gameObject.layer==8||Excluded(r.transform))continue;list.Add(r.transform);}
 foreach(var canvas in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)){if(canvas.renderMode==RenderMode.WorldSpace&&canvas.gameObject.scene==gameObject.scene)list.Add(canvas.transform);}
 foreach(var l in station.GetComponentsInChildren<Light>())list.Add(l.transform);
 var rng=new System.Random(981);Vector3 center=new Vector3(0,1.6f,3);
 foreach(var t in list){if(!t)continue;t.SetParent(debris.transform,true);var f=t.GetComponent<OrbitalFragment>();if(!f)f=t.gameObject.AddComponent<OrbitalFragment>();Vector3 dir=(t.position-center).normalized;f.velocity=dir*.18f+new Vector3((float)rng.NextDouble()*.3f-.15f,(float)rng.NextDouble()*.24f+.06f,(float)rng.NextDouble()*.3f-.15f);f.spin=new Vector3((float)rng.NextDouble()*12-6,(float)rng.NextDouble()*16-8,(float)rng.NextDouble()*12-6);FloatingCount++;}
 foreach(var ps in Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None)){var settings=ps.main;settings.gravityModifier=0;}StartCoroutine(Explosion(center,polish?polish.particleMaterial:null));
 }
 IEnumerator Explosion(Vector3 p,Material mat){
 var go=new GameObject("RoomExplosion");go.transform.position=p;var ps=go.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.loop=false;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(.4f,1.5f);main.startSpeed=new ParticleSystem.MinMaxCurve(2,5);main.startSize=new ParticleSystem.MinMaxCurve(.03f,.16f);main.gravityModifier=0;main.startColor=new ParticleSystem.MinMaxGradient(new Color(1,.12f,0),new Color(1,.8f,.3f));main.simulationSpace=ParticleSystemSimulationSpace.World;
 var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=1;ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=mat;ps.Emit(260);Destroy(go,2);
 var light=new GameObject("RoomFlash");light.transform.position=p;var l=light.AddComponent<Light>();l.type=LightType.Point;l.range=18;l.color=new Color(1,.4f,.08f);for(float t=0;t<1;t+=Time.deltaTime/.8f){l.intensity=(1-t)*22;yield return null;}Destroy(light);
 }
 void OnDestroy(){Time.timeScale=1;AudioListener.pause=false;if(overlay)Destroy(overlay);if(debris)Destroy(debris);}
}}
