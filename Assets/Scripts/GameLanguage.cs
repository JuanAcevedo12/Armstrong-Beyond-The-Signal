using System;using System.Collections.Generic;using System.Linq;using System.Text.RegularExpressions;using UnityEngine;using UnityEngine.SceneManagement;using UnityEngine.UI;
namespace Caronte {
public static class GameLanguage {
 public static bool English {get;private set;}
 public static event Action Changed;
 static Dictionary<string,string> entries;static Regex pattern;
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] static void Reset(){entries=null;pattern=null;Changed=null;English=PlayerPrefs.GetInt("Caronte.Language",0)==1;}
 [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Boot(){SceneManager.sceneLoaded-=SceneLoaded;SceneManager.sceneLoaded+=SceneLoaded;Load();}
 static void SceneLoaded(Scene s,LoadSceneMode m){foreach(var root in s.GetRootGameObjects())foreach(var t in root.GetComponentsInChildren<Text>(true))if(!(t is LocalizedText)&&!t.GetComponent<SerializedTextLocale>())t.gameObject.AddComponent<SerializedTextLocale>();}
 public static void Toggle(){SetEnglish(!English);}
 public static void SetEnglish(bool english){if(English==english)return;English=english;PlayerPrefs.SetInt("Caronte.Language",english?1:0);PlayerPrefs.Save();if(Changed!=null)Changed();}
 static void Load(){if(entries!=null)return;entries=new Dictionary<string,string>(StringComparer.Ordinal);var file=Resources.Load<TextAsset>("CaronteTranslations");if(!file){Debug.LogError("Missing CaronteTranslations");return;}
  foreach(var line in file.text.Split('\n')){int tab=line.IndexOf('\t');if(tab<1)continue;var a=Decode(line.Substring(0,tab));var b=Decode(line.Substring(tab+1).TrimEnd('\r'));entries[a]=b;if(a.ToUpperInvariant()!=a)entries[a.ToUpperInvariant()]=b.ToUpperInvariant();}
  var keys=entries.Keys.Where(k=>k.Length>0).OrderByDescending(k=>k.Length).Select(k=>(char.IsLetter(k[0])?@"(?<!\p{L})":"")+Regex.Escape(k)+(char.IsLetter(k[k.Length-1])?@"(?!\p{L})":""));
  pattern=new Regex(string.Join("|",keys),RegexOptions.CultureInvariant);
 }
 static string Decode(string s){return s.Replace(@"\n","\n").Replace(@"\t","\t");}
 public static string Translate(string source){if(string.IsNullOrEmpty(source)||!English)return source;Load();string exact;if(entries.TryGetValue(source,out exact))return exact;return pattern.Replace(source,m=>entries[m.Value]);}
}
public sealed class SerializedTextLocale:MonoBehaviour {
 Text label;string original,last;
 void Awake(){label=GetComponent<Text>();original=label.text;Apply();}
 void OnEnable(){GameLanguage.Changed+=Apply;}
 void OnDisable(){GameLanguage.Changed-=Apply;}
 void LateUpdate(){if(label.text!=last){original=label.text;Apply();}}
 void Apply(){if(!label)return;last=GameLanguage.Translate(original);label.text=last;}
}
}