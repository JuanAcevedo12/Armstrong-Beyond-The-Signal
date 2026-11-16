using UnityEngine;
namespace Caronte{
public class StationRedAlert:MonoBehaviour{
 public bool Active{get;private set;}float next;
 public void Activate(){Active=true;Apply();}
 void Apply(){foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(l.gameObject.scene==gameObject.scene)l.color=new Color(1,.025f,.015f);}
 void LateUpdate(){if(Active&&Time.unscaledTime>=next){next=Time.unscaledTime+.15f;Apply();}}
}
}