# El Fresno: catálogo de contenido

Propuesta de contenido para la Fase 2 (sección 23.2 del GDD) · 5 de noviembre de 2026 · **por aprobar**

Este documento propone el contenido que le falta al Fresno: Injertos, Frutos, Mutaciones, maldiciones, Tránsitos, Raíces, nodos, enemigos y la economía de una run. Amplía las secciones 10 (Fresno), 11 (economía), 13.2 (chequeos), 16.3 (ingreso gratuito) y 17 (doctrina contra el power creep) del GDD. No cambia el GDD: si algo de aquí choca con él, manda el GDD hasta que el diseñador apruebe el cambio. Donde toca los mismos temas, sigue a `docs/HUECOS.md` y a `docs/propuestas/META_Y_ONBOARDING.md`. Las contradicciones que encontré están en el apartado 13.

## Lo más importante

- **105 Injertos en 7 Ramas, 35 de ellos marcados ★ para la Fase 2.** Los ★ usan casi solo eventos que la Fase 1 ya emite. Los otros 70 quedan como datos con `implementado: false`.
- **Cualquier equipo puede jugar cualquier Rama.** El Injerto `_01` de cada Rama hace que los Quiebres de cualquier planeta apliquen su estado. Tener brujas del planeta mejora la Rama, pero no es requisito.
- **Las maldiciones suman, no multiplican:** +25 % cada una, +125 % con cinco. Así la quinta vale lo mismo que la primera y el tope semanal sigue teniendo sentido (apartado 6.1).
- **Una zona son 10 pisos.** El mapa de cada zona tiene 28 nodos en 3 carriles y cada ruta visita 10. Así hay bifurcaciones sin romper los "10 nodos por zona" del GDD.
- **La cuenta cierra.** Una run típica da 300 000 almitas y 225 de Savia en unos 65 minutos. El tope semanal de 600 000 almitas equivale a dos runs y al 36 % de lo que cuesta subir una bruja al nivel 70. Los 500 Sales de Baño semanales salen de superar 8 zonas.
- **"Lo que el Fresno se queda, vuelve al 30 %".** La misma tasa para una Mancha perdida y para lo que pasa del tope, para que perder nunca convenga más que ganar.
- **El núcleo necesita cinco eventos nuevos** (retraso y adelanto, curación, escudo, derrota y comienzo de combate). El Fruto de Saturno que trae el GDD ya necesita el de retraso.

## Cómo leer este documento

| Marca | Significado |
| --- | --- |
| ★ | Recomendado para implementar primero en la Fase 2: prueba bien el sistema y usa mecánicas que ya existen en la Fase 1 |
| `saturno_03` | ID en snake_case, listo para JSON: planeta sin acento y número de dos cifras |
| Común, Raro, Astral | Rareza del Injerto. Cada Rama tiene 8 comunes, 5 raros y 2 astrales |
| Disparador | El momento en que ocurre el efecto. Cada uno corresponde a un evento del núcleo (apartado 2.1) |
| TUNING | Número propuesto sin probar. La lista completa está en el apartado 14 |
| ATQ | Ataque de la bruja que dispara el efecto |
| Todas | Las cuatro brujas del equipo. La Invitada recibe los efectos solo mientras actúa |
| Ronda | 100 AV; la primera, 150 (sección 9.2) |
| Turnos de un estado | Se cuentan en los turnos de quien lo sufre |
| Puente | Injerto que mejora algo de otra Rama |
| Bruja de Sol, de Luna… | Bruja cuyo planeta dominante es ese; es el que forma los Aspectos (HUECOS.md, C-13) |
| Sección y apartado | "Sección" remite al GDD; "apartado", a este documento |

Los números siguen el formato del GDD: espacio para los miles (3 000) y punto decimal (1.25). Todos los números de las tablas son TUNING salvo los que cita el GDD.

## 1. Reglas de diseño del catálogo

1. **Cada Rama habla el idioma de su planeta** (sección 7.1). Sol quema en área, Luna protege y controla, Marte se concentra en un enemigo, Mercurio gana tiempo, Venus sostiene, Júpiter manda y Saturno retrasa. El Fruto lleva el nombre de la Quintaesencia de su planeta; un Injerto común lleva el de la sal Turbia y uno raro el de la Destilada.
2. **Cualquier equipo puede jugar cualquier Rama.** El Injerto `_01` de cada Rama hace que los Quiebres de cualquier planeta apliquen el estado de esa Rama. Ninguna bruja queda fuera del Fresno por su planeta.
3. **Las Ramas se cruzan.** Cada Rama tiene Injertos puente que mejoran una Reacción Híbrida o una mecánica de otra Rama. Los efectos del mismo tipo se suman entre Ramas: +1 turno de Quemadura de Sol más +1 turno de Saturno dan +2. La Jardinera castiga a quien crece en una sola Rama (apartado 11.6).
4. **Presupuesto por rareza.** Común: un número de +8 a +15 % o un disparador pequeño. Raro: un disparador fuerte o un bono de +20 a +40 % con condición. Astral: cambia una regla. Los bonos de daño entran en la suma ΣB de la fórmula de daño (sección 9.8), así que se suman y no se multiplican. Todo lo que se acumula dice su tope.
5. **El Spark sigue siendo un acertijo de orden.** Ningún efecto da "probabilidad de Spark", agrega golpes ocultos ni mueve frames al azar. Los golpes extra, como los de Tempestad Mayor, tienen frame fijo y se ven en la Partitura antes de confirmar.
6. **Sin tiradas nuevas en combate.** Injertos, Frutos, Mutaciones y maldiciones usan disparadores y contadores, nunca probabilidades. El azar del Fresno vive en el mapa, en las ofertas y en la Salmuera Turbia, y todo sale de la semilla.
7. **Nada se vende y nada se duplica.** Injertos, Mutaciones, Raíces, Eco y Savia solo se ganan jugando (sección 16.4). Ningún efecto da brujas, copias ni grados. Ningún Injerto copia el truco protegido de una bruja "kk" (sección 17.4): no hay revivir (Axa), duplicar Granos (Topacia), cancelar el ataque definitivo de un jefe (Muertita) ni veneno que escale con la vida máxima del enemigo (Salitre).
8. **Sin bucles y con el tono del juego.** Un efecto que dispara un Injerto no vuelve a disparar ese mismo Injerto en la misma acción. Los nombres vienen de la alquimia, la astrología, la sal, las almas y el árbol, sin tarot (sección 8.3) ni otras franquicias. Los estados se describen por su efecto, nunca por heridas: pavor antes que gore (sección 6.5).

## 2. Vocabulario común

### 2.1 Disparadores y eventos del núcleo

La Fase 1 emite estos eventos (sección 23.1): TurnStarted, ActionUsed, HitLanded, AspectFormed, BreakTriggered, SalinityBandChanged, ChainStarted, ChainEnded, AscensionCast y BattleEnded. Cada disparador del catálogo se conecta así:

| Disparador | Código | Evento del núcleo | ¿Existe en la Fase 1? |
| --- | --- | --- | --- |
| Pasivo | `pasivo` | Modificador que se aplica al armar el combate | Sí; no necesita evento |
| Al iniciar combate | `al_iniciar_combate` | BattleStarted | **Nuevo** |
| Al iniciar turno | `al_iniciar_turno` | TurnStarted | Sí |
| Al usar acción (básico, Hechizo, Autohechizo, poción) | `al_usar_accion`, `al_usar_pocion` | ActionUsed con su tipo | Sí, si ActionUsed incluye el tipo de acción |
| Al golpear, al recibir golpe | `al_golpear`, `al_recibir_golpe` | HitLanded | Sí |
| Al Spark | `al_spark` | HitLanded con isSpark | Sí |
| Al formar Aspecto (incluye las Reacciones) | `al_formar_aspecto` | AspectFormed | Sí |
| Al Quebrar | `al_quebrar` | BreakTriggered | Sí |
| Al cambiar de banda de Salinidad | `al_cambiar_banda` | SalinityBandChanged | Sí |
| Al abrir o terminar una Cadena | `al_abrir_cadena`, `al_terminar_cadena` | ChainStarted, ChainEnded | Sí |
| Al lanzar Ascensión | `al_lanzar_ascension` | AscensionCast | Sí |
| Al terminar combate | `al_terminar_combate` | BattleEnded | Sí |
| Al causar retraso, al adelantar, al recibir retraso | `al_causar_retraso`, `al_adelantar`, `al_recibir_retraso` | AVShifted (con signo, origen y objetivo) | **Nuevo**: el retraso ya existe (Quiebre, Eclipse, Plomo Líquido), pero no tiene evento |
| Al curar | `al_curar` | HealApplied | **Nuevo** |
| Al absorber daño con escudo | `al_absorber_escudo` | ShieldAbsorbed | **Nuevo** |
| Al aplicar estado | `al_aplicar_estado` | StatusApplied | **Nuevo**; solo lo usan Injertos pendientes |
| Al derrotar enemigo, al caer una aliada | `al_derrotar`, `al_caer_aliada` | UnitDefeated | **Nuevo**; se deriva de HitLanded |
| Al actuar un enemigo | `al_actuar_enemigo` | ActionUsed de un enemigo | Sí |
| Fuera de combate | `fuera_de_combate` | Capa del Fresno (Mercader, Evento) | Se construye en la Fase 2 |

Los 35 Injertos ★ y los 7 Frutos solo necesitan cinco eventos nuevos: BattleStarted, AVShifted, HealApplied, ShieldAbsorbed y UnitDefeated.

### 2.2 Valores de referencia

El GDD nombra los estados de Quiebre pero no les da números. El catálogo asume estos, que siguen la propuesta C-15 de HUECOS.md (daño por turno en % del ATQ durante 2 turnos). Si la Fase 1 fijó otros, mandan los de la Fase 1 y los porcentajes de los Injertos se mantienen.

| Estado | Valor que asumo |
| --- | --- |
| Quemadura | 15 % del ATQ de quien la aplicó, al empezar cada turno del enemigo; 2 turnos |
| Sangrado | 20 % del ATQ por turno; 2 turnos. El Sangrado fuerte del Quiebre de Marte hace 30 % |
| Veneno | 15 % del ATQ por turno; 2 turnos |
| Encanto | El siguiente ataque del enemigo hace −30 % |
| Electrocución | 20 % del ATQ al objetivo y salta a 1 enemigo cercano |
| Encierro | Retraso extra de 15 % y −10 % de Velocidad durante 2 turnos |
| Gravedad | Retraso extra de 30 % |
| Escudo | Dura hasta romperse o hasta 2 turnos de su dueña |
| Regla general | Aplicar un estado que ya está renueva su duración; no se acumula salvo que el efecto lo diga |

**Firmeza.** Después de un Quiebre, la Firmeza se recupera completa al empezar el siguiente turno del enemigo (HUECOS.md, C-15); la Élite Despierta y varios Injertos lo asumen. Referencia: un ataque básico de un planeta débil quita 10, un Hechizo 20, un eslabón de Cadena de 15 a 30 y un Cierre 30. Con esa escala, un enemigo normal tiene de 40 a 100 de Firmeza, una Élite de 150 a 260 y un Jefe de 400 a 600.

## 3. Cómo se ganan y se combinan los Injertos

### 3.1 Ofertas

- **Combate:** al ganar eliges 1 de 3 Injertos (más los que agregue la Bendición del Paria). De las tres opciones, una siempre es de una Rama que ya tienes (si tienes alguna) y otra es de una Rama del cielo de la semana, si ya está abierta (apartado 3.5); la tercera es libre.
- **Jefe:** eliges 1 de 3 Injertos raros o astrales, con al menos un astral.
- **Mercader:** vende 2 Injertos (apartado 9.5). Algunos Eventos también dan uno.
- **Reglas de toda oferta:** nunca un Injerto que ya tienes, nunca uno de una Rama bloqueada y como máximo un astral por oferta.

Rareza de cada opción según la zona:

| Zona | Común | Raro | Astral |
| --- | --- | --- | --- |
| Raíces Putrefactas | 70 % | 25 % | 5 % |
| Tronco de Ceniza | 60 % | 30 % | 10 % |
| Ramas del Eclipse | 50 % | 35 % | 15 % |
| Copa Astral | 40 % | 40 % | 20 % |

Una run completa junta unos 24 Injertos: 16 de combates, 4 de jefes, de 0 a 3 del Mercader, de 0 a 2 de Eventos y hasta 3 del Eco.

### 3.2 Frutos

