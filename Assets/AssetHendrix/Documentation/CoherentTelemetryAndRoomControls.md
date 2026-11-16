# Telemetría coherente, escritorio lateral y controles de sala

Escena: Assets/Scenes/Prototipo.unity. Nuevos recursos dentro de Assets/AssetHendrix.

El mismo reloj del motor gobierna la fase orbital, la iluminación/eclipses, la captura y las ventanas de enlace. La telemetría representa el último intervalo de cálculo de 30 segundos. La misión terrestre conserva 100 minutos por órbita y 36 minutos en eclipse. La rotación del planeta es decorativa; no sustituye a la fase orbital.

Se distingue generación en W, consumo servido en W, flujo de batería en W y energía almacenada en Wh. La carga se limita a la capacidad útil; el excedente se contabiliza como energía no almacenada. El consumo varía según subsistemas, instrumento activo, transmisión y maniobras iónicas. La tabla de diseño conserva el consumo nominal máximo; la operación muestra consumo instantáneo. El resultado distingue falta real de energía, captura y datos recibidos. Las alternativas se evalúan por el resultado de operación; no se fuerza un apagón si la energía realmente alcanza.

Solo se transmiten datos dentro de una ventana y con energía disponible. El tiempo de emisión se limita por memoria pendiente, tasa Mbps/8 y energía del equipo. Datos capturados = transmitidos + almacenados + perdidos. Se verifica también energía inicial + generada = consumida + batería + excedente descartado.

La velocidad inicial es 60 segundos simulados por segundo real. En la telemetría hay controles ×1, ×5, ×30 y Completar. La aceleración afecta al mismo reloj y a la órbita. Gráficas: energía de las últimas dos órbitas; datos de las últimas 12 horas. Se guardan muestras cada minuto simulado y se reduce el número de puntos de dibujo para evitar sobrecargar la interfaz.

Escritorio y monitores agrupados en ConsoleDeskGroup, al costado izquierdo de la sala. E aproxima la cámara a los dos monitores según su orientación actual.

Botón rojo circular y sin texto en la esquina superior derecha. En la consola se pulsa con el cursor normal; al recorrer la sala se mantiene ALT para liberar el cursor. Una pulsación separa los elementos de la habitación: mesa, teclas, monitores, piezas de brazos, suelo, paredes, estructura, aros luminosos y fuentes de luz. Cada objeto recibe su propia velocidad y giro sin gravedad. En la prueba final se movieron 165 elementos; el número aumenta si hay piezas de ensamblaje presentes. El jugador puede moverse con WASD, subir con Espacio y bajar con Ctrl.

Escape dentro de la consola vuelve a explorar. Escape durante el recorrido pausa el mundo, la misión, los brazos, partículas, fragmentos y sonido, y muestra Continuar / Reiniciar / Salir. Otra pulsación de Escape reanuda. Reiniciar vuelve a cargar Prototipo y restaura toda la sala; Salir cierra la aplicación en una compilación o termina Play en el editor.

Pruebas: Escape pulsado a través del sistema de entrada pausó y reanudó; tiempo de misión y posición de escombros permanecieron inmóviles durante pausa; Reiniciar eliminó los restos y devolvió estado inicial y escritorio lateral; Salir terminó Play. Una órbita terrestre produjo 36 minutos de eclipse; las seis configuraciones de referencia completaron la misión. En 30 días, error de conservación de energía menor de 1e-6 Wh y de datos menor de 1e-8 MB.
