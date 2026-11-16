using UnityEngine;
namespace Caronte{
public static class MissionResourceGuides{
 public static MissionConfig Reference(int mission){var c=new MissionConfig();c.mission=mission;c.choices=(int[])MissionCatalog.Missions[mission].required.Clone();c.secondInstrument=mission==4;return c;}
}
}