- Tres Injertos de la misma Rama, de cualquier rareza, maduran su Fruto. Los Injertos del Eco cuentan.
- **Máximo dos Frutos activos.** Si madura un tercero, eliges en ese momento qué dos Frutos conservas; la decisión dura toda la run.
- Por qué el tope: con unos 24 Injertos por run, sin tope se juntan cuatro o cinco Frutos y la run deja de pedir decisiones.

### 3.3 Eco

El Eco guarda 3 Injertos y 1 movimiento entrenado (sección 10.5). Propongo dos límites: **como máximo 2 Injertos de la misma Rama y 1 astral.** Así ninguna run empieza con un Fruto maduro, pero el Eco deja uno a punto de madurar.

### 3.4 Desbloqueo de Ramas

La sección 10.6 dice que la Ofrenda desbloquea Ramas nuevas. META_Y_ONBOARDING.md (apartado 4.3) ya fija cuándo: Sol, Luna y Marte desde el inicio, y las otras cuatro en Altura 12, 16, 20 y 25 (la Ofrenda acelera la Altura). Este catálogo propone el orden:

| Ramas | Se abren |
| --- | --- |
| Sol, Luna y Marte | Desde que se abre el Fresno (Altura 10) |
| Venus | Altura 12 |
| Saturno | Altura 16 |
| Mercurio | Altura 20, con el Fresno completo |
| Júpiter | Altura 25 |

Primero llegan las acciones simples (área, escudo, un solo objetivo y curación). Después llegan las de tiempo y mando, que piden dominar la Cadena (sección 9.9). En la Fase 2 todas se implementan; el bloqueo es solo un dato.

### 3.5 Cielo de la semana

El GDD dice en las secciones 7.7 y 17.3 que los Tránsitos rotan cada semana, y en las 10.7 y 22.1 que cambian cada seis. META_Y_ONBOARDING.md (MC-02) propone dos capas: un **Tránsito de temporada** de seis semanas (apartado 7) y un **cielo de la semana** pequeño que favorece dos planetas. Aquí está su contenido en el Fresno:

- Los pares siguen el orden caldeo, el mismo que da nombre a los días (sección 7.7): Saturno y Júpiter; Marte y Sol; Venus y Mercurio; Luna y Saturno; Júpiter y Marte; Sol y Venus; Mercurio y Luna. En 7 semanas, cada planeta sale dos veces.
- El ciclo de 7 semanas no coincide con las versiones de 6, así que cada versión empieza con un cielo distinto.
- Efecto: una de las 3 opciones de cada Combate es de una de esas dos Ramas, y madurar el Fruto de una de ellas da +10 % de almitas en esa run.

### 3.6 Bendición del Paria: aclaraciones

- +1 opción de Injerto por cada bruja 3★, con **tope de 5 opciones** en pantalla. A partir de la tercera 3★, cada una da 1 Relevo por zona (volver a sacar las opciones de un Injerto).
- +10 % de almitas por cada 3★ y +5 % por cada 4★, sumados al resto de bonos (apartado 12.2).
- La Invitada no cuenta: es la bruja de otra persona.

## 4. Las siete Ramas

### 4.1 Rama del Sol (Oro)

**Tema:** el Sol premia el daño en área y la Quemadura que se extiende; quemar a todos a la vez.

**Fruto `sol_fruto`, Corona Solar:** todos tus golpes en área aplican Quemadura (2 turnos), y los enemigos con Quemadura reciben +15 % de daño de todas las fuentes.

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
| ★ | `sol_01` | Brasa Pálida | Común | Tus Quiebres, de cualquier planeta, también aplican Quemadura (2 turnos) | Al Quebrar |
| ★ | `sol_02` | Ceniza Salobre | Común | +15 % de daño de tus golpes en área | Pasivo |
|  | `sol_03` | Rescoldo | Común | La Quemadura que aplicas dura 1 turno más | Al aplicar estado |
|  | `sol_04` | Lumbre de Secta | Común | Al formar Trígono, el objetivo del segundo eslabón recibe Quemadura (2 turnos) | Al formar Aspecto (Trígono) |
| ★ | `sol_05` | Horno de Sal | Común | Cuando una bruja sube a la banda Sobresalada, todos los enemigos reciben Quemadura (2 turnos). Una vez por bruja y combate | Al cambiar de banda de Salinidad |
|  | `sol_06` | Alba | Común | Al empezar el combate, todos los enemigos reciben Quemadura (1 turno) | Al iniciar combate |
|  | `sol_07` | Pira | Común | Al derrotar a un enemigo con Quemadura, la Quemadura pasa a otro enemigo que no la tenga, con los turnos que le quedaban | Al derrotar enemigo |
|  | `sol_08` | Salmuera de Mediodía | Común | La Salmuera Áurea da +15 % de ataque en vez de +10 % y dura 3 turnos | Al usar poción |
|  | `sol_09` | Fuego Pálido | Raro | La Quemadura hace +40 % de daño | Pasivo |
| ★ | `sol_10` | Corona de Ceniza | Raro | Al formar un Aspecto en el que participa una bruja de Sol, el siguiente eslabón golpea también a todos los demás enemigos, al 40 % de su daño | Al formar Aspecto |
|  | `sol_11` | Eclipse Dorado | Raro | Puente con Saturno. La Reacción Eclipse también aplica Quemadura a todos los enemigos y retrasa 40 % en vez de 30 % | Al formar Aspecto (Eclipse) |
|  | `sol_12` | Destello | Raro | Cuando un golpe en área hace Spark, aplica Quemadura (2 turnos) a todos los que golpea | Al Spark |
|  | `sol_13` | Mediodía | Raro | Al empezar el turno de una bruja, si 3 o más enemigos tienen Quemadura, ella gana +10 de Ánima | Al iniciar turno |
|  | `sol_14` | Sol Negro | Astral | Al Quebrar a un enemigo, todas las Quemaduras activas estallan: hacen de golpe el daño de sus turnos restantes y terminan | Al Quebrar |
| ★ | `sol_15` | Orto Helíaco | Astral | La primera Ascensión de cada combate cuesta 50 de Ánima menos y golpea a todos los enemigos | Al lanzar Ascensión |

### 4.2 Rama de la Luna (Plata)

**Tema:** la Luna convierte la defensa en control; sus escudos protegen y engañan.

**Fruto `luna_fruto`, Espejo Lunar:** cuando un escudo absorbe daño, el atacante recibe el 50 % de lo absorbido y se retrasa 10 % (una vez por ataque).

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
|  | `luna_01` | Bruma Salina | Común | Al empezar el combate, todas reciben un escudo del 10 % de su vida máxima | Al iniciar combate |
|  | `luna_02` | Halo | Común | Los escudos que das son 20 % más fuertes | Pasivo |
| ★ | `luna_03` | Velo de Plata | Común | La bruja que Quiebra recibe un escudo del 10 % de su vida máxima | Al Quebrar |
|  | `luna_04` | Encierro Largo | Común | El Encierro (Quiebre de Luna) retrasa 10 % más | Al Quebrar (Luna) |
|  | `luna_05` | Menguante | Común | Cuando una bruja baja de banda de Salinidad, recibe un escudo del 10 % de su vida máxima | Al cambiar de banda de Salinidad |
|  | `luna_06` | Sueño Ligero | Común | Los enemigos con la Velocidad reducida hacen −10 % de daño | Pasivo |
| ★ | `luna_07` | Marea Alta | Común | La Leche de Luna da un escudo del 30 % en vez del 20 % | Al usar poción |
| ★ | `luna_08` | Espejismo | Común | Al abrir una Cadena, todos los enemigos se retrasan 8 % | Al abrir Cadena |
| ★ | `luna_09` | Marea de Plata | Raro | Al empezar su turno, si una bruja conserva escudo, gana +15 de Ánima | Al iniciar turno |
|  | `luna_10` | Helada Pálida | Raro | Puente con Saturno. La Reacción Helada Negra también da un escudo del 15 % de su vida máxima a cada bruja de la Cadena | Al formar Aspecto (Helada Negra) |
|  | `luna_11` | Luz Prestada | Raro | Puente con Venus. La curación que pasa de la vida máxima se vuelve escudo, hasta el 20 % de la vida máxima | Al curar |
|  | `luna_12` | Oposición Serena | Raro | La Oposición sube +10 de Salinidad a cada bruja en vez de +20 | Al formar Aspecto (Oposición) |
|  | `luna_13` | Noche Clara | Raro | Mientras una bruja tiene escudo, sus Hechizos le suben +5 de Salinidad en vez de +10 | Al usar acción (Hechizo) |
| ★ | `luna_14` | Luna Negra | Astral | Cuando un golpe de Luna Quiebra a un enemigo, este pierde su siguiente turno (una vez por enemigo y combate). Contra Jefes: −50 % de Velocidad durante un turno | Al Quebrar |
|  | `luna_15` | Doble Reflejo | Astral | Cuando un enemigo golpea a una bruja con escudo, el daño se reparte en partes iguales entre todas las brujas que tienen escudo | Al recibir golpe |

### 4.3 Rama de Marte (Hierro)

**Tema:** Marte junta toda la fuerza en un solo enemigo hasta que cae.

**Fruto `marte_fruto`, Acero Carmesí:** tus golpes a un solo objetivo hacen +30 % de daño a los enemigos con Sangrado y renuevan su duración.

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
| ★ | `marte_01` | Herrumbre | Común | Tus Quiebres, de cualquier planeta, también aplican Sangrado (2 turnos) | Al Quebrar |
| ★ | `marte_02` | Filo Único | Común | +15 % de daño de tus golpes a un solo objetivo | Pasivo |
|  | `marte_03` | Embestida | Común | El primer golpe de cada combate hace +40 % de daño | Al iniciar combate |
|  | `marte_04` | Cicatriz | Común | Cada vez que recibe un golpe, la bruja gana +5 % de ataque hasta el final del combate (máx. 5 veces) | Al recibir golpe |
|  | `marte_05` | Rastro de Hierro | Común | +20 % de daño contra enemigos con menos del 30 % de vida | Pasivo |
| ★ | `marte_06` | Cuadratura de Guerra | Común | La Cuadratura da +60 % de daño a la Firmeza en vez de +40 % | Al formar Aspecto (Cuadratura) |
|  | `marte_07` | Vinagre Templado | Común | El Vinagre de Marte quita +80 % de Firmeza en vez de +50 % | Al usar poción |
|  | `marte_08` | Hierro Tenaz | Común | El Sangrado que aplicas dura 1 turno más | Al aplicar estado |
|  | `marte_09` | Sangre Férrea | Raro | El Sangrado hace +40 % de daño | Pasivo |
|  | `marte_10` | Espinas Hondas | Raro | Puente con Venus. Espinas Rojas cura con el 50 % del daño en vez del 30 % | Al formar Aspecto (Espinas Rojas) |
| ★ | `marte_11` | Golpe de Gracia | Raro | El Cierre de una Cadena hace +30 % de daño si su objetivo tiene Sangrado | Al terminar Cadena (Cierre) |
|  | `marte_12` | Sed de Hierro | Raro | Al derrotar a un enemigo, +1 Grano de Sal (máx. una vez por turno) | Al derrotar enemigo |
|  | `marte_13` | Frenesí Templado | Raro | El Frenesí de Sal Negra baja la defensa −30 % en vez de −50 % | Al usar acción (Autohechizo) |
| ★ | `marte_14` | Duelo | Astral | Al empezar el combate se marca al enemigo con más vida: +40 % de daño contra él y −20 % contra los demás. Cuando cae, la marca pasa al enemigo con más vida | Al iniciar combate y al derrotar enemigo |
|  | `marte_15` | Ira del Hierro | Astral | Cuando cae una bruja, las demás ganan +25 % de ataque y +50 de Ánima hasta el final del combate (máx. 2 veces por combate) | Al caer una aliada |

### 4.4 Rama de Mercurio (Azogue)

**Tema:** Mercurio gana por tiempo: actuar antes, actuar más veces y envenenar mientras tanto.

