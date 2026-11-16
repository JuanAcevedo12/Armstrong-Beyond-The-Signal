using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace Caronte {
[DefaultExecutionOrder(350)]
public sealed class MissionPresentation : MonoBehaviour {
 public static readonly string[] Fragments={"El conocimiento","despierta","la curiosidad","y nos impulsa","a explorar","lo desconocido."};
 public AudioClip soundtrack,buttonSound,assemblySound,futuristicSound,explosionSound;
 public Material particleMaterial;
 public bool[] unlocked=new bool[6];
 public int UnlockedCount {get {int n=0;foreach(bool b in unlocked)if(b)n++;return n;}}
 public int ButtonSounds{get;private set;} public int OrbitalExplosions{get;private set;} public int RoomExplosions{get;private set;} public int UnlockBursts{get;private set;}
 CaronteStation station;MissionControlDirector control;StationRoomController room;OrbitalFailureController loss;StationPolish polish;
 AudioSource music,assembly,futuristic,cataclysm;AudioSource[] clicks=new AudioSource[8];int clickIndex;
 Canvas messageCanvas;Text header,phrase;Text[] cells=new Text[6];Image[] tiles=new Image[6];Image redVeil;GameObject veilRoot;
 readonly HashSet<Button> hooked=new HashSet<Button>();float nextScan,nextSpark;bool wasBusy,orbitBroken,roomBroken;Vector3 orbitPosition;float pulseUntil;
 readonly List<ParticleSystem> systems=new List<ParticleSystem>();
 void Start(){
  station=GetComponent<CaronteStation>();control=GetComponent<MissionControlDirector>();room=GetComponent<StationRoomController>();loss=GetComponent<OrbitalFailureController>();polish=GetComponent<StationPolish>();
  music=Source("Soundtrack",station.playerCamera.transform,0,.1144f,1,20);music.clip=soundtrack;music.loop=true;if(soundtrack)music.Play();
  assembly=Source("AssemblyMechanical",station.assemblyOrigin,1,.72f,2.4f,18);assembly.clip=assemblySound;assembly.loop=true;
  futuristic=Source("AssemblyFuturistic",station.assemblyOrigin,1,.55f,2.4f,18);futuristic.clip=futuristicSound;futuristic.loop=true;
  cataclysm=Source("StationExplosionFull",station.playerCamera.transform,0,1,1,100);
  for(int i=0;i<clicks.Length;i++)clicks[i]=Source("ConsoleClick"+i,transform,1,.57f,2.5f,18);
  BuildMessageScreen();BuildVeil();BuildExterior();RefreshMessage();
 }
 AudioSource Source(string name,Transform parent,float blend,float volume,float min,float max){
  var g=new GameObject(name);g.transform.SetParent(parent,false);var a=g.AddComponent<AudioSource>();a.playOnAwake=false;a.spatialBlend=blend;a.volume=volume;a.minDistance=min;a.maxDistance=max;a.rolloffMode=AudioRolloffMode.Linear;a.dopplerLevel=0;return a;
 }
 RectTransform Rect(Transform parent,string name,Vector2 pos,Vector2 size){
  var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=(RectTransform)g.transform;r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;return r;
 }
 Text Label(Transform parent,string name,string value,Vector2 pos,Vector2 size,int font){
  var r=Rect(parent,name,pos,size);var t=r.gameObject.AddComponent<Caronte.LocalizedText>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=font;t.alignment=TextAnchor.MiddleCenter;t.color=Color.white;t.raycastTarget=false;return t;
 }
 void BuildMessageScreen(){
  var root=new GameObject("RecoveredTransmissionScreen",typeof(RectTransform),typeof(Canvas));root.transform.SetParent(station.videoScreen,false);root.transform.localPosition=new Vector3(0,1.12f,-.055f);root.transform.localScale=Vector3.one*.002f;
  var rt=root.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(1200,250);messageCanvas=root.GetComponent<Canvas>();messageCanvas.renderMode=RenderMode.WorldSpace;messageCanvas.worldCamera=station.playerCamera;
  var bg=Rect(root.transform,"MessagePanel",Vector2.zero,new Vector2(1200,250)).gameObject.AddComponent<Image>();bg.color=new Color(.008f,.025f,.04f,.98f);bg.raycastTarget=false;
  header=Label(root.transform,"ProgressHeading","",new Vector2(18,-10),new Vector2(1164,36),25);
  for(int i=0;i<6;i++){var r=Rect(root.transform,"Fragment"+(i+1),new Vector2(18+i*195,-58),new Vector2(188,108));tiles[i]=r.gameObject.AddComponent<Image>();tiles[i].raycastTarget=false;cells[i]=Label(r,"FragmentText","",new Vector2(5,-4),new Vector2(178,100),23);}
  phrase=Label(root.transform,"RecoveredSentence","",new Vector2(24,-178),new Vector2(1152,60),25);
 }
 void RefreshMessage(){
  int count=UnlockedCount;header.text="TRANSMISIÓN RECUPERADA  /  "+count+" DE 6 FRAGMENTOS";string s="";
  for(int i=0;i<6;i++){tiles[i].color=unlocked[i]?new Color(.025f,.22f,.19f):new Color(.025f,.05f,.075f);cells[i].color=unlocked[i]?new Color(.4f,1,.83f):new Color(.4f,.49f,.56f);cells[i].text=(i+1).ToString("00")+" / "+(unlocked[i]?"RECUPERADO":"BLOQUEADO")+"\n"+(unlocked[i]?Fragments[i]:"···");if(unlocked[i])s+=(s.Length>0?" ":"")+Fragments[i];else s+=(s.Length>0?" ":"")+"[···]";}
  phrase.text=s;phrase.color=count==6?new Color(.5f,1,.8f):new Color(.75f,.88f,.95f);
 }
 public void UnlockFragment(int mission){
  if(mission<0||mission>=6||unlocked[mission])return;
  unlocked[mission]=true;UnlockBursts++;pulseUntil=Time.time+2.5f;RefreshMessage();
  Burst("MessageUnlockParticles",station.videoScreen.TransformPoint(new Vector3(0,1.12f,-.15f)),0,100,.3f,.75f,.012f,.035f,new Color(.15f,1,.75f),1.4f);
 }
 void BuildVeil(){
  veilRoot=new GameObject("PostExplosionRedPulse",typeof(Canvas),typeof(CanvasScaler));veilRoot.transform.SetParent(transform,false);var c=veilRoot.GetComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;c.sortingOrder=3100;
  var r=new GameObject("TransparentRedVeil",typeof(RectTransform),typeof(Image));r.transform.SetParent(veilRoot.transform,false);var rt=(RectTransform)r.transform;rt.anchorMin=Vector2.zero;rt.anchorMax=Vector2.one;rt.offsetMin=rt.offsetMax=Vector2.zero;redVeil=r.GetComponent<Image>();redVeil.raycastTarget=false;redVeil.color=new Color(.65f,0,.015f,0);veilRoot.SetActive(false);
 }
 void BuildExterior(){
  var ps=Particles("ExteriorDust",new Vector3(0,4,20),0,true,12,10,.07f,.015f,.05f,new Color(.45f,.75f,1,.55f));
  var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Box;sh.scale=new Vector3(28,12,16);ps.Play();
 }
 ParticleSystem Particles(string name,Vector3 pos,int layer,bool loop,float rate,float life,float speed,float minSize,float maxSize,Color color){
  var g=new GameObject(name);g.transform.SetParent(transform,false);g.transform.position=pos;g.layer=layer;
  var ps=g.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var m=ps.main;m.playOnAwake=false;m.loop=loop;m.duration=2;m.maxParticles=loop?240:600;m.startLifetime=new ParticleSystem.MinMaxCurve(life*.65f,life);m.startSpeed=new ParticleSystem.MinMaxCurve(speed*.45f,speed);m.startSize=new ParticleSystem.MinMaxCurve(minSize,maxSize);m.startColor=color;m.simulationSpace=ParticleSystemSimulationSpace.World;m.gravityModifier=0;
  var em=ps.emission;em.rateOverTime=rate;em.enabled=loop;var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.12f;
  var colors=ps.colorOverLifetime;colors.enabled=true;var grad=new Gradient();grad.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.12f),new GradientAlphaKey(.6f,.65f),new GradientAlphaKey(0,1)});colors.color=new ParticleSystem.MinMaxGradient(grad);
  var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
  var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=particleMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;renderer.maxParticleSize=.25f;
  systems.Add(ps);return ps;
 }
 void Burst(string name,Vector3 pos,int layer,int count,float radius,float speed,float min,float max,Color color,float life){
  var ps=Particles(name,pos,layer,false,0,life,speed,min,max,color);var sh=ps.shape;sh.radius=radius;ps.Emit(count);Destroy(ps.gameObject,life+.4f);
 }
 void BindButtons(){
  foreach(var b in station.app.GetComponentsInChildren<Button>(true))Hook(b);
  foreach(var b in station.GetComponentsInChildren<Button>(true))Hook(b);
 }
 void Hook(Button b){if(!hooked.Add(b))return;var button=b;b.onClick.AddListener(()=>{if(!buttonSound||!button)return;var a=clicks[clickIndex++%clicks.Length];var cv=button.GetComponentInParent<Canvas>();a.transform.position=MobileRuntime.Active&&cv&&cv.renderMode==RenderMode.ScreenSpaceOverlay?station.selectionScreen.position:button.transform.position;a.PlayOneShot(buttonSound);ButtonSounds++;});}
 public void PlayOrbitalExplosion(Vector3 position){
  OrbitalExplosions++;var a=Source("CanSatExplosion3D",transform,1,1,12,650);a.transform.position=position;a.PlayOneShot(explosionSound);Destroy(a.gameObject,explosionSound?explosionSound.length+1:4);
  Burst("OrbitalFireball",position,0,220,1,5,.3f,1.8f,new Color(1,.25f,.02f),2.5f);
  Burst("OrbitalHotSparks",position,0,220,.6f,9,.05f,.18f,new Color(1,.75f,.15f),4);
  if(station.app.spacecraft)Burst("OrbitalMonitorExplosion",station.app.spacecraft.position,8,150,.2f,1,.03f,.18f,new Color(1,.3f,.02f),2.5f);
 }
 public void PlayRoomExplosion(){
  RoomExplosions++;roomBroken=true;assembly.Stop();futuristic.Stop();music.volume=.0264f;veilRoot.SetActive(true);
  cataclysm.PlayOneShot(explosionSound);
  Vector3 center=new Vector3(0,1.8f,3);
  Burst("StationFireball",center,0,350,2.5f,7,.12f,.65f,new Color(1,.2f,.01f),3);
  Burst("StationSparks",center,0,500,3,9,.015f,.065f,new Color(1,.75f,.2f),5);
  for(int i=0;i<6;i++){var p=center+new Vector3((i%3-1)*2.2f,-.8f,(i/3-.5f)*3);var ps=Particles("StationPersistentFire"+i,p,0,true,23,2.1f,.9f,.1f,.4f,new Color(1,.18f,.015f,.8f));var flame=ps.main;flame.startSize3D=true;flame.startSizeX=new ParticleSystem.MinMaxCurve(.12f,.3f);flame.startSizeY=new ParticleSystem.MinMaxCurve(.4f,.9f);flame.startSizeZ=.1f;var sh=ps.shape;sh.shapeType=ParticleSystemShapeType.Cone;sh.angle=16;sh.radius=.25f;ps.transform.rotation=Quaternion.Euler(-90,0,0);ps.Play();}
  var embers=Particles("StationFloatingEmbers",center,0,true,28,5,1.2f,.012f,.04f,new Color(1,.6f,.1f));var es=embers.shape;es.radius=4;embers.Play();
 }
 void LateUpdate(){
  if(!station)return;
  if(Time.time>=nextScan){nextScan=Time.time+.5f;BindButtons();hooked.RemoveWhere(b=>!b);systems.RemoveAll(p=>!p);}
  if(room&&room.IsPaused)return;
  if(polish&&polish.WindowCraft&&polish.WindowCraft.gameObject.activeInHierarchy)orbitPosition=polish.WindowCraft.position;
  if(control.MessageDecoded){UnlockFragment(control.AssignedMission);var decoded=station.videoScreen.Find("OrbitalVideoCanvas/DecodedTransmission");if(decoded)foreach(var t in decoded.GetComponentsInChildren<Text>())if(t.fontSize==38)t.text=Fragments[control.AssignedMission];}
  if(header&&Time.time<pulseUntil)header.color=Color.Lerp(new Color(.3f,1,.7f),Color.white,.5f+.5f*Mathf.Sin(Time.time*9));
  bool busy=station.Busy&&!room.RoomDestroyed;
  if(busy!=wasBusy){wasBusy=busy;if(busy){if(assemblySound)assembly.Play();if(futuristicSound)futuristic.Play();}else{assembly.Stop();futuristic.Stop();if(station.Assembled)Burst("FabricationComplete",station.assemblyOrigin.position,0,120,.25f,1,.015f,.04f,new Color(.2f,1,.85f),1.5f);}}
  if(busy&&station.laserWelder&&station.laserWelder.Firing&&Time.time>=nextSpark){nextSpark=Time.time+.12f;Burst("WeldHotSparks",station.laserWelder.LastTarget,0,20,.035f,1.6f,.008f,.025f,new Color(1,.65f,.12f),.65f);}
  if(loss.Destroyed&&!orbitBroken){orbitBroken=true;PlayOrbitalExplosion(orbitPosition==Vector3.zero?station.app.spacecraft.position:orbitPosition);}
  if(!loss.Destroyed)orbitBroken=false;
  if(room.RoomDestroyed&&!roomBroken)PlayRoomExplosion();
  if(roomBroken&&redVeil)redVeil.color=new Color(.65f,.005f,.015f,.09f+.17f*(.5f+.5f*Mathf.Sin(Time.time*2.5f)));
  if(music&&!roomBroken)music.volume=Mathf.MoveTowards(music.volume,control.radioSource&&control.radioSource.isPlaying?.0572f:.1144f,Time.unscaledDeltaTime*.15f);
 }
 void OnDestroy(){foreach(var ps in systems)if(ps)Destroy(ps.gameObject);}
}
}
