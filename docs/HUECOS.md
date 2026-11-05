# Huecos del diseño y sistemas por construir

Revisión del 5 de noviembre de 2026. Cubre el GDD completo, las referencias visuales, el documento de retratos y la Fase 1, que se programa en paralelo. Responde dos preguntas: **qué está mal o falta en el diseño** y **qué hay que construir, además de las tres fases, para tener un producto que se pueda lanzar**. Cómo se integra cada cosa y en qué orden está en [HOJA_DE_RUTA.md](HOJA_DE_RUTA.md).

Cómo leerlo:

- Cada hueco tiene un ID (`C-` para contradicciones, `H-` para huecos de diseño, `S-` para sistemas que faltan) que puedes citar en un prompt.
- **Propuesta** es mi recomendación por defecto. Si no dices otra cosa, los agentes la usan y la marcan `// TUNING`.
- **Fase** es la primera fase que necesita la respuesta. Lo que bloquea una fase próxima va primero.

## 1. Estado del proyecto

| Pieza | Estado | Dónde |
| --- | --- | --- |
| GDD | Completo como concepto (25 secciones, 1 430 líneas). Tiene huecos de números y de contenido (secciones 3 a 5 de este documento) | `docs/GDD.md` |
| Referencias visuales y paleta | Completas, con contraste y daltonismo medidos | `docs/REFERENCIAS.md`, `docs/paleta.svg` |
| Retratos 2D | Primera ola en curso con Codex: 9 de 37 generaciones; ancla de estilo elegida (Marisal v3); Cervanda falló dos veces por cola de cánido | `docs/Retratos para Codex.md`, `art/retratos/` |
| Fase 1: combate | En curso en otra sesión, rama `claude/youthful-dijkstra-hos1on`, todavía sin subir. El núcleo compila, los datos de 6 brujas, enemigos y jefe están escritos, y está afinando al jefe con un simulador de balance | Se integrará en `Assets/SalYAlmas/` |
| Fases 2 y 3 | Prompts listos en el GDD 23. La 3 no cabe en una sesión (ver la hoja de ruta) | `docs/GDD.md` §23 |
| Proyecto de Unity | Todavía no existe en `main`; lo crea la Fase 1 | — |
| Guía para agentes y CI | Agregadas en esta revisión | `CLAUDE.md`, `.github/workflows/ci.yml` |
| Propuestas de contenido | Agregadas en esta revisión: Fresno, crafting, meta-juego y onboarding | `docs/propuestas/` |
| Audio y banda sonora | Agregados en esta revisión: plan de integración y prompts de Suno | `docs/audio/` |

## 2. Lo que comprobé con números

**El gacha cuadra.** Con 0.6 % de base, pity suave desde la tirada 60 con +7 % por tirada y pity duro en 70, una 5★ sale en **52.9 tiradas** de promedio (tasa consolidada de 1.89 %) y la destacada en **66.1**. El GDD dice 53 y 66. El peor caso de la destacada es 140, correcto.

- Hay una ambigüedad que mueve el promedio: si la tirada 60 ya lleva el +7 % da 52.9; si el primer +7 % llega en la 61 da 53.6. Las dos caen dentro del ±1 que pide la Fase 2. **Propuesta:** la tirada 60 ya tiene 7.6 % (0.6 + 7). Escríbelo así en el GDD 16.2.
- Con las 70 tiradas gratis por versión (16.3), empezando sin pity, **78 %** de los jugadores consigue la destacada. La meta de la 22.3 es 50 % o más: se cumple con margen. Con 60 tiradas la probabilidad es apenas 28 %, porque casi todo el peso está en el pity; si el ingreso gratis baja, la promesa se cae rápido.

**La economía de almitas no cuadra entre sí.**

| Concepto | Almitas |
| --- | --- |
| Una bruja del nivel 1 al 70 (`100·n^1.5`) | 1 610 000 |
| Una bruja del nivel 1 al 80 | 2 254 000 |
| Todos los grados de D a SS (`20 000·1.8^k`) | 447 000 |