**Fruto `mercurio_fruto`, Mente Relámpago:** +10 de Velocidad a todas. Cada vez que adelantas a una aliada, todos los enemigos con Veneno reciben al instante el daño de un turno de Veneno (máx. una vez por turno).

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
| ★ | `mercurio_01` | Humo Ácido | Común | Tus Quiebres, de cualquier planeta, también aplican Veneno (2 turnos) | Al Quebrar |
| ★ | `mercurio_02` | Pies Ligeros | Común | +8 de Velocidad a todas | Pasivo |
|  | `mercurio_03` | Salida Rápida | Común | Al empezar el combate, la bruja más rápida se adelanta 20 % | Al iniciar combate |
|  | `mercurio_04` | Veneno Lento | Común | El Veneno que aplicas dura 1 turno más | Al aplicar estado |
|  | `mercurio_05` | Mensajera | Común | Al usar una poción, la bruja se adelanta 10 % | Al usar poción |
|  | `mercurio_06` | Tónico Largo | Común | El Tónico de Azogue da +25 de Velocidad en vez de +20 y dura 3 turnos | Al usar poción |
|  | `mercurio_07` | Trato de Mercader | Común | Los precios del Mercader de Almas bajan 20 % | Fuera de combate (Mercader) |
|  | `mercurio_08` | Prisa Venenosa | Común | Los enemigos con Veneno tienen −10 de Velocidad | Pasivo |
| ★ | `mercurio_09` | Azogue Vivo | Raro | El Sextil adelanta 25 % en vez de 15 % | Al formar Aspecto (Sextil) |
| ★ | `mercurio_10` | Estafeta | Raro | Al terminar una Cadena, la bruja que hizo la Apertura se adelanta 30 % | Al terminar Cadena |
|  | `mercurio_11` | Tempestad Ácida | Raro | Puente con Júpiter. La Tempestad aplica Veneno (2 turnos) a todos los enemigos | Al formar Aspecto (Tempestad) |
|  | `mercurio_12` | Veneno Volátil | Raro | Al derrotar a un enemigo con Veneno, su Veneno pasa a otro enemigo con la duración completa | Al derrotar enemigo |
|  | `mercurio_13` | Hilo de Azogue | Raro | El Autohechizo de Hilos cuesta el 5 % de la vida en vez del 10 % | Al usar acción (Autohechizo) |
|  | `mercurio_14` | Caduceo | Astral | Al empezar el combate, la bruja más rápida actúa dos veces seguidas | Al iniciar combate |
| ★ | `mercurio_15` | Sextil Pleno | Astral | El Sextil también da el efecto del Trígono: +1 Grano de Sal y curación del 5 % de la vida máxima | Al formar Aspecto (Sextil) |

### 4.5 Rama de Venus (Cobre)

**Tema:** Venus sostiene al equipo y desarma al enemigo con encanto.

**Fruto `venus_fruto`, Jardín Esmeralda:** cada curación que haces también daña al enemigo con más vida por el 30 % de lo curado.

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
| ★ | `venus_01` | Cardenillo | Común | Tus Quiebres, de cualquier planeta, también aplican Encanto | Al Quebrar |
| ★ | `venus_02` | Mano Tibia | Común | +15 % de curación hecha | Pasivo |
|  | `venus_03` | Trígono Tibio | Común | La curación del Trígono sube del 5 % al 10 % de la vida máxima | Al formar Aspecto (Trígono) |
|  | `venus_04` | Retoño | Común | Al empezar el combate, todas reciben regeneración del 3 % de su vida máxima por turno durante 3 turnos | Al iniciar combate |
|  | `venus_05` | Bálsamo Doble | Común | El Bálsamo Cúprico limpia dos estados negativos en vez de uno | Al usar poción |
| ★ | `venus_06` | Rocío | Común | Al ganar un combate, todas curan el 10 % de su vida máxima | Al terminar combate |
|  | `venus_07` | Mirada Dulce | Común | Los enemigos con Encanto reciben +10 % de daño | Pasivo |
|  | `venus_08` | Encanto de Feria | Común | +2 en los chequeos de Encanto de los Eventos del Fresno | Fuera de combate (Evento) |
|  | `venus_09` | Savia de Cobre | Raro | La regeneración también baja −3 de Salinidad por turno | Al iniciar turno |
|  | `venus_10` | Espinas Dulces | Raro | Puente con Marte. Espinas Rojas también aplica Encanto a su objetivo | Al formar Aspecto (Espinas Rojas) |
|  | `venus_11` | Flor de Sal | Raro | Al curar a una bruja en banda Sobresalada o más alta, también le baja −10 de Salinidad | Al curar |
| ★ | `venus_12` | Cobre que Canta | Raro | Cada curación da +5 de Ánima a quien cura (máx. una vez por turno) | Al curar |
|  | `venus_13` | Jardín Compartido | Raro | Al curar a una bruja, las demás curan el 30 % de esa cantidad | Al curar |
|  | `venus_14` | Encanto Cruzado | Astral | Un enemigo con Encanto dirige su siguiente ataque a otro enemigo. Contra Jefes: su siguiente ataque hace −50 % | Al actuar un enemigo |
| ★ | `venus_15` | Cobre Vivo | Astral | Si una de las dos brujas es de Venus, las sectas distintas forman Trígono en vez de Cuadratura | Al formar Aspecto |

### 4.6 Rama de Júpiter (Estaño)

**Tema:** Júpiter manda y hace que cada aliada valga más junto a las demás.

**Fruto `jupiter_fruto`, Ley Tonante:** al abrir una Cadena, todas ganan +20 % de ataque hasta que termine, y cada Spark dentro de la Cadena aplica Electrocución a su objetivo.

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
| ★ | `jupiter_01` | Estática Gris | Común | Tus Quiebres, de cualquier planeta, también aplican Electrocución | Al Quebrar |
| ★ | `jupiter_02` | Voz de Mando | Común | +10 % de ataque a todas | Pasivo |
| ★ | `jupiter_03` | Tambor | Común | Al abrir una Cadena, todas ganan +10 de Ánima | Al abrir Cadena |
|  | `jupiter_04` | Orden de Marcha | Común | Al empezar el combate, +15 de Resonancia | Al iniciar combate |
|  | `jupiter_05` | Esencia Repartida | Común | La Esencia de Alma da +15 de Ánima a todas en vez de +30 a una | Al usar poción |
|  | `jupiter_06` | Conjunción Real | Común | La Conjunción da +35 % al siguiente eslabón en vez de +25 % | Al formar Aspecto (Conjunción) |
|  | `jupiter_07` | Heraldo | Común | +2 en los chequeos de Temple de los Eventos del Fresno | Fuera de combate (Evento) |
|  | `jupiter_08` | Estandarte | Común | +10 % de daño a la Firmeza para todas | Pasivo |
|  | `jupiter_09` | Trueno de Estaño | Raro | La Electrocución hace 30 % del ataque en vez de 20 % y salta a un enemigo más | Al aplicar estado |
| ★ | `jupiter_10` | Coro de Estaño | Raro | Cada Spark da +4 de Ánima a todo el equipo | Al Spark |
|  | `jupiter_11` | Tempestad Mayor | Raro | Puente con Mercurio. La Tempestad da 3 golpes extra en vez de 2, con frames fijos que se ven en la Partitura | Al formar Aspecto (Tempestad) |
|  | `jupiter_12` | Mandato | Raro | Al terminar una Cadena, todas ganan +15 % de ataque durante 2 turnos | Al terminar Cadena |
|  | `jupiter_13` | Gran Benéfico | Raro | Las mejoras que das duran 1 turno más | Al aplicar estado |
|  | `jupiter_14` | Júpiter en Sagitario | Astral | La Invitada puede cerrar dos Cadenas por combate en vez de una | Al abrir Cadena |
| ★ | `jupiter_15` | Resonancia Tonante | Astral | Puedes abrir la Cadena con 75 de Resonancia en vez de 100; abrirla consume 75 | Pasivo |

### 4.7 Rama de Saturno (Plomo)

**Tema:** Saturno detiene el reloj del enemigo; cada retraso es una herida.

**Fruto `saturno_fruto`, Hora Final (del GDD, sin cambios):** cada retraso que causas hace daño igual al 20 % de tu ataque. Aclaraciones para implementarlo: "tu ataque" es el ATQ de la bruja que causó el retraso; cuenta cada retraso por separado, sin importar su tamaño; incluye los retrasos del Quiebre, de las Reacciones, de las Salmueras y de los Injertos. Para que funcione, el núcleo debe emitir AVShifted (apartado 2.1).

| ★ | ID | Nombre | Rareza | Efecto | Disparador |
| --- | --- | --- | --- | --- | --- |
| ★ | `saturno_01` | Ceniza Gris | Común | Tus Quiebres, de cualquier planeta, retrasan 10 % más | Al Quebrar |
| ★ | `saturno_02` | Peso Muerto | Común | Los enemigos tienen −8 de Velocidad | Pasivo |
| ★ | `saturno_03` | Péndulo | Común | Al formar un Aspecto en el que participa una bruja de Saturno, el objetivo del segundo eslabón se retrasa 8 % | Al formar Aspecto |
|  | `saturno_04` | Arena de Reloj | Común | Al empezar el combate, todos los enemigos se retrasan 15 % | Al iniciar combate |
|  | `saturno_05` | Plomo Hondo | Común | El Plomo Líquido retrasa 30 % en vez de 20 % | Al usar poción |
|  | `saturno_06` | Disciplina | Común | Los ataques básicos bajan −10 de Salinidad en vez de −5 | Al usar acción (básico) |
|  | `saturno_07` | Hora Gris | Común | La Reacción Eclipse quita tres mejoras en vez de dos | Al formar Aspecto (Eclipse) |
|  | `saturno_08` | Lastre | Común | +10 % de daño contra enemigos más lentos que la bruja que ataca | Pasivo |
| ★ | `saturno_09` | Plomo Lento | Raro | Cada retraso que causas también quita 5 de Firmeza | Al causar retraso |
|  | `saturno_10` | Helada Honda | Raro | Puente con Luna. La Reacción Helada Negra también retrasa 15 % a los demás enemigos | Al formar Aspecto (Helada Negra) |
|  | `saturno_11` | Memento | Raro | Si retrasas a un enemigo normal con menos del 10 % de vida, cae en el acto. No afecta a Élites ni a Jefes | Al causar retraso |
|  | `saturno_12` | Tiempo Prestado | Raro | Puente con Mercurio. Cada retraso que causas da +3 de Resonancia (máx. +30 por combate) | Al causar retraso |
|  | `saturno_13` | Órbita Lenta | Raro | Los estados negativos que aplicas duran 1 turno más; se suma a los Injertos de duración de otras Ramas | Al aplicar estado |
| ★ | `saturno_14` | Reloj de Arena Invertido | Astral | Cuando retrasas a un enemigo, la bruja que lo retrasó se adelanta la mitad de ese porcentaje | Al causar retraso |
|  | `saturno_15` | Cuerpo de Plomo | Astral | Los enemigos no pueden retrasar a tus brujas. Cada retraso evitado da +10 de Ánima a esa bruja | Al recibir retraso |

### 4.8 Resumen para la Fase 2

Los 35 Injertos ★ cubren todos los disparadores importantes. Cada Rama lleva al menos un común, un raro y un astral, así que los pesos de rareza funcionan aunque solo se ofrezcan los ★.

| Disparador | ★ que lo usan | Evento |
| --- | --- | --- |
| Pasivo | `sol_02`, `marte_02`, `mercurio_02`, `venus_02`, `jupiter_02`, `jupiter_15`, `saturno_02` | Modificador |
| Al Quebrar | `sol_01`, `luna_03`, `luna_14`, `marte_01`, `mercurio_01`, `venus_01`, `jupiter_01`, `saturno_01` | BreakTriggered |
| Al formar Aspecto | `sol_10`, `marte_06`, `mercurio_09`, `mercurio_15`, `venus_15`, `saturno_03` | AspectFormed |
| Al abrir o terminar Cadena | `luna_08`, `jupiter_03`, `marte_11`, `mercurio_10` | ChainStarted, ChainEnded |
| Al cambiar de banda | `sol_05` | SalinityBandChanged |
| Al iniciar turno | `luna_09` | TurnStarted |
| Al Spark | `jupiter_10` | HitLanded |
| Al lanzar Ascensión | `sol_15` | AscensionCast |
| Al usar poción | `luna_07` | ActionUsed |
| Al terminar combate | `venus_06` | BattleEnded |
| Al curar | `venus_12` | HealApplied (nuevo) |
| Al iniciar combate y al derrotar | `marte_14` | BattleStarted y UnitDefeated (nuevos) |
| Al causar retraso | `saturno_09`, `saturno_14` | AVShifted (nuevo) |

Los 7 Frutos deben implementarse completos. Los de Luna, Mercurio, Venus y Saturno usan ShieldAbsorbed, AVShifted y HealApplied.

**Formato de datos sugerido.** El generador de ofertas excluye todo lo que tenga `implementado: false`:

