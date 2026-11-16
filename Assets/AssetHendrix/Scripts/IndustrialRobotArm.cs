using UnityEngine;
namespace Caronte{
public class IndustrialRobotArm:MonoBehaviour{
 public Transform leftJaw,rightJaw;public Quaternion leftJawRest=Quaternion.identity,rightJawRest=Quaternion.identity;public float GripOpen=1;public int CloseCount{get;private set;}public int OpenCount{get;private set;} public int robotIndex=0;public Transform baseYaw,shoulder,elbow,wrist,gripSocket;
 public Vector3 upperRest,foreRest,toolRest;public float MaxGripError{get;private set;}public int SolveCount{get;private set;}public bool IsHolding{get;private set;}
 public void Calibrate(){upperRest=elbow.localPosition;foreRest=wrist.localPosition;toolRest=gripSocket.localPosition;if(leftJaw)leftJawRest=leftJaw.localRotation;if(rightJaw)rightJawRest=rightJaw.localRotation;}
 public void ResetMetrics(){MaxGripError=0;SolveCount=0;}
public void SetHolding(bool holding){IsHolding=holding;SetGripPose(holding?0:1);}
 public void MoveTo(Vector3 target){
 if(!baseYaw||!shoulder||!elbow||!wrist||!gripSocket)return;
 if(upperRest.sqrMagnitude<.01f)Calibrate();
 Vector3 horizontal=target-baseYaw.position;horizontal.y=0;if(horizontal.sqrMagnitude>.001f)baseYaw.rotation=Quaternion.LookRotation(horizontal.normalized,Vector3.up);
 float upperLength=upperRest.magnitude,foreLength=foreRest.magnitude,toolLength=toolRest.magnitude;
 Vector3 wristGoal=target+Vector3.up*toolLength;Vector3 delta=wristGoal-shoulder.position;float rawDistance=delta.magnitude;Vector3 direction=rawDistance>.001f?delta/rawDistance:Vector3.up;float distance=Mathf.Clamp(rawDistance,Mathf.Abs(upperLength-foreLength)+.005f,upperLength+foreLength-.005f);
 Vector3 bend=Vector3.ProjectOnPlane(Vector3.up,direction);if(bend.sqrMagnitude<.001f)bend=Vector3.ProjectOnPlane(-baseYaw.forward,direction);bend.Normalize();
 float along=(upperLength*upperLength-foreLength*foreLength+distance*distance)/(2*distance);float height=Mathf.Sqrt(Mathf.Max(0,upperLength*upperLength-along*along));
 Vector3 elbowGoal=shoulder.position+direction*along+bend*height;wristGoal=shoulder.position+direction*distance;
 shoulder.rotation=Quaternion.FromToRotation(baseYaw.TransformDirection(upperRest),elbowGoal-shoulder.position)*baseYaw.rotation;
 elbow.rotation=Quaternion.FromToRotation(shoulder.TransformDirection(foreRest),wristGoal-elbow.position)*shoulder.rotation;
 wrist.rotation=baseYaw.rotation*Quaternion.FromToRotation(toolRest,Vector3.down);
 MaxGripError=Mathf.Max(MaxGripError,Vector3.Distance(gripSocket.position,target));SolveCount++;
 }


public System.Collections.IEnumerator AnimateGrip(bool holding,float duration){IsHolding=holding;if(holding)CloseCount++;else OpenCount++;float from=GripOpen,to=holding?0:1;for(float t=0;t<1;t+=Time.deltaTime/Mathf.Max(.02f,duration)){SetGripPose(Mathf.Lerp(from,to,Mathf.SmoothStep(0,1,t)));yield return null;}SetGripPose(to);}


public void SetGripPose(float open){GripOpen=Mathf.Clamp01(open);float angle=Mathf.Lerp(-8,25,GripOpen);if(leftJaw)leftJaw.localRotation=leftJawRest*Quaternion.AngleAxis(-angle,Vector3.forward);if(rightJaw)rightJaw.localRotation=rightJawRest*Quaternion.AngleAxis(angle,Vector3.forward);}
}
}