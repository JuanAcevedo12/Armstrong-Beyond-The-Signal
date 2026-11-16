using System;using System.Linq;using UnityEngine;using UnityEditor;using UnityEngine.Rendering;
public static class MissionPlanetBuilder {
 const string Folder="Assets/AssetsJX/Prefabs/Planets";
 static void Ensure(string p){if(AssetDatabase.IsValidFolder(p))return;int i=p.LastIndexOf('/');Ensure(p.Substring(0,i));AssetDatabase.CreateFolder(p.Substring(0,i),p.Substring(i+1));}
 public static GameObject Make(string file,string name,bool sun=false,bool removeRing=false){
  string path="Assets/AssetsJX/PLANETAS/"+file;var importer=(ModelImporter)AssetImporter.GetAtPath(path);bool readable=importer.isReadable;importer.isReadable=true;importer.SaveAndReimport();
  var temp=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(path));temp.transform.position=Vector3.zero;
  var filters=temp.GetComponentsInChildren<MeshFilter>().Where(f=>!removeRing||Mathf.Min(f.sharedMesh.bounds.size.x,Mathf.Min(f.sharedMesh.bounds.size.y,f.sharedMesh.bounds.size.z))>Mathf.Max(f.sharedMesh.bounds.size.x,Mathf.Max(f.sharedMesh.bounds.size.y,f.sharedMesh.bounds.size.z))*.1f).ToArray();
  var combined=new Mesh{name="Planet_"+name,indexFormat=IndexFormat.UInt32};combined.CombineMeshes(filters.Select(f=>new CombineInstance{mesh=f.sharedMesh,transform=f.transform.localToWorldMatrix}).ToArray(),true,true);
  combined.RecalculateBounds();var b=combined.bounds;float diameter=Mathf.Max(b.size.x,Mathf.Max(b.size.y,b.size.z));combined.vertices=combined.vertices.Select(v=>(v-b.center)/diameter).ToArray();combined.RecalculateBounds();
  string meshPath=Folder+"/Meshes/"+name+".asset";var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);if(mesh){EditorUtility.CopySerialized(combined,mesh);UnityEngine.Object.DestroyImmediate(combined);}else{mesh=combined;AssetDatabase.CreateAsset(mesh,meshPath);}
  string matPath=Folder+"/Materials/"+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(matPath);
  var fresh=sun?new Material(Shader.Find("Universal Render Pipeline/Unlit")):new Material(filters[0].GetComponent<Renderer>().sharedMaterial);fresh.name=name;
  if(sun){fresh.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/AssetsJX/PLANETAS/sun_diff.png"));fresh.SetColor("_BaseColor",Color.white);}else{fresh.SetFloat("_Metallic",0);fresh.SetFloat("_Smoothness",.12f);}
  if(material){EditorUtility.CopySerialized(fresh,material);UnityEngine.Object.DestroyImmediate(fresh);}else{material=fresh;AssetDatabase.CreateAsset(material,matPath);}
  var g=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));g.GetComponent<MeshFilter>().sharedMesh=mesh;var r=g.GetComponent<MeshRenderer>();r.sharedMaterial=material;r.shadowCastingMode=ShadowCastingMode.Off;r.receiveShadows=false;
  var prefab=PrefabUtility.SaveAsPrefabAsset(g,Folder+"/"+name+".prefab");UnityEngine.Object.DestroyImmediate(g);UnityEngine.Object.DestroyImmediate(temp);importer.isReadable=readable;importer.SaveAndReimport();return prefab;
 }
 public static string Build(){
  foreach(var p in new[]{Folder,Folder+"/Meshes",Folder+"/Materials"})Ensure(p);
  var earth=Make("planeta tierra.fbx","Earth_JX");var moon=Make("Luna.fbx","Moon_JX");var jupiter=Make("Jupiter.fbx","Jupiter_JX",false,true);var europa=Make("europa moon.fbx","Europa_JX");var sun=Make("Sol.fbx","Sun_JX",true);
  var d=UnityEngine.Object.FindFirstObjectByType<Caronte.MissionDestination>();d.bodyModels=new[]{earth,earth,moon,null,jupiter,sun};d.europaModel=europa;d.ApplyMission(d.app.Configuration.mission);
  EditorUtility.SetDirty(d);AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(d.gameObject.scene);UnityEditor.SceneManagement.EditorSceneManager.SaveOpenScenes();return "Five imported models assigned. Earth reused for telescope mission; Mars retains its current visual. Jupiter uses its spherical body; source ring excluded.";
 }
}