```json
[
  { "id": "saturno_09", "rama": "saturno", "rareza": "raro", "disparador": "al_causar_retraso",
    "efecto": { "tipo": "quitar_firmeza", "valor": 5 }, "implementado": true },
  { "id": "saturno_11", "rama": "saturno", "rareza": "raro", "disparador": "al_causar_retraso",
    "efecto": { "tipo": "derrotar_si_vida_menor", "umbral": 0.10, "excluye": ["elite", "jefe"] },
    "implementado": false }
]
```

## 5. Mutaciones del Árbol

Cada Élite ofrece **dos Mutaciones** que no tengas; eliges una o ninguna. Duran toda la run y solo se quitan con la Purga del Mercader. No tienen tope, porque cada una cobra su precio.

| ★ | ID | Nombre | Bendición | Maldición |
| --- | --- | --- | --- | --- |
| ★ | `mutacion_savia_negra` | Savia Negra (del GDD) | +30 % de daño | −30 % de curación recibida |
| ★ | `mutacion_bandas_hondas` | Bandas Hondas | Los efectos buenos de las bandas de Salinidad se duplican: +20 % de crítico en Salobre, +40 % de daño en Sobresalada y +80 % de daño del Ascendente en Cristalización Turbia | Los malos también: −30 % de defensa en Sobresalada y −10 % de vida máxima por turno en Cristalización Turbia |
| ★ | `mutacion_corteza_plomo` | Corteza de Plomo | +25 % de defensa | −10 de Velocidad a todas |
| ★ | `mutacion_medula_azogue` | Médula de Azogue | +15 de Velocidad a todas | −20 % de vida máxima |
| ★ | `mutacion_raiz_sedienta` | Raíz Sedienta | +40 % de curación hecha | El máximo de Granos baja de 5 a 4 |
|  | `mutacion_hojarasca` | Hojarasca | +1 opción al elegir Injerto | Los Combates dan −25 % de almitas |
|  | `mutacion_fruto_temprano` | Fruto Temprano | Los Frutos maduran con 2 Injertos en vez de 3 | Cada Fruto activo baja −10 % la vida máxima |
|  | `mutacion_anillo_crecimiento` | Anillo de Crecimiento | Cada combate ganado sin que caiga ninguna bruja da +3 % de ataque (máx. +30 %) | Si cae una bruja, pierdes todo lo acumulado y empiezas el siguiente combate con −10 % de ataque |
|  | `mutacion_savia_dulce` | Savia Dulce | Al ganar cada combate, todas curan el 15 % de su vida máxima | Los Santuarios ya no curan vida; solo bajan la Salinidad |
|  | `mutacion_injerto_torcido` | Injerto Torcido | Los Injertos raros y astrales cuentan doble para madurar Frutos | Los comunes no cuentan |
|  | `mutacion_ramas_cruzadas` | Ramas Cruzadas | La Cuadratura también da +1 Grano de Sal | El Trígono ya no cura |
|  | `mutacion_vislumbre_largo` | Vislumbre Largo | El Vislumbre dura 3 turnos en vez de 2 | Al terminar, la bruja no puede lanzar Hechizos en su siguiente turno |
|  | `mutacion_corazon_hueco` | Corazón Hueco | Las Ascensiones cuestan 20 % menos de Ánima | Recibir daño ya no da Ánima |
| ★ | `mutacion_sal_viva` | Sal Viva | El Spark da +75 % de daño en vez de +50 % | Los golpes sin Spark hacen −10 % de daño |
|  | `mutacion_hojas_que_miran` | Hojas que Miran | Ves la debilidad oculta de todos los enemigos, sin importar tu Paralaje | +10 de Paralaje al terminar cada zona |
|  | `mutacion_raiz_glotona` | Raíz Glotona | +1 Sal Negra extra al vencer cada Élite | El Vislumbre y el Frenesí de Sal Negra suben +20 de Salinidad extra |

Las seis ★ solo cambian números que la Fase 1 ya maneja: daño, defensa, Velocidad, vida, Granos, bandas de Salinidad y Spark.

## 6. Maldiciones opcionales

Al entrar a cada zona eliges de 0 a 5 maldiciones de esta lista. Duran solo esa zona, no se quitan y no se repiten. Cada una suma +25 % a las almitas que ganes en esa zona.

| ★ | ID | Nombre | Efecto en la zona |
| --- | --- | --- | --- |
| ★ | `maldicion_sal_amarga` | Sal Amarga | Empiezas cada combate con 2 Granos en vez de 3 |
| ★ | `maldicion_corteza_petrea` | Corteza Pétrea | Los enemigos tienen +30 % de Firmeza |
| ★ | `maldicion_savia_espesa` | Savia Espesa | Los enemigos tienen +25 % de vida |
| ★ | `maldicion_espinas` | Espinas del Fresno | Los enemigos hacen +20 % de daño |
|  | `maldicion_viento_ajeno` | Viento Ajeno | Los enemigos tienen +10 de Velocidad |
| ★ | `maldicion_hambre_sal` | Hambre de Sal | Cada bruja empieza cada combate con +20 de Salinidad |
| ★ | `maldicion_coro_mudo` | Coro Mudo | La Resonancia que ganas baja 25 % |
|  | `maldicion_santuario_frio` | Santuario Frío | El descanso del Santuario solo da −20 de Salinidad y +15 % de vida |
|  | `maldicion_carta_borrada` | Carta Borrada | En cada combate, el enemigo con más vida se vuelve Sin Carta (apartado 11.1). No afecta a Jefes |
|  | `maldicion_agua_salobre` | Agua Salobre | Tu curación baja 30 % |
|  | `maldicion_mercader_avaro` | Mercader Avaro | Los precios del Mercader suben 50 % y tiene una ranura menos |
|  | `maldicion_elite_despierta` | Élite Despierta | La primera vez que Quiebras a una Élite, su Firmeza se rellena al 50 % |

Ninguna maldición oculta la Partitura ni toca el Spark: deben ser difíciles, no injustas.

### 6.1 ¿Multiplicativo o aditivo?

La sección 10.3 dice que cada maldición "multiplica las almitas por 1.25". Eso se puede leer de dos formas:

| Maldiciones | Aditivo (+25 % cada una) | Multiplicativo (×1.25 cada una) | Lo que suma la última: aditivo / multiplicativo |
| --- | --- | --- | --- |
| 0 | ×1.00 | ×1.00 | — |
| 1 | ×1.25 | ×1.25 | +0.25 / +0.25 |
| 2 | ×1.50 | ×1.56 | +0.25 / +0.31 |
| 3 | ×1.75 | ×1.95 | +0.25 / +0.39 |
| 4 | ×2.00 | ×2.44 | +0.25 / +0.49 |
| 5 | ×2.25 | ×3.05 | +0.25 / +0.61 |

**Recomendación: aditivo, +25 % por maldición y +125 % con cinco.** Las razones:

1. **Cada maldición vale lo mismo.** En la versión multiplicativa, la quinta vale más del doble que la primera y empuja al "todo o nada". En la aditiva, eliges cada maldición según si la puedes manejar.
2. **Se lee en una etiqueta.** "+75 % de almitas" cabe en un botón; "×1.95" pide calculadora.
3. **Respeta el tope semanal.** Con ×3.05, una sola run da 915 000 almitas, una vez y media el tope: la cuarta y la quinta maldición solo producirían Savia. Con ×2.25, una run con cinco maldiciones da unas 675 000, apenas por encima del tope. La regla queda así: cinco maldiciones llenan el tope en una sola run.
4. **Entra en la misma suma que los demás bonos** (apartado 12.2), sin reglas de orden.

Si se aprueba, la sección 10.3 debe decir "cada una suma +25 % de almitas".

## 7. Tránsitos de temporada

Un Tránsito dura una versión de seis semanas y cambia las reglas del Fresno y a la Jardinera (sección 10.7). Cada uno favorece dos planetas.

### 7.1 Reglas

| ★ | ID | Tránsito | Regla | Contrapeso | Favorece |
| --- | --- | --- | --- | --- | --- |
|  | `transito_gran_conjuncion` | Gran Conjunción | Júpiter y Saturno cuentan como el mismo planeta para formar Conjunción. Esa Conjunción da +40 % en vez de +25 % y retrasa 10 % al objetivo | La Oposición sube +30 de Salinidad en vez de +20 | Júpiter, Saturno |
|  | `transito_mercurio_cazimi` | Mercurio Cazimi (en el corazón del Sol) | Sol y Mercurio cuentan como el mismo planeta para formar Conjunción. Esa Conjunción da +40 % y adelanta 15 % a la segunda bruja | Las demás Conjunciones dan +15 % en vez de +25 % | Sol, Mercurio |
|  | `transito_luna_tauro` | Luna exaltada en Tauro | Los escudos son 50 % más fuertes, y curar a una bruja con escudo suma la mitad de lo curado a su escudo | −10 % de daño a todas | Luna, Venus |
| ★ | `transito_marte_retrogrado` | Marte retrógrado (del GDD) | El daño directo sube 40 % | El Sangrado cura a quien lo sufre: los equipos de Sangrado y de Espinas Rojas pierden su motor | Marte (golpe único), Júpiter |
| ★ | `transito_saturno_domicilio` | Saturno en Domicilio (del GDD) | Los retrasos duplican su efecto | Vale también para los enemigos: sus retrasos sobre tus brujas son dobles | Saturno, Luna |
|  | `transito_venus_piscis` | Venus exaltada en Piscis | La curación que pasa de la vida máxima se vuelve escudo (hasta el 25 %), y las mejoras y el Encanto duran 1 turno más | Mercurio está en caída en Piscis: las brujas de Mercurio hacen −15 % de daño | Venus, Júpiter |
|  | `transito_sol_aries` | Sol exaltado en Aries | El primer Quiebre de cada combate aplica Quemadura a todos los enemigos, y la Quemadura hace +50 % de daño | Saturno está en caída en Aries: tus retrasos son 25 % más débiles | Sol, Marte |
|  | `transito_mercurio_retrogrado` | Mercurio retrógrado | El daño de tu Veneno vuelve como curación a tu bruja con menos vida (50 % de lo hecho), y el Veneno se acumula hasta 2 veces | El Sextil ya no adelanta: retrasa 10 % a la segunda bruja | Mercurio, Venus |
|  | `transito_marte_capricornio` | Marte exaltado en Capricornio | +25 % de daño a la Firmeza, y el Quiebre deja al enemigo con +20 % de daño recibido en vez de +10 % | Júpiter está en caída en Capricornio: las brujas de Júpiter hacen −15 % de daño | Marte, Saturno |
|  | `transito_eclipse_luna` | Eclipse de Luna | Sol y Luna cuentan como opuestos: forman Oposición y disparan la Reacción Eclipse | Con el cielo oscuro, el Trígono no cura | Sol, Luna |

Las dignidades usadas (exaltación, caída, domicilio) son las de la astrología clásica. Sol y Luna nunca aparecen retrógrados, porque en la astrología no lo están.

### 7.2 Jardinera y recompensa de cada temporada

La Jardinera es débil a los dos planetas que favorece el Tránsito, más uno oculto. Cada temporada le agrega una regla.

| Tránsito | Jardinera | Regla extra de la Jardinera | Recompensa temática |
| --- | --- | --- | --- |
| Gran Conjunción | Jardinera de Estaño y Plomo | Su Poda también quita 20 de Resonancia | Título *Testigo de la Gran Conjunción*; marco del Libro Mayor "Dos coronas" |
| Mercurio Cazimi | Jardinera Áurea | Cada 3 turnos se cubre de oro: +30 % de defensa hasta que la Quiebres | Título *En el Corazón del Sol*; color de sal "Azogue dorado" |
| Luna exaltada en Tauro | Jardinera de Plata | Su Poda rompe todos tus escudos | Título *La que Durmió en Tauro*; decoración "Lámpara de luna" para las Termas |
| Marte retrógrado | Jardinera de Herrumbre | Cada 3 turnos se aplica Sangrado a sí misma, que la cura; Quebrarla se lo quita | Título *Contra el Hierro*; marco "Herrumbre" |
| Saturno en Domicilio | Jardinera de Plomo | Su Poda también retrasa 30 % a todas tus brujas | Título *La que Detuvo la Hora*; marco "Reloj de plomo" |
| Venus exaltada en Piscis | Jardinera de Cobre | Se cura 5 % cada vez que poda | Título *Exaltada en Piscis*; decoración "Peces de cobre" |
| Sol exaltado en Aries | Jardinera Solar | Al podar, aplica Quemadura (1 turno) a todas tus brujas | Título *Alba en Aries*; color de sal "Brasa" |
| Mercurio retrógrado | Jardinera de Azogue | Al podar, intercambia su lugar en la línea de turnos con tu bruja más rápida | Título *La que Leyó al Revés*; marco "Espejo" |
| Marte exaltado en Capricornio | Jardinera de Obsidiana | Tiene una segunda barra de Firmeza | Título *Filo de Obsidiana*; decoración "Remolino de sal" |
| Eclipse de Luna | Jardinera del Eclipse | Al podar, tapa un planeta como la Corneja (apartado 11.6) | Título *Bajo la Sombra Roja*; marco "Luna roja" |

