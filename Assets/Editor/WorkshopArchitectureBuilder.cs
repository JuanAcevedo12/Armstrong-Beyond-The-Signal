using System;using System.Linq;using System.Collections.Generic;using UnityEngine;using UnityEditor;using UnityEngine.Rendering;
public static class WorkshopArchitectureBuilder {
 const string Folder="Assets/AssetsJX/Prefabs/Architecture";
 struct V{public Vector3 p,n;public Vector2 uv;public Vector4 t;public static V Lerp(V a,V b,float f){return new V{p=Vector3.Lerp(a.p,b.p,f),n=Vector3.Lerp(a.n,b.n,f).normalized,uv=Vector2.Lerp(a.uv,b.uv,f),t=Vector4.Lerp(a.t,b.t,f)};}}
 static void Ensure(string p){if(AssetDatabase.IsValidFolder(p))return;int i=p.LastIndexOf('/');Ensure(p.Substring(0,i));AssetDatabase.CreateFolder(p.Substring(0,i),p.Substring(i+1));}
 static Material Mat(string name,Color color,float metal=.65f,float smooth=.35f,bool transparent=false){
  string path=Folder+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(m)return m;
  m=new Material(Shader.Find(transparent?"Universal Render Pipeline/Unlit":"Universal Render Pipeline/Lit"));m.name=name;m.SetColor("_BaseColor",color);m.SetFloat("_Metallic",metal);m.SetFloat("_Smoothness",smooth);
  if(transparent){m.SetFloat("_Surface",1);m.SetFloat("_Blend",0);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.SetFloat("_Cull",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;}
  AssetDatabase.CreateAsset(m,path);return m;
 }
 static List<V> Clip(List<V> polygon,int axis,float boundary,bool less){
  var result=new List<V>();if(polygon.Count==0)return result;
  V previous=polygon[polygon.Count-1];float da=(axis==0?previous.p.x:previous.p.y)-boundary;bool ina=less?da<=0:da>=0;
  foreach(var current in polygon){float db=(axis==0?current.p.x:current.p.y)-boundary;bool inb=less?db<=0:db>=0;
   if(ina!=inb)result.Add(V.Lerp(previous,current,da/(da-db)));if(inb)result.Add(current);previous=current;da=db;ina=inb;
  }return result;
 }
 static Mesh OpenCenter(Mesh source){
  var verts=source.vertices;var normals=source.normals;var uvs=source.uv;var tans=source.tangents;var inds=source.triangles;
  var result=new List<V>();var indices=new List<int>();
  for(int k=0;k<inds.Length;k+=3){
   var tri=new List<V>();for(int j=0;j<3;j++){int i=inds[k+j];tri.Add(new V{p=verts[i],n=normals[i],uv=uvs.Length>i?uvs[i]:Vector2.zero,t=tans.Length>i?tans[i]:new Vector4(1,0,0,1)});}
   for(int region=0;region<4;region++){
    var p=new List<V>(tri);
    if(region==0)p=Clip(p,0,-.4f,true);
    if(region==1)p=Clip(p,0,.4f,false);
    if(region>=2){p=Clip(p,0,-.4f,false);p=Clip(p,0,.4f,true);p=Clip(p,1,region==2?-.4f:.4f,region==2);}
    for(int j=1;j<p.Count-1;j++){int start=result.Count;result.Add(p[0]);result.Add(p[j]);result.Add(p[j+1]);indices.Add(start);indices.Add(start+1);indices.Add(start+2);}
   }
  }
  var m=new Mesh{indexFormat=IndexFormat.UInt32};m.SetVertices(result.Select(v=>v.p).ToList());m.SetNormals(result.Select(v=>v.n).ToList());m.SetUVs(0,result.Select(v=>v.uv).ToList());m.SetTangents(result.Select(v=>v.t).ToList());m.SetTriangles(indices,0);m.RecalculateBounds();return m;
 }
 static GameObject Bake(string id,bool cut=false){
  string name="Module_"+id+(cut?"_Open":"");string assetPath=Folder+"/Modules/"+name+".prefab";var existing=AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);if(existing)return existing;
  string path=AssetDatabase.FindAssets("t:Model",new[]{"Assets/AssetsJX/Fabs/ENTORNO"}).Select(AssetDatabase.GUIDToAssetPath).First(p=>p.Contains(id));
  var importer=(ModelImporter)AssetImporter.GetAtPath(path);bool readable=importer.isReadable;importer.isReadable=true;importer.SaveAndReimport();
  var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);var tmp=UnityEngine.Object.Instantiate(model);tmp.transform.position=Vector3.zero;
  var rr=tmp.GetComponentsInChildren<Renderer>();Bounds b=rr[0].bounds;foreach(var r in rr)b.Encapsulate(r.bounds);
  Quaternion rotation=b.size.x<b.size.z?Quaternion.Euler(0,90,0):Quaternion.identity;
  var filters=tmp.GetComponentsInChildren<MeshFilter>();
  var combines=filters.Select(mf=>new CombineInstance{mesh=mf.sharedMesh,transform=Matrix4x4.Rotate(rotation)*mf.transform.localToWorldMatrix}).ToArray();
  var mesh=new Mesh{indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(combines,true,true);mesh.RecalculateBounds();b=mesh.bounds;
  var vertices=mesh.vertices;var normals=mesh.normals;var tangents=mesh.tangents;Vector3 size=b.size;
  for(int i=0;i<vertices.Length;i++){var p=vertices[i]-b.center;vertices[i]=new Vector3(p.x/size.x,p.y/size.y,p.z/size.z);if(normals.Length==vertices.Length)normals[i]=Vector3.Scale(normals[i],size).normalized;if(tangents.Length==vertices.Length){var t=new Vector3(tangents[i].x/size.x,tangents[i].y/size.y,tangents[i].z/size.z).normalized;tangents[i]=new Vector4(t.x,t.y,t.z,tangents[i].w);}}
  mesh.vertices=vertices;if(normals.Length==vertices.Length)mesh.normals=normals;else mesh.RecalculateNormals();if(tangents.Length==vertices.Length)mesh.tangents=tangents;mesh.RecalculateBounds();
  if(cut){var clipped=OpenCenter(mesh);UnityEngine.Object.DestroyImmediate(mesh);mesh=clipped;}mesh.name=name;
  AssetDatabase.CreateAsset(mesh,Folder+"/Meshes/"+name+".asset");
  var go=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));go.GetComponent<MeshFilter>().sharedMesh=mesh;
  var source=rr[0].sharedMaterial;var material=new Material(source);material.name=name;material.SetFloat("_Cull",0);AssetDatabase.CreateAsset(material,Folder+"/Materials/"+name+".mat");go.GetComponent<MeshRenderer>().sharedMaterial=material;
  var prefab=PrefabUtility.SaveAsPrefabAsset(go,assetPath);UnityEngine.Object.DestroyImmediate(go);UnityEngine.Object.DestroyImmediate(tmp);importer.isReadable=readable;importer.SaveAndReimport();return prefab;
 }
 static GameObject Cube(Transform parent,string name,Vector3 p,Vector3 size,Material mat,bool collide=true){
  var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localScale=size;g.GetComponent<Renderer>().sharedMaterial=mat;if(!collide)UnityEngine.Object.DestroyImmediate(g.GetComponent<Collider>());return g;
 }
 static GameObject Module(Transform parent,GameObject prefab,string name,Vector3 p,Vector3 size,Quaternion rot){
  var g=(GameObject)PrefabUtility.InstantiatePrefab(prefab);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=p;g.transform.localRotation=rot;g.transform.localScale=size;return g;
 }
 public static string Build(){
  if(EditorApplication.isPlaying)throw new Exception("Stop play mode.");
  foreach(var p in new[]{Folder,Folder+"/Materials",Folder+"/Meshes",Folder+"/Modules"})Ensure(p);
  var station=UnityEngine.Object.FindFirstObjectByType<Caronte.CaronteStation>();if(station.transform.Find("WorkshopStructure_JX"))throw new Exception("Replacement structure already exists");
  var scene=station.gameObject.scene;
  System.IO.Directory.CreateDirectory(Application.dataPath+"/../Temp/WorkshopBackup");
  System.IO.File.Copy(scene.path,Application.dataPath+"/../Temp/WorkshopBackup/Prototipo.before.unity",true);
  string[] names={"Floor","FloorPanel","FloorNavigationStrip","RearBulkhead","RightBulkhead","FrontBulkhead","Ceiling","CeilingRib","CeilingLight","WindowMullion","WindowEdgeLight","WindowLowerFrame","WindowUpperFrame","WindowSafetyCollider"};
  var old=new GameObject("WorkshopStructure_Original");old.transform.SetParent(station.transform,false);
  foreach(var t in station.transform.Cast<Transform>().ToArray())if(t!=old.transform&&(names.Contains(t.name)||t.name.StartsWith("FloorExtension_")||t.name.StartsWith("WindowMullionExtension_")))t.SetParent(old.transform,true);
  PrefabUtility.SaveAsPrefabAsset(old,Folder+"/WorkshopStructure_Original.prefab");UnityEngine.Object.DestroyImmediate(old);
  var root=new GameObject("WorkshopStructure_JX");root.transform.SetParent(station.transform,false);
  var floor=new GameObject("PressureDeck");floor.transform.SetParent(root.transform,false);
  var hull=new GameObject("PressureHull");hull.transform.SetParent(root.transform,false);
  var roof=new GameObject("CeilingWithHatch");roof.transform.SetParent(root.transform,false);
  var launch=new GameObject("LaunchContainment");launch.transform.SetParent(root.transform,false);
  var dark=Mat("HullGraphite",new Color(.085f,.105f,.14f));var steel=Mat("StructuralSteel",new Color(.32f,.4f,.48f),.75f,.4f);var glass=Mat("PressureGlass",new Color(.12f,.55f,.7f,.11f),.1f,.8f,true);var safety=Mat("LaunchSafetyGlass",new Color(.15f,.65f,.8f,.18f),.1f,.75f,true);
  var glow=Mat("NavigationCyan",new Color(.03f,.45f,.6f),.25f,.4f);glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.03f,.65f,1)*1.5f);EditorUtility.SetDirty(glow);
  var plate=Bake("8d4844ec");var window=Bake("519d4e0c",true);var rearWindow=Bake("f10197eb",true);var bulk=Bake("50ddb0ba");var door=Bake("bb382278");var rim=Bake("1bdbd4c4",true);
  Cube(floor.transform,"SealedFloor",new Vector3(0,-.15f,5),new Vector3(12,.3f,20),dark);
  for(int x=0;x<3;x++)for(int z=0;z<5;z++){var p=new Vector3(-4+x*4,.015f,-3+z*4);Module(floor.transform,plate,"DeckTile_"+x+"_"+z,p,new Vector3(3.94f,3.94f,.03f),Quaternion.Euler(90,0,0));}
  foreach(float x in new[]{-4.9f,4.9f})Cube(floor.transform,"WalkwayGuide",new Vector3(x,.035f,5),new Vector3(.035f,.015f,19.5f),glow,false);
  Cube(hull.transform,"RightSolidHull",new Vector3(6.12f,3,5),new Vector3(.24f,6,20),dark);
  Cube(hull.transform,"ForwardSolidHull",new Vector3(0,3,15.12f),new Vector3(12,6,.24f),dark);
  for(int z=0;z<5;z++)Module(hull.transform,bulk,"RightWallModule_"+z,new Vector3(5.88f,3,-3+z*4),new Vector3(3.9f,4.7f,.16f),Quaternion.Euler(0,90,0));
  for(int x=0;x<3;x++)Module(hull.transform,x==2?door:bulk,x==2?"MaintenanceDoor":"ForwardWallModule_"+x,new Vector3(-4+x*4,2.8f,14.88f),new Vector3(3.7f,4.8f,.16f),Quaternion.identity);
  for(int i=0;i<5;i++){
   float z=-3+i*4;
   Module(hull.transform,window,"LeftObservationFrame_"+i,new Vector3(-6,3,z),new Vector3(3.98f,3.8f,.24f),Quaternion.Euler(0,90,0));
   var pane=Cube(hull.transform,"LeftPressureGlass_"+i,new Vector3(-6.02f,3,z),new Vector3(.045f,3.04f,3.18f),glass);pane.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  }
  for(int i=0;i<3;i++){
   float x=-4+i*4;Module(hull.transform,rearWindow,"RearObservationFrame_"+i,new Vector3(x,3,-5),new Vector3(3.98f,3.8f,.24f),Quaternion.identity);
   var pane=Cube(hull.transform,"RearPressureGlass_"+i,new Vector3(x,3,-5.02f),new Vector3(3.18f,3.04f,.045f),glass);pane.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
  }
  foreach(float y in new[]{.55f,5.45f}){Cube(hull.transform,"LeftWindowSpandrel",new Vector3(-6,y,5),new Vector3(.32f,1.1f,20),dark);Cube(hull.transform,"RearWindowSpandrel",new Vector3(0,y,-5),new Vector3(12,1.1f,.32f),dark);}
  for(int i=0;i<=5;i++)Cube(hull.transform,"LeftStructuralPost_"+i,new Vector3(-5.94f,3,-5+i*4),new Vector3(.32f,6,.18f),steel);
  for(int i=0;i<=3;i++)Cube(hull.transform,"RearStructuralPost_"+i,new Vector3(-6+i*4,3,-4.94f),new Vector3(.18f,6,.32f),steel);
  for(int i=0;i<4;i++)Cube(hull.transform,"CornerColumn_"+i,new Vector3(i<2?-6:6,3,i%2==0?-5:15),new Vector3(.4f,6,.4f),steel);
  Cube(roof.transform,"RoofLeft",new Vector3(-4,6.15f,5),new Vector3(4,.3f,20),dark);
  Cube(roof.transform,"RoofRight",new Vector3(4,6.15f,5),new Vector3(4,.3f,20),dark);
  Cube(roof.transform,"RoofFrontOfOpening",new Vector3(0,6.15f,-.4f),new Vector3(4,.3f,9.2f),dark);
  Cube(roof.transform,"RoofBehindOpening",new Vector3(0,6.15f,11.6f),new Vector3(4,.3f,6.8f),dark);
  for(int x=0;x<3;x++)for(int z=0;z<5;z++){if(x==1&&(z==2||z==3))continue;Module(roof.transform,plate,"CeilingPanel_"+x+"_"+z,new Vector3(-4+x*4,5.98f,-3+z*4),new Vector3(3.85f,3.85f,.05f),Quaternion.Euler(90,0,0));}
  for(int i=0;i<=5;i++){float z=-5+i*4;if(z>4.2f&&z<8.2f){foreach(float bx in new[]{-4f,4f})Cube(roof.transform,"SplitRoofBeam_"+i,new Vector3(bx,5.78f,z),new Vector3(4,.18f,.16f),steel);}else Cube(roof.transform,"RoofBeam_"+i,new Vector3(0,5.78f,z),new Vector3(12,.18f,.16f),steel);if(Mathf.Abs(z-6.2f)>2.3f)Cube(roof.transform,"RoofLight_"+i,new Vector3(0,5.67f,z),new Vector3(5,.035f,.055f),glow,false);}
  Module(roof.transform,rim,"LaunchHatchFrame",new Vector3(0,5.92f,6.2f),new Vector3(4.6f,4.6f,.18f),Quaternion.Euler(90,0,0));
  foreach(float x in new[]{-2.16f,2.16f})Cube(roof.transform,"HatchGuideRail",new Vector3(x,5.88f,6.2f),new Vector3(.16f,.22f,4.4f),steel);
  var airlock=root.AddComponent<Caronte.WorkshopAirlock>();
  for(int i=0;i<2;i++){
   float x=i==0?-1:1;var leaf=new GameObject("HatchLeaf_"+i);leaf.transform.SetParent(launch.transform,false);leaf.transform.localPosition=new Vector3(x,6.02f,6.2f);
   Module(leaf.transform,plate,"ImportedHatchPanel",Vector3.zero,new Vector3(1.98f,3.98f,.15f),Quaternion.Euler(90,0,0));
   var bc=leaf.AddComponent<BoxCollider>();bc.size=new Vector3(2,.15f,4);
   airlock.hatchLeaves[i]=leaf.transform;airlock.hatchClosed[i]=leaf.transform.localPosition;airlock.hatchOpen[i]=new Vector3(x*3.1f,6.02f,6.2f);
  }
  for(int i=0;i<4;i++){
   bool side=i<2;Vector3 p=side?new Vector3(i==0?-1.85f:1.85f,8.6f,6.2f):new Vector3(0,8.6f,i==2?4.35f:8.05f);
   var pane=Cube(launch.transform,"RetractableSafetyGlass_"+i,p,side?new Vector3(.045f,4.65f,3.7f):new Vector3(3.7f,4.65f,.045f),safety,true);
   pane.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;airlock.glassPanels[i]=pane.transform;airlock.glassRaised[i]=p;airlock.glassLowered[i]=new Vector3(p.x,3.675f,p.z);
   foreach(float edge in new[]{-1.82f,1.82f})Cube(launch.transform,"GlassGuide",side?new Vector3(p.x,4.1f,6.2f+edge):new Vector3(edge,4.1f,p.z),new Vector3(.035f,3.8f,.035f),steel,false);
  }
  airlock.launchExit=new Vector3(station.assemblyOrigin.position.x,13f,station.assemblyOrigin.position.z);
  var prefab=PrefabUtility.SaveAsPrefabAsset(root,Folder+"/WorkshopStructure_Futuristic_JX.prefab");UnityEngine.Object.DestroyImmediate(root);
  var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab);instance.transform.SetParent(station.transform,false);
  AssetDatabase.SaveAssets();UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene);
  return "Original structure archived; futuristic JX structure instantiated. Renderers="+instance.GetComponentsInChildren<Renderer>().Length+"; colliders="+instance.GetComponentsInChildren<Collider>().Length;
 }
}
