using UnityEngine;using UnityEngine.UI;using System;
namespace Caronte{
public class MissionDesignPanel:MonoBehaviour{
 CaronteStation station;Transform root,cards;Text missionText,conditions;Button[] tabs=new Button[11];int category;string revision;
 Color ink=new Color(.85f,.93f,1),cyan=new Color(.25f,.82f,1);
 RectTransform Rect(Transform parent,string name,float x,float y,float w,float h){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(parent,false);var r=g.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=new Vector2(0,1);r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);return r;}
 Text Label(Transform p,string t,float x,float y,float w,float h,int size,Color c){var r=Rect(p,"Text",x,y,w,h);var l=r.gameObject.AddComponent<Caronte.LocalizedText>();l.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");l.text=t;l.fontSize=size;l.color=c;l.raycastTarget=false;l.resizeTextForBestFit=true;l.resizeTextMinSize=14;l.resizeTextMaxSize=size;return l;}
 Button Button(Transform p,string name,float x,float y,float w,float h,Action action){var r=Rect(p,name,x,y,w,h);r.gameObject.AddComponent<Image>().color=new Color(.06f,.18f,.26f);var b=r.gameObject.AddComponent<Button>();b.onClick.AddListener(()=>{if(station.ConsoleActive&&!station.Busy&&station.app.Stage==1){action();var fx=station.GetComponent<StationPolish>();if(fx)fx.PulseScreen();}});return b;}
 public void Build(Transform p,CaronteStation s){station=s;root=p;
 var previous=Button(p,"PreviousMission",0,0,50,42,()=>station.app.SelectMission((station.app.Configuration.mission+5)%6));Label(previous.transform,"<",16,4,35,34,26,cyan);
 var next=Button(p,"NextMission",802,0,50,42,()=>station.app.SelectMission((station.app.Configuration.mission+1)%6));Label(next.transform,">",16,4,35,34,26,cyan);
 var confirm=Button(p,"ConfirmMission",65,0,725,42,()=>station.app.SelectMission(station.app.Configuration.mission));missionText=Label(confirm.transform,"",6,2,713,40,25,cyan);conditions=Label(p,"",0,51,852,89,20,ink);
 for(int i=0;i<11;i++){int cat=i;var b=Button(p,"Category_"+i,(i%4)*214,147+(i/4)*40,205,34,()=>{category=cat;revision=null;});tabs[i]=b;StationPolish.Decorate(b.transform,new[]{8,0,1,2,9,3,5,7,4,9,10}[i],6,5,24);Label(b.transform,MissionCatalog.Categories[i],38,3,163,28,18,ink);}
 cards=Rect(p,"OptionCards",0,279,852,310);Draw();
 }
 public void FocusCategory(int index){category=Mathf.Clamp(index,0,10);Draw();}
 void LateUpdate(){if(station==null)return;var control=GetComponent<MissionControlDirector>();if(control&&control.IsTutorial&&control.TutorialCategory>=0&&category!=control.TutorialCategory){category=control.TutorialCategory;Draw();}string now=JsonUtility.ToJson(station.app.Configuration)+category+(control?control.MissionConfirmed.ToString()+control.AssignedMission:"");if(now!=revision){Draw();revision=now;}}
 void Draw(){var c=station.app.Configuration;var m=c.Mission;var control=GetComponent<MissionControlDirector>();missionText.text="MISIÓN "+(c.mission+1)+"/6 · "+m.name.ToUpper()+(control?(control.MissionConfirmed&&c.mission==control.AssignedMission?" ✓":" · CONFIRMAR"):"");
 conditions.text=m.conditions+"\nΔv "+MissionCatalog.DeltaV[c.mission]+" m/s · "+MissionCatalog.Radiation[c.mission]+" krad · presupuesto "+m.budget+" cr\n"+m.goal;
 for(int i=0;i<11;i++)tabs[i].GetComponent<Image>().color=i==category?new Color(.08f,.42f,.45f):new Color(.06f,.18f,.26f);
 for(int i=cards.childCount-1;i>=0;i--)DestroyImmediate(cards.GetChild(i).gameObject);
 for(int i=0;i<3;i++){int option=i;var o=MissionCatalog.Options[category][i];var b=Button(cards,"Option_"+i,0,i*86,852,80,()=>station.app.SelectComponent(category,option));b.GetComponent<Image>().color=c.choices[category]==i?new Color(.07f,.32f,.38f):new Color(.05f,.12f,.2f);
 var icon=StationPolish.Decorate(b.transform,new[]{8,0,1,2,9,3,5,7,4,9,10}[category],8,10,54);icon.variant=i;Label(b.transform,o.criterion,80,5,510,25,22,cyan);Label(b.transform,o.name,80,31,500,22,18,ink);
 Label(b.transform,o.impact+ (o.power>0?" · Consume "+o.power+" W":""),80,54,750,22,16,ink);
 Label(b.transform,o.cost.ToString("0.#")+" cr / "+o.mass.ToString("0.#")+" kg",590,7,245,24,20,ink);
 }
 if(c.mission==4&&category==1){var b=Button(cards,"SecondaryInstrument",0,263,852,38,()=>station.app.ToggleSecondInstrument());Label(b.transform,(c.secondInstrument?"[✓]":"[ ]")+" Añadir sensor de campos y partículas · 5 cr / 10 kg / 12 W",12,5,820,30,21,cyan);}
 }
}}
