using System.Collections;
using UnityEngine;
namespace Caronte {
public sealed class WorkshopAirlock : MonoBehaviour {
 public Transform[] glassPanels=new Transform[4],hatchLeaves=new Transform[2];
 public Vector3[] glassRaised=new Vector3[4],glassLowered=new Vector3[4],hatchClosed=new Vector3[2],hatchOpen=new Vector3[2];
 public Vector3 launchExit=new Vector3(0,10.5f,6.2f);
 public bool IsCycling{get;private set;}public string Phase{get;private set;}="Standby";
 public int CompletedLaunches{get;private set;}public float HatchClearance{get;private set;}
 StationRoomController room;
 void Awake(){room=GetComponentInParent<StationRoomController>();}
 bool Interrupted {get{return room&&room.RoomDestroyed;}}
 public IEnumerator Launch(Transform craft){
  if(IsCycling||!craft)yield break;IsCycling=true;
  try{
   Phase="Lowering safety glass";yield return Move(glassPanels,glassRaised,glassLowered,1.15f);
   if(Interrupted||!craft)yield break;
   Phase="Opening upper hatch";yield return Move(hatchLeaves,hatchClosed,hatchOpen,1.1f);HatchClearance=4;
   if(Interrupted||!craft)yield break;
   Phase="Vertical ascent";Vector3 start=craft.position;
   for(float t=0;t<1;t+=Time.deltaTime/2.8f){if(Interrupted||!craft)yield break;craft.position=Vector3.Lerp(start,launchExit,t*t);yield return null;}
   if(craft){craft.position=launchExit;craft.gameObject.SetActive(false);}
   Phase="Closing upper hatch";yield return Move(hatchLeaves,hatchOpen,hatchClosed,1.1f);HatchClearance=0;
   if(Interrupted)yield break;
   Phase="Retracting safety glass";yield return Move(glassPanels,glassLowered,glassRaised,1.15f);
   if(!Interrupted){Phase="Standby";CompletedLaunches++;}
  }finally{IsCycling=false;}
 }
 IEnumerator Move(Transform[] targets,Vector3[] from,Vector3[] to,float seconds){
  for(float t=0;t<1;t+=Time.deltaTime/seconds){if(Interrupted)yield break;float s=Mathf.SmoothStep(0,1,t);for(int i=0;i<targets.Length;i++)if(targets[i])targets[i].localPosition=Vector3.Lerp(from[i],to[i],s);yield return null;}
  for(int i=0;i<targets.Length;i++)if(targets[i])targets[i].localPosition=to[i];
 }
}
}