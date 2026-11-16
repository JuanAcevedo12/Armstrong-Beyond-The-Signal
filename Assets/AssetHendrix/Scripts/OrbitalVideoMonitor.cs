using UnityEngine;using UnityEngine.UI;
namespace Caronte{
[DefaultExecutionOrder(200)]
public class OrbitalVideoMonitor:MonoBehaviour{
 public CaronteStation station;public RenderTexture videoTexture;public Camera feedCamera;
 RawImage picture;Text label,clock,decodedText,destinationLabel;GameObject decodedPanel,failurePanel;Text failureDetails;Transform host;bool signal;
 public bool SignalAvailable{get{return signal;}}
 RectTransform R(Transform p,string n,float x,float y,float w,float h){var g=new GameObject(n,typeof(RectTransform));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 Text T(Transform p,string value,float x,float y,float w,float h,int size){var t=R(p,"Text",x,y,w,h).gameObject.AddComponent<Caronte.LocalizedText>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");t.text=value;t.fontSize=size;t.color=new Color(.35f,.9f,1);t.raycastTarget=false;return t;}
 public void Build(){
 if(!station||!station.videoScreen)return;var old=station.videoScreen.Find("OrbitalVideoCanvas");if(old)DestroyImmediate(old.gameObject);
 var root=new GameObject("OrbitalVideoCanvas",typeof(RectTransform),typeof(Canvas));host=root.transform;host.SetParent(station.videoScreen,false);host.localScale=Vector3.one*.002f;
 var rt=root.GetComponent<RectTransform>();rt.sizeDelta=new Vector2(1200,800);var cv=root.GetComponent<Canvas>();cv.renderMode=RenderMode.WorldSpace;cv.worldCamera=station.playerCamera;
 var bg=R(host,"Background",0,0,1200,800).gameObject.AddComponent<Image>();bg.color=new Color(.008f,.02f,.035f);bg.raycastTarget=false;
 T(host,"CARONTE / CÁMARA DE A BORDO",26,18,1140,45,29);destinationLabel=T(host,"",26,64,1140,24,19);
 picture=R(host,"OrbitalVideo",22,90,1156,570).gameObject.AddComponent<RawImage>();picture.texture=videoTexture;picture.raycastTarget=false;picture.enabled=false;
 label=T(host,"SIN SEÑAL · ESPERANDO INSERCIÓN ORBITAL",32,350,1130,90,32);label.alignment=TextAnchor.MiddleCenter;
 clock=T(host,"CÁMARA EN ESPERA / ENLACE CERRADO",26,700,1140,65,24);
 var decoded=R(host,"DecodedTransmission",22,430,1156,245);decodedPanel=decoded.gameObject;var panel=decodedPanel.AddComponent<Image>();panel.color=new Color(.005f,.04f,.055f,.96f);panel.raycastTarget=false;T(decoded,"MENSAJE DECODIFICADO / CANAL CONFIRMADO",20,8,1110,40,27);var wave=R(decoded,"CommunicationSignal",20,54,1110,85).gameObject.AddComponent<CommunicationWave>();wave.color=new Color(.2f,1,.7f);wave.raycastTarget=false;decodedText=T(decoded,"",20,155,1110,65,38);decodedPanel.SetActive(false);
 var failure=R(host,"ConnectionFailureDetails",22,210,1156,455);failurePanel=failure.gameObject;var failureBackground=failurePanel.AddComponent<Image>();failureBackground.color=new Color(.09f,.008f,.015f,1);failureBackground.raycastTarget=false;T(failure,"MISIÓN FALLIDA / CAUSAS",20,12,1110,42,29).color=new Color(1,.4f,.3f);failureDetails=T(failure,"",20,70,1110,365,25);failureDetails.resizeTextForBestFit=true;failureDetails.resizeTextMinSize=17;failureDetails.resizeTextMaxSize=25;failureDetails.color=new Color(1,.85f,.78f);failurePanel.SetActive(false);
 }
 void Start(){if(!station)station=GetComponent<CaronteStation>();Build();if(!videoTexture){videoTexture=new RenderTexture(1280,720,24);videoTexture.name="OrbitalVideoRuntime";videoTexture.Create();picture.texture=videoTexture;}var go=new GameObject("CanSatPOVCamera");go.transform.SetParent(transform,false);feedCamera=go.AddComponent<Camera>();feedCamera.targetTexture=videoTexture;feedCamera.cullingMask=1<<29;feedCamera.clearFlags=CameraClearFlags.Skybox;feedCamera.fieldOfView=2f*Mathf.Atan(Mathf.Tan(34f*Mathf.Deg2Rad)/2f)*Mathf.Rad2Deg;feedCamera.nearClipPlane=.05f;feedCamera.farClipPlane=2000;feedCamera.enabled=false;
 var planet=transform.Find("DestinationEarth");if(planet)planet.gameObject.layer=29;var stars=transform.Find("Starfield");if(stars)foreach(var t in stars.GetComponentsInChildren<Transform>(true))t.gameObject.layer=29;
 }
 void LateUpdate(){
 if(!picture)return;var destination=GetComponent<MissionDestination>();if(destinationLabel&&destination)destinationLabel.text=destination.DestinationName;var room=GetComponent<StationRoomController>();if(room&&room.IsPaused){if(feedCamera)feedCamera.enabled=false;return;}
 var fx=GetComponent<StationPolish>();var loss=GetComponent<OrbitalFailureController>();var engine=station.app.CurrentEngine;
 signal=engine!=null&&station.app.Stage>=3&&fx&&fx.WindowCraft&&fx.WindowCraft.gameObject.activeInHierarchy&&!(loss&&loss.Destroyed)&&engine.Powered&&engine.ElectronicsAlive&&engine.Result.config.LinkQuality>=.12f&&!(room&&room.RoomDestroyed);
 var control=GetComponent<MissionControlDirector>();bool failed=control&&control.Terminal&&!string.IsNullOrEmpty(control.FailureDetails);if(failurePanel){failurePanel.SetActive(failed);if(failed)failureDetails.text=control.FailureDetails;}label.rectTransform.anchoredPosition=new Vector2(32,failed?-92:-350);label.rectTransform.sizeDelta=new Vector2(1130,failed?105:90);if(decodedPanel){decodedPanel.SetActive(signal&&control&&control.MessageDecoded);if(control)decodedText.text=control.DecodedMessage;}
 picture.enabled=signal;label.enabled=!signal;if(feedCamera)feedCamera.enabled=signal;
 if(signal){var planet=transform.Find("DestinationEarth");feedCamera.transform.position=fx.WindowCraft.position;Vector3 inward=(planet.position-feedCamera.transform.position).normalized;Vector3 tangent=Vector3.Cross(inward,Vector3.up).normalized;feedCamera.transform.rotation=Quaternion.LookRotation(inward+tangent*.9f+Vector3.up*.12f,Vector3.up);clock.text="● EN DIRECTO / "+engine.MissionClock+"\n"+(engine.Eclipse?"PASO NOCTURNO":"HEMISFERIO ILUMINADO")+" · ÓRBITA "+engine.OrbitNumber;clock.color=new Color(.3f,1,.75f);}
 else{bool lost=engine!=null||(control&&control.Terminal);label.text=failed?"PÉRDIDA DE CONEXIÓN\nSIN VIDEO DEL CANSAT":lost?"PÉRDIDA DE SEÑAL\nSIN VIDEO DEL CANSAT":"SIN SEÑAL\nESPERANDO INSERCIÓN ORBITAL";label.color=lost?new Color(1,.4f,.25f):new Color(.35f,.9f,1);clock.text=lost?"ENLACE INTERRUMPIDO / CONTROL DE MISIÓN":"CÁMARA EN ESPERA / ENLACE CERRADO";}
 }
 void OnDestroy(){if(feedCamera)Destroy(feedCamera.gameObject);if(host)Destroy(host.gameObject);}
}
}