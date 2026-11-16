# CARONTE - Estación de ensamblaje

Escena: Assets/Scenes/Prototipo.unity. Todos los recursos nuevos están en Assets/AssetHendrix.

WASD: desplazamiento con colisiones. Mouse: vista en primera persona. Shift: caminar más rápido. E cerca del escritorio: activar consola, enfocar monitor derecho y liberar cursor. Esc: salir de consola y recuperar cámara. Clic derecho recupera la captura del mouse si el editor la libera.

Monitor derecho: único panel interactivo. Monitor izquierdo: imagen 3D y barras de recursos, sin raycaster ni selección sobre objetos de la sala.

Flujo: configurar -> ensamblar -> observar brazos recoger seis módulos de bandejas -> regresar al panel -> lanzar. Cambiar configuración invalida el ensamblaje. No se puede modificar el diseño durante ensamblaje. El lanzamiento está bloqueado por estado lógico y por botón hasta terminar los seis módulos.

El planeta y la sala son geometrías provisionales con materiales sencillos. La textura terrestre es procedural, no un mapa científico. Los brazos usan trayectorias e interpolación visual, no dinámica industrial. La previsualización y simulación anterior se conservan en una cámara aislada con RenderTexture; el satélite físico se construye con una copia de la configuración.

Pruebas: consola con raycast de pantalla; batería de 120 Wh seleccionada; lanzamiento rechazado antes y durante ensamblaje; seis módulos completados; lanzamiento aceptado después (etapa 2); consola desactivada para exploración.
