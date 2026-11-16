using System.Collections;using UnityEngine;using UnityEngine.UI;
namespace Caronte{
public enum ControlPhase{Briefing,Designing,Launched,Verifying,SuccessHold,Completed,TimedOut,Failed}
[DefaultExecutionOrder(100)]
public class MissionControlDirector:MonoBehaviour{
 public CarontePrototype app;public CaronteStation station;public StationRoomController room;
 public float[] launchWindows={90,85,80,75,70,60};
 public float followupSeconds=20,orbitVerificationSeconds=18;public float destructionWarningSeconds=40; public bool AwaitingContinuation{get;private set;}public bool MessageDecoded{get;private set;}public string FailureReason{get;private set;}="";public string FailureDetails{get;private set;}="";public float OrbitStartedAt{get;private set;}public string DecodedMessage{get{return MissionPresentation.Fragments[Mathf.Clamp(AssignedMission,0,5)];}} MissionResult pendingResult;float destructionAt;
 public AudioClip[] englishTutorialRecordings=new AudioClip[6],englishMissionRecordings=new AudioClip[6],englishVoiceClips=new AudioClip[52];
 public AudioClip TutorialAudio(int i){var a=GameLanguage.English?englishTutorialRecordings:tutorialRecordings;return a!=null&&i>=0&&i<a.Length?a[i]:null;}
 public AudioClip MissionAudio(int i){var a=GameLanguage.English?englishMissionRecordings:missionRecordings;return a!=null&&i>=0&&i<a.Length?a[i]:null;}
 public AudioClip VoiceAudio(int i){var a=GameLanguage.English?englishVoiceClips:voiceClips;return a!=null&&i>=0&&i<a.Length?a[i]:null;}
 public AudioClip[] tutorialRecordings=new AudioClip[6],missionRecordings=new AudioClip[6];public int GeneralFailureCalls{get;private set;}public bool LaunchCallPlaying{get{return Phase==ControlPhase.Launched&&radioSource&&radioSource.isPlaying;}}public AudioSource radioSource;public AudioClip[] voiceClips=new AudioClip[49];public Material fireMaterial;
 public ControlPhase Phase{get;private set;}public int AssignedMission{get;private set;}
 public bool MissionConfirmed{get;private set;}public float Remaining{get;private set;}
 public float LastSuccessAt{get;private set;}public float NextContactAt{get;private set;}
 public string CurrentSubtitle{get;private set;}="";public bool Terminal{get{return Phase==ControlPhase.TimedOut||Phase==ControlPhase.Failed;}}
 public bool IsTutorial{get{return AssignedMission==0;}}public int TutorialCategory{get;private set;}=-1;public bool TutorialFinished{get;private set;}public AudioClip tutorialCompleteVoice;
 public bool CanAssemble{get{return (Phase==ControlPhase.Designing||(IsTutorial&&Phase==ControlPhase.Briefing))&&MissionConfirmed&&app.Configuration.mission==AssignedMission&&Remaining>0&&!room.RoomDestroyed;}}
 public bool CanLaunch{get{return CanAssemble;}}
 Canvas canvas;CanvasGroup subtitleGroup;Text subtitle,timer,timerHeading,objective,successText;GameObject successBanner;
 Coroutine narrative;GameObject fxRoot;Vector3 lastShake;float failureAt;bool initialized;
 static readonly string[] Calls={
 "Armstrong... Armstrong. Aquí Control de Misión. Confirme que me recibe.",
 "Armstrong, su señal sigue firme. Ahora necesitamos mirar más lejos.",
 "Armstrong, el siguiente destino es la Luna. La aproximación no admite errores.",
 "Armstrong, ponemos rumbo a Marte. Esta vez la distancia será parte del enemigo.",
 "Armstrong, entramos en territorio de Júpiter. La radiación no dará tregua.",
 "Armstrong, esta es la última orden. Vamos a acercarnos al Sol."
 };
 static readonly string[] Orders={
 "Prepárese para la primera misión. Observación terrestre. Necesitamos esas imágenes de vuelta.",
 "Su segunda misión es un telescopio espacial. Mantenga fría la óptica y estable la nave.",
 "Ensamble un orbitador lunar. Debe frenar al llegar y resistir cada paso por la sombra.",
 "Prepare el orbitador de Marte. Energía y comunicaciones decidirán cuánto podremos recuperar.",
 "Prepare Júpiter y Europa. Necesitamos imágenes y mediciones de campos y partículas.",
 "Prepare la sonda solar. Proteja la electrónica, elija su propulsión y no retire el escudo."
 };
 void Start(){
 if(!radioSource){radioSource=gameObject.AddComponent<AudioSource>();radioSource.playOnAwake=false;radioSource.spatialBlend=0;radioSource.volume=.8f;var filter=gameObject.AddComponent<AudioLowPassFilter>();filter.cutoffFrequency=3200;filter.lowpassResonanceQ=1;}
 if(voiceClips.Length<52)System.Array.Resize(ref voiceClips,52);if(tutorialCompleteVoice)voiceClips[48]=tutorialCompleteVoice;radioSource.Stop();radioSource.mute=false;BuildOverlay();initialized=true;AssignedMission=0;BeginBriefing();
 }
 RectTransform Rect(Transform p,string name,Vector2 anchor,Vector2 pivot,Vector2 pos,Vector2 size){var go=new GameObject(name,typeof(RectTransform));go.transform.SetParent(p,false);var rt=go.GetComponent<RectTransform>();rt.anchorMin=rt.anchorMax=anchor;rt.pivot=pivot;rt.anchoredPosition=pos;rt.sizeDelta=size;return rt;}
 Text Label(Transform p,string value,Vector2 anchor,Vector2 pos,Vector2 size,int font,Color color,TextAnchor align){var rt=Rect(p,"Text",anchor,anchor,pos,size);var t=rt.gameObject.AddComponent<Caronte.LocalizedText>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=font;t.color=color;t.alignment=align;t.raycastTarget=false;t.resizeTextForBestFit=true;t.resizeTextMinSize=18;t.resizeTextMaxSize=font;return t;}
 void BuildOverlay(){
 var root=new GameObject("MissionControlOverlay",typeof(Canvas),typeof(CanvasScaler));root.transform.SetParent(transform,false);canvas=root.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=3200;var scaler=root.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
 var clock=Rect(root.transform,"LaunchDeadline",new Vector2(0,1),new Vector2(0,1),new Vector2(24,-22),new Vector2(470,150));clock.gameObject.AddComponent<Image>().color=new Color(.025f,.045f,.07f,.88f);clock.GetComponent<Image>().raycastTarget=false;
 timerHeading=Label(clock,"CONTROL DE MISIÓN / 01",new Vector2(0,1),new Vector2(14,-8),new Vector2(445,28),18,new Color(.65f,.8f,.9f),TextAnchor.MiddleLeft);
 timer=Label(clock,"--:--",new Vector2(0,1),new Vector2(14,-34),new Vector2(445,64),48,new Color(1,.18f,.14f),TextAnchor.MiddleLeft);
 objective=Label(clock,"",new Vector2(0,1),new Vector2(14,-106),new Vector2(445,38),19,Color.white,TextAnchor.UpperLeft);
 var box=Rect(root.transform,"IntercomSubtitles",new Vector2(.5f,0),new Vector2(.5f,0),new Vector2(0,100),new Vector2(1180,118));box.gameObject.AddComponent<Image>().color=new Color(.012f,.026f,.045f,.94f);box.GetComponent<Image>().raycastTarget=false;subtitleGroup=box.gameObject.AddComponent<CanvasGroup>();subtitleGroup.blocksRaycasts=false;
 Label(box,"CONTROL DE MISIÓN / INTERCOM",new Vector2(0,1),new Vector2(18,-8),new Vector2(1140,24),18,new Color(.38f,.82f,1),TextAnchor.MiddleLeft);
 subtitle=Label(box,"",new Vector2(0,1),new Vector2(18,-38),new Vector2(1140,70),27,new Color(.9f,.96f,1),TextAnchor.UpperLeft);subtitleGroup.alpha=0;
 var success=Rect(root.transform,"MissionSuccess",new Vector2(.5f,1),new Vector2(.5f,1),new Vector2(0,-24),new Vector2(740,78));success.gameObject.AddComponent<Image>().color=new Color(.02f,.15f,.095f,.96f);success.GetComponent<Image>().raycastTarget=false;successBanner=success.gameObject;
 successText=Label(success,"MISIÓN EXITOSA",new Vector2(.5f,.5f),Vector2.zero,new Vector2(710,70),30,new Color(.25f,1,.65f),TextAnchor.MiddleCenter);successBanner.SetActive(false);
 }
 void StartNarrative(IEnumerator routine){if(narrative!=null)StopCoroutine(narrative);if(radioSource)radioSource.Stop();narrative=StartCoroutine(routine);}
 void BeginBriefing(){AwaitingContinuation=false;MessageDecoded=false;pendingResult=null;FailureReason="";FailureDetails="";Phase=ControlPhase.Briefing;MissionConfirmed=false;Remaining=launchWindows[AssignedMission];successBanner.SetActive(false);StartNarrative(Briefing());}
IEnumerator Briefing(){
 if(IsTutorial){yield return Tutorial();yield break;}
 var text=new[]{Calls[AssignedMission],Orders[AssignedMission],"Seleccione "+MissionCatalog.Missions[AssignedMission].name+" en la pantalla. Confirme la misión, configure el CanSat y ensámblelo.","Tiene "+Mathf.RoundToInt(launchWindows[AssignedMission])+" segundos para ensamblar el CanSat y autorizar el lanzamiento. El reloj empieza ahora, Armstrong."};
 if(MissionAudio(AssignedMission)){yield return RecordedBlock(MissionAudio(AssignedMission),text,null,true);yield break;}
 for(int i=0;i<text.Length;i++){if(i==3)StartDeadline();yield return Say(text[i],5,-1);}
}
 IEnumerator Tutorial(){if(TutorialAudio(0)){yield return RecordedTutorial();yield break;}
 yield return new WaitForSeconds(1);
 yield return Say("Armstrong... Armstrong. Aquí Control de Misión. Hoy aprenderá a diseñar, ensamblar y operar un CanSat.",5,29);
 yield return Say("Su objetivo es obtener imágenes de la Tierra y transmitir al menos 1000 MB útiles. Cada componente cambia masa, costo, consumo y supervivencia.",6,30);
 yield return Say("WASD para caminar, mouse para mirar. Acérquese al escritorio de enfrente y pulse E. Escape permite volver a explorar.",6,31);
 yield return Say("La pantalla de diseño permite elegir misión y componentes. Pulse el título de Observación terrestre para confirmar la orden.",5,32);
 while(!MissionConfirmed){yield return Say("La pantalla de diseño permite elegir misión y componentes. Pulse el título de Observación terrestre para confirmar la orden.",5,32);}
 yield return Say("En la pantalla de telemetría verá el modelo y los datos. Las marcas de masa y presupuesto son límites máximos: manténgase por debajo. Potencia, batería y propelente muestran mínimos: alcance o supere la marca.",7,33);
 yield return Say("Verde indica que cumple. Amarillo señala un desvío de hasta cinco por ciento; rojo, un desvío mayor. La batería debe sostener al CanSat durante los eclipses.",6,34);
 yield return Say("La pantalla de video, al extremo derecho, recibe la cámara del CanSat. Solo tendrá señal en órbita. Si la nave funciona, buscará y decodificará un mensaje.",6,35);
 string[] reasons={"Una plataforma pequeña mantiene el costo y admite hasta 150 kg.","La cámara óptica obtendrá las imágenes de la superficie.","Los paneles fijos proporcionan energía en la zona iluminada.","La batería mediana guarda 140 Wh para atravesar la sombra.","El gas frío permite los pequeños ajustes de esta órbita.","La antena de parche basta para el enlace cercano a la Tierra.","Las ruedas de reacción mantienen la cámara apuntando.","Las mantas pasivas bastan para este entorno térmico.","La computadora comercial es suficiente en esta misión terrestre.","El lanzador ligero alcanza la órbita terrestre con esta masa.","La estación de 13 metros recibe los datos de esta misión cercana."};
 for(int i=0;i<11;i++){TutorialCategory=i;var c=app.Configuration;int chosen=MissionCatalog.Missions[0].required[i];yield return Say("En "+MissionCatalog.Categories[i]+", elija "+MissionCatalog.Options[i][chosen].name+". "+reasons[i],5,36+i);while(c.choices[i]!=chosen){yield return Say("En "+MissionCatalog.Categories[i]+", elija "+MissionCatalog.Options[i][chosen].name+". "+reasons[i],5,36+i);c=app.Configuration;}}
 TutorialCategory=-1;
 yield return Say("Los dos brazos tomarán piezas pequeñas de las mesas y las unirán con soldadura. Ensamble cuando esté listo; después vuelva con E y pulse Lanzar.",7,47);
 yield return Say("Desde ahora dispone de 90 segundos. También puede experimentar con diseños incorrectos: el ensamblaje se permite, pero una mala elección puede terminar en pérdida de señal.",7,3);
 StartDeadline();
 }
 void StartDeadline(){if(Terminal||room.RoomDestroyed)return;Phase=ControlPhase.Designing;Remaining=launchWindows[AssignedMission];}
IEnumerator Say(string text,float hold,int clipIndex){
 CurrentSubtitle=text;subtitle.text=text;var selectedVoice=VoiceAudio(clipIndex);bool recorded=radioSource&&selectedVoice;
 if(recorded){radioSource.Stop();radioSource.clip=selectedVoice;radioSource.mute=false;radioSource.pitch=Mathf.Clamp(Time.timeScale,.1f,3);radioSource.Play();if(clipIndex==27)GeneralFailureCalls++;}
 for(float t=0;t<1;t+=Time.deltaTime/.25f){subtitleGroup.alpha=Mathf.SmoothStep(0,1,t);yield return null;}
 subtitleGroup.alpha=1;float elapsed=0;while(elapsed<hold||(recorded&&(radioSource.isPlaying||room.IsPaused))){elapsed+=Time.deltaTime;yield return null;}
 for(float t=0;t<1;t+=Time.deltaTime/.3f){subtitleGroup.alpha=1-Mathf.SmoothStep(0,1,t);yield return null;}subtitleGroup.alpha=0;CurrentSubtitle="";
}
 public void OnMissionSelected(int mission){if(Terminal||Phase==ControlPhase.Completed)return;MissionConfirmed=mission==AssignedMission;if(!MissionConfirmed)app.status.text="CONTROL DE MISIÓN · Orden actual: "+MissionCatalog.Missions[AssignedMission].name;}
 public void ExplainOrder(){app.status.text=Phase==ControlPhase.Briefing?"CONTROL DE MISIÓN · Espere la orden de inicio.":!MissionConfirmed?"ARMSTRONG · Seleccione y confirme "+MissionCatalog.Missions[AssignedMission].name:"CONTROL DE MISIÓN · La orden actual no coincide con el diseño.";}
 public void OnLaunchAccepted(){
 if(Terminal)return;TutorialCategory=-1;Phase=ControlPhase.Launched;StartNarrative(Say("Lanzamiento autorizado. Armstrong, mantenga la señal. Control seguirá cada segundo de la inserción.",4,24));
 }
 public void OnOrbitReached(){if(Terminal||Phase==ControlPhase.SuccessHold||Phase==ControlPhase.Completed)return;OrbitStartedAt=Time.time;MessageDecoded=false;pendingResult=null;Phase=ControlPhase.Verifying;StartNarrative(VerifyOrbit());}
 IEnumerator VerifyOrbit(){
 float began=Time.time;yield return Say("CanSat en órbita. Armstrong, no se aparte de los monitores. Verifique energía y comunicaciones. Buscaremos una señal durante dieciocho segundos de órbita.",4.5f,25);
 float wait=Mathf.Max(0,orbitVerificationSeconds-(Time.time-began));if(wait>0)yield return new WaitForSeconds(wait);
 if(!Terminal&&Phase==ControlPhase.Verifying){if(pendingResult!=null){var completed=pendingResult;pendingResult=null;OnMissionResult(completed);}else app.CompleteSimulation();}
 }
 public void OnMissionResult(MissionResult result){
 if(Terminal||Phase==ControlPhase.SuccessHold||Phase==ControlPhase.Completed)return;
 if(result.config.mission!=AssignedMission||!MissionConfirmed||result.missionLost||result.useful<result.config.Mission.dataGoal){FailureDetails=MissionDiagnostics.Explain(result);FailureReason=MissionDiagnostics.FirstCause(result);result.failureReason=FailureDetails;Fail("Armstrong, misión fallida. "+FailureReason,false);return;}
 if(Time.time<OrbitStartedAt+orbitVerificationSeconds){pendingResult=result;return;}MessageDecoded=true;
 Phase=ControlPhase.SuccessHold;LastSuccessAt=Time.time;NextContactAt=0;
 successText.text="MISIÓN EXITOSA\n"+MissionCatalog.Missions[AssignedMission].name;successBanner.SetActive(true);StartNarrative(SuccessThenNext());
 }
 IEnumerator SuccessThenNext(){
 yield return Say("Armstrong, señal confirmada. Mensaje decodificado. Revise el fragmento en la pantalla de video: «"+DecodedMessage+"».",4,49);
 if(IsTutorial){TutorialFinished=true;yield return Say("Armstrong, veo que ya tiene experiencia. Tutorial completado: nave operativa y datos recibidos. Ya puede continuar con la segunda misión desde el botón de la pantalla de control.",6,48);}
 else if(AssignedMission<5)yield return Say("Armstrong, confirmamos órbita y objetivo cumplido. Misión exitosa. Cuando esté listo, pulse Continuar con la siguiente misión en la pantalla de control.",4.2f,26);
 if(AssignedMission==5){Phase=ControlPhase.Completed;successText.text="TODAS LAS MISIONES COMPLETADAS";yield return Say("Armstrong, la última señal ha llegado. Las seis misiones están completas. Control de Misión cierra el canal. Buen trabajo.",6,28);yield break;}
 AwaitingContinuation=true;
 }
 public void ContinueToNextMission(){if(!AwaitingContinuation||Terminal||AssignedMission>=5)return;AwaitingContinuation=false;AssignedMission++;app.SendMessage("ResetDesign");BeginBriefing();
 }
 void Update(){
 if(!initialized)return;if(radioSource&&Time.timeScale>0)radioSource.pitch=Mathf.Clamp(Time.timeScale,.1f,3);canvas.enabled=!room.IsPaused;if(room.IsPaused)return;
 if(room.RoomDestroyed&&!Terminal){Phase=ControlPhase.Failed;if(narrative!=null)StopCoroutine(narrative);if(radioSource)radioSource.Stop();subtitleGroup.alpha=0;}
 if(Terminal&&destructionAt>0&&!room.RoomDestroyed&&Time.time>=destructionAt){room.ExplodeRoom();CreatePersistentFire();failureAt=Time.time;}
 if(Phase==ControlPhase.Designing){Remaining=Mathf.Max(0,Remaining-Time.deltaTime);if(Remaining<=0)Fail("Armstrong... Armstrong. Se agotó el tiempo. No tenemos lanzamiento. El sistema está colapsando.",true);}
 timerHeading.text="CONTROL DE MISIÓN / "+(AssignedMission+1).ToString("00");
 objective.text=MissionCatalog.Missions[AssignedMission].name+(MissionConfirmed?" · CONFIRMADA":" · SELECCIONAR Y CONFIRMAR");
 timer.color=Phase==ControlPhase.SuccessHold||Phase==ControlPhase.Completed?new Color(.25f,1,.65f):Phase==ControlPhase.Launched||Phase==ControlPhase.Verifying?new Color(.4f,.85f,1):new Color(1,.18f,.14f);
 if(Phase==ControlPhase.Designing||Terminal){int total=Mathf.CeilToInt(Remaining);timer.text=(total/60).ToString("00")+":"+(total%60).ToString("00");}
 else timer.text=Phase==ControlPhase.Briefing?(IsTutorial?"TUTORIAL":"ENLACE ENTRANTE"):Phase==ControlPhase.Launched?"LANZADO":Phase==ControlPhase.Verifying?"VERIFICANDO":Phase==ControlPhase.Completed?"COMPLETADAS":"COMPLETADA";
 }
 public void Fail(string message,bool timeout){
 if(Terminal)return;AwaitingContinuation=false;MessageDecoded=false;if(timeout){FailureReason="Plazo de lanzamiento agotado";FailureDetails="No se autorizó el lanzamiento antes de que terminara la cuenta regresiva.";}var alert=GetComponent<StationRedAlert>();if(!alert)alert=gameObject.AddComponent<StationRedAlert>();alert.Activate();destructionAt=Time.time+(timeout?20:destructionWarningSeconds);Phase=timeout?ControlPhase.TimedOut:ControlPhase.Failed;Remaining=0;successBanner.SetActive(false);
 if(!timeout){var loss=GetComponent<OrbitalFailureController>();if(loss)loss.BreakApart();}room.RequireRestart();
 StartNarrative(FailureNarrative(message));failureAt=Time.time;
 }
IEnumerator FailureNarrative(string message){
 bool timeout=Phase==ControlPhase.TimedOut;
 yield return Say(timeout?message:"Armstrong, misión fallida. Revise las causas del fallo en el monitor de conexión.",5,timeout?50:27);
 if(!string.IsNullOrEmpty(FailureDetails)){var causes=FailureDetails.Split('\n');for(int i=0;i<Mathf.Min(3,causes.Length);i++)yield return Say(causes[i],3.5f,-1);}
 yield return Say("Alerta general. Luces de emergencia activadas. La estación está colapsando. Escape para reiniciar o salir.",4,51);
 CurrentSubtitle="SIN RESPUESTA DE CONTROL · ESC PARA REINICIAR O SALIR";subtitle.text=CurrentSubtitle;subtitleGroup.alpha=1;
}
 void CreatePersistentFire(){
 if(fxRoot)return;fxRoot=new GameObject("DeadlineFailureCameraFX");fxRoot.transform.SetParent(station.playerCamera.transform,false);
 for(int i=0;i<3;i++){var fire=new GameObject("PersistentFire_"+i);fire.transform.SetParent(fxRoot.transform,false);fire.transform.localPosition=new Vector3((i-1)*.8f,-.65f,1.6f);fire.transform.localRotation=Quaternion.Euler(-90,0,0);
 var ps=fire.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=ps.main;main.loop=true;main.duration=2;main.playOnAwake=false;main.maxParticles=110;main.startLifetime=new ParticleSystem.MinMaxCurve(.7f,1.6f);main.startSpeed=new ParticleSystem.MinMaxCurve(.25f,.65f);main.startSize=new ParticleSystem.MinMaxCurve(.12f,.35f);main.startColor=new ParticleSystem.MinMaxGradient(new Color(1,.13f,.01f,.8f),new Color(1,.65f,.12f,.9f));main.gravityModifier=-.03f;main.simulationSpace=ParticleSystemSimulationSpace.Local;
 var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=12;shape.radius=.15f;var em=ps.emission;em.rateOverTime=26;
 var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(new Color(1,.85f,.2f),0),new GradientColorKey(new Color(1,.15f,.01f),.45f),new GradientColorKey(new Color(.1f,.02f,0),1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.85f,.15f),new GradientAlphaKey(0,1)});var colors=ps.colorOverLifetime;colors.enabled=true;colors.color=new ParticleSystem.MinMaxGradient(gradient);
 var sizes=ps.sizeOverLifetime;sizes.enabled=true;sizes.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.5f),new Keyframe(.45f,1),new Keyframe(1,.25f)));
 var renderer=ps.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=fireMaterial?fireMaterial:station.GetComponent<StationPolish>().particleMaterial;renderer.renderMode=ParticleSystemRenderMode.Billboard;ps.Play();
 }
 var bursts=new GameObject("RepeatingEmergencySparks");bursts.transform.SetParent(fxRoot.transform,false);bursts.transform.localPosition=new Vector3(0,-.3f,2);
 var sparks=bursts.AddComponent<ParticleSystem>();sparks.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var sm=sparks.main;sm.loop=true;sm.duration=2.4f;sm.startLifetime=.7f;sm.startSpeed=2;sm.startSize=.025f;sm.gravityModifier=0;sm.maxParticles=100;sm.startColor=new Color(1,.6f,.15f);
 var se=sparks.emission;se.rateOverTime=0;se.SetBursts(new[]{new ParticleSystem.Burst(0,40)});var sh=sparks.shape;sh.shapeType=ParticleSystemShapeType.Sphere;sh.radius=.6f;sparks.GetComponent<ParticleSystemRenderer>().sharedMaterial=station.GetComponent<StationPolish>().particleMaterial;sparks.Play();
 }
 void LateUpdate(){
 if(!fxRoot||room.IsPaused)return;var cam=station.playerCamera.transform;cam.localPosition-=lastShake;float strength=Time.time-failureAt<2?.024f:.006f;lastShake=new Vector3(Mathf.Sin(Time.time*37),Mathf.Cos(Time.time*29),0)*strength;cam.localPosition+=lastShake;
 }
 void OnDestroy(){if(canvas)Destroy(canvas.gameObject);if(fxRoot)Destroy(fxRoot);if(station&&station.playerCamera)station.playerCamera.transform.localPosition-=lastShake;}