Los títulos se ganan venciendo a la Jardinera de la temporada con 3 maldiciones o más en la Copa. El cosmético, con 5.

### 7.3 Orden de lanzamiento y reglas de rotación

| Versión | Tránsito | Favorece |
| --- | --- | --- |
| 1.0 | Gran Conjunción | Júpiter, Saturno |
| 1.1 | Mercurio Cazimi | Sol, Mercurio |
| 1.2 | Luna exaltada en Tauro | Luna, Venus |
| 1.3 | Marte retrógrado | Marte, Júpiter |
| 1.4 | Saturno en Domicilio | Saturno, Luna |
| 1.5 | Venus exaltada en Piscis | Venus, Júpiter |
| 1.6 | Sol exaltado en Aries | Sol, Marte |
| 1.7 | Mercurio retrógrado | Mercurio, Venus |
| 1.8 | Marte exaltado en Capricornio | Marte, Saturno |
| 1.9 | Eclipse de Luna | Sol, Luna |

Reglas que siguen la doctrina 17.3:

1. **Dos Tránsitos seguidos nunca favorecen el mismo planeta.** El orden de arriba lo cumple, también al volver de 1.9 a 1.0.
2. **Un Tránsito nunca favorece el planeta principal de la Regente limitada de su versión.** Así el Fresno no empuja a tirar. En 1.0 las limitadas son Marisal (Marte) y Selenia (Luna); por eso 1.0 abre con la Gran Conjunción. Si una versión futura choca con esta regla, su Tránsito se cambia por otro de la lista que cumpla la regla 1.
3. **En diez versiones, seis planetas quedan favorecidos 3 veces y Mercurio 2.** El siguiente Tránsito que se diseñe debe favorecer a Mercurio.

### 7.4 Hitos del Tránsito

Los 640 Sales de Baño por versión de la sección 16.3 salen de cuatro hitos que cualquier jugador constante alcanza:

| Hito | Sales de Baño |
| --- | --- |
| Superar 4 zonas con el Tránsito activo | 160 |
| Madurar un Fruto de un planeta favorecido | 160 |
| Vencer a la Jardinera de la temporada | 160 |
| Superar 16 zonas en la temporada | 160 |
| **Total** | **640** |

## 8. Raíces

Las Raíces son talentos de cuenta con tope que se compran con Savia (sección 10.6). Sus bonos son pequeños para que el Fresno siga siendo de habilidad. Con todas al máximo, lo más fuerte es un Grano extra al empezar cada combate; lo demás son una ranura de poción, un 9 % de vida y ventajas de información o de economía. Se suman a la Bendición del Fresno por Altura (+5 % de ATQ y vida como máximo, META_Y_ONBOARDING.md, apartado 4.3).

| ★ | ID | Raíz | Efecto por nivel | Tope | Savia por nivel | Total |
| --- | --- | --- | --- | --- | --- | --- |
| ★ | `raiz_cinturon` | Cinturón Ancho (del GDD) | +1 ranura de poción en el Fresno. Con el Sendero de Alquimista, cada ranura guarda +1 carga en su lugar | 1 | 800 | 800 |
| ★ | `raiz_grano` | Grano Heredado (del GDD) | Empiezas cada combate del Fresno con +1 Grano (máx. 5) | 1 | 1 000 | 1 000 |
| ★ | `raiz_corteza` | Corteza Gruesa | +3 % de vida máxima en el Fresno | 3 | 100, 200, 300 | 600 |
| ★ | `raiz_anima` | Ánima Latente | El Ánima que se conserva entre combates sube de 100 a 150, y luego a 200 | 2 | 250, 500 | 750 |
| ★ | `raiz_coro` | Coro Previo | +10 de Resonancia al empezar cada combate | 2 | 200, 400 | 600 |
|  | `raiz_sal_negra` | Bolsa de Sal Negra | +1 Sal Negra al empezar la run | 2 | 250, 500 | 750 |
|  | `raiz_agua_quieta` | Agua Quieta | El descanso del Santuario cura +10 % de vida extra | 2 | 150, 300 | 450 |
| ★ | `raiz_regateo` | Regateo de Almas | −5 % en los precios del Mercader | 3 | 100, 150, 200 | 450 |
|  | `raiz_relevo` | Ojo de Injertador | 1 Relevo por zona: vuelves a sacar las opciones de un Injerto | 2 | 250, 500 | 750 |
|  | `raiz_folio` | Lectura del Folio | Ves el nombre y el chequeo de cada Evento del mapa antes de entrar | 1 | 200 | 200 |
|  | `raiz_libro` | Libro Abierto | +1 en los chequeos de los Eventos del Fresno | 3 | 100, 200, 300 | 600 |
|  | `raiz_contadora` | Mano de Contadora | +3 % de almitas en el Fresno | 5 | 50, 100, 150, 200, 250 | 750 |
|  | `raiz_caldero` | Caldero Hondo | El Caldero prepara 1 Salmuera más | 1 | 400 | 400 |
|  |  | **Total** |  |  |  | **8 100** |

Con 525 a 600 de Savia por semana, un jugador regular completa las Raíces en 13 a 15 semanas, unas dos versiones y media (apartado 12.3).

## 9. Nodos

### 9.1 El mapa de una zona

- **Cada zona es una rejilla de 3 carriles y 10 pisos.** Recorres un nodo por piso: 10 nodos por zona y 40 por run. El mapa completo de una zona tiene 28 nodos: 3 por piso del 1 al 9 y el Jefe en el 10.
- **Bifurcación.** Cada nodo conecta con el de arriba en su carril y, con 50 % de probabilidad, también con uno de un carril vecino. Las líneas no se cruzan. Todo nodo tiene al menos una entrada y una salida, y los tres nodos del piso 9 llevan al Jefe.
- **Todo el mapa se ve desde el principio.** Planear la ruta es parte de la habilidad. Solo los Eventos se muestran como "?" hasta entrar (la Raíz Lectura del Folio los revela).
- **Generación.** Todo sale de la semilla. Si una semilla no cumple las restricciones de abajo, se prueba con semilla + 1, siempre igual.

**Pisos fijos:**

| Piso | Contenido |
| --- | --- |
| 1 | Tres Combates: eliges por dónde entrar |
| 9 | Dos Santuarios y un Mercader. Antes del Jefe eliges entre guardar o gastar lo suelto |
| 10 | Jefe |

**Pisos 2 a 8 (21 nodos):**

| Tipo | Raíces | Tronco | Ramas | Copa |
| --- | --- | --- | --- | --- |
| Combate | 10 | 9 | 9 | 8 |
| Élite | 2 | 3 | 3 | 4 |
| Evento LitRPG | 4 | 4 | 4 | 4 |
| Santuario de Sal | 1 | 1 | 1 | 1 |
| Mercader de Almas | 1 | 1 | 1 | 1 |
| Caldero | 2 | 2 | 1 | 1 |
| Nido de Almas | 1 | 1 | 2 | 2 |
| **Total** | **21** | **21** | **21** | **21** |

**Restricciones de colocación:**

1. No hay Élites en el piso 2, y nunca hay dos Élites seguidas en una ruta.
2. El Santuario de los pisos 2 a 8 va entre los pisos 4 y 6.
3. El Mercader de los pisos 2 a 8 va entre los pisos 3 y 7, y no en el mismo piso que ese Santuario.
4. Ningún piso tiene tres nodos del mismo tipo, salvo Combate.
5. Toda ruta del piso 1 al 9 cruza al menos 3 Combates y como máximo 2 Élites.

**En una ruta de 10 nodos** hay entre 3 y 6 Combates (típico: 4), de 0 a 2 Élites (típico: 1), de 0 a 3 Eventos (típico: 1), de 0 a 2 Santuarios y el Jefe. Una ruta puede no pasar por ningún Santuario si en el piso 9 eliges al Mercader: es la apuesta de gastar en lugar de guardar.

La **Mancha de Sal** no es un tipo de nodo que genere el mapa: es una marca sobre un piso (apartado 9.8).

### 9.2 Recompensas por tipo de nodo

| Nodo | Recompensa |
| --- | --- |
| Combate | Almitas, 1 Injerto de 3 y 1 pizca de Sal Cruda del planeta que más daño hizo en el combate (ingrediente para el Caldero) |
| Élite | Almitas, 1 Sal Negra y una Mutación de 2 (o ninguna) |
| Jefe | Almitas, Savia, 1 Injerto raro o astral de 3, 1 material del Fresno, +25 % de vida y −20 de Salinidad a todas |
| Evento LitRPG | Lo que diga el evento (apartado 9.9) |
| Santuario, Caldero, Mercader, Nido | Apartados 9.3 a 9.6 |

Las almitas exactas por nodo están en el apartado 12.1.

### 9.3 Santuario de Sal

- **Al entrar, todas tus almitas sueltas se guardan.** No hay que elegirlo.
- Después eliges una cosa (sección 10.2):
  - **Descansar:** −40 de Salinidad y +30 % de vida a todas.
  - **Entrenar:** un movimiento de Cadena de una bruja sube un nivel de entrenamiento hasta el final de la run. Cada nivel da +15 % de daño y quita +5 de Firmeza; máximo 2 niveles por movimiento. El Eco puede guardar ese movimiento con sus niveles.
- Abandonar la run dentro de un Santuario no deja Mancha.

### 9.4 Caldero

Eliges una opción (sección 14):

- **Preparar:** 2 cargas de una Salmuera básica que ya esté en tu Recetario, sin ingredientes.
- **Refinar:** 1 carga de una Salmuera mejorada, con 2 pizcas de Sal Cruda del mismo planeta.
- **Experimentar:** combinas 2 pizcas distintas. Si es una receta nueva, se anota en el Recetario (también vale fuera del Fresno) y recibes 1 carga; si no, sale una Salmuera Turbia.

Las pizcas de Sal Cruda son de la run y se pierden al terminarla. Cada ranura del cinturón guarda hasta 3 cargas.

### 9.5 Mercader de Almas

Solo acepta almitas sueltas (sección 10.2). Tiene 6 ranuras y cada artículo se compra una vez por visita.

| Ranura | Artículo | Precio en Raíces |
| --- | --- | --- |
| 1 | Injerto común | 5 000 |
| 2 | Injerto raro; en Ramas y Copa, 1 de cada 3 veces es astral | 10 000; astral 20 000 |
| 3 | Salmuera básica, 1 carga | 2 000 |
| 4 | Salmuera mejorada, 1 carga | 4 000 |
| 5 | Sal Negra | 6 000 |
| 6 | Un servicio al azar: **Descanso menor** (−20 de Salinidad a todas), **Vendaje de corteza** (+20 % de vida a todas), **Purga** (quita una Mutación) o **Relevo** | 3 000; 3 000; 8 000; 2 000 |

- **Precio por zona:** Raíces ×1, Tronco ×1.5, Ramas ×2 y Copa ×3, redondeado a 500.
- **Descuentos:** Regateo de Almas (hasta −15 %) y Trato de Mercader (−20 %) se suman, hasta −35 %. Mercader Avaro suma +50 %.
- Como referencia: en las Raíces llegas al Mercader de media zona con unas 12 000 almitas sueltas, lo justo para un Injerto común y una Salmuera.

### 9.6 Nido de Almas

- Almitas (apartado 12.1) y 1 **material del Fresno** de la zona: Raíz Negra, Corteza de Ceniza, Pluma de Eclipse u Hoja Astral. Propongo que sirvan para las armas Astrales y Quintaesencia (sección 15.2).
- Eliges un extra: 1 carga de Salmuera básica, 1 Sal Negra o 1 Relevo.
- Con Paralaje medio o más, la primera vez por zona y temporada el Nido también trae una **almita con nombre**: un fragmento de historia (sección 11.2).

### 9.7 Ascenso corto

El Ascenso corto recorre una sola zona. Las Raíces están abiertas desde que se abre el Fresno (Altura 10, antes de la run completa en Altura 20), y cada zona siguiente se abre al vencer al Jefe de la anterior en cualquier modo. Usa el mismo mapa, las mismas recompensas y la misma Savia por zona. Al vencer al Jefe, la run termina y se guarda todo lo suelto. Cuenta para el tope semanal y para los hitos (apartado 12.4).

