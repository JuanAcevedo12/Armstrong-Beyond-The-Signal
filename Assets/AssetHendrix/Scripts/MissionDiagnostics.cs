using System;using System.Collections.Generic;
namespace Caronte{
public static class MissionDiagnostics{
 public static List<string> Causes(MissionResult r){var c=r.config;var a=new List<string>();
 if(c.LauncherCapacity<=0)a.Add("Lanzador incompatible: el vehículo elegido no alcanza este destino.");
 if(c.Mass>c.MaxMass)a.Add("Masa excesiva: "+c.Mass.ToString("0.0")+" kg frente a "+c.MaxMass.ToString("0")+" kg admitidos por la plataforma.");
 if(c.LauncherCapacity>0&&c.Mass>c.LauncherCapacity)a.Add("Lanzador sobrecargado: "+c.Mass.ToString("0.0")+" kg frente a "+c.LauncherCapacity.ToString("0")+" kg de capacidad para este destino.");
 if(c.Cost>c.Mission.budget)a.Add("Presupuesto excedido: "+c.Cost.ToString("0.0")+" créditos frente al límite de "+c.Mission.budget.ToString("0")+".");
 if(c.choices[1]!=c.Mission.required[1]||(c.mission==4&&!c.secondInstrument))a.Add(c.mission==4?"Carga científica incompleta: Júpiter y Europa requieren cámara y sensor de campos y partículas.":"Instrumento inadecuado: esta misión requiere "+MissionCatalog.Options[1][c.Mission.required[1]].name+".");
 if(c.ScienceQuality<.99f&&c.choices[6]<c.Mission.required[6])a.Add("Apuntamiento insuficiente: el instrumento o la antena necesitan un control de actitud más preciso.");
 if(c.mission!=0&&c.choices[7]!=c.Mission.required[7])a.Add("Control térmico inadecuado: esta misión requiere "+MissionCatalog.Options[7][c.Mission.required[7]].name+".");
 if((c.mission==2||c.mission==3||c.mission==4)&&c.choices[4]==2)a.Add("Inserción fallida: el motor iónico no proporciona el frenado requerido en este escenario.");
 if(c.mission==1&&c.choices[2]==2)a.Add("Contaminación térmica: el RTG impide mantener fría la óptica infrarroja.");
 if(c.mission==5&&c.choices[2]==1)a.Add("Paneles desplegables incompatibles con el entorno térmico de la aproximación solar.");
 if(c.AverageGeneration<c.Load&&r.blackoutSeconds>0)a.Add("Déficit de energía: genera en promedio "+c.AverageGeneration.ToString("0")+" W frente a "+c.Load.ToString("0")+" W de consumo de referencia.");
 if(r.blackoutSeconds>0)a.Add("Batería agotada: "+Math.Ceiling(r.blackoutSeconds/60)+" minutos sin alimentación durante la simulación; reserva instalada "+c.Capacity.ToString("0")+" Wh.");
 if(c.LinkQuality<.9f)a.Add("Enlace insuficiente: la combinación de antena y estación terrena no sostiene las comunicaciones a esta distancia.");
 if(new[]{5f,30f,300f}[c.choices[8]]<MissionCatalog.Radiation[c.mission])a.Add("Electrónica dañada por radiación: tolerancia de "+new[]{5f,30f,300f}[c.choices[8]]+" krad frente a "+MissionCatalog.Radiation[c.mission]+" krad de la misión.");
 if(r.useful<c.Mission.dataGoal)a.Add("Objetivo científico incompleto: se recuperaron "+r.useful.ToString("0")+" de "+c.Mission.dataGoal.ToString("0")+" MB útiles.");
 if(a.Count==0)a.Add("La configuración o el resultado no corresponden a la misión confirmada por Control.");
 return a;}
 public static string Explain(MissionResult r){return string.Join("\n",Causes(r).ToArray());}
 public static string FirstCause(MissionResult r){return Causes(r)[0];}
}
}