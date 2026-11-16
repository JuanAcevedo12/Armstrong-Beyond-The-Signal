using UnityEngine;using UnityEngine.Rendering;
namespace Caronte{
public class SpaceScenery:MonoBehaviour{
 public Mesh starMesh;public Material starMaterial,meteorMaterial;MaterialPropertyBlock meteorTint;public int meteorPoolSize=6;
 public int StarCount{get{return starMesh?starMesh.vertexCount/4:0;}}public int ActiveMeteors{get;private set;}
 LineRenderer[] streaks;Vector3[] positions,velocities;float[] ages,lifetimes,lengths;float nextMeteor;System.Random random=new System.Random(2718);
 float Pick(float min,float max){return min+(max-min)*(float)random.NextDouble();}
 public void Build(){
 var root=transform.Find("Starfield");if(!root){root=new GameObject("Starfield").transform;root.SetParent(transform,false);}root.gameObject.SetActive(true);
 foreach(Transform old in root)if(old.name!="DeepStarfield"&&!old.name.StartsWith("ShootingStar_"))old.gameObject.SetActive(false);
 var stars=root.Find("DeepStarfield");if(!stars){stars=new GameObject("DeepStarfield",typeof(MeshFilter),typeof(MeshRenderer)).transform;stars.SetParent(root,false);}stars.gameObject.SetActive(true);stars.gameObject.layer=29;stars.GetComponent<MeshFilter>().sharedMesh=starMesh;var renderer=stars.GetComponent<MeshRenderer>();renderer.sharedMaterial=starMaterial;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
 streaks=new LineRenderer[meteorPoolSize];positions=new Vector3[meteorPoolSize];velocities=new Vector3[meteorPoolSize];ages=new float[meteorPoolSize];lifetimes=new float[meteorPoolSize];lengths=new float[meteorPoolSize];
 for(int i=0;i<meteorPoolSize;i++){var t=root.Find("ShootingStar_"+i);if(!t){t=new GameObject("ShootingStar_"+i,typeof(LineRenderer)).transform;t.SetParent(root,false);}t.gameObject.layer=29;var line=t.GetComponent<LineRenderer>();line.sharedMaterial=meteorMaterial?meteorMaterial:starMaterial;line.positionCount=2;line.useWorldSpace=true;line.startWidth=3;line.endWidth=.1f;line.numCapVertices=3;line.shadowCastingMode=ShadowCastingMode.Off;line.receiveShadows=false;line.enabled=false;streaks[i]=line;ages[i]=100;}
 nextMeteor=Time.time+.4f;
 }
 void Start(){Build();}
 void Spawn(){for(int i=0;i<streaks.Length;i++)if(!streaks[i].enabled){positions[i]=new Vector3(Pick(-560,-380),Pick(85,235),Pick(320,850));velocities[i]=new Vector3(Pick(-20,15),Pick(-140,-75),Pick(-280,-180));ages[i]=0;lifetimes[i]=Pick(.75f,1.5f);lengths[i]=Pick(25,65);streaks[i].enabled=true;break;}}
 void Update(){var room=GetComponent<StationRoomController>();if(room&&room.IsPaused)return;if(streaks==null)return;if(Time.time>=nextMeteor){Spawn();nextMeteor=Time.time+Pick(1.4f,3.6f);}ActiveMeteors=0;for(int i=0;i<streaks.Length;i++){var line=streaks[i];if(!line.enabled)continue;ages[i]+=Time.deltaTime;float t=ages[i]/lifetimes[i];if(t>=1){line.enabled=false;continue;}ActiveMeteors++;positions[i]+=velocities[i]*Time.deltaTime;float fade=Mathf.Sin(t*Mathf.PI);line.SetPosition(0,positions[i]);line.SetPosition(1,positions[i]-velocities[i].normalized*lengths[i]*Mathf.Min(1,t*5));line.startColor=new Color(.8f,.93f,1,fade);line.endColor=new Color(.25f,.6f,1,0);if(meteorTint==null)meteorTint=new MaterialPropertyBlock();meteorTint.Clear();meteorTint.SetColor("_BaseColor",new Color(.85f,.95f,1,fade));line.SetPropertyBlock(meteorTint);}}
}
}