### 9.8 Mancha de Sal

El GDD dice que recuperas la Mancha "si llegas a ese nodo" (sección 10.4). Como el mapa cambia en cada run, propongo anclarla a **zona y piso**:

1. Al caer todo el equipo, lo suelto queda como Mancha en ese piso de esa zona. Si caes ante un Jefe, la Mancha queda en el piso 9, frente a su puerta.
2. En la siguiente run, la Mancha se ve en el mapa desde que entras a la zona. Al terminar cualquier nodo de ese piso, la recuperas.
3. **Lo recuperado se guarda al instante**, como si pasaras por un Santuario. No lo arriesgas dos veces.
4. Si caes antes de llegar a ese piso, el Fresno absorbe la Mancha y te devuelve el 30 % como Savia (3 de Savia por cada 1 000 almitas). La nueva Mancha toma su lugar. Solo existe una a la vez.
5. Un Ascenso corto en otra zona no toca la Mancha, salvo que caigas en él. La Mancha no caduca.
6. Abandonar la run fuera de un Santuario cuenta como caer en el nodo donde estás.

### 9.9 Eventos LitRPG

**Escala propuesta** (la sección 13.2 no la da): las estadísticas van de 0 a 20. La Calculista empieza con 3 en cada una y termina la historia principal con unos 10 en la mejor. Cada bruja del equipo suma +2 a su rasgo, con un tope de +4 por chequeo. Los Injertos de evento dan +2 y la Raíz Libro Abierto hasta +3. El chequeo pasa si el total iguala o supera el umbral, y el jugador ve ambos números antes de elegir.

**Umbral por zona:** Raíces 7, Tronco 10, Ramas 13 y Copa 16.

| ID | Evento | Zona | Opción con chequeo | Éxito | Fallo | Otras opciones |
| --- | --- | --- | --- | --- | --- | --- |
| `evento_cristalizado` | **El Cristalizado que Susurra.** Una peregrina a medio volverse sal repite un nombre | Raíces | Escucharla: Temple 7 | Aprendes el nombre: una almita con nombre y eliges 1 Injerto raro de Luna o Saturno | El susurro se te queda: +20 de Salinidad a todas y +5 de Paralaje | Romper la sal y liberarla: +3 000 almitas. Que una bruja se coma su sal: +1 Sal Negra y +1 a sus Almas digeridas |
| `evento_caldero_viejo` | **El Caldero Abandonado.** Un caldero frío con la costra de una receta olvidada | Raíces, Tronco | Raspar el fondo: Oficio 7 (10 en Tronco) | Aprendes una Salmuera que no tenías y ganas 2 cargas | 1 carga de Salmuera Turbia | Llevarte el caldero roto: +1 material del Fresno |
| `evento_folio` | **Un Folio Mal Sumado.** Una hoja del Libro Mayor donde la cuenta de un alma no cuadra | Todas | Corregir la suma: Cálculo 7, 10, 13 o 16 según la zona | Eliges 1 Injerto entre 4 de cualquier Rama abierta | El folio se corrige solo y ahora lleva tu nombre: +10 de Paralaje | Arrancar el folio: +3 000 almitas ×zona; la siguiente Élite tiene +20 % de vida |
| `evento_prestamista` | **La Balanza del Prestamista.** Un espíritu del Ducado de las Balanzas presta almitas | Tronco | Regatear: Encanto 10 | Respeta a quien sabe contar: te regala 7 500 almitas y te presta 15 000 sin interés | Se ofende: el siguiente Mercader cobra +25 % | Aceptar sin regatear: +15 000 almitas sueltas ahora; frente al Jefe pagas 22 500 sueltas, y si no las tienes, el Jefe tiene +25 % de vida. Rechazar: nada |
| `evento_grifo_savia` | **El Grifo de Savia.** Una grieta en la corteza por la que corre savia tibia | Tronco | Recogerla con cuidado: Oficio 10 | +40 de Savia y +20 % de vida a todas | La corteza se cierra: −10 % de vida a todas | Beber directo de la grieta: −30 de Salinidad a todas y +5 de Paralaje |
| `evento_emisaria` | **La Emisaria de la Casa.** Una bruja de una Casa también escala el Fresno | Tronco, Ramas | Negociar el paso: Temple 10 (13 en Ramas). Si la perdonaste en la historia, pasa sin chequeo (marca de Destino, sección 13.5) | Te comparte su Partitura: +15 % de daño de Cadena en esta zona | Te sigue: la siguiente Élite de la zona es ella, con +20 % de vida | Desafiarla: combate de Élite en el acto, con su recompensa. Ignorarla: nada |
| `evento_cornejas` | **El Consejo de las Cornejas.** Aves posadas en fila juzgan a la Calculista | Ramas | Responder su acertijo: Cálculo 13 | Revelan todos los Eventos de la zona y ganas 1 Injerto común de Mercurio o Júpiter | Se llevan el 10 % de tus almitas sueltas | Darles 1 Sal Negra: la Corneja del Eclipse empieza con −20 % de Firmeza. Ahuyentarlas: combate contra 3 Grajas de Sal |
| `evento_constelacion` | **La Constelación Equivocada.** Hay una estrella de más, y se mueve cuando dejas de mirarla | Ramas, Copa | Contarlas otra vez: Cálculo 13 (16 en Copa) | La estrella de más sigue dormida: tu siguiente Combate se vuelve Nido | Despierta: tu siguiente Combate es contra la Mirada del Firmamento (en Ramas, con −30 % de vida) y +10 de Paralaje | No mirar: nada |
| `evento_hoja` | **La Hoja que No Quiere Caer.** Un alma madura se aferra a la rama porque teme a la Jardinera | Copa | Convencerla de soltarse: Encanto 16 | Cae y vuelve al Fresno: +60 de Savia y, la primera vez, el título *La que Dejó Caer* | Se aferra más: tu siguiente Combate suma una Podadora | Esconderla en tu sombrero: +1 Injerto astral, pero la Jardinera empieza con su Poda cargada. Arrancarla: +18 000 almitas y +1 a las Almas digeridas de una bruja |
| `evento_tu_letra` | **Tu Propia Letra.** Un folio escrito con tu letra cuenta una run que todavía no juegas | Copa, solo con Paralaje medio o más | Leerlo completo: Temple 16 | Ves la debilidad oculta de la Jardinera y eliges 1 Injerto de tu Rama con más Injertos | +15 de Paralaje; en esta zona las ventanas de estado fallan, con glifos en lugar de números | Cerrarlo sin leer: nada |

Reparto de chequeos: Cálculo 3, Temple 3, Encanto 2 y Oficio 2.

## 10. Estado que persiste en una run

El GDD no dice qué cruza de un nodo a otro. Propuesta:

| Estado | Entre nodos | Regla |
| --- | --- | --- |
| Vida | Se conserva | Una bruja que cae se levanta con 10 % de vida si el combate se gana. Si cae todo el equipo, la run termina. Al vencer a un Jefe, todas curan 25 % |
| Salinidad | Se conserva | Baja −20 al terminar cada combate y −20 más al vencer a un Jefe. Al empezar un combate se limita a 140, para que el Éxtasis no llegue gratis en el primer turno |
| Ánima | Se conserva hasta 100 | Lo que pase de 100 se pierde al terminar el combate (150 y 200 con la Raíz Ánima Latente). Se puede guardar una Ascensión I; la III se gana dentro del combate |
| Resonancia | Vuelve a 0 | Una Cadena en el primer turno de cada combate rompería el ritmo. La Raíz Coro Previo da +10 o +20 |
| Granos de Sal | Vuelven a 3 | Es la regla del combate (sección 9.3). La Raíz Grano Heredado da +1 |
| Cargas de pociones | Se conservan | No se recargan solas: Caldero, Mercader y Nido. Entras con el cinturón cargado desde tu inventario, una carga por ranura, y esas cargas se gastan del inventario |
| Sal Negra | Inventario de la run | Empiezas con 1 (+2 con la Raíz) y cada Élite da 1. Al terminar la run, la que sobre pasa a tu inventario, con un máximo de 3. Gastarla o guardarla es una decisión |
| Efectos de "una vez por combate" | Se reinician | Vislumbre, Cierre de la Invitada, Luna Negra y similares |
| Estados, escudos y mejoras | Se borran | Nada del combate cruza al siguiente, salvo lo de esta tabla |
| Éxtasis Abisal | Se resuelve | Si el combate termina durante el Éxtasis, la Salinidad baja a 60 como si hubiera terminado |
| Injertos, Frutos, Mutaciones y movimientos entrenados | Toda la run |  |
| Maldiciones | Solo en su zona |  |
| Almitas sueltas | Hasta guardarse | Se guardan en un Santuario o al terminar la run. Si caes, forman la Mancha |
| Equipo | Fijo | Las cuatro brujas y la Invitada se eligen al empezar y no cambian. Así la Bendición del Paria no se puede aprovechar con cambios a media run |
| Paralaje | Es de la cuenta | Sube y baja como en la sección 6.5 |

La run se puede pausar en cualquier nodo (sección 10.1); el estado se guarda tal cual.

## 11. Enemigos

### 11.1 Convenciones

- **Nivel por zona:** Raíces 45, Tronco 52, Ramas 60 y Copa 70. Con eso, un equipo de 4★ y 3★ en grado B (nivel 50 a 60) supera hasta las Ramas con buen orden, y la Copa pide estrategia (sección 17.7).
- **Debilidad oculta:** las Élites y los Jefes tienen una debilidad entre corchetes que solo se ve con Paralaje medio o más (sección 6.5).
- **Paralaje alto:** los enemigos se vuelven aberrantes: +20 % de vida y una acción extra marcada en su patrón. A cambio, +20 % de almitas, o +30 % con Paralaje máximo. Escala de HUECOS.md (H-15): medidor de 0 a 100 con bandas en 25, 50 y 80.
- **Sin Carta:** no se forma un Aspecto si alguno de los dos eslabones o acciones golpea a un Sin Carta: no hay bono, ni Reacción, ni Injertos de "al formar Aspecto". No tienen debilidades fijas: cualquier golpe baja su Firmeza, los Hechizos y eslabones al 50 % de lo normal y los ataques básicos al 200 %. Obligan a pelear con básicos y Quiebres en lugar de Aspectos.
- **Huéspedes:** nunca entran completos en cuadro. Se ven ojos en patrón de constelación y metal líquido que se pliega en el borde de la pantalla (secciones 6.5 y 19.1). Sin sustos: su turno se anuncia en la línea de turnos como cualquier otro.
- **Cobertura:** en cada zona, cada planeta es debilidad de al menos un enemigo normal y de al menos dos enemigos si se cuentan las Élites. Así ninguna composición queda bloqueada.

### 11.2 Raíces Putrefactas

| Enemigo | Tipo | Débil a | Firmeza | Patrón de IA |
| --- | --- | --- | --- | --- |
| Raicilla Salobre | Normal | Sol, Marte | 60 | Golpea a la bruja con menos vida; cada 3 turnos se enraíza y gana +30 % de defensa un turno |
| Larva de Sal Negra | Normal | Sol, Venus | 40 | Se pega a la bruja con más Salinidad y le sube +10 en cada turno de la larva |
| Cristalizado Peregrino | Normal | Luna, Venus, Júpiter | 80 | Alterna: un turno solo susurra, el siguiente da un golpe fuerte |
| Hongo Sordo | Normal | Marte, Mercurio | 60 | Cada 3 turnos suelta esporas: la bruja objetivo no puede lanzar Hechizos en su siguiente turno |
| Escarabajo Farolero | Normal | Mercurio, Saturno | 50 | Huye al terminar su tercer turno y se lleva el 5 % de tus almitas sueltas; vencerlo antes suma +1 000 almitas al combate |
| Reina de la Podre | Élite | Sol, Júpiter, Saturno [Venus] | 200 | Cada 2 turnos invoca una Larva de Sal Negra (máx. 2); al Quebrarla, sus larvas se retrasan 50 % |
| La que Bebe la Sal | Élite | Luna, Venus, Saturno [Marte] | 180 | Cada 2 turnos bebe 15 de Salinidad de la bruja con más Salinidad y se cura 5 % por cada 10; al juntar 60, golpea a todas |

### 11.3 Tronco de Ceniza