Los grados, que el GDD presenta como la progresión cara y principal, cuestan cinco veces menos que subir de nivel. Y la meta "nivel 70 en siete días sin pagar" (11.3) implica unas **230 000 almitas al día** para una sola bruja, pero el GDD no tiene ninguna tabla de ingreso de almitas. Ver H-01.

## 3. Contradicciones y erratas del GDD

Las tres primeras ya están corregidas en el GDD en esta revisión.

| ID | Dónde | Problema | Propuesta |
| --- | --- | --- | --- |
| C-01 | 4.4 y 7.4 | La tabla de decisiones decía cuatro bandas de Salinidad; la 7.4 tiene cinco | **Corregido:** cinco bandas |
| C-02 | 9.5 y 12.3 | Dos bloques "embedded content" rotos de la exportación | **Corregido:** diagrama de la Cadena y tabla de costos de grado |
| C-03 | Encabezado | "AñDocumento" | **Corregido** |
| C-04 | 2 y 22.3 | "Primera 5★ garantizada antes del día 2" contra el KPI "primera 5★ antes de 2 horas" | Meta de 2 horas; el día 2 queda como peor caso garantizado |
| C-05 | 9.7 y 23.1 | La Ascensión III pide grado S, pero la Fase 1 usa brujas en grado B y pide probar la III | En la Fase 1, probarla con una bandera de depuración o una bruja de prueba en S |
| C-06 | 12.3 | "Juicio I: Ascendente nivel 40": el Ascendente no tiene nivel numérico, sube por Dignidad (5 niveles) | Es el nivel mínimo de la bruja; el Ascendente del Juicio pelea a ese nivel |
| C-07 | 7.4 | −5 de Salinidad por básico y −10 por turno sin magia: ¿un básico da −15? | Sí: el básico es un turno sin magia. Total −15 |
| C-08 | 7.4 | El estado "Desalada" no está definido | Un turno sin poder lanzar Hechizo ni ganar Salinidad; el básico sí se puede |
| C-09 | 7.4 y 8.2 | ¿Hilos sube +25 de Salinidad? ¿Frenesí suma +40 y además +25? ¿Frenesí gasta una Sal Negra del inventario? | Un solo costo por forma: Aguja y Sello +25 y 2 Granos; Hilos +25 y 10 % de vida; Frenesí +40 y una Sal Negra |
| C-10 | 9.3 y 8.2 | El Autohechizo "no termina el turno", pero Hilos "actúa de nuevo de inmediato" | Hilos da un turno extra completo, sin otro Autohechizo dentro |
| C-11 | 7.3 | "Ascensiones seguidas en la misma ronda": el sistema por AV no define "ronda" | Ronda = ciclo de AV (150 el primero, 100 los siguientes) |
| C-12 | 9.6 | "Spark cuando dos Ascensiones se lanzan al mismo tiempo": en turnos no hay "al mismo tiempo" | Dos Ascensiones lanzadas en la misma ventana de interrupción se reproducen encimadas desde el frame 0 |
| C-13 | 7.2 y 7.3 | Cada bruja tiene dos planetas: ¿cuál forma el Aspecto? La secta de Mercurio es "Variable" | El dominante. La secta de Mercurio nunca decide: Mercurio siempre forma Conjunción, Oposición o Sextil |
| C-14 | 9.8 | No hay probabilidad ni daño crítico base, pero la banda Salobre suma +10 % de crítico | 5 % de probabilidad y +50 % de daño crítico de base |
| C-15 | 9.4 | El Quiebre no dice cuánto dura, cuándo se recupera la Firmeza ni cuánto dañan quemadura, sangrado, veneno y electrocución | La Firmeza se recupera en el siguiente turno del enemigo; daños por turno en % de ATQ durante 2 turnos |
| C-16 | 9.5 | ¿La Resonancia vuelve a 0 al abrir la Cadena? ¿Suman las acciones enemigas o el Spark fuera de Cadena? | Vuelve a 0; solo suman acciones aliadas; el Spark de Ascensiones también suma |
| C-17 | 9.1 y 9.5 | La Invitada cierra una Cadena, pero no se dice si tiene vida, Salinidad o Ánima | No está en el campo: solo Regencia y un Cierre por combate; sin vida, Salinidad ni Ánima |
| C-18 | 9.3 | La Ascensión "se lanza en cualquier momento": ¿también en el turno enemigo o dentro de una Cadena? | Entre dos acciones cualesquiera, nunca dentro de una Cadena |
| C-19 | 9.7 | El Ánima sube "al recibir daño", sin cantidad | +10 por golpe recibido, con tope de +30 por turno enemigo |
| C-20 | 7.3 | Trígono cura 5 %: ¿a quién? | A las dos brujas del Aspecto |
| C-21 | 14.2 y 23.1 | La Fase 1 pide las 11 Salmueras en el cinturón, pero el Agua de Olvido es "fuera de combate" y baja el Paralaje, que no existe en la Fase 1 | En la Fase 1 queda en datos, sin usarse en combate |
| C-22 | 14.1 y 11.1 | Las bases y los reactivos (agua termal, vinagre, leche lunar, flor de sal) no están en la tabla de recursos. El Destilado de Rocío usa Savia, que es moneda de cuenta | Agregar "Bases y reactivos" a la 11.1; cambiar la Savia del Destilado por "rocío del Árbol", un reactivo del Fresno |
| C-23 | 12.2 y 16.2 | Exclusión de duplicados con el 75/25: si pierdes y ya tienes todas las 5★ estándar, ¿sale Polvo Astral y cuenta como pérdida? ¿Qué pasa si ya tienes a la destacada? | El 25 % solo existe si queda alguna 5★ estándar que no tengas; si no, la 5★ es la destacada. Si ya tienes a la destacada, el banner lo avisa antes de tirar y la 5★ sale del estándar |
| C-24 | 16.2 | ¿El banner de inicio comparte contador? ¿La garantía de destacada pasa entre banners limitados? | El de inicio tiene contador propio; la garantía pasa entre limitados, como el contador |
| C-25 | 2 y 16.6 | Bélgica: "banners solo con moneda ganada jugando", pero las Sales de Baño se ganan y también se compran (Perlas 1:1) | El libro contable distingue Sales de Baño ganadas y compradas desde la Fase 2 |
| C-26 | 18 y retratos | El GDD no nombraba a las 4★ y 3★; el documento de retratos les da nombre y Ascendente. La Fase 1 usa "Zarigüeya" y "Topo" | **Decidido y aplicado en el GDD 17, 18 y 20:** Brea, Cempa, Muertita, Salitre, Topacia y Axa, con IDs canónicos. La Fase 1.5 renombra los IDs de datos |
| C-27 | 7.7 | "+15 % de daño a las brujas de ese planeta" en su día: ¿dominante o cualquiera de los dos? | Solo el dominante |
| C-28 | 21.2 y 23.3 | El backend puede ser UGS, Nakama o PlayFab, pero el prompt de la Fase 3 solo nombra UGS | Decidir antes de la Fase 3 (D-04 en la hoja de ruta) |
| C-29 | 19 y 6 | "Combate, Termas y exploración son 3D", pero no hay diseño de exploración | Ver H-02 |