IEnumerator RecordedTutorial(){
 string[] lines=new string[]{"Armstrong... Armstrong. Aquí Control de Misión. Hoy aprenderá a diseñar, ensamblar y operar un CanSat.","Su objetivo es obtener imágenes de la Tierra y transmitir al menos 1000 MB útiles. Cada componente cambia masa, costo, consumo y supervivencia.","WASD para caminar, mouse para mirar. Acérquese al escritorio de enfrente y pulse E. Escape permite volver a explorar.","La pantalla de diseño permite elegir misión y componentes. Pulse el título de Observación terrestre para confirmar la orden.","En la pantalla de telemetría verá el modelo y los datos. Las marcas de masa y presupuesto son límites máximos: manténgase por debajo. Potencia, batería y propelente muestran mínimos: alcance o supere la marca.","Verde indica que cumple. Amarillo señala un desvío de hasta cinco por ciento; rojo, un desvío mayor. La batería debe sostener al CanSat durante los eclipses.","La pantalla de video, al extremo derecho, recibe la cámara del CanSat. Solo tendrá señal en órbita. Si la nave funciona, buscará y decodificará un mensaje.","En Plataforma, elija Pequeña. Una plataforma pequeña mantiene el costo y admite hasta 150 kg.","En Instrumento, elija Cámara óptica. La cámara óptica obtendrá las imágenes de la superficie.","En Energía, elija Paneles fijos. Los paneles fijos proporcionan energía en la zona iluminada.","En Batería, elija Mediana. La batería mediana guarda 140 Wh para atravesar la sombra.","En Propulsión, elija Gas frío. El gas frío permite los pequeños ajustes de esta órbita.","En Antena, elija De parche. La antena de parche basta para el enlace cercano a la Tierra.","En Actitud, elija Ruedas de reacción. Las ruedas de reacción mantienen la cámara apuntando.","En Control térmico, elija Mantas pasivas. Las mantas pasivas bastan para este entorno térmico.","En Computadora, elija Comercial. La computadora comercial es suficiente en esta misión terrestre.","En Lanzador, elija Ligero. El lanzador ligero alcanza la órbita terrestre con esta masa.","En Estación terrena, elija Antena 13 m. La estación de 13 metros recibe los datos de esta misión cercana.","Los dos brazos tomarán piezas pequeñas de las mesas y las unirán con soldadura. Ensamble cuando esté listo; después vuelva con E y pulse Lanzar.","Desde ahora dispone de 90 segundos. También puede experimentar con diseños incorrectos: el ensamblaje se permite, pero una mala elección puede terminar en pérdida de señal."};
 int[] starts={0,3,6,10,15,19},counts={3,3,4,5,4,1};
 for(int part=0;part<6;part++){var text=new string[counts[part]];var categories=new int[counts[part]];for(int i=0;i<text.Length;i++){int n=starts[part]+i;text[i]=lines[n];categories[i]=n>=7&&n<=17?n-7:-1;}yield return RecordedBlock(TutorialAudio(part),text,categories,part==5);}
 TutorialCategory=-1;if(!MissionConfirmed)yield return Say("Armstrong, confirme Observación terrestre en la pantalla de diseño para comenzar.",4,-1);
}


