using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEngine.Rendering;
public static class LatestWorkshopModels {
 const string Root="Assets/AssetsJX/Prefabs/Architecture";
 public static GameObject Bake(string folder,string name){
  var path=AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("t:Model",new[]{"Assets/AssetsJX/Fabs/ENTORNO/ULTIMOS MODELOS/"+folder})[0]);
  var imp=(ModelImporter)AssetImporter.GetAtPath(path);bool old=imp.isReadable;imp.isReadable=true;imp.SaveAndReimport();
  var tmp=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));tmp.transform.position=Vector3.zero;
  var rr=tmp.GetComponentsInChildren<Renderer>();var combines=tmp.GetComponentsInChildren<MeshFilter>().Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray();
  var mesh=new Mesh{indexFormat=IndexFormat.UInt32,name=name};mesh.CombineMeshes(combines,true,true);mesh.RecalculateBounds();var b=mesh.bounds;var v=mesh.vertices;var n=mesh.normals;var t=mesh.tangents;
  for(int i=0;i<v.Length;i++){var p=v[i]-b.center;v[i]=new Vector3(p.x/b.size.x,p.y/b.size.y,p.z/b.size.z);if(n.Length==v.Length)n[i]=Vector3.Scale(n[i],b.size).normalized;if(t.Length==v.Length){var q=new Vector3(t[i].x/b.size.x,t[i].y/b.size.y,t[i].z/b.size.z).normalized;t[i]=new Vector4(q.x,q.y,q.z,t[i].w);}}
  mesh.vertices=v;mesh.normals=n;mesh.tangents=t;mesh.RecalculateBounds();
  string mp=Root+"/Meshes/"+name+".asset";if(AssetDatabase.LoadAssetAtPath<Mesh>(mp)){var existing=AssetDatabase.LoadAssetAtPath<Mesh>(mp);EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;}else AssetDatabase.CreateAsset(mesh,mp);
  var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=rr[0].sharedMaterial;
  var prefab=PrefabUtility.SaveAsPrefabAsset(g,Root+"/Modules/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g);UnityEngine.Object.DestroyImmediate(tmp);imp.isReadable=old;imp.SaveAndReimport();return prefab;
 }
 public static string Apply(){
  var floor=Bake("Suelo","LatestFloor");var wall=Bake("Pared","LatestWall");
  string path=Root+"/WorkshopStructure_Futuristic_JX.prefab";var root=PrefabUtility.LoadPrefabContents(path);int floors=0,walls=0;
  foreach(var f in root.GetComponentsInChildren<MeshFilter>(true)){
   bool deck=f.name.StartsWith("DeckTile_");bool panel=f.name.StartsWith("RightWallModule_")||f.name.StartsWith("ForwardWallModule_");if(!deck&&!panel)continue;
   var src=(deck?floor:wall);f.sharedMesh=src.GetComponent<MeshFilter>().sharedMesh;var r=f.GetComponent<MeshRenderer>();r.sharedMaterial=src.GetComponent<MeshRenderer>().sharedMaterial;r.receiveShadows=false;
   if(deck){f.transform.localRotation=Quaternion.identity;f.transform.localScale=new Vector3(3.98f,.06f,3.98f);var p=f.transform.localPosition;p.y=-.01f;f.transform.localPosition=p;floors++;}
   else {f.transform.localScale=new Vector3(3.94f,5.86f,.16f);var p=f.transform.localPosition;p.y=3;f.transform.localPosition=p;walls++;}
  }
  PrefabUtility.SaveAsPrefabAsset(root,path);PrefabUtility.UnloadPrefabContents(root);AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();return floors+" floor modules; "+walls+" wall panels";
 }
}