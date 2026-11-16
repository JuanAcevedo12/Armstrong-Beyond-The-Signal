using UnityEngine;using UnityEngine.UI;
namespace Caronte {
[AddComponentMenu("UI/Localized Text")]
public sealed class LocalizedText:Text {
 string source;
 public override string text {get{return base.text;}set{value=value??"";if(source==value)return;source=value;base.text=GameLanguage.Translate(source);}}
 protected override void OnEnable(){if(source==null)source=base.text;GameLanguage.Changed+=RefreshLanguage;RefreshLanguage();base.OnEnable();}
 protected override void OnDisable(){GameLanguage.Changed-=RefreshLanguage;base.OnDisable();}
 void RefreshLanguage(){base.text=GameLanguage.Translate(source??base.text);}
 public string DisplayText {get{return base.text;}}
}
}