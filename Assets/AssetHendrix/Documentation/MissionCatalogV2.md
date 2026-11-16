# Catálogo de misiones / versión 2

Fuente prioritaria: CARONTE Componentes y misiones(1).docx, cargado el 14 de noviembre de 2026. Sustituye el documento anterior.

11 categorías, 33 alternativas. La masa de lanzamiento suma piezas más propelente calculado con la ecuación de Tsiolkovski y el impulso específico del motor. La plataforma mediana admite 850 kg de nave completa. Lanzador y estación terrena suman costo, no masa de nave.

Referencias verificadas (masa calculada con redondeo, consumo en W, costo en cr): Tierra 100.67 / 70 / 42; telescopio 1532.69 / 355 / 263.5; Luna 691.37 / 225 / 138; Marte 810.68 / 250 / 151; Júpiter 1603.36 / 314 / 320.5; Sol 549.46 / 769 / 186.5.

La consola presenta misión, condiciones, categorías y tres tarjetas por categoría. Cada tarjeta muestra criterio, opción, efecto, masa y costo. En Júpiter se añade el segundo instrumento con un control independiente. Seleccionar misión conserva el diseño para comparar sus consecuencias.

Los modelos se encuentran en Prefabs/Catalog: 33 prefabs. Nueve categorías pertenecen a la nave; lanzador y estación terrena son infraestructura externa. CatalogVisuals controla los modelos a partir de identificadores. MissionDesignPanel controla la interfaz.

Ensamblaje: dos trabajadores en paralelo, con listas de piezas distintas y pinzas independientes. Cada pieza se recoge de una bandeja, se transporta, se orienta y se acopla con partículas. La configuración de Júpiter instala 39 piezas. El lanzamiento requiere que ambos trabajadores hayan concluido y que InstalledCount coincida con TotalParts.

Los valores de recursos proceden del documento y son didácticos. La simulación conserva 30 días de operación y contactos periódicos simplificados de 8 minutos cada 6 horas; no modela físicamente los meses/años de crucero. Las metas de MB son parámetros de demostración, no especificaciones NASA. La pérdida de calidad incluye instrumento, apuntamiento, temperatura, radiación y comunicación. Los umbrales de antena/estación reproducen las penalizaciones de referencia del documento.

Pruebas: seis combinaciones de referencia viables y con meta cumplida; conservación de datos con error menor de 1e-8 MB; sobredimensionar paneles de la misión terrestre bloquea lanzamiento; un solo instrumento de Júpiter reduce la ciencia al 50%; dos trabajadores concurrentes; 39 piezas completadas, 79 emisiones visuales y lanzamiento habilitado al terminar.
