using System;using System.Collections.Generic;
namespace Caronte{
[Serializable]public class MissionConfig{
 public int mission;public int[] choices={0,0,0,1,0,0,1,0,0,0,0};public bool secondInstrument;
 public int payload,solar=1,battery=1,radio=1,storage=1,orbit;public float duty=.25f;
 public MissionDefinition Mission{get{return MissionCatalog.Missions[mission];}}
 public MissionConfig Copy(){var c=(MissionConfig)MemberwiseClone();c.choices=(int[])choices.Clone();return c;}
 public float DryMass{get{return MissionCatalog.Total(this,true);}} public float Propellant{get{return DryMass*(float)(Math.Exp(MissionCatalog.DeltaV[mission]/(9.81*new[]{65,320,2800}[choices[4]]))-1);}}
 public float Mass{get{return DryMass+Propellant;}}public float Cost{get{return MissionCatalog.Total(this,false);}}
 public float MaxMass{get{return new[]{150,850,6000}[choices[0]];}}
 public float LauncherCapacity{get{return MissionCatalog.LaunchCaps[choices[9]][mission];}}
 public float Capacity{get{return new[]{35,140,1050}[choices[3]];}}public float Storage{get{return 8192;}}
 public float Generation{get{return choices[2]==2?330:new[]{150f,2000f}[choices[2]]*MissionCatalog.SolarFlux[mission]*(mission==5?.70909f:1);}}
 public float AverageGeneration{get{return Generation*(choices[2]==2?1:1-Mission.eclipse);}}
 public float PayloadPower{get{return MissionCatalog.Options[1][choices[1]].power+(secondInstrument&&choices[1]!=2?12:0);}}
 public float Load{get{float p=0;for(int i=0;i<11;i++)if(i!=2)p+=MissionCatalog.Options[i][choices[i]].power;return p+(secondInstrument&&choices[1]!=2?12:0);}}
 public float LinkQuality{get{float ant=choices[5]==0?(mission==0?1:.001f):choices[5]==1?(Mission.rangeKm<=1500000?1:.11f):1;float gs=choices[10]==0?(mission==0||mission==2?1:mission==4?.035f:.15f):choices[10]==1?(mission==4?.24f:1):1;return ant*gs;}}
 public float Rate{get{return (mission==0?2:.2f)*LinkQuality;}}
 public float Altitude{get{return mission==0?500:mission==2?100:mission==3?400:800;}}
 public double Period{get{return mission==0?6000:mission==2?7050:mission==3?7028.5714:7200;}}
 public bool CanLaunch{get{return Mass<=MaxMass&&LauncherCapacity>0&&Mass<=LauncherCapacity&&Cost<=Mission.budget;}}
 public float ScienceQuality{
 get{float acc=new[]{1,.1f,.005f}[choices[6]];float q=0;
 if(mission==4){if(choices[1]==0&&acc<=.1f)q+=.5f;if((choices[1]==2||secondInstrument)&&acc<=1)q+=.5f;}
 else{int need=Mission.required[1];if(choices[1]==need){float req=need==0?.1f:need==1?.005f:1;q=acc<=req?1:need==1&&choices[6]==1?.03f:0;}}
 bool thermal=mission==0||choices[7]==Mission.required[7];if(!thermal)q=0;
 if(mission==1&&choices[2]==2)q=0;
 if((mission==2||mission==3||mission==4)&&choices[4]==2)q=0;
 if(mission==5&&choices[2]==1)q=0;return q;}
 }
 public bool OperationalRisk{get{return !CanLaunch||ScienceQuality<.99f||AverageGeneration<Load||Capacity<Load*(float)(Period/3600)*Mission.eclipse||LinkQuality<.9f||new[]{5f,30f,300f}[choices[8]]<MissionCatalog.Radiation[mission];}} public string Review{get{if(LauncherCapacity<=0)return "LANZADOR INCOMPATIBLE · No alcanza el destino seleccionado";if(Mass>MaxMass)return "MASA EXCEDIDA · "+Mass.ToString("0")+" / "+MaxMass+" kg de plataforma";if(Mass>LauncherCapacity)return "LANZADOR SOBRECARGADO · "+Mass.ToString("0")+" / "+LauncherCapacity+" kg";if(Cost>Mission.budget)return "PRESUPUESTO EXCEDIDO · "+Cost+" / "+Mission.budget+" cr";if(ScienceQuality<1)return "CIENCIA LIMITADA · Revisa instrumento, actitud, temperatura o maniobra";return AverageGeneration<Load?"ENERGÍA LIMITADA · Generación media menor al consumo":"DISEÑO VIABLE · Verifica batería y comunicaciones";}}
}
[Serializable]public class Sample{public double time;public float energy,stored,sent,generationW,consumptionW,chargeW;public bool eclipse;}
[Serializable]public class MissionResult{
 public MissionConfig config;public bool missionLost; public string failureReason;public double initialEnergyWh,generatedWh,consumedWh,curtailedWh;public double captured,sent,lost,useful;public float minEnergy,instrumentSeconds,blackoutSeconds;public double elapsed;
 public List<Sample> samples=new List<Sample>();public List<string> events=new List<string>();
 public float Score{get{if(missionLost)return 0;return (float)(70*Math.Min(1,useful/config.Mission.dataGoal)+15*Math.Max(0,1-lost/Math.Max(1,captured))+15*Math.Max(0,1-blackoutSeconds/2592000f));}}
}
public class MissionEngine{
 public const double Duration=2592000,StepSeconds=30;
 public MissionResult Result;double batteryWh;public float Energy{get{return (float)batteryWh;}}public double Stored;
 public bool Eclipse,Contact,Observing,Powered;public bool ElectronicsAlive{get;private set;}=true;
 public float GeneratedW,ConsumedW,BatteryFlowW;public double LastCapturedMB,LastSentMB;
 public double OrbitPhase{get{return Math.Max(0,Result.elapsed-StepSeconds*.5)%Result.config.Period/Result.config.Period;}}
 public double VisualAngle{get{return 2*Math.PI*(OrbitPhase-(1-Result.config.Mission.eclipse*.5))+Math.PI;}}
 public string MissionClock{get{var t=TimeSpan.FromSeconds(Result.elapsed);return "T+"+(int)t.TotalDays+" d "+t.Hours.ToString("00")+":"+t.Minutes.ToString("00")+":"+t.Seconds.ToString("00");}}
 public int OrbitNumber{get{return (int)(Math.Max(0,Result.elapsed-StepSeconds*.5)/Result.config.Period)+1;}}
 public double NextContactSeconds{get{return Contact?0:21600-Result.elapsed%21600;}}
 bool first=true,oldEclipse,oldContact,oldLow,oldPowered=true,failed;
 public MissionEngine(MissionConfig c){Result=new MissionResult{config=c.Copy(),minEnergy=c.Capacity,initialEnergyWh=c.Capacity};batteryWh=c.Capacity;Eclipse=false;Contact=true;Powered=true;Log("Inicio de operación · "+c.Mission.name);}
 void Log(string s){Result.events.Add(MissionClock+" · "+s);}
 public void Step(){
 if(Result.elapsed>=Duration)return;var c=Result.config;double dt=Math.Min(StepSeconds,Duration-Result.elapsed),hours=dt/3600,mid=Result.elapsed+dt*.5;
 double phase=mid%c.Period/c.Period;bool shadow=c.Mission.eclipse>0&&phase>=1-c.Mission.eclipse;
 bool contact=mid%21600<480;ElectronicsAlive=mid/Duration<=Math.Min(1,new[]{5f,30f,300f}[c.choices[8]]/MissionCatalog.Radiation[c.mission]);
 if(!ElectronicsAlive&&!failed){Log("Pérdida de enlace: electrónica fuera de tolerancia de radiación");failed=true;}
 GeneratedW=c.choices[2]==2||!shadow?c.Generation:0;
 double generated=GeneratedW*hours,available=batteryWh+generated,energyBefore=batteryWh;
 double antennaPower=MissionCatalog.Options[5][c.choices[5]].power;
 double payloadPower=c.PayloadPower,ionPower=c.choices[4]==2?600:0;
 double basePower=c.Load-payloadPower-antennaPower-ionPower;
 bool maneuver=ionPower>0&&mid<3*86400;
 double requestedBase=(basePower+(maneuver?ionPower:0))*hours;
 double used=Math.Min(available,requestedBase);available-=used;Powered=ElectronicsAlive&&used>=requestedBase-1e-9;
 bool instrumentRequested=phase<c.duty&&(!shadow||c.choices[1]==2||c.mission==1);
 Observing=Powered&&instrumentRequested&&available>=payloadPower*hours;
 LastCapturedMB=0;LastSentMB=0;
 if(Observing){double payloadWh=payloadPower*hours;used+=payloadWh;available-=payloadWh;
 double rate=c.choices[1]==2?.005:c.choices[1]==1?.05:.02;if(c.secondInstrument&&c.choices[1]!=2)rate+=.005;
 LastCapturedMB=rate*dt;Result.captured+=LastCapturedMB;double accepted=Math.Min(LastCapturedMB,Math.Max(0,c.Storage-Stored));Stored+=accepted;Result.lost+=LastCapturedMB-accepted;Result.instrumentSeconds+=(float)dt;}
 if(Powered&&contact&&Stored>0&&c.Rate>0){
 double mbPerSecond=c.Rate/8.0,txSeconds=Math.Min(dt,Stored/mbPerSecond);
 if(antennaPower>0)txSeconds=Math.Min(txSeconds,available*3600/antennaPower);
 LastSentMB=Math.Min(Stored,Math.Max(0,txSeconds*mbPerSecond));double txWh=txSeconds*antennaPower/3600;used+=txWh;available-=txWh;
 Stored=Math.Max(0,Stored-LastSentMB);Result.sent+=LastSentMB;Result.useful+=LastSentMB*c.ScienceQuality;
 }
 double curtailed=Math.Max(0,available-c.Capacity);batteryWh=Math.Min(c.Capacity,Math.Max(0,available));
 Result.generatedWh+=generated;Result.consumedWh+=used;Result.curtailedWh+=curtailed;
 ConsumedW=(float)(used/hours);BatteryFlowW=(float)((batteryWh-energyBefore)/hours);
 if(!Powered)Result.blackoutSeconds+=(float)dt;Result.minEnergy=Math.Min(Result.minEnergy,Energy);Result.elapsed+=dt;
 Eclipse=shadow;Contact=contact;
 if(first||Eclipse!=oldEclipse)Log(Eclipse?"Eclipse: paneles sin iluminación":"Iluminación: generación solar disponible");
 if(first||Contact!=oldContact)Log(Contact?"Inicio de ventana terrestre":"Fin de ventana terrestre");
 bool low=Energy<c.Capacity*.2;if(low&&!oldLow)Log("Batería por debajo del 20 %");
 if(Powered!=oldPowered)Log(Powered?"Energía recuperada: equipos operativos":"Sin energía: equipos y enlace apagados");
 first=false;oldEclipse=Eclipse;oldContact=Contact;oldLow=low;oldPowered=Powered;
 if(Result.elapsed>=Duration){Result.missionLost=!c.CanLaunch||c.ScienceQuality<.99f||Result.useful<c.Mission.dataGoal||Result.blackoutSeconds>Duration*.01||!ElectronicsAlive;Result.failureReason=Result.missionLost?"Objetivo o recursos insuficientes · "+c.Review:"";}
 if((int)Result.elapsed%60==0||Result.elapsed>=Duration)Result.samples.Add(new Sample{time=Result.elapsed,energy=Energy,stored=(float)Stored,sent=(float)Result.sent,generationW=GeneratedW,consumptionW=ConsumedW,chargeW=BatteryFlowW,eclipse=Eclipse});
 }
 public void Complete(){while(Result.elapsed<Duration)Step();}
}}
