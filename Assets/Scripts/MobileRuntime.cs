using System;using System.Runtime.InteropServices;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.EventSystems;using UnityEngine.InputSystem.UI;
namespace Caronte {
public static class MobileRuntime {
 static bool? detected;
#if UNITY_EDITOR
 public static bool ForceTouchForTesting;
#endif
 public static bool Active {get {
#if UNITY_EDITOR
 if(ForceTouchForTesting)return true;
#endif
 if(!detected.HasValue){bool mobile=Application.isMobilePlatform;
#if UNITY_WEBGL && !UNITY_EDITOR
 mobile=mobile||CaronteIsMobileTouch()!=0;
#endif
 detected=mobile;}return detected.Value;}}
#if UNITY_WEBGL && !UNITY_EDITOR
 [DllImport("__Internal")]static extern int CaronteIsMobileTouch();
#endif
 public static Vector2 Move,LookDelta;public static bool UpHeld,DownHeld;
 public static float Lift {get{return (UpHeld?1:0)-(DownHeld?1:0);}}
 public static Vector2 ConsumeLook(){var delta=LookDelta;LookDelta=Vector2.zero;return delta;}
 public static void Clear(){Move=LookDelta=Vector2.zero;UpHeld=DownHeld=false;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){detected=null;Clear();}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Boot(){SceneManager.sceneLoaded-=Loaded;SceneManager.sceneLoaded+=Loaded;}
 static void Loaded(Scene scene,LoadSceneMode mode){Clear();if(!Active)return;foreach(var root in scene.GetRootGameObjects()){var station=root.GetComponentInChildren<CaronteStation>(true);if(station&&!station.GetComponent<MobileControls>())station.gameObject.AddComponent<MobileControls>();}}
 public static void EnsureEvents(){if(EventSystem.current)return;new GameObject("TouchEventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));}
}
}