IEnumerator RecordedBlock(AudioClip clip,string[] lines,int[] categories,bool startClockOnLast){
 if(!clip){for(int i=0;i<lines.Length;i++){if(categories!=null&&i<categories.Length)TutorialCategory=categories[i];yield return Say(lines[i],5,-1);}if(startClockOnLast&&Phase==ControlPhase.Briefing)StartDeadline();yield break;}
 radioSource.Stop();radioSource.clip=clip;radioSource.mute=false;radioSource.pitch=Mathf.Clamp(Time.timeScale,.1f,3);radioSource.Play();
 float total=0;var ends=new float[lines.Length];for(int i=0;i<lines.Length;i++){total+=Mathf.Max(12,GameLanguage.Translate(lines[i]).Length);ends[i]=total;}int prior=-1;
 while(radioSource.isPlaying||room.IsPaused){if(room.IsPaused){yield return null;continue;}float fraction=Mathf.Clamp01(radioSource.time/clip.length);int index=0;while(index<ends.Length-1&&fraction>ends[index]/total)index++;
 if(index!=prior){prior=index;CurrentSubtitle=lines[index];subtitle.text=CurrentSubtitle;if(categories!=null&&index<categories.Length)TutorialCategory=categories[index];if(startClockOnLast&&index==lines.Length-1&&Phase==ControlPhase.Briefing)StartDeadline();}
 float start=index==0?0:ends[index-1]/total;float end=ends[index]/total;float fade=Mathf.Min((fraction-start)*clip.length/.18f,(end-fraction)*clip.length/.18f);subtitleGroup.alpha=Mathf.Clamp01(fade);yield return null;}
 subtitleGroup.alpha=0;CurrentSubtitle="";if(startClockOnLast&&Phase==ControlPhase.Briefing)StartDeadline();
}
}}