## 4. Huecos de diseño

Son reglas o contenido que el GDD nombra pero no define. Sin ellos, el agente de cada fase inventa en el código.

### Bloquean la Fase 2

| ID | Hueco | Propuesta |
| --- | --- | --- |
| H-01 | **Ingreso de almitas.** No hay tabla de cuánto da cada actividad, y las escalas de nivel y grado no cuadran (sección 2) | Tabla de fuentes por actividad y día, y reescalar: o el nivel cuesta menos (`40·n^1.5`, 640 000 al 70) o los grados cuestan más (base 80 000). Ver `docs/propuestas/CRAFTING.md`, sección de economía |
| H-02 | Contenido del Fresno: 105 Injertos (solo hay un Fruto de ejemplo), Mutaciones, maldiciones, Tránsitos, Raíces, enemigos por zona | `docs/propuestas/FRESNO.md` |
| H-03 | Estado que persiste entre nodos de una run (vida, Salinidad, Ánima, Resonancia, Granos, pociones) | `docs/propuestas/FRESNO.md` |
| H-04 | Maldiciones: ¿×1.25 multiplicativo (×3.05 con cinco) o aditivo (+125 %)? | `docs/propuestas/FRESNO.md` |
| H-05 | Frames de golpe de las 12 familias de armas. Deciden el Spark y el GDD no da ninguno | `docs/propuestas/CRAFTING.md` |
| H-06 | **Qué arma puede usar cada bruja** y cómo cambia sus movimientos (¿reemplaza frames o suma golpes?) | `docs/propuestas/CRAFTING.md` |
| H-07 | 120 armas, 8 legendarias, 25 prefijos, estaciones regionales y matriz de Salmueras en tres potencias | `docs/propuestas/CRAFTING.md` |
| H-08 | Condiciones de los Juicios por bruja y grado (96 condiciones para 24 brujas) | Plantilla de 12 condiciones por papel y una propia por Regente. Tarea de contenido de la Fase 6 |
| H-09 | Dignidad del Ascendente: qué da cada uno de los 5 niveles y cuánto cuesta | +3 % de daño del Ascendente por nivel y un efecto en Domicilio |
| H-10 | Talentos de Cadena del 1 al 10: costo y efecto por nivel | +4 % al movimiento por nivel; materiales regionales desde el 6 |
| H-11 | Sal refinada por grado y costo de refinar en el Altar | Destilada para C a A; Quintaesencia para S y SS |
| H-12 | Vía Mayor y Vía Menor: qué pasiva cambia en cada bruja | Una línea por bruja en su ficha de kit |

