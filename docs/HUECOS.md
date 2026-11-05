# Huecos del diseño y sistemas por construir

Revisión del 5 de noviembre de 2026. Cubre el GDD completo, las referencias visuales, el documento de retratos, el reporte de la Fase 1 ([FASE1.md](FASE1.md)) y lo que encontraron las cinco propuestas de `docs/propuestas/` y `docs/audio/`. Responde dos preguntas: **qué está mal o falta en el diseño** y **qué hay que construir, además de las tres fases, para tener un producto que se pueda lanzar**. Cómo se integra cada cosa y en qué orden está en [HOJA_DE_RUTA.md](HOJA_DE_RUTA.md).

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
| Fase 1: combate | **Terminada y fusionada en `main`.** 140 pruebas pasan con `dotnet test`; simulador de balance; escena de prueba con IMGUI. Falta correr las pruebas EditMode en Unity y la prueba con jugadores | `docs/FASE1.md`, `Assets/SalYAlmas/`, `Tests/` |
| Fases 2 y 3 | Prompts listos en el GDD 23. La 3 no cabe en una sesión (ver la hoja de ruta) | `docs/GDD.md` §23 |
| Proyecto de Unity | Creado y con sus `.meta` en `main`. Se abrió con Unity 6000.6.7f1; el reporte de la Fase 1 pedía 6000.0 LTS (ver D-14 en la hoja de ruta) | `ProjectSettings/` |
| Guía para agentes y CI | Agregadas en esta revisión | `CLAUDE.md`, `.github/workflows/ci.yml` |
| Propuestas de contenido | Completas: Fresno (105 Injertos y economía de la run), crafting (12 familias con frames, 120 armas, 25 prefijos, Salmueras) y meta-juego (primera sesión, bucle diario, pase, tienda, social) | `docs/propuestas/` |
| Audio y banda sonora | Completos: plan de integración con FMOD (unos 130 efectos, voces, mezcla, háptica) y 66 cues con 63 prompts para Suno v6 | `docs/audio/` |

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

### 3.1 Cómo las resolvió la Fase 1

La Fase 1 tuvo que decidir muchas de estas para poder programar. Casi todas coinciden con la propuesta; donde no, su decisión ya está probada y conviene adoptarla en el GDD.

| ID | Decisión de la Fase 1 | ¿Coincide? |
| --- | --- | --- |
| C-05 | Ascensión III implementada y probada con grado S forzado; en la prueba con jugadores nadie la verá | Sí |
| C-07 | Magia es Hechizo, Autohechizo, Vislumbre, Ascensión, Éxtasis o ser eslabón de una Cadena que abre ella; el básico da −15 | Sí |
| C-08 | Desalada: un turno sin magia, solo básico | Sí |
| C-09 y C-10 | Hilos cuesta 10 % de la vida máxima y no puede matar; su turno extra no permite otros Hilos ni otra poción. Frenesí gasta 1 Sal Negra | Sí |
| C-11 | Aspecto entre Ascensiones de brujas distintas en el mismo ciclo de AV | Sí |
| C-12 | Ascensiones encimadas pueden hacer Spark; solo la segunda recibe el +50 % | Sí, más preciso |
| C-13 | El Aspecto usa el planeta dominante | Sí |
| C-14 | Crítico base de 5 % y +50 %; los enemigos no hacen crítico | Sí |
| C-15 | El Quiebre dura hasta el siguiente turno del enemigo; daños por turno de 2 turnos con coeficientes por planeta | Sí |
| C-18 | La Ascensión se lanza en cualquier punto de decisión, también antes de un turno enemigo, sin gastar turno | Sí |
| C-19 | +8 de Ánima por golpe recibido, sin tope | Casi: adoptar 8 |
| C-20 | Trígono cura a las dos brujas | Sí |

Quedan abiertas C-16 (Resonancia al abrir), C-17 (si la Invitada tiene vida) y C-21 (Agua de Olvido en combate).

### 3.2 Contradicciones que encontraron la Fase 1 y las propuestas

Cada documento tiene su lista completa con más contexto; aquí están sin repetir y con la propuesta que cada uno ya aplica.

| ID | Dónde | Problema | Propuesta | Fuente |
| --- | --- | --- | --- | --- |
| C-30 | 7.7 y 17.3 contra 10.7 y 22.1 | Los Tránsitos son semanales en unas secciones y de seis semanas en otras | Tránsito de temporada cada seis semanas, más un "cielo de la semana" con dos planetas favorecidos | Fresno, meta |
| C-31 | 1 y 10.1 | "Un anillo del Fresno, 15 a 25 min" contra "una zona, 15 a 20 min" | Ascenso corto = una zona, 15 a 20 min | Fresno, meta |
| C-32 | 10.1 y 23.2 | Con 10 nodos por zona no hay rutas que se bifurquen | 10 pisos con 3 carriles (28 nodos en el mapa, 10 por ruta) | Fresno |
| C-33 | 10.4 | "Si llegas a ese nodo" no funciona si el mapa cambia en cada run | La Mancha se ancla a zona y piso | Fresno |
| C-34 | 11.1 y 16.3 | Las Sales de Baño de logros y del pase gratuito no están en la tabla de ingreso | Reparto nuevo de las 70 tiradas que las incluye | Meta |
| C-35 | 12.2 y 16.4 | Con las 3★ completas el gacha da Sal Cruda y recetas: es vender sal de forma indirecta | Polvo Astral | Crafting, meta |
| C-36 | 6, 22.1 y 23.3 | Doce regiones iniciales posibles, pero se construyen dos y llegan más por versión | Decidir cuántas regiones salen al lanzamiento (D-13) | Meta, audio |
| C-37 | 9.9 | Combate automático desde el principio: la primera Cadena la haría la máquina | El automático se abre después de la primera Cadena manual | Meta |
| C-38 | 10.6 | Si el nivel de cuenta solo sube con la Ofrenda, no sube antes de abrir el Fresno | Altura con Crecimiento por todo lo que juegas, más la Ofrenda | Meta |
| C-39 | 13.1 y 13.4 | Los Títulos aparecen en la ficha de una bruja, pero dan pasivas como si fueran de la Calculista | Títulos de la Calculista; las brujas tienen los suyos sin pasiva | Meta |
| C-40 | 1 y 11.3 | Sesiones de 3 a 5 min no alcanzan para "nivel 70 en siete días" | Se resuelve con H-01 | Meta |
| C-41 | 9.6 | "+1 almita" por Spark no pesa nada frente a 100 000 al día | 10 % de que sea una almita con nombre | Crafting, Fresno |
| C-42 | 10.3 | La Bendición del Paria no tiene tope ni dice si cuenta la Invitada | Tope de 5 opciones; la Invitada no cuenta | Fresno |
| C-43 | 14.2 | Júpiter es el único planeta sin Salmuera | Salmuera Estánnea: +20 de Resonancia | Fresno |
| C-44 | 9.4, 6.5 y 13.2 | Firmeza, Paralaje y estadísticas de la Calculista sin escala | Calculista de 0 a 20; Paralaje según H-15 | Fresno |
| C-45 | 10.5 | Un Eco con 3 Injertos de una Rama da su Fruto desde el primer nodo | Máximo 2 por Rama y 1 astral | Fresno |
| C-46 | 17.4 | Nada prohíbe que un Injerto copie el truco de una bruja "kk" | Regla nueva: ningún Injerto lo copia | Fresno |
| C-47 | 13.1, 7.4 y 9.8 | La ficha muestra DEF y la banda Sobresalada la baja, pero la fórmula de daño no usa DEF | Que R_DEF salga de la DEF, o quitar DEF de la ficha | Fase 1, crafting |
| C-48 | 12.1 y 23.1 | El prompt pedía Autohechizo para las 3★; la 12.1 lo reserva a 4★ y 5★ | Las 3★ no tienen Autohechizo (así quedó la Fase 1) | Fase 1 |
| C-49 | 7.2 y 18.2 | El nombre de la mezcla es del par sin orden: Muertita repite *Humo Óseo* y Topacia *Escarcha Sangrienta* | Nombre por par ordenado (dominante primero), para que cada bruja tenga el suyo | Fase 1 |
| C-50 | 9.1 | ¿La líder del equipo propio también aporta Regencia? ¿Los jefes actúan más de una vez por turno? | Solo la Invitada; los jefes pueden tener varias acciones (el Guardián tiene dos) | Fase 1 |
| C-51 | 15.3 y 6 | "Botín de la segunda región" no tiene sentido con regiones en orden libre | Nombrar la región: Tauro | Crafting |
| C-52 | 15.1 y 17 | "El mejor para Spark" choca con la doctrina contra el power creep | El Grimorio queda solo 6 % arriba de la siguiente familia | Crafting |
| C-53 | 14.1 y 15.5 | Las Salmueras se descubren experimentando, pero la Herbolaria muestra todo | La Herbolaria completa una casilla por cada ingrediente que ya tuviste | Crafting |
| C-54 | 14.1 y 14.2 | El Filtro de Pureza pide Sal Destilada: no puede tener versión básica | Solo potencias mejorada y maestra | Crafting |
| C-55 | 14.3 | "Máximo cinco de cada tipo" no contempla las tres potencias | Cinco por Salmuera y potencia | Crafting |
| C-56 | 11.3 | No dice si *n* es el nivel al que llegas o del que sales | Costo de pasar de *n* a *n*+1 | Crafting |
| C-57 | 4.4, 12.5 y 19.2 | Rechaza cinemáticas de 8 a 10 s, pero las transformaciones duran eso | La regla es para combate; la transformación pasa una vez, fuera de combate y se puede saltar | Audio |
| C-58 | 19.3 y 7.5 | La versión corta de la Ascensión III pierde el canto, pero las Ascensiones son cantadas | La versión corta conserva un pedazo del canto | Audio, música |
| C-59 | 7.5 y 20.3 | La Cadena es silente, pero lleva "voz solista" | Esa voz es de la música, sin palabras | Audio, música |
| C-60 | 6 y 20.1 | Acuario es Escandinavia e Islandia, pero la música le pone "gélico" (errata de gaélico, que es celta) | Sabor nórdico; el celta, para Cáncer | Música |
| C-61 | 20.2 | Violonchelo y flautas están en la base, pero son los solistas de Marisal y Tormentina | Reservar el registro solista de cada Regente | Música |
| C-62 | 6 y 18.1 | La Calculista tiene voz personalizable pero calcula en silencio | Solo reacciones, cuatro voces por idioma | Audio |
| C-63 | 5 | El modo recatado no dice qué cambia en el audio | Tabla del plan de audio, sección 4 | Audio |
| C-64 | 21.1 | El ahorro de batería es "a 30 fps", pero la gama baja ya corre a 30 | Ahorro = 30 fps más menos resolución y efectos | Audio |
| C-65 | 18.1 y 21.2 | Voces en "español latinoamericano" y textos en "español de México" | Un solo nombre: español de México | Audio |
| C-66 | 10.1 | La Jardinera es la jefa de la Copa: son cuatro jefes, no cuatro más ella | Corregir la redacción | Música |
| C-67 | 20.4 | Los Huéspedes suenan a "radiotelescopio", un sonido electrónico, en una banda sonora acústica | Va en el diseño sonoro, no en la música | Música |
| C-68 | 24 | El GDD no dice nada del uso de IA en música e imágenes | Agregar una regla: qué se genera, con qué plan, cómo se declara en las tiendas y qué hace una persona | Música |
| C-69 | 21.3 | Las propuestas de Fresno y audio piden eventos con nombres que no son los del núcleo | La mayoría ya existe en la Fase 1: BattleStarted, ActionShifted (avances y retrasos), Healed, ShieldGained, UnitDefeated y SalinityChanged (valor de 0 a 150). Falta la causa en ActionShifted y un evento de escudo absorbido. La Fase 1.5 alinea los nombres de las propuestas con los del núcleo | Fresno, audio |

## 4. Huecos de diseño

Son reglas o contenido que el GDD nombra pero no define. Sin ellos, el agente de cada fase inventa en el código.

### Bloquean la Fase 2

| ID | Hueco | Propuesta |
| --- | --- | --- |
| H-01 | **Ingreso de almitas.** No hay tabla de cuánto da cada actividad, y las escalas de nivel y grado no cuadran (sección 2) | Bajar el nivel a `40·n^1.5` (644 000 al 70). Con eso, un día de juego da unas 100 000 almitas y nivel, grados y arma pesan 2 : 1 : 1. Las tres propuestas ya usan esa escala o se ajustan con un solo factor (×0.4 en el Fresno). Falta tu decisión D-02 y una tabla única de ingresos por actividad en la Fase 2A |
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
