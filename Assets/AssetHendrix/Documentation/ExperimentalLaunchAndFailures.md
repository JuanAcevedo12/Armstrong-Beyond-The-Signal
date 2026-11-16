# Lanzamiento experimental y fallos visuales

Se puede ensamblar y lanzar aunque existan restricciones de diseño; se mantiene el ensamblaje completo como condición de salida. El botón indica advertencias. Al lanzar, la consola se cierra y el jugador recupera la vista de primera persona.

La operación distingue contacto programado y pérdida de enlace. Energía insuficiente, radiación, instrumento inadecuado, fallo térmico o enlace débil producen estados visibles. La representación orbital se anima a una vuelta cada 150 s en el monitor y 180 s junto al planeta; los cálculos de misión mantienen su reloj simulado.

Al terminar una misión con riesgo operativo se genera el desenlace visual didáctico: explosión y separación del satélite en fragmentos con traslación y giro sin gravedad. Esto es una consecuencia del juego; no es un modelo físico de explosiones. La sala y los monitores se conservan. El resultado registra missionLost y failureReason y una misión perdida recibe cero puntos. Un diseño viable completa la misión sin destrucción.

OrbitalFailureController sincroniza alertas, resultado y efectos. OrbitalFragment anima los escombros. LiveTelemetryPanel presenta curvas de batería y datos reales del motor, además de barras de batería y ciencia útil. Las gráficas se mantienen al terminar y se limpian al rediseñar.

Los brazos tienen una salida inicial de 0.12 s / 0.53 s, pausas distintas, velocidades distintas, codos asimétricos y trayectorias con puntos intermedios diferentes. Las listas de piezas siguen siendo independientes y el ensamblaje es simultáneo.

Las partículas de acoplamiento usan gravedad 2.8, colisión 3D con el entorno, rebote 0.22, amortiguación 0.65 y pérdida de vida al tocar una superficie. La vida es menor de 1.2 s. La explosión orbital usa partículas sin gravedad.

Cada categoría y cada opción dispone de un icono vectorial de referencia. Los puntos inferiores distinguen las tres variantes.

Validación: lanzamiento de prueba aceptado con presupuesto excedido; cierre automático de consola; 28 piezas ensambladas con trabajadores concurrentes; apagones y alerta de pérdida de conexión; desenlace con 76 fragmentos entre ambas representaciones orbitales; dos curvas visibles con muestras del motor; misión nominal sin destrucción y resultado de 100 puntos.