### Bloquean la Fase 3

| ID | Hueco | Propuesta |
| --- | --- | --- |
| H-13 | **Exploración de regiones.** El GDD dice que es 3D, pero no dice cómo se juega una región. Es la decisión de producción más cara | Para el lanzamiento: mapa ilustrado de la región con puntos de interés, escenas con retratos 2D y combates; solo las Termas son 3D navegables. Los dioramas 3D se suman después |
| H-14 | Guion: arco de la Calculista, nombres de las Siete Sabias, las 12 Casas con su Ascendente corrupto, las misiones de Aries y Capricornio, los tres finales y el secreto | Biblia narrativa en una sesión de escritura antes de la Fase 3B |
| H-15 | Paralaje en números: cuánto sube cada fuente, umbrales de las cuatro bandas, cuánto baja un baño | Medidor de 0 a 100; bandas en 25, 50 y 80; un baño baja 20 y tiene enfriamiento de un día |
| H-16 | Reloj de eclipses: qué cambia en datos cuando una región "se corrompe un poco más" | Tres estados por región con variantes de eventos, enemigos y un final local |
| H-17 | Senderos de la Calculista: cómo sube cada uno de nivel y qué da cada nivel | 10 niveles por Sendero con experiencia por combate |
| H-18 | Creador de la Calculista: especies, cuerpos, voces, pronombres | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-19 | Interacciones en las Termas (reposo, tocar, conversar): qué hacen, si hay afinidad y qué da | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-20 | Catálogo y precios de cosméticos, pase *Grimorio* y tarjeta *Diezmo de Sal* | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-21 | Audio: lista de cues, capas, efectos, voces y cómo se integra | `docs/audio/INTEGRACION_AUDIO.md` y `docs/audio/SOUNDTRACK_SUNO.md` |

### Bloquean el producto, no una fase concreta

