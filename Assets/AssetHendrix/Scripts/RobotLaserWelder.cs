using System.Collections;using System.Collections.Generic;using UnityEngine;using UnityEngine.Rendering;
namespace Caronte{
public class RobotLaserWelder:MonoBehaviour{
 public Transform head,nozzle;public Material beamMaterial;public StationPolish effects;public StationRoomController room;public float weldSeconds=.48f;
 public bool IsBusy{get{return Firing||targets.Count>0||routine!=null;}}public int WeldCount{get;private set;}public bool Firing{get;private set;}public Vector3 LastTarget{get;private set;}
 Queue<Vector3> targets=new Queue<Vector3>();Coroutine routine;LineRenderer beam,core;Light spot;
 void Awake(){EnsureEffects();}
 void EnsureEffects(){if(beam)return;beam=Line("RedWeldingBeam",.055f,new Color(1,.015f,.008f,.65f));core=Line("WeldingBeamCore",.012f,new Color(1,.5f,.35f,1));var go=new GameObject("LaserImpactLight");go.transform.SetParent(transform,false);spot=go.AddComponent<Light>();spot.type=LightType.Point;spot.color=new Color(1,.025f,.01f);spot.range=1.2f;spot.intensity=3;spot.enabled=false;}
 LineRenderer Line(string name,float width,Color color){var g=new GameObject(name,typeof(LineRenderer));g.transform.SetParent(transform,false);var line=g.GetComponent<LineRenderer>();line.sharedMaterial=beamMaterial;line.useWorldSpace=true;line.positionCount=2;line.widthMultiplier=width;line.startColor=line.endColor=color;var tint=new MaterialPropertyBlock();tint.SetColor("_BaseColor",new Color(color.r*6,color.g*6,color.b*6,1));line.SetPropertyBlock(tint);line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.enabled=false;return line;}
 public void Weld(Vector3 target){if(room&&room.RoomDestroyed)return;EnsureEffects();targets.Enqueue(target);if(routine==null)routine=StartCoroutine(Process());}
 Quaternion Aim(Vector3 target){Vector3 now=nozzle.position-head.position;return Quaternion.FromToRotation(now.normalized,(target-head.position).normalized)*head.rotation;}
 IEnumerator Process(){while(targets.Count>0){LastTarget=targets.Dequeue();Quaternion from=head.rotation,to=Aim(LastTarget);for(float t=0;t<1;t+=Time.deltaTime/.16f){head.rotation=Quaternion.Slerp(from,to,Mathf.SmoothStep(0,1,t));yield return null;}head.rotation=Aim(LastTarget);Firing=true;beam.enabled=core.enabled=spot.enabled=true;WeldCount++;if(effects)effects.DockEffect(LastTarget);for(float t=0;t<weldSeconds;t+=Time.deltaTime){if(room&&room.RoomDestroyed){Cancel();yield break;}beam.SetPosition(0,nozzle.position);beam.SetPosition(1,LastTarget);core.SetPosition(0,nozzle.position);core.SetPosition(1,LastTarget);spot.transform.position=LastTarget;spot.intensity=2.8f+Mathf.Sin(Time.time*80)*1.1f;yield return null;}Firing=false;beam.enabled=core.enabled=spot.enabled=false;yield return new WaitForSeconds(.06f);}routine=null;}
 public void Cancel(){targets.Clear();if(routine!=null)StopCoroutine(routine);routine=null;Firing=false;if(beam)beam.enabled=false;if(core)core.enabled=false;if(spot)spot.enabled=false;}
 void LateUpdate(){if(room&&room.RoomDestroyed)Cancel();}
 void OnDisable(){Cancel();}
}
}