| Enemigo | Tipo | Débil a | Firmeza | Patrón de IA |
| --- | --- | --- | --- | --- |
| Corteza Viva | Normal | Marte, Júpiter | 100 | Cada 2 turnos se cubre con +50 % de defensa hasta que la Quiebras |
| Ámbar con Recuerdo | Normal | Luna, Venus, Mercurio | 60 | No ataca el primer turno; después copia la última mejora que recibió una de tus brujas |
| Enjambre de Ceniza | Normal | Sol, Júpiter | 40 | Cinco golpes pequeños por turno; cada golpe sube +2 de Salinidad a la bruja golpeada |
| Carcoma | Normal | Marte, Saturno | 70 | Su golpe deja a la bruja con −30 % de curación recibida durante 2 turnos |
| Leñadora Cristalizada | Normal | Sol, Venus, Saturno | 90 | Protege al enemigo con menos vida: recibe los golpes de un solo objetivo dirigidos a él |
| Guardián del Anillo | Élite | Cambia con cada anillo: Sol y Luna, luego Marte y Mercurio, luego Júpiter y Saturno [Venus en los tres] | 150 por anillo | Tres anillos de Firmeza; al Quebrar uno, muestra el siguiente. Golpe pesado cada 2 turnos |
| Sin Carta del Nudo | Élite, Sin Carta | Ninguna fija | 160 | Una figura de madera anudada donde debería haber un rostro. Ataca a la última bruja que actuó en tu Cadena; con 50 % de vida, golpea a todas cada 2 turnos |

### 11.4 Ramas del Eclipse

| Enemigo | Tipo | Débil a | Firmeza | Patrón de IA |
| --- | --- | --- | --- | --- |
| Graja de Sal | Normal | Júpiter, Saturno | 50 | Rápida; si golpea a una bruja sin escudo, roba 1 Grano compartido, que vuelve al vencerla |
| Vencejo del Vendaval | Normal | Luna, Marte | 40 | Velocidad 160; golpes ligeros a la bruja más lenta |
| Nido Hueco | Normal | Sol, Venus | 90 | No ataca; cada 2 turnos eclosiona una Graja de Sal (máx. 2) |
| Penumbra | Normal | Sol, Luna | 70 | Mientras haya otro enemigo vivo, recibe −50 % de daño de los golpes que no sean de Sol ni de Luna |
| Veleta Viva | Normal | Rota cada turno, en orden visible: Mercurio, Venus, Júpiter, Marte | 60 | Golpea a la bruja cuyo planeta coincide con su debilidad del momento |
| Gran Duque sin Luz | Élite | Luna, Mercurio, Saturno [Sol] | 220 | Un búho sin luz en los ojos. Cada 2 turnos da +20 % de ataque y adelanta 15 % a los demás enemigos; si lo Encierras, pierden la mejora |
| Halcón Gnomon | Élite | Sol, Venus, Júpiter [Saturno] | 200 | Su sombra marca en la línea de turnos su "mediodía": cada 3 turnos, golpe en área con +50 % |

### 11.5 Copa Astral

| Enemigo | Tipo | Débil a | Firmeza | Patrón de IA |
| --- | --- | --- | --- | --- |
| Hoja Madura | Normal | Luna, Venus | 50 | No ataca. Si sigue viva al terminar su tercer turno, se suelta y se va: el combate da −20 % de almitas |
| Podadora | Normal | Marte, Saturno | 80 | Dos golpes cruzados a una bruja; al Quebrarla, cae y retrasa 20 % a otro enemigo |
| Astro Caído | Normal | Sol, Júpiter, Saturno | 90 | Golpe en área cada 2 turnos; al vencerlo, +1 Grano |
| Lucero Hambriento | Normal | Luna, Mercurio | 60 | Cada golpe quita 15 de Ánima a la bruja golpeada |
| Hoja sin Carta | Normal, Sin Carta | Ninguna fija | 70 | Un alma sin carta natal que nadie vino a podar. Cada turno cura 10 % al enemigo con menos vida |
| Mirada del Firmamento | Élite, Huésped menor | Rota con sus ojos, en orden visible: Sol y Saturno, Luna y Marte, Venus y Júpiter, Mercurio [ninguna] | 260 | Cada Aspecto que formas le suma 1 de Mirada; con 3, abre los ojos: golpe a todas y +20 de Salinidad. Quebrarla vuelve la Mirada a 0. Pelear contra ella sube +5 de Paralaje |
| Constelación Desatada | Élite | Mercurio, Venus, Sol [Luna] | 220 | Tiene cinco estrellas; cada Spark apaga una y le quita 15 % de daño. Con todas encendidas, su golpe en área retrasa 20 % |

### 11.6 Jefes de zona

| Jefe | Zona | Débil a | Firmeza | Mecánica central | Al 50 % de vida | Qué le pide al jugador |
| --- | --- | --- | --- | --- | --- | --- |
| La Madre Raíz | Raíces | Sol, Marte [Júpiter] | 400 | **Raíces que salan.** Tiene tres raíces que son objetivos aparte (débiles a Sol y Marte, 40 de Firmeza cada una). Cada 3 turnos, cada raíz viva sube +10 de Salinidad a todas tus brujas. Si rompes las tres, ella queda Quebrada en el acto | Las raíces rebrotan una vez | Daño en área y manejar la Salinidad |
| El Leñador Hueco | Tronco | Marte, Saturno [Luna] | 500 | **Tala anunciada.** Cada 3 turnos marca con su hacha a tu bruja más rápida, y la marca se ve en la línea de turnos. En su siguiente turno la tala: 60 % de su vida máxima. Si lo Quiebras antes, la tala se cancela | De su tronco salen dos Ámbares con Recuerdo | Orden, retrasos y Quiebres a tiempo |
| La Corneja del Eclipse | Ramas | Saturno, Venus [Mercurio] | 550 | **Ala que eclipsa.** Al empezar cada ronda tapa un planeta y su glifo aparece sobre ella: los golpes de ese planeta hacen −50 % de daño y no forman Aspectos. Siempre tapa el planeta que más daño le hizo en la ronda anterior | Tapa dos planetas por ronda | Variar planetas y Ramas |
| La Jardinera | Copa | Los dos planetas favorecidos del Tránsito [uno por temporada] | 600 | **Poda.** Cada 3 turnos anuncia la Poda sobre tu Rama con más Injertos. Si no la Quiebras antes de su siguiente turno, los Injertos de esa Rama y su Fruto se apagan hasta tu siguiente Quiebre. Cada temporada suma una regla (apartado 7.2) | Anuncia la Poda cada 2 turnos | Builds cruzadas y Quiebres a tiempo |

## 12. Economía de la run

### 12.1 Almitas por nodo y zona

Cada zona multiplica la base: Raíces ×1, Tronco ×1.5, Ramas ×2 y Copa ×3.

| Nodo | Raíces | Tronco | Ramas | Copa | Extra |
| --- | --- | --- | --- | --- | --- |
| Combate | 3 000 | 4 500 | 6 000 | 9 000 | 1 Injerto y 1 pizca de Sal Cruda |
| Élite | 8 000 | 12 000 | 16 000 | 24 000 | 1 Sal Negra y 1 Mutación |
| Evento (promedio) | 3 000 | 4 500 | 6 000 | 9 000 | Varía de 0 al doble |
| Nido | 6 000 | 9 000 | 12 000 | 18 000 | 1 material del Fresno y 1 extra |
| Jefe | 15 000 | 22 500 | 30 000 | 45 000 | 1 Injerto raro o astral y 1 material |
| Savia al vencer al Jefe | 30 | 45 | 60 | 90 | Directa, no pasa por el tope |
| Santuario, Caldero, Mercader | 0 | 0 | 0 | 0 |  |

**Escala de almitas.** Estos números usan la curva actual del GDD (Costo(n) = 100·n^1.5, sección 11.3). HUECOS.md (H-01) propone reescalar el nivel o los grados. Si cambia la curva, todo lo que está en almitas en este documento (nodos, precios, tope semanal y la paridad de la Savia) se multiplica por el mismo factor; por ejemplo, ×0.4 con 40·n^1.5. La Savia que se gana no cambia.

**Spark en el Fresno:** la sección 9.6 da "una almita extra" por Spark, que no se nota en esta escala. Propongo +1 % de las almitas del nodo por cada Spark, con tope de +10 % por combate.

### 12.2 Bonos de almitas: una sola suma

Todos los bonos se suman en un porcentaje y multiplican la base una sola vez:

| Bono | Valor |
| --- | --- |
| Maldiciones | +25 % cada una, hasta +125 % |
| Bendición del Paria | +10 % por cada 3★ y +5 % por cada 4★, hasta +40 % |
| Sendero de Contadora de Almas | +15 % (sección 13.3) |
| Raíz Mano de Contadora | Hasta +15 % |
| Paralaje alto o máximo | +20 % o +30 % |
| Luna llena real | +10 % (sección 7.7) |
| Fruto de una Rama del cielo de la semana | +10 % en esa run |

Ejemplo: tres maldiciones y dos brujas 3★ dan 75 % + 20 % = +95 %, es decir, ×1.95.

### 12.3 Tope semanal y Savia

- **Tope semanal: 600 000 almitas guardadas.** Cuenta lo que entra a tu cuenta: Santuarios, fin de la run y Manchas recuperadas. Lo que gastas en el Mercader nunca llega a contar.
- **Pasado el tope, las almitas se convierten en Savia al 30 %:** 3 de Savia por cada 1 000 almitas. Paridad de referencia: 1 de Savia vale 100 almitas.
- **Mancha absorbida: la misma tasa, 30 %** (sección 10.4). Una sola regla: "lo que el Fresno se queda, vuelve al 30 %". Si la tasa del tope fuera menor que la de la Mancha, a un jugador por encima del tope le convendría perder a propósito.
- **Savia directa:** 30, 45, 60 y 90 por cada Jefe vencido, 225 en una run completa. Algunos Eventos dan más.
- El tope se reinicia cada lunes, día de la Luna.

### 12.4 Sales de Baño semanales

Los 500 Sales de Baño semanales de la sección 16.3 salen de hitos por zonas superadas en la semana, en run completa o en Ascenso corto:

| Zonas superadas en la semana | Sales de Baño | Acumulado |
| --- | --- | --- |
| 2 | 100 | 100 |
| 4 | 100 | 200 |
| 6 | 150 | 350 |
| 8 | 150 | 500 |

Ocho zonas son dos runs completas u ocho Ascensos cortos: unas 2 horas y 10 minutos por semana en cualquiera de los dos casos. No dependen de maldiciones ni de llegar a la Copa, porque la sección 16.3 los cuenta como ingreso de un jugador promedio.

### 12.5 La cuenta

**Una zona típica.** Una ruta con 4 Combates, 1 Élite, 1 Evento, un Nido una de cada tres veces y el Jefe da, en Raíces:

4 × 3 000 + 8 000 + 3 000 + 6 000 ÷ 3 + 15 000 = **40 000 almitas**

**Una run completa sin bonos:**

40 000 × (1 + 1.5 + 2 + 3) = 40 000 × 7.5 = **300 000 almitas**, más **225 de Savia**, 4 materiales del Fresno, de 1 a 3 Sal Negra para tu inventario y unos 24 Injertos.

Según la ruta, varía entre 180 000 (sin Élites ni Nidos) y 390 000 (dos Élites y un Nido por zona).

**Contra la sección 11.3.** Subir una bruja del nivel 1 al 70 cuesta 1 669 000 almitas. La meta de hacerlo en siete días pide unas 238 000 almitas diarias, 1.67 millones por semana. El tope del Fresno (600 000) es el 36 % de eso: una fuente grande que no reemplaza el juego diario. Una run típica (300 000) paga los niveles 61 a 66 de una bruja (303 700), o dos veces y media el paso de A a S (116 640).

**Con maldiciones:**

| Maldiciones por zona | Almitas de una run | Runs para llenar el tope |
| --- | --- | --- |
| 0 | 300 000 | 2 |
| 3 | 525 000 | 1.15 |
| 5 | 675 000 | 0.9; sobran 75 000, que dan 225 de Savia |

En la práctica el Mercader se lleva del 10 al 20 % de lo ganado, así que sin maldiciones el tope se llena durante la tercera run.

**Contra la sección 16.3.** Los hitos del apartado 12.4 dan 500 Sales de Baño por semana, 3 000 por versión: lo mismo que la fila "Fresno semanal (500 × 6)". Los hitos del Tránsito (apartado 7.4) dan los 640 de la fila "Tránsito de temporada".

**Savia y Raíces.** Un jugador regular hace dos runs por semana (450 de Savia) y pierde una Mancha cada dos semanas. Por ejemplo, 50 000 almitas absorbidas dan 150 de Savia, unas 75 por semana. Con unos 525 a 600 por semana, completa las Raíces (8 100) en unas 13 a 15 semanas. Un jugador intensivo, con cinco runs y cinco maldiciones por semana, convierte cerca de 2.7 millones de almitas sobre el tope (unos 8 100 de Savia) y las completa en cerca de una semana. Es aceptable porque las Raíces son pequeñas; después su Savia necesita un sumidero (pregunta 13 del apartado 14.2).

### 12.6 Tiempo de una run

| Parte | Cantidad | Minutos |
| --- | --- | --- |
| Combates | 16 × 1.5 | 24 |
| Élites | 4 × 2.5 | 10 |
| Jefes | 4 × 4 | 16 |
| Eventos | 4 × 1 | 4 |
| Santuarios, Calderos, Mercaderes y Nidos | 12 × 0.5 | 6 |
| Mapa, maldiciones y elección de Injertos | 4 zonas × 1.5 | 6 |
| **Total** | 40 nodos | **66** |

Con más Élites o combates difíciles llega a 80 minutos: dentro de los 60 a 80 de la sección 10.1. Un Ascenso corto suma unos 16 minutos (4 Combates, 1 Élite, 1 Evento, el Jefe y 3 nodos de servicio), dentro de los 15 a 20 que pide el GDD.

## 13. Huecos y contradicciones del GDD

| # | Sección | Qué dice | El problema | Qué propone este documento |
| --- | --- | --- | --- | --- |
| 1 | 7.7 y 17.3 contra 10.7 y 22.1 | Los Tránsitos son "semanales" y favorecen planetas "cada semana" | En 10.7 y 22.1 el Tránsito cambia cada seis semanas | Tránsito de temporada más cielo de la semana, como propone META_Y_ONBOARDING.md, MC-02 (apartado 3.5) |
| 2 | 1 contra 10.1 | La sesión de "un anillo del Fresno" dura de 15 a 25 minutos | En 10.1 la unidad es la zona y el Ascenso corto dura de 15 a 20; "anillo" no aparece en otro lugar | Hablar de zonas y de 15 a 20 minutos |
| 3 | 10.1 contra 23.2 | Cada zona tiene 10 nodos | La 23.2 pide rutas que se bifurcan; con solo 10 nodos no hay entre qué elegir | 10 pisos de 3 carriles: 28 nodos en el mapa, 10 por ruta (apartado 9.1) |
| 4 | 10.3 | Cada maldición "multiplica las almitas por 1.25" | No dice si es ×3.05 o +125 % con cinco | Aditivo (apartado 6.1) |
| 5 | 9.6 y 23.1 contra 11.3 | El Spark da "una almita extra" | Los costos son de cientos a cientos de miles de almitas; una almita no se nota | +1 % de las almitas del nodo por Spark, tope +10 % (apartado 12.1) |
| 6 | 23.1 contra 10.3 y 10.7 | La Fase 1 emite diez eventos | El Fruto de Saturno y Saturno en Domicilio dependen del retraso, que no tiene evento | Cinco eventos nuevos (apartado 2.1) |
| 7 | 10.4 contra 23.2 | Recuperas la Mancha "si llegas a ese nodo" | El generador con semilla cambia el mapa en cada run; ese nodo ya no existe | Anclarla a zona y piso (apartado 9.8) |
| 8 | 10.4 y 11.4 | Hay tope semanal y "una tasa menor" pasado el tope | No dan ningún número | 600 000 almitas y 30 % (apartado 12.3) |
| 9 | 16.3 | "Fresno semanal: 500 Sales de Baño" | Ninguna sección dice cómo se ganan | Hitos por zonas superadas (apartado 12.4) |
| 10 | 10.3 | +1 opción de Injerto por cada 3★ | Sin tope: cuatro 3★ dan siete cartas en un teléfono. No dice si la Invitada cuenta | Tope de 5 opciones; la Invitada no cuenta (apartado 3.6) |
| 11 | 14.2 | Una Salmuera por planeta | Júpiter (Sal Estánnea) no tiene ninguna | Pregunta 12 del apartado 14.2 |
| 12 | 9.4 | Firmeza y Quiebre | No dice cuánta Firmeza quita cada golpe ni cuándo se recupera después del Quiebre | Escala de referencia (apartado 2.2); recuperación en el siguiente turno del enemigo, como HUECOS.md C-15 |
| 13 | 6.5 y 14.2 | Paralaje en bandas; el Agua de Olvido resta 20 | Las bandas no tienen números | Escala de 0 a 100 de HUECOS.md, H-15 (apartado 11.1) |
| 14 | 13.2 | Chequeos con umbrales fijos | Las estadísticas no tienen escala | Escala de 0 a 20 (apartado 9.9) |
| 15 | 10.5 | El Eco guarda 3 Injertos | Tres de la misma Rama darían un Fruto maduro desde el primer nodo | Máximo 2 por Rama y 1 astral (apartado 3.3) |
| 16 | 10.6 contra 23.2 | La Ofrenda desbloquea Ramas | La Fase 2 pide 7 Ramas sin decir cuáles empiezan abiertas | Orden por Altura sobre META_Y_ONBOARDING.md (apartado 3.4) |
| 17 | 10.2 | Élite: "ofrece una Mutación"; Santuario: "entrenar" | No dice si la Mutación se puede rechazar ni cuánto mejora el entrenamiento | 1 de 2 o ninguna (apartado 5); +15 % y +5 de Firmeza por nivel (apartado 9.3) |
| 18 | 10.3 | Savia Negra: "−30 % de curación" | No dice si es curación hecha o recibida | Recibida (apartado 5) |
| 19 | 7.4 | Tras el Éxtasis, la bruja queda "Desalada un turno" | El estado Desalada no se define en ninguna sección | HUECOS.md C-08 ya lo define; este catálogo no lo usa |
| 20 | 17.4 | Los trucos de las brujas "kk" están protegidos | No prohíbe que un Injerto los copie | Regla 7 del catálogo (apartado 1) |
| 21 | 13.3 contra 10.3 | Contadora de Almas da +15 % de almitas | No dice cómo se combina con el Paria y las maldiciones | Una sola suma (apartado 12.2) |

## 14. Valores TUNING y decisiones pendientes

### 14.1 Valores TUNING

Todos los números de las tablas de los apartados 4 a 11 son TUNING, salvo los que cita el GDD: el Fruto de Saturno, Savia Negra, los dos Tránsitos del GDD, el 30 % de la Mancha, el descanso del Santuario y los 500 y 640 Sales de Baño. Los valores de sistema que más pesan:

| Valor | Propuesta | Apartado |
| --- | --- | --- |
| Pesos de rareza de Injertos por zona | 70/25/5, 60/30/10, 50/35/15 y 40/40/20 | 3.1 |
| Tope de Frutos activos | 2 | 3.2 |
| Límites del Eco | 2 por Rama y 1 astral | 3.3 |
| Orden de las Ramas por Altura | Venus 12, Saturno 16, Mercurio 20 y Júpiter 25 | 3.4 |
| Bono del cielo de la semana | +10 % de almitas | 3.5 |
| Tope de opciones del Paria | 5 | 3.6 |
| Valores de los estados | Quemadura 15 %, Sangrado 20 %, Veneno 12 %, Encanto −30 %, Electrocución 20 % | 2.2 |
| Escala de Firmeza | Básico 10, Hechizo 20, Cierre 30; normales de 40 a 100, Élites de 150 a 260, Jefes de 400 a 600 | 2.2 y 11 |
| Rejilla del mapa | 3 carriles, 10 pisos, 50 % de conexión diagonal | 9.1 |
| Reparto de nodos por zona | Tabla del apartado 9.1 | 9.1 |
| Entrenamiento del Santuario | +15 % de daño y +5 de Firmeza por nivel, máximo 2 | 9.3 |
| Cargas por ranura en la run | 3 | 9.4 |
| Precios del Mercader | Tabla del apartado 9.5 | 9.5 |
| Escala de estadísticas y umbrales | 0 a 20; umbrales 7, 10, 13 y 16; tope de +4 por brujas | 9.9 |
| Persistencia | Revivir con 10 %, −20 de Salinidad por combate, tope de 140 al empezar, Ánima conservada hasta 100, Sal Negra guardada hasta 3, +25 % de vida tras un Jefe | 10 |
| Nivel de enemigos por zona | 45, 52, 60 y 70 | 11.1 |
| Bonos de Paralaje alto y máximo | +20 % y +30 % de almitas | 11.1 |
| Almitas base y multiplicador por zona | Tabla del apartado 12.1; ×1, ×1.5, ×2 y ×3 | 12.1 |
| Almitas por Spark | +1 % por Spark, tope +10 % | 12.1 |
| Luna llena | +10 % de almitas | 12.2 |
| Tope semanal | 600 000 almitas | 12.3 |
| Savia por Jefe | 30, 45, 60 y 90 | 12.3 |
| Costos de las Raíces | 8 100 de Savia en total | 8 |
| Hitos semanales de Sales de Baño | 2, 4, 6 y 8 zonas | 12.4 |

### 14.2 Preguntas para el diseñador

1. **¿Las maldiciones suman o multiplican?** Recomiendo que sumen: +25 % cada una (apartado 6.1).
2. **¿Cuántos Frutos puede haber activos?** Recomiendo 2, eligiendo al madurar el tercero.
3. **¿La tasa pasado el tope es igual a la de la Mancha?** Recomiendo que sí, 30 %. La alternativa es 10 %, para que las Raíces duren más a los jugadores intensivos, pero así perder a propósito daría más Savia que ganar.
4. **¿El tope semanal queda en 600 000 almitas?** Recomiendo que sí, revisándolo con datos del lanzamiento suave: es el 36 % del costo semanal de la meta de la sección 11.3.
5. **¿En qué orden se abren las Ramas?** META_Y_ONBOARDING.md fija Sol, Luna y Marte al inicio y cuatro Alturas para las demás. Recomiendo Venus, Saturno, Mercurio y Júpiter, en ese orden.
6. **¿La Bendición del Paria tiene tope y cuenta a la Invitada?** Recomiendo tope de 5 opciones, Relevos para la tercera y cuarta 3★, y que la Invitada no cuente.
7. **¿El Eco puede traer 3 Injertos de la misma Rama?** Recomiendo que no: máximo 2 por Rama y 1 astral.
8. **¿Vida y Salinidad persisten entre nodos?** Recomiendo que sí, con −20 de Salinidad por combate, tope de 140 al empezar y revivir con 10 % de vida tras ganar.
9. **¿El equipo queda fijo durante la run?** Recomiendo que sí.
10. **¿La Mancha recuperada se guarda al instante?** Recomiendo que sí: el castigo ya fue perderla una vez.
11. **¿Los bonos de almitas, incluido el Paria, cuentan dentro del tope?** Recomiendo que sí. Dentro del tope, el bono del Paria ahorra tiempo; su ventaja real son las opciones extra de Injerto.
12. **¿Se agrega una Salmuera de Júpiter a la sección 14.2?** Recomiendo que sí: *Salmuera Estánnea* (Sal Estánnea y agua termal), +20 de Resonancia. Mientras no exista, `jupiter_05` usa la Esencia de Alma.
13. **¿Qué hace la Savia cuando las Raíces están completas?** Recomiendo un canje semanal limitado: 100 de Savia por 1 Sal Negra, máximo 3 por semana, más decoraciones del Fresno para las Termas.
14. **¿Los materiales del Fresno (Raíz Negra, Corteza de Ceniza, Pluma de Eclipse y Hoja Astral) entran al crafting?** Recomiendo que sí, en las armas Astrales y Quintaesencia, para conectar el Fresno con la sección 15.
15. **¿Se adopta la regla de no favorecer el planeta de la Regente limitada de la versión?** Recomiendo que sí; protege la promesa de la sección 16 de no vender poder.
16. **¿Cuál es la escala de estadísticas de la Calculista?** Recomiendo de 0 a 20, con los umbrales del apartado 9.9. Afecta también a la historia de la Fase 3.
17. **¿Se confirma C-15 de HUECOS.md (la Firmeza se recupera en el siguiente turno del enemigo)?** Recomiendo que sí: varios Injertos y la Élite Despierta lo asumen.
18. **¿Qué escala de almitas se adopta (HUECOS.md, H-01)?** El Fresno se ajusta con un solo factor (apartado 12.1). Recomiendo decidirla antes de la Fase 2A, porque cambia el tope semanal y los precios.
