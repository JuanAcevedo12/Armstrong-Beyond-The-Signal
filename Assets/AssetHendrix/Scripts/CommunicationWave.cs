using UnityEngine;using UnityEngine.UI;
namespace Caronte{
[RequireComponent(typeof(CanvasRenderer))]
public class CommunicationWave:MaskableGraphic{
 void Update(){SetVerticesDirty();}
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;const int n=160;Vector2 previous=Vector2.zero;for(int i=0;i<n;i++){float u=i/(float)(n-1);float x=r.x+u*r.width;float y=r.center.y+(Mathf.Sin(u*50-Time.time*4)*.55f+(Mathf.PerlinNoise(u*45+Time.time*.7f,1.3f)-.5f)*.55f)*r.height*.42f;Vector2 p=new Vector2(x,y);if(i>0){var delta=p-previous;var normal=new Vector2(-delta.y,delta.x).normalized*1.5f;int index=vh.currentVertCount;vh.AddVert(previous-normal,color,Vector2.zero);vh.AddVert(previous+normal,color,Vector2.zero);vh.AddVert(p+normal,color,Vector2.zero);vh.AddVert(p-normal,color,Vector2.zero);vh.AddTriangle(index,index+1,index+2);vh.AddTriangle(index,index+2,index+3);}previous=p;}}
}
}