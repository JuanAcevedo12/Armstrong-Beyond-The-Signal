# Prototipo — control de misión y señal decodificada

## Interacción y monitores
Orden físico: telemetría/arquitectura, diseño/control, video de a bordo al extremo derecho.
La mesa se desplazó y amplió para sostener los tres monitores sin salientes.
E conserva el encuadre de los monitores de telemetría y diseño. El video se consulta explorando la sala.
No se reproducen audios del comandante. AudioSource silenciado, referencias de clips vacías y Say sin reproducción.
El guion vigente está en GuionControlMision.md.

## Guías
Masa y presupuesto: máximo (≤). Potencia media, capacidad de batería y propelente: referencia mínima (≥).
Verde: cumple. Amarillo: desvío fuera de guía de hasta 5 %. Rojo: desvío mayor de 5 %.
El amarillo también representa incumplimiento; no concede tolerancia al presupuesto real.
Potencia mínima toma el mayor valor entre consumo de referencia y consumo del diseño.
Reserva mínima toma la mayor entre batería de referencia y consumo por duración del eclipse.
El propelente es una referencia del diseño recomendado; el modelo calcula la carga según motor, masa seca y delta-v.

## Órbita, video y decodificación
Sin satélite exterior al inicio ni línea orbital en el planeta grande.
El planeta se encuadra a través de la ventana y el satélite tiene una trayectoria exterior visible, con período visual aproximado de 20 segundos.
El video solo renderiza planeta y estrellas, nunca la sala.
Cada misión correcta debe permanecer en verificación orbital durante 18 segundos antes de decodificar.
El resultado se comprueba con el modelo de misión de 30 días. El botón Completar no salta el mínimo de 18 segundos.
Una misión correcta muestra una onda con ruido, MENSAJE DECODIFICADO y Mensaje 1–6.
El mensaje es provisional, editable posteriormente.
Tras éxito y narrativa se habilita CONTINUAR CON LA SIGUIENTE MISIÓN en el monitor de control.
Se espera la acción del jugador; se retiró el avance automático de 20 segundos.
La sexta misión cierra la campaña.

## Derrota
Se mantiene la posibilidad de ensamblar y lanzar diseños incorrectos.
MissionDiagnostics genera un informe con valores reales: plataforma, lanzador, masa, presupuesto, ciencia, actitud, térmica, energía, batería, radio y radiación.
La primera causa se anuncia en subtítulos. Se muestran hasta tres causas adicionales en la llamada; el monitor contiene el diagnóstico completo.
El informe también se guarda en MissionResult.failureReason.
Al fallar, el CanSat pierde señal y se fragmenta; todas las fuentes de luz activas de la escena pasan a rojo.
La habitación se destruye 20 segundos después del fallo orbital; los efectos y fragmentos continúan hasta reiniciar o salir.
Por vencimiento del lanzamiento, la destrucción sigue siendo inmediata.
Escape permite reiniciar o salir de una misión terminada.

## Verificación
Ensamblaje real de 29 piezas y lanzamiento completados.
No hay satélite exterior antes de la inserción. Comprobado desplazamiento orbital y encuadre por la ventana.
Sin mensaje decodificado a los 4.8 y 12.3 segundos; mensaje y botón disponibles después de completar la verificación.
Botón Continuar probado: pasa de misión 1 a misión 2.
Presupuesto de prueba: verde en 42/45, amarillo en 42/41 y rojo en 42/39; límites originales restaurados.
Prueba inválida: 759.6 kg / 150 kg, diagnóstico múltiple, luces rojas y destrucción posterior de 169 piezas de la sala.
Las seis configuraciones de referencia cumplieron la meta de datos en MissionEngine.
Onda de comunicaciones verificada con 636 vértices y captura visual.
Plazos de lanzamiento guardados: 90, 85, 80, 75, 70 y 60 segundos.
Los valores acelerados de prueba no se guardan.
El paquete MRUK existente puede emitir errores de contexto global duplicado al repetir Play en el editor.

## Corrección de distribución y movimiento
El diagnóstico de derrota se muestra únicamente bajo PÉRDIDA DE CONEXIÓN en el tercer monitor. Se eliminó el informe superpuesto del monitor de control.
Se desactivaron 121 colliders de la previsualización guardada y el controlador ignora su capa 8; las piezas nuevas del catálogo tampoco bloquean al personaje. El pasillo izquierdo se recorrió de z=-2 a z=2.5 sin colisiones laterales.
La animación orbital avanza por fotograma, independiente de los pasos de 30 segundos del modelo. La previsualización central y la representación exterior comparten esa fase visual. Muestreo: 60 fotogramas con avance en todos ellos y 18 grados por segundo.
Planeta exterior: diámetro visual de 14 a 140 unidades (×10), situado en (-200,35,40). Órbita exterior: radio 95, período 20 segundos (velocidad angular ×5). Plano de recorte de la cámara del jugador ampliado a 2000.
Probado el fallo con presupuesto 48.5/45: diagnóstico bajo pérdida de conexión, sin texto superpuesto.

Cámara de a bordo con zoom óptico 2× (campo vertical 37.2738° frente a 68°). Se retiró la creación del botón rojo de emergencia de la esquina superior derecha.

## Cielo y destinos
Sky_2 se conserva. Se añadió un campo de 2600 estrellas en una sola malla, sin colliders, más seis trazos reutilizables con aparición aleatoria y fundido. Las estrellas antiguas cercanas se desactivan para no quedar delante de los planetas. Las cámaras de video alcanzan 2000 unidades para ver el campo profundo.
Destino por misión: Tierra; Tierra como referencia de L2; Luna; Marte; Júpiter con Europa; Sol. El material cambia en el cuerpo exterior y en la representación del monitor; el video indica el destino. Europa solo aparece en la quinta misión.
Texturas, materiales, malla, scripts y capturas se guardan en AssetHendrix. Verificados los seis cambios de material, Europa y su desactivación, el cambio real a Luna/Marte/Júpiter y el funcionamiento de los trazos en ejecución.

## Actualización de celda V2 y audio
Se sustituyen ambos brazos por la versión V2 con pinzas articuladas y se añade un robot láser trasero. Audios del usuario activados y vinculados. La derrota orbital espera 40 segundos antes de destruir la habitación. Ver CeldaV2_Audios.md para el mapa de audios, temporización por bloques y pruebas.