| ID | Hueco | Propuesta |
| --- | --- | --- |
| H-22 | **Primera sesión (FTUE)** minuto a minuto: el GDD pide primera Cadena antes del minuto 3 y primera 5★ antes de 2 horas, sin guion | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-23 | De dónde salen las 3 200 Sales de Baño del banner de inicio: el 16.3 solo cubre el ingreso por versión | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-24 | Misiones diarias y semanales, logros, nivel de cuenta y desbloqueo progresivo de cada sistema | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-25 | Mazmorra del día planetario y fase lunar real (7.7): formato, recompensas, hora de reinicio y zona horaria | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-26 | Invitada "de un amigo": no hay sistema de amigos | `docs/propuestas/META_Y_ONBOARDING.md` |
| H-27 | Kits de las 18 brujas que no entran en la Fase 1 y sus Ascendentes | Plantilla de kit más el simulador de balance; tarea de contenido de la Fase 6 |
| H-28 | Bestiario de las 12 regiones, Huéspedes y Sin Carta | Tarea de contenido de la Fase 6 |
| H-29 | Compartir la transformación (12.5): formato, marca de agua, redes | Clip de 10 s con marca de agua y la hoja de sistema compartible |
| H-30 | Reglas de evento de versión: duración, tienda de evento, moneda de evento | `docs/propuestas/META_Y_ONBOARDING.md` |

## 5. Sistemas que faltan para un producto final

Ninguno de estos está en los tres prompts de la sección 23, y casi todos son obligatorios para publicar en tiendas. La hoja de ruta los reparte en las Fases 3 a 6.

### Cuenta, guardado y servidor

| ID | Sistema | Por qué hace falta |
| --- | --- | --- |
| S-01 | Cuenta: inicio anónimo, vincular con Apple y Google, transferir a otro teléfono | Sin cuenta no hay guardado en la nube ni compras restaurables |
| S-02 | **Borrar la cuenta desde el juego** | Obligatorio en App Store desde 2022 para apps que crean cuentas |
| S-03 | Guardado en la nube con resolución de conflictos y migración de versiones del guardado | El guardado local de la Fase 2 no sobrevive a cambiar de teléfono; cada versión cambia el esquema |
| S-04 | **Validación de runs en el servidor** repitiendo las entradas con el núcleo determinista | Las recompensas del Fresno y los Juicios valen dinero indirectamente; sin validar, se falsifican. Es la razón de que el núcleo sea determinista |
| S-05 | Reloj del servidor: reinicio diario, semana, día planetario, fase lunar | Si el reloj es el del teléfono, cambiar la hora regala recompensas |
| S-06 | Puerta de versión mínima y actualización de datos sin pasar por la tienda | Para balancear y lanzar eventos sin esperar revisión de Apple y Google |
| S-07 | Herramientas internas: buscar un jugador, ver su historial, enviar correo de compensación, revertir una compra reembolsada | Soporte al cliente desde el primer día del lanzamiento suave |

### Monetización y cumplimiento

| ID | Sistema | Por qué hace falta |
| --- | --- | --- |
| S-08 | Compras reales: recibos validados en servidor, reembolsos que retiran Perlas, precios por país | La Fase 3 solo pide modo de pruebas |
| S-09 | Separar Sales de Baño ganadas y compradas en el libro contable | Versión de Bélgica (C-25) y contabilidad de moneda prepagada |
| S-10 | Página de probabilidades por banner e historial de un año | 16.6; Corea, Apple y Google lo exigen |
| S-11 | Límite de gasto y recordatorios | 16.6 |
| S-12 | Privacidad y términos: consentimiento (GDPR, LGPD de Brasil, ley mexicana de datos personales, CCPA), aviso de privacidad, edad mínima | Requisito de las tiendas y de los mercados del lanzamiento suave |
| S-13 | Revisión legal de la moneda prepagada en Japón (Perlas) | La ley de pagos japonesa impone obligaciones a los saldos prepagados grandes; afecta si las Perlas caducan |
| S-14 | Clasificación por edad (IARC, ESRB, PEGI, CERO) con el modo recatado | 17+ en App Store y su equivalente en cada sistema |
| S-15 | Búsqueda de marcas del título y de los nombres (sección 24) | Antes de anunciar |

### Experiencia de producto

