# Celda de ensamblaje V2 y audio

## Robots y distribución
La escena Prototipo usa dos instancias articuladas del modelo BrazoRobot2, con apertura de las piezas Mano_2/Mano_3 y Mano_5/Mano_4.
IndustrialRobotArm conserva la solución IK; los dedos abren antes de recoger, cierran durante el agarre y abren al depositar.
Los robots anteriores quedan desactivados en la escena.
El robot láser se encuentra detrás, entre los dos brazos. Su cabeza apunta a la pieza ya montada, emite un haz rojo y genera el efecto de soldadura en el punto de impacto.
Las entregas simultáneas se encolan; el ensamblaje no se considera terminado hasta que finaliza la última soldadura.
La sala se amplió hasta z=15; se redujeron la mesa central y la profundidad de las mesas laterales, separadas de las bases.
Materiales editables: BrazoRobot2/BrazoRobot2.mat y Robot láser/RobotLaser.mat. Conservan Metallic, Smoothness y Normal Strength del shader PBR.
Los prefabs de ambos brazos están en BrazoRobot2.

## Audios
Se referencian los 20 MP3 originales de Assets/AssetsMaximo/Audios; no se movieron ni modificaron.
MissionControlDirector.tutorialRecordings contiene las seis partes de misión 1.
missionRecordings[1..5] contiene MISION 2 a MISION 6.
voiceClips: 24 lanzamiento; 25 inserción orbital; 26 éxito de misiones intermedias; 27 derrota GENERAL; 28 fin de campaña; 48 fin de tutorial; 49 mensaje decodificado; 50 vencimiento del plazo; 51 alerta de la estación.
La derrota general se reproduce una sola vez por fallo, aunque haya varios motivos. Los motivos particulares se muestran en subtítulos y en el monitor, sin repetir la voz general.
El lanzamiento espera el final de su audio antes de pasar a la inserción orbital.
Al lanzar durante el tutorial se interrumpe la explicación y se reproduce el audio de lanzamiento.
La pausa detiene la voz mediante AudioListener.pause. El tiempo acelerado de prueba ajusta la velocidad del AudioSource, hasta un máximo de 3.
Los subtítulos de los archivos largos se distribuyen por bloques, con duración proporcional a la longitud de las frases. RecordedTutorial define la agrupación inicial de los seis archivos (3, 3, 4, 5, 4 y 1 párrafos); estos cortes son ajustables para afinar la coincidencia con las grabaciones.
Tras una derrota orbital hay 40 segundos de alerta antes de destruir la sala (20 segundos adicionales). Por vencimiento del plazo se esperan 20 segundos en vez de destruirla de inmediato.

## Pruebas
Alcance de los dos brazos comprobado en las seis configuraciones de referencia.
Ensamblaje real: 29 piezas, 29 soldaduras, 19 cierres de la pinza izquierda y 10 de la derecha, con 1913 fotogramas de trabajo simultáneo.
Apertura medida entre las puntas: aproximadamente 0.571 m abiertas y 0.231 m cerradas.
Error máximo de contacto observado en el ensamblaje: menor de 0.000006 m.
Reproducción comprobada de los bloques del tutorial, interrupción al lanzar, inserción, mensaje decodificado, fin del tutorial y comienzo de MISION 2.
Derrota con varios motivos: voz general reproducida una vez; sala intacta a los 20 s y explosión a los 40.00 s.
Se comprobó visualmente el haz rojo desde la cabeza del robot trasero hasta el CanSat.
