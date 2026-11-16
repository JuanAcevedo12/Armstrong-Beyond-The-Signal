using UnityEngine;using UnityEngine.UI;using System.Collections.Generic;
namespace Caronte{
public class MissionGraph:MaskableGraphic{
 public List<Sample> samples;public bool energy;public float capacity;public double windowSeconds;public int maxPoints=700;
 protected override void OnPopulateMesh(VertexHelper vh){
 vh.Clear();if(samples==null||samples.Count<2)return;
 var r=rectTransform.rect;double end=samples[samples.Count-1].time;double start=windowSeconds>0?System.Math.Max(0,end-windowSeconds):0;
 double span=windowSeconds>0?System.Math.Max(windowSeconds,System.Math.Min(end,windowSeconds)):System.Math.Max(1,end);
 int first=0;while(first<samples.Count-1&&samples[first].time<start)first++;
 int stride=System.Math.Max(1,(samples.Count-first+maxPoints-2)/(maxPoints-1));float max=System.Math.Max(1,capacity);
 if(!energy){max=1;for(int i=first;i<samples.Count;i++)max=Mathf.Max(max,samples[i].stored,samples[i].sent);}
 for(int series=0;series<(energy?1:2);series++){
 int previous=first;
 for(int i=first+stride;i<samples.Count+stride;i+=stride){
 int current=System.Math.Min(i,samples.Count-1);if(current<=previous)break;
 var a=samples[previous];var b=samples[current];float av=energy?a.energy:series==0?a.stored:a.sent,bv=energy?b.energy:series==0?b.stored:b.sent;
 Vector2 p=new Vector2(r.xMin+(float)((a.time-start)/span)*r.width,r.yMin+av/max*r.height),q=new Vector2(r.xMin+(float)((b.time-start)/span)*r.width,r.yMin+bv/max*r.height);
 var n=(q-p).normalized;var d=new Vector2(-n.y,n.x)*1.15f;int k=vh.currentVertCount;Color c=energy?new Color(.31f,.82f,.65f):series==0?new Color(.49f,.77f,1):new Color(1,.73f,.3f);
 vh.AddVert(p-d,c,Vector2.zero);vh.AddVert(p+d,c,Vector2.zero);vh.AddVert(q+d,c,Vector2.zero);vh.AddVert(q-d,c,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);previous=current;if(current==samples.Count-1)break;
 }}}
}}