| ID | Sistema | Por qué hace falta |
| --- | --- | --- |
| S-16 | Tutorial y onboarding guionizados | KPI de primera Cadena antes del minuto 3 |
| S-17 | Ajustes: gráficos y fps, ahorro de batería, volúmenes, idioma de texto y de voz por separado, modo recatado, notificaciones, cuenta, soporte, legal | Requisito básico; el modo recatado es parte del cumplimiento |
| S-18 | **Reducir destellos** y prueba de fotosensibilidad del Spark, el destello cromático y la Ascensión III | Riesgo de crisis fotosensibles; las tiendas y la prensa lo revisan |
| S-19 | Notificaciones push con opción de apagarlas | Retención; iOS pide permiso explícito |
| S-20 | Soporte dentro del juego (formulario o servicio de tickets) | Lanzamiento suave |
| S-21 | Captura de fallos y monitoreo de rendimiento en producción | 21.4 pide calidad, pero nada mide fallos reales |
| S-22 | Pipeline de localización: tablas con contexto para traducir, seudolocalización, longitud de texto, fuentes CJK y la fuente de glifos del Salmódico | La Fase 3 pide tres idiomas sin pipeline |

### Producción y herramientas

| ID | Sistema | Por qué hace falta |
| --- | --- | --- |
| S-23 | **Herramientas de contenido:** editor de brujas con vista previa de la Partitura, validador de datos (IDs, frames, rangos) e importador desde hoja de cálculo | Hay que producir 24 kits, 120 armas, 105 Injertos y 12 regiones. A mano en JSON no escala |
| S-24 | Simulador de balance en la CI con la regla del +5 % (17.1) | 21.4 lo pide y la Fase 1 ya tiene un simulador; falta convertirlo en una prueba |
| S-25 | Combates grabados como pruebas doradas, corridos en .NET y en un teléfono con IL2CPP | El servidor (x64) y el teléfono (ARM64) pueden diferir en `double`; sin esta prueba, la validación de S-04 rechaza jugadores honestos |
| S-26 | Compilaciones nocturnas de Android e iOS y subida a pruebas internas (TestFlight, Play) | 21.4. iOS necesita un runner de macOS y certificados de Apple |
| S-27 | Distribución de contenido: Addressables con CDN, Play Asset Delivery, catálogo remoto | Descarga inicial de 2 GB como máximo (21.1) |
| S-28 | Integración del arte: importación de retratos por ID, atlas, decisión de Spine o Live2D (con su licencia), rig base compartido, morphs de fase | Los retratos de Codex no tienen camino a Unity |
| S-29 | Almacenamiento de archivos grandes: Git LFS o almacenamiento externo | Hoy hay 29 MB de PNG en Git; el juego completo tendrá varios GB. **Decidido por ahora: sin LFS.** Revisar antes de que entren modelos 3D o audio |
| S-30 | Protocolo de prueba con jugadores: guion, observación y encuesta | Los criterios de aceptación de las tres fases dependen de pruebas con jugadores |
| S-31 | Comunidad y marketing: preinscripción, Discord, adelantos en redes (22.4) | Antes del lanzamiento suave |

## 6. Riesgos técnicos que conviene atacar pronto

1. **Determinismo entre plataformas (S-25).** El núcleo de la Fase 1 calcula en `double`. En una misma máquina eso es determinista, pero el compilador de IL2CPP para ARM64 puede fusionar multiplicaciones y sumas y cambiar el último bit. Mitigación: redondear a entero en cada golpe y en cada cambio de estado, y correr los combates grabados en un teléfono antes de construir la validación del servidor.
2. **Tamaño del repositorio (S-29).** Los retratos pesan 3 MB cada uno y vienen 37 más, luego modelos, texturas y audio. Por ahora se quedan en Git sin LFS. Conviene revisarlo antes de que entren modelos 3D y audio, porque mover archivos a LFS después obliga a reescribir el historial.
3. **Contenido a escala (S-23).** El cuello de botella del lanzamiento no es el código sino producir y equilibrar 24 brujas, 120 armas y 12 regiones. Sin herramientas, cada lote cuesta una sesión entera.
4. **La Fase 3 no cabe en una sesión.** Mezcla historia, 3D, Timeline, tienda, servidor, audio, tres idiomas y rendimiento en dispositivos. La hoja de ruta la parte en tres.
5. **Música generada con IA.** Los derechos de uso comercial dependen del plan de Suno y la música 100 % generada puede no tener protección de derechos de autor. Ver `docs/audio/SOUNDTRACK_SUNO.md`.
