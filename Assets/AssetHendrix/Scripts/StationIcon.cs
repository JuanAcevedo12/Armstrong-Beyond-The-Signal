using UnityEngine;using UnityEngine.UI;
namespace Caronte {
public class StationIcon : MaskableGraphic {
 public int kind,variant; Vector2 Point(float x,float y){var r=rectTransform.rect;return new Vector2(r.xMin+x*r.width,r.yMin+y*r.height);}
 void Line(VertexHelper vh,float x,float y,float u,float v,float width=.045f){Vector2 a=Point(x,y),b=Point(u,v);var d=(b-a).normalized;var n=new Vector2(-d.y,d.x)*rectTransform.rect.width*width*.5f;int k=vh.currentVertCount;vh.AddVert(a-n,color,Vector2.zero);vh.AddVert(a+n,color,Vector2.zero);vh.AddVert(b+n,color,Vector2.zero);vh.AddVert(b-n,color,Vector2.zero);vh.AddTriangle(k,k+1,k+2);vh.AddTriangle(k,k+2,k+3);}
 void Box(VertexHelper vh,float x,float y,float w,float h){Line(vh,x,y,x+w,y);Line(vh,x+w,y,x+w,y+h);Line(vh,x+w,y+h,x,y+h);Line(vh,x,y+h,x,y);}
 void Circle(VertexHelper vh,float x,float y,float radius){for(int i=0;i<24;i++){float a=i/24f*Mathf.PI*2,b=(i+1)/24f*Mathf.PI*2;Line(vh,x+Mathf.Cos(a)*radius,y+Mathf.Sin(a)*radius,x+Mathf.Cos(b)*radius,y+Mathf.Sin(b)*radius);}}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();switch(kind%11){
 case 0:Box(vh,.12f,.2f,.75f,.55f);Circle(vh,.5f,.48f,.17f);Box(vh,.23f,.75f,.25f,.12f);break;
 case 1:Box(vh,.1f,.2f,.8f,.6f);for(int i=1;i<4;i++)Line(vh,.1f+i*.2f,.2f,.1f+i*.2f,.8f);Line(vh,.1f,.5f,.9f,.5f);break;
 case 2:Box(vh,.15f,.2f,.66f,.6f);Box(vh,.81f,.37f,.08f,.25f);Line(vh,.34f,.5f,.65f,.5f);Line(vh,.5f,.35f,.5f,.65f);break;
 case 3:Line(vh,.5f,.15f,.5f,.65f);Line(vh,.22f,.15f,.78f,.15f);Line(vh,.2f,.8f,.5f,.55f);Line(vh,.5f,.55f,.8f,.8f);Circle(vh,.5f,.55f,.12f);break;
 case 4:Box(vh,.23f,.23f,.54f,.54f);Box(vh,.37f,.37f,.26f,.26f);for(int i=0;i<3;i++){float a=.32f+i*.18f;Line(vh,a,.1f,a,.23f);Line(vh,a,.77f,a,.9f);Line(vh,.1f,a,.23f,a);Line(vh,.77f,a,.9f,a);}break;
 case 5:Circle(vh,.5f,.5f,.33f);Circle(vh,.5f,.5f,.13f);Box(vh,.68f,.68f,.16f,.16f);break;
 case 7:Line(vh,.15f,.8f,.5f,.95f);Line(vh,.5f,.95f,.85f,.8f);Line(vh,.85f,.8f,.78f,.3f);Line(vh,.78f,.3f,.5f,.08f);Line(vh,.5f,.08f,.22f,.3f);Line(vh,.22f,.3f,.15f,.8f);break;case 8:Box(vh,.24f,.2f,.52f,.6f);for(int i=0;i<3;i++)Line(vh,.3f,.3f+i*.17f,.7f,.3f+i*.17f);break;case 9:Line(vh,.3f,.2f,.3f,.7f);Line(vh,.3f,.7f,.5f,.95f);Line(vh,.5f,.95f,.7f,.7f);Line(vh,.7f,.7f,.7f,.2f);Line(vh,.3f,.2f,.7f,.2f);Line(vh,.4f,.15f,.38f,0);Line(vh,.6f,.15f,.62f,0);break;case 10:Line(vh,.2f,.8f,.5f,.5f);Line(vh,.5f,.5f,.8f,.8f);Line(vh,.5f,.5f,.5f,.2f);Line(vh,.2f,.2f,.8f,.2f);Circle(vh,.5f,.82f,.08f);break;default:Line(vh,.15f,.75f,.4f,.5f);Line(vh,.4f,.5f,.85f,.85f);Line(vh,.2f,.18f,.8f,.18f);break;}for(int i=0;i<=variant;i++)Line(vh,.3f+i*.16f,.02f,.37f+i*.16f,.02f,.055f);}
 }
}