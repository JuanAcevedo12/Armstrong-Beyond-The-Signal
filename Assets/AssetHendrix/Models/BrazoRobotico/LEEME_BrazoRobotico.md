# Brazo robótico industrial

Integrado como brazo izquierdo (índice 0) en Assets/Scenes/Prototipo.unity.
El brazo derecho original se conserva.

## Archivos editables
- BrazoRobotico.mat: material usado por la escena y por el prefab.
- IndustrialRobotPBR.shader: material PBR para URP con controles visibles y efectivos.
- Robot_MetallicSmoothness.png: mapa combinado, metallic en rojo y smoothness en alfa.
- BrazoRobotico_Animado.prefab: brazo con jerarquía articulada y punto de agarre.

## Material
Base Color usa la textura basecolor original. Normal Map usa la normal original, importada como mapa normal.
Metallic multiplica la textura metallic original: 0 elimina el carácter metálico; 1 conserva su intensidad original.
Smoothness multiplica el canal de suavidad obtenido como 1 menos roughness.
Valores iniciales: Metallic 0.8, Smoothness 0.65, Normal Strength 0.8.
Las imágenes JPG originales no se modificaron.
Se usa un shader PBR específico para mantener el control Metallic visible incluso con un mapa metálico asignado.

## Movimiento
El FBX contiene mallas separadas y no incluye huesos ni clips.
Se añadieron pivotes BaseYaw, ShoulderPivot, ElbowPivot, WristPivot y GripSocket.
IndustrialRobotArm resuelve la posición de la pinza con cinemática inversa de dos segmentos, giro de base y orientación de muñeca.
CaronteStation solo redirige el movimiento del brazo 0 al nuevo controlador; conserva el recorrido interpolado, la soldadura y los desfases entre ambos brazos.
Las piezas recogidas se vinculan al GripSocket real del modelo. Las mallas mantienen su escala al articularse.
La Pinza es una sola malla: se orienta con la muñeca; no tiene apertura independiente de los dedos.
Los cables se conservan como piezas rígidas unidas a sus segmentos.

## Verificación
Comprobados 57 recorridos con muestreo de posiciones intermedias.
En ejecución se completaron 29/29 piezas y hubo 3050 fotogramas con ambos trabajadores activos.
Desviación máxima observada del punto de agarre: aproximadamente 0.000003 m.
Material y shader compilados sin errores.
