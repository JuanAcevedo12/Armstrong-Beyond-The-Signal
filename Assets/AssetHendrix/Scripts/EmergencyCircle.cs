using UnityEngine;using UnityEngine.UI;
namespace Caronte{public class EmergencyCircle:Image{
 protected override void OnPopulateMesh(VertexHelper vh){vh.Clear();var r=rectTransform.rect;var c=r.center;float rad=Mathf.Min(r.width,r.height)*.5f;vh.AddVert(c,color,Vector2.zero);for(int i=0;i<=48;i++){float a=i/48f*Mathf.PI*2;vh.AddVert(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*rad,color,Vector2.zero);if(i>0)vh.AddTriangle(0,i,i+1);}}
}}