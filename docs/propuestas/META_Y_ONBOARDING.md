# Propuesta: meta-juego, primera sesión y bucle diario

Propuesta por aprobar · 5 de noviembre de 2026 · diseño de producto y retención

> **Estado: propuesta, no aprobada.** El GDD manda. Mientras no se apruebe, los agentes pueden usar estos valores como base, marcados `// TUNING` (CLAUDE.md y decisión D-07 de la hoja de ruta).

**Qué amplía del GDD:** sección 2 (primeras sesiones), 6 (Calculista y región inicial), 6.5 (Paralaje y baño), 7.7 (día planetario y fase lunar), 9.1 y 9.9 (Invitada y desbloqueo progresivo), 10.6 (Ofrenda y nivel de cuenta), 11.1 (fuentes de Sales de Baño), 13.4 (Títulos), 16 (pase, tarjeta, tienda y cumplimiento) y 22 (eventos y KPIs).

**Qué huecos cierra:** H-18 a H-26 y H-30 de [HUECOS.md](../HUECOS.md), más S-16 y S-19. Se alinea con C-04, C-06, C-17, C-24, C-25, C-27 y H-15.

**Cómo leer las referencias:** los números solos son secciones del GDD; los que llevan § son de esta propuesta; C-, H-, S- y D- son IDs de HUECOS y de la hoja de ruta; MP- y MC- son las preguntas y contradicciones de §16.

**Abreviaturas:** SB = Sales de Baño. Altura = nivel de cuenta (§4). Los nombres de las 4★ y 3★ son los oficiales del GDD 18 (D-03): Cempa (coyota), Topacia (topo), Muertita (zarigüeya), Axa (ajolote), Salitre (cabra) y Brea (gata negra). La jaguar y la lince todavía no tienen nombre.

## Lo más importante

- **La primera Cadena llega en el minuto 2.** El juego abre en plena pelea, antes de crear a la Calculista. La Partitura empieza sin Spark y el jugador lo encuentra al mover a una bruja. El combate no termina sin un Spark.
- **La Calculista se crea dentro del Libro Mayor** en el minuto 3: "Alma sin folio. Escríbete." Tarda de 1 a 3 minutos.
- **Las 3 200 SB del banner de inicio salen del juego, no de la preinscripción:** correo de bienvenida, Prólogo, primeras cuatro misiones, seis logros y Altura 2 a 5. Se juntan hacia el minuto 65.
- **Primera 5★: meta de 2 horas de juego y red de seguridad el día 2.** Si el día 2 te faltan Sales, el juego te adelanta lo que ganarías en la primera hora. No regala nada nuevo.
- **Sin energía:** el tope está en la recompensa, nunca en el tiempo de juego. Las salinas dan tres Cosechas al día que se acumulan hasta nueve.
- **Encargos diarios de 3 a 4 minutos:** cuatro encargos de 15 SB, uno solo con combate. Ninguno pide tirar, pagar ni tener una bruja del gacha.
- **El nivel de cuenta (Altura) sube con todo lo que juegas,** y la Ofrenda al Fresno lo acelera. Si solo subiera con la Ofrenda, el jugador nuevo tendría que elegir entre su cuenta y sus brujas.
- **Reinicio a las 04:00 del servidor:** Américas en UTC−6 fijo (Ciudad de México todo el año) y Asia en UTC+9 (Japón y Corea). El domingo de Sol abre su salina y abarata la transmutación de sales.
- **El Grimorio dura una versión (42 días) y su carril premium solo trae Sales y cosméticos.** La tarjeta da unas 26 tiradas extra por versión y el pase premium 8. Las 70 gratis no se tocan.
- **Sin chat libre al lanzamiento.** La Invitada, los sellos y las visitas a las Termas cubren lo social sin moderar texto en tres idiomas.
- **Confianza por bruja sin poder:** voces, recuerdos, poses y colores. Nada de lo que da Confianza se vende, y ningún cosmético de pago muestra más piel que el atuendo gratis más revelador de esa bruja.

## 1. Principios

1. **El tope está en la recompensa, no en el tiempo.** Siempre puedes jugar. Pasado el tope, el combate sigue dando práctica y algo de almitas.
2. **Perder un día no te castiga de más.** Las Cosechas, el desafío de evento y el calendario de llegada se acumulan. El Grimorio deja un margen del 20 %.
3. **Ninguna misión ni logro pide tirar, pagar ni juntar brujas del gacha** (sección 2, *kompu gacha*). Lo que pide un planeta o un papel se cumple con brujas gratis o con la Invitada.
4. **Todo lo que se vende es una tirada o un cosmético** (16.4). Ni el pase premium ni la tarjeta traen almitas, sal, materiales, Savia ni niveles comprados.
5. **Un sistema nuevo, una regla, un botón.** Cada desbloqueo se presenta con una frase del Libro Mayor y una sola acción guiada.
6. **El reloj es del servidor.** Día planetario, luna y reinicios entran por `IClock` (hoja de ruta 2.3). Cambiar la hora del teléfono no cambia nada (S-05).
7. **Las Termas son zona segura** (6.5). Ahí no hay horror, ni sustos, ni ventas emergentes.

## 2. Primera sesión (FTUE)

### 2.1 Los primeros 30 minutos

Cada paso tiene un ID (F01 a F21) que usa la analítica (sección 14).

| Paso | Minuto | Qué ve | Qué decide | Qué aprende o se desbloquea |
| --- | --- | --- | --- | --- |
| F01 | 0:00–0:25 | Logo, fecha de nacimiento, términos y privacidad, idioma de voz (por defecto el del teléfono) | Idioma de voz | La fecha fija los límites de gasto por edad (§12.5). Sin crear cuenta: inicio anónimo (S-01) |
| F02 | 0:25–0:45 | Cinemática *La caída* (20 s, saltable): un alma cae del Río Lácteo, roza la copa del Fresno y aterriza en el caparazón de la Tortuga en plena pelea | Nada | El tono: canto en Salmódico, melancolía, el Fresno en el horizonte |
| F03 | 0:45–1:30 | Combate 1. Cempa, Topacia y Muertita contra un Cristalizado del Vado. Una indicación por turno | Objetivo; básico o Hechizo | Ataque básico, Hechizo con ritual de sal, Granos de Sal, velocidad ×2 |
| F04 | 1:30–2:20 | El Cristalizado muerde el caparazón. La Resonancia se llena por guion. El Libro Mayor se abre solo: "Calcula." | Ordenar a las tres en la Partitura hasta ver una nota dorada | Cadena, Partitura, papeles (Apertura, Enlace, Cierre) y Spark |
| F05 | 2:20–3:00 | Cadena silente de 5 s, campana del Spark, Quiebre y victoria. Las almitas flotan hacia el Fresno | Nada | Tres logros en un solo folio de resumen: 240 SB |
| F06 | 3:00–6:00 | El Libro Mayor no encuentra a la Calculista: "Alma sin folio. Escríbete." | Especie, cuerpo, voz, pronombres, nombre y signo (§2.3) | Libro Mayor y tu propia ventana de estado |
| F07 | 6:00–8:00 | Charla en el caparazón con la tripulación y la Herbolaria. La Tortuga pone rumbo a tu región | Una respuesta con chequeo visible de Encanto o Cálculo, imposible de fallar | Chequeos de umbral fijo y primera marca de Destino |
| F08 | 8:00–10:30 | Combate 2: dos enemigos con Firmeza y planetas débiles | A quién quebrar primero | Firmeza, debilidades y Quiebre, ya explicados. Al final guardas tu primera Partitura y aparece el botón Auto |
| F09 | 10:30–14:00 | Jefe del Prólogo: el Barquero Cristalizado, medio hecho sal, que susurra nombres | Cuándo lanzar la Ascensión I | Ánima y Ascensión I (logro, 80 SB). En el Prólogo no se puede perder: la vida no baja de 1 |
| F10 | 14:00–16:00 | Llegada a las Termas. Las brujas entran al manantial en bata de baño; tu favorita reposa en la orilla | Tocarla y conversar | Termas: reposo, tocar y conversar |
| F11 | 16:00–19:00 | Buzón con el correo de bienvenida (1 600 SB). El manantial muestra el banner de inicio | Qué Regente marcar entre cuatro (con prueba opcional de 60 s de cada una). Primera tirada de diez | Gacha, termómetro de pity, vapor por rareza y exclusión: "Cempa, Topacia y Muertita ya viajan contigo: no pueden salir" |
| F12 | 19:00–22:00 | Equipo de cuatro con la 4★ nueva | A quién subir de nivel (hay botón de "recomendado") | Nivel de bruja y almitas |
| F13 | 22:00–30:00 | Capítulo de llegada, misión 1: la Tortuga atraca en tu región. Mapa ilustrado con puntos de interés (D-01), barriles con almitas y una almita con nombre | Una decisión con chequeo visible | Exploración, almitas con nombre y primer Título: *Recién Caída* (en tres variantes, ver §8.3) |

### 2.2 De los 30 minutos a las 2 horas

| Paso | Minuto | Dónde | Qué pasa | Qué se desbloquea |
| --- | --- | --- | --- | --- |
| F14 | ~30–40 | Misión 2 | Una bruja pasa a Salobre con un aviso del Libro Mayor | Salinidad: barra, bandas y cómo bajarla (logro, 80 SB). Cumple la 9.9: primera hora |
| F15 | ~40–45 | Altura 5 | Aparece el tablero con cuatro encargos; uno ya está cumplido | Encargos diarios. La tienda se ve completa, sin ventanas emergentes |
| F16 | ~45–55 | Misión 3 | Primera Salmuera en el Caldero; Cempa usa su Autohechizo | Caldero, cinturón y Autohechizo con su costo de Salinidad (logro, 80 SB) |
| F17 | ~55–65 | Misión 4 | Llega una Invitada de la caravana (NPC) | Invitada, Regencia y Cadena de cinco eslabones. Con esta misión llegas a 3 200 SB |
| F18 | ~65–70 | Termas | Segunda tirada de diez. La Regente elegida sale a más tardar en la tirada 20, con la emergencia dorada completa | **Primera 5★.** Después: vincular la cuenta y pedir permiso de notificaciones, con explicación |
| F19 | ~70–95 | Misión 5 | Jefe del capítulo: primer encuentro con la Casa de la región. La Regente debuta | Ascensión II |
| F20 | ~95–110 | Exploración libre | Altura 8. El esqueje del Fresno que crece en la Tortuga pide su primera Ofrenda | Ofrenda (la primera vale doble), Talentos de Cadena y Dignidad |
| F21 | ~110–120 | Termas | El Fresno "llama" desde el horizonte. La Herbolaria anuncia el Yunque | Cierre natural: "Mañana hay encargos nuevos y el Fresno se abre en Altura 10" |

La sesión termina con dos razones para volver: encargos nuevos y el Fresno a un nivel de distancia.

### 2.3 Creación de la Calculista

| Paso | Opciones | Reglas |
| --- | --- | --- |
| Especie | 12 al lanzamiento: lobo, zorro, gato, liebre, oso, ciervo, cuervo, murciélago, ajolote, coyote, serpiente y polilla | Cada especie se elige en dos escalas: antropomorfa completa o kemonomimi (18.1). Así entra quien no viene del fandom furry |
| Cuerpo | 4 complexiones; deslizadores de estatura y constitución; pelaje o piel, ojos y peinado | Los deslizadores tienen mínimos adultos. Ninguna combinación da rasgos infantiles (sección 5) |
| Voz | 4 voces por idioma (dos agudas, dos graves) y un control de tono | Independiente del cuerpo y de los pronombres |
| Pronombres | Ella, él o elle | Cambian textos y Títulos. Se cambian gratis cuando quieras |
| Nombre | De 2 a 12 caracteres, con filtro en tres idiomas | Lleva una etiqueta de 4 dígitos para no exigir nombres únicos |
| Signo ascendente | Los 12 signos, con una frase de carácter cada uno. Ayuda opcional: "calcula mi signo" con fecha, hora y lugar | El cálculo corre en el teléfono y solo se guarda el signo. Define la región inicial y da +1 a una estadística por elemento |

- **Elemento del signo:** Fuego (Aries, Leo, Sagitario) +1 Temple; Tierra (Tauro, Virgo, Capricornio) +1 Oficio; Aire (Géminis, Libra, Acuario) +1 Cálculo; Agua (Cáncer, Escorpio, Piscis) +1 Encanto.
- **Región inicial:** si salen las 12 regiones (la hoja de ruta las reparte entre la 3B y la Fase 6), empiezas en la de tu signo. Si salen menos, recomiendo las cuatro **cardinales**, una por elemento: Aries, Cáncer, Libra y Capricornio. Tu signo te lleva a la de su elemento, y cuando salga tu región exacta te espera un capítulo de "regreso a casa" (pregunta MP-01).
- **Dos caminos:** *Rápido* (un preset y tu signo, 1 minuto) o *Detallado* (hasta 3 minutos). Todo se edita gratis después en el Espejo de las Termas, menos el signo, que se fija al empezar el Capítulo de llegada.

### 2.4 Primer combate guionizado

- **Equipo:** Cempa (4★, Mercurio y Sol), Topacia (3★, Saturno y Marte) y Muertita (3★, Saturno y Luna). Las tres son reclutables gratis y se quedan contigo. Cempa, la embaucadora del folclor mexicano, abre el juego en el mercado del lanzamiento suave.
- **Enemigo:** el Cristalizado del Vado, débil a Saturno y Mercurio. Horror contenido: una figura de sal que todavía susurra (6.5).
- **Orden inicial de la Partitura:** Cempa → Muertita → Topacia, sin Spark.
- **Orden que buscas:** Muertita → Topacia → Cempa. Muertita y Topacia forman **Conjunción** (las dos son Saturno: +25 % al siguiente eslabón). Topacia y Cempa forman **Sextil** (Mercurio). El último golpe del Enlace de Topacia y el primero del Cierre de Cempa caen a 2 frames o menos: **Spark**. No hay Oposición, así que no hay Reacción Híbrida ni Salinidad que explicar todavía.
- **Pistas:** a los 15 s el Libro Mayor dice "Hay un orden donde dos golpes caen juntos". A los 30 s aparece una flecha fantasma que lleva a Cempa al Cierre.
- **Garantía:** si confirmas un orden sin Spark, el Cristalizado aguanta y la Resonancia se llena otra vez por guion. La segunda Cadena trae la flecha fija. El combate no termina sin un Spark.
- **Salinidad oculta:** el núcleo no la sube hasta F14 (bandera de tutorial en los datos del Prólogo). Así no hay efectos invisibles antes de explicarla.
- **Auto:** aparece al final del combate 2, después de una Cadena hecha a mano. Usa la Partitura que guardaste. Es un ajuste a la 9.9 (MC-05).

### 2.5 Primera tirada

Reglas del banner de inicio (amplían 16.2 y C-24):

| Regla | Valor |
| --- | --- |
| Tiradas | 20 como máximo, a 160 SB cada una |
| 5★ | Una sola, la Regente que marcaste entre las cuatro del banner estándar. Sale a más tardar en la tirada 20 |
| Tasas antes de la 20 | 0.6 % de 5★, como cualquier banner. Si la 5★ sale antes, las tiradas que quedan ya no tienen 5★, y la página de probabilidades lo dice |
| 4★ | Garantizada cada 10 tiradas, con exclusión de las que ya tienes |
| Contador | Propio; no pasa a otros banners |
| Cambiar la elegida | Gratis hasta que salga |
| Resultados guionizados | Ninguno. Lo único garantizado es lo que dice la página de probabilidades |

La primera tirada se hace de diez. La emergencia completa se ve la primera vez y se puede saltar siempre (16.1). Los banners limitado y estándar se ven desde F11, pero el de inicio va primero y con la tarjeta más grande.

### 2.6 Cuenta de Sales de Baño del inicio

| Fuente | Cuándo llega | SB | Tiradas | ¿Para todas las cuentas? |
| --- | --- | --- | --- | --- |
| Correo de bienvenida | F11, minuto 16 | 1 600 | 10 | Sí, siempre |
| Historia: Prólogo (160) y misiones 1 a 4 del Capítulo de llegada (4 × 160) | Minutos 14 a 65 | 800 | 5 | Sí |
| Seis logros de *Primeros pasos* (6 × 80, §8.2) | Minutos 3 a 50 | 480 | 3 | Sí |
| Recompensas de Altura 2 a 5 (4 × 80) | Minutos 14 a 45 | 320 | 2 | Sí |
| **Total del inicio** | **Hacia el minuto 65** | **3 200** | **20** | |
| Preinscripción, por metas de registro | Con el correo de bienvenida | Hasta 1 600 | Hasta 10 | No: solo cuentas del lanzamiento |

- **Comprobación de tiempos.** En F11 tienes al menos 2 160 SB: correo 1 600, Prólogo 160, cuatro logros 320 y Altura 2 con 80. Tras la primera tirada te quedan 560. Las misiones 1 a 4, dos logros y Altura 3 a 5 suman las 1 040 que faltan. La tirada 20 cae en F17 o F18.
- **La preinscripción no cuenta para el inicio.** Quien instala en el mes 3 no la tiene, así que el inicio no puede depender de ella. Quien sí la tiene puede hacer las 20 tiradas en el minuto 16.
- **Sin regalar de más.** Las 3 200 SB del inicio son el 29 % de una versión gratuita (11 200). La preinscripción, el 14 %.

### 2.7 Primera 5★: día 2 contra 2 horas

La sección 2 dice "garantizada antes del día 2"; la 22.3 y el prompt 23.3 dicen "antes de 2 horas de juego" (C-04). **Recomendación:** las dos, con funciones distintas.

1. **Meta de diseño: minuto 65 de juego.** El KPI de la 22.3 se mide en tiempo de juego: el 90 % de las cuentas con 2 horas jugadas ya tiene su 5★.
2. **Red de seguridad: el Adelanto del día 2.** El día de calendario no se puede garantizar con tiempo de juego, porque depende de cuánto juegues. Así que el día 2 de servidor, si ya pasaste F11 y te faltan SB para la tirada 20, el calendario de llegada te adelanta la diferencia (como máximo 1 040 SB). Las recompensas de historia, logros y Altura que la cubrían quedan marcadas como cobradas.
3. **Redacción propuesta para la sección 2:** "La primera bruja 5★ llega antes de 2 horas de juego y, para quien vuelve el día 2, ese mismo día."

Así se cumple la 22.3 y la sección 2 deja de ser una promesa imposible de medir.

### 2.8 Después de las 2 horas: la primera semana

| Momento | Altura | Qué llega |
| --- | --- | --- |
| Día 1, segunda sesión | 10 | Fresno: Ascenso corto en Raíces Putrefactas, Savia y Raíces. Yunque con una primera arma Forjada que mueve frames y crea un Spark nuevo. Grimorio y regalos |
| Día 2 | 12 | Salinas del día, Altar del Árbol para refinar sal, calendario lunar y segunda región |
| Día 3 | 14–15 | Juicio I (grado C). Amigos e Invitadas de jugadores. Primer eclipse tras dos regiones: aparece el Paralaje y el baño que olvida |
| Días 4–5 | 16–18 | Senderos. Vislumbre con la misión *La primera Sal Negra*. Reforja. Grieta de luna nueva si toca |
| Día 7 | 20 | Fresno completo, Eco y Tránsito de temporada. Primera semana de encargos semanales |
| Semana 2 | ~25 | Grado B con Vía Mayor o Vía Menor. Termas de amigos |
| Semanas 3–4 | ~30 | Grado A |
| Semanas 6–8 | ~38 | Grado S y Ascensión III |

**Calendario de llegada.** Siete días de entrada, no seguidos, dentro de los primeros 30 días.

| Día | Recompensa |
| --- | --- |
| 1 | 20 000 almitas |
| 2 | 160 SB y, si hace falta, el Adelanto (§2.7) |
| 3 | 10 Sal Cruda a elegir |
| 4 | 160 SB |
| 5 | 3 Sal Negra |
| 6 | Color de sal *Primera Sal* (cosmético) |
| 7 | 320 SB y marco del Libro Mayor *Folio nuevo* |

### 2.9 Reserva de cuenta nueva

Lo que una cuenta nueva gana una sola vez, además de las 70 tiradas por versión de la 16.3.

| Fuente | SB | Tiradas | Cuándo |
| --- | --- | --- | --- |
| Inicio (§2.6) | 3 200 | 20 | Primera hora |
| Calendario de llegada | 640 | 4 | Primera semana |
| Regiones de lanzamiento: 800 por región (5 misiones × 80 y 400 de exploración), 11 regiones más el resto de la inicial | ~9 300 | ~58 | Meses 1 a 3 |
| Resto de los logros de lanzamiento | 4 320 | 27 | Meses 1 a 6 |
| **Total** | **~17 400** | **~109** | **Una sola vez** |

- Las misiones 1 a 4 de la región inicial pagan 160 porque son el tutorial. Las demás pagan 80.
- Con 12 regiones al lanzamiento, la reserva vale cerca de 1.5 versiones gratuitas, repartida en meses. Si salen solo 4 regiones, baja a unas 70 tiradas.
- Quien llega en versiones posteriores encuentra más historia acumulada. Es bueno: ayuda a ponerse al día.

## 3. Desbloqueo progresivo

| Sistema | Cuándo | Por qué ahí |
| --- | --- | --- |
| Ataque básico, Hechizo y Granos de Sal | F03, minuto 1 | Lo mínimo para jugar un turno |
| Velocidad ×2 | F03 | 9.9: nadie espera animaciones |
| Cadena, Partitura y Spark | F04, minuto 2 | Es el gancho central y el KPI de la sección 2 |
| Quiebre | Visto en F05, explicado en F08 | Primero el espectáculo, después la regla |
| Libro Mayor | F06, minuto 3 | La creación ocurre dentro de él |
| Chequeos de estadística y marcas de Destino | F07, minuto 7 | La historia no lineal se presenta temprano y sin riesgo |
| Combate automático | F08, minuto 10 | Después de una Cadena hecha a mano |
| Ascensión I | F09, minuto 12 | Clímax del Prólogo |
| Termas: reposo, tocar y conversar | F10, minuto 14 | La base aparece justo antes de la tirada, que ocurre ahí |
| Gacha, correo, vestidor y menú de logros | F11, minuto 16 | El correo de bienvenida paga la primera tirada |
| Nivel de bruja | F12, minuto 20 | Primer sumidero de almitas |
| Títulos | F13, minuto 28 | Llegan con la primera decisión |
| Salinidad | F14, minuto 35 | 9.9: dentro de la primera hora, cuando ya entiendes la Cadena |
| Encargos diarios | Altura 5, minuto 40 | Dan una razón concreta para volver el día 2 |
| Caldero y Salmueras | F16, minuto 50 | Las pociones bajan la Salinidad: llegan justo después de ella |
| Autohechizo | F16, minuto 50 | Cuesta Salinidad; sin ella no se entiende |
| Invitada de la caravana (NPC) | F17, minuto 60 | Cadena de cinco eslabones; prepara lo social |
| Ascensión II | F19, minuto 80 | Jefe del primer capítulo |
| Ofrenda, Talentos de Cadena y Dignidad | Altura 8, minuto 100 | Sumideros de largo plazo cuando ya sobran almitas |
| Fresno: Ascenso corto, Savia y Raíces | Altura 10, día 1 o 2 | El endgame se prueba pronto y en 15 a 20 min. La Savia enseña que perder también deja algo |
| Crafting en el Yunque | Altura 10 | La primera arma Forjada demuestra que el arma mueve frames y crea un Spark |
| Grimorio | Altura 10; la experiencia cuenta desde el día 1 | Cuando ya hay encargos que lo alimentan |
| Regalos | Altura 10 | Hace falta tener algo que regalar |
| Salinas del día y refinar en el Altar del Árbol | Altura 12, día 2 | El grado C pide sal refinada |
| Calendario lunar | Altura 12 | La luna llena aplica desde el día 1; se explica cuando ya hay salinas |
| Juicios de Pureza | Altura 14 y bruja en nivel 40 (C-06) | Primera subida de grado, hacia el día 3 |
| Amigos, sugeridos e Invitadas de jugadores | Altura 15, día 3 | Cuando tu equipo ya vale la pena prestarlo |
| Paralaje y baño que olvida | Primer eclipse (tras dos regiones), día 3 | El primer vistazo de la Grieta (6.5). Antes, el Paralaje corre oculto y en banda Baja no cambia nada |
| Senderos | Altura 16 | Cambian el combate; mejor cuando ya dominas lo básico |
| Vislumbre | Altura 18, misión *La primera Sal Negra* (3 Sal Negra) | 9.9: "más adelante". Cuesta Sal Negra, que aún no tenías |
| Reforja | Altura 18 | Afinar el Spark tiene sentido con varias armas |
| Grieta de luna nueva | Altura 18 | Da Sal Negra; llega con el Vislumbre |
| Fresno completo, Eco y Tránsito | Altura 20, día 7 | Run de 60 a 80 min cuando ya hay equipo |
| Grado B: Vía Mayor o Vía Menor | Juicio II, bruja en nivel 50, semana 2 | Primera decisión de especialización (7.2) |
| Termas sociales (visitas a amigos) | Altura 25, semana 2 | Social ligero cuando ya hay lista de amigos |
| Ascensión III | Grado S, semanas 6 a 8 | 9.7 |
| Relectura | Al terminar la historia principal (13.5) | Las decisiones pesan más si no se deshacen pronto (MP-11) |
| Tienda de Perlas y cosméticos | Visible desde F11; sin ventanas emergentes hasta el día 2 | Transparencia sin presión en la primera sesión |
| Diezmo de Sal y Grimorio premium | Visibles en la tienda desde Altura 5 y 10 | Se ofrecen cuando el jugador ya sabe qué compra |

## 4. Nivel de cuenta: la Altura

El Fresno crece cuando liberas almas. Cada misión, encargo, Juicio o zona del Fresno libera almas y suma **Crecimiento**. La Ofrenda al Fresno (10.6) también suma, y es la fuente que nunca se llena.

**Por qué no solo la Ofrenda:** el Fresno es endgame, y antes de abrirlo la cuenta no subiría. Además, la Ofrenda gasta almitas, así que el jugador nuevo tendría que elegir entre subir su cuenta o sus brujas en el peor momento (MC-07). El esqueje del Fresno en la Tortuga permite ofrendar desde las Termas.

### 4.1 Fuentes de Crecimiento

| Fuente | Crecimiento |
| --- | --- |
| Prólogo | 1 200 |
| Misión de historia | 400 |
| Encargo diario | 100 cada uno y 100 por terminar los cuatro (500 al día) |
| Encargo semanal | 300 cada uno (1 800 por semana) |
| Logro | 50 a 200 |
| Zona del Fresno terminada | 200 |
| Juicio ganado | 500 |
| Primera victoria en un nivel de salina | 200 |
| Préstamo de tu Invitada | 20 por uso, tope de 400 al día |
| Ofrenda | 1 por cada 100 almitas; la primera del día vale doble hasta 20 000 almitas |

### 4.2 Curva

| Altura | Crecimiento por nivel | Acumulado | Momento típico sin pagar |
| --- | --- | --- | --- |
| 1 a 10 | 600 | 5 400 | Día 1 o 2 |
| 10 a 20 | 2 400 | 29 400 | Día 7 |
| 20 a 30 | 5 000 | 79 400 | Día 21 |
| 30 a 40 | 8 000 | 159 400 | Día 50 |
| 40 a 50 | 14 000 | 299 400 | Día 100 |
| 50 a 60 | 20 000 | 499 400 | Día 180 |

Supone de 2 500 a 3 500 de Crecimiento al día para quien juega diario.

### 4.3 Qué da la Altura

| Altura | Desbloquea |
| --- | --- |
| 2 a 5 | 80 SB por nivel (parte del inicio) |
| 5 | Encargos diarios |
| 8 | Ofrenda, Talentos de Cadena y Dignidad |
| 10 | Fresno (Ascenso corto), Yunque, Grimorio y regalos |
| 12 | Salinas (nivel I), Altar del Árbol, calendario lunar y Marea 1 |
| 14, 15, 16, 18 | Juicios; amigos; Senderos; Vislumbre y reforja |
| 20 | Fresno completo, Eco, Tránsito, salina II y Marea 2 |
| 25 | Termas de amigos |
| 28, 36, 44, 52 | Salinas III a VI |
| 30, 40, 50 | Marea 3, 4 y 5 |
| Cada 10 | Una decoración de las Termas y un fragmento de historia (10.6) |
| 60 | Ofrenda perpetua (§4.4) |

- **Ramas nuevas del Fresno (10.6):** empiezas con tres Ramas de Injertos (Sol, Luna y Marte). Las otras cuatro llegan en Altura 12, 16, 20 y 25.
- **Bendición del Fresno (bono de poder con tope, 10.6):** +1 % de ATQ y VIDA dentro del Fresno por cada 10 de Altura, tope de +5 % en Altura 50.
- **Amigos:** tope de 30 en Altura 15, 40 en Altura 30 y 50 en Altura 45.
- **Marea de Sal (nivel de mundo, MP-10):** las regiones se juegan en cualquier orden, así que sus enemigos y recompensas escalan con la Marea, de 0 a 5. Puedes bajarla un nivel cuando quieras.

### 4.4 Hitos de Ofrenda

Además del Crecimiento, la Ofrenda lleva un contador de por vida. Cada 500 000 almitas ofrendadas hay un hito: una decoración, un fragmento de historia o 20 Polvo Astral (11.1). Pasada la Altura 60, la **Ofrenda perpetua** sigue dando hitos. Es el sumidero que nunca se llena (11.4).

## 5. Bucle diario y semanal

El día y la semana cambian a las 04:00 del servidor; la semana empieza el lunes, día de la Luna (detalle en §6.6).

### 5.1 Encargos diarios

- **Cuatro encargos al día.** Cada uno da 15 SB, 100 de Crecimiento, 100 de experiencia del Grimorio y 5 000 almitas.
- **Al terminar los cuatro:** +100 de Crecimiento, +100 de experiencia y +10 000 almitas.
- **Total diario:** 60 SB (16.3), 500 de Crecimiento, 500 de experiencia y 30 000 almitas.
- **Pago parcial:** cada encargo paga solo, así que hacer tres de cuatro no es perder el día.
- **Tiempo objetivo:** de 3 a 4 minutos (sección 1).

### 5.2 Catálogo de encargos diarios

| # | Encargo | Tipo | Disponible desde | Tiempo |
| --- | --- | --- | --- | --- |
| 1 | Conversa con una bruja en las Termas | Termas | F10 | 15 s |
| 2 | Reposa con tu favorita | Termas | F10 | 10 s |
| 3 | Gana un combate cualquiera | Ancla | Altura 5 | 60 a 90 s |
| 4 | Gana en la salina del día (recolectar cuenta) | Ancla | Altura 12 | 5 a 90 s |
| 5 | Gana un nodo del Fresno | Ancla | Altura 10 | 60 a 90 s |
| 6 | Abre una Cadena | Hazaña | Altura 5 | Dentro del ancla |
| 7 | Logra 2 Spark | Hazaña | Altura 5 | Dentro del ancla |
| 8 | Lanza una Ascensión | Hazaña | Altura 5 | Dentro del ancla |
| 9 | Provoca un Quiebre con el planeta del día | Hazaña | Altura 12 | Dentro del ancla |
| 10 | Usa una bruja del planeta del día (o una Invitada de ese planeta) | Hazaña | Altura 12 | Dentro del ancla |
| 11 | Prepara una Salmuera | Taller | F16 | 20 s |
| 12 | Refina sal en el Altar del Árbol | Taller | Altura 12 | 15 s |
| 13 | Fabrica o reforja un arma | Taller | Altura 10 | 20 s |
| 14 | Sube de nivel a una bruja o un Talento de Cadena | Progreso | Altura 5 | 15 s |
| 15 | Haz una Ofrenda al Fresno | Progreso | Altura 8 | 10 s |
| 16 | Lee un folio del Libro Mayor (almita con nombre o diario de bruja) | Libro | F13 | 20 s |
| 17 | Da un regalo a una bruja | Termas | Altura 10 | 10 s |
| 18 | Báñate en las Termas (se vale meter solo los pies) | Termas | Paralaje | 15 s |
| 19 | Usa la Invitada de un amigo o de un sugerido | Social | Altura 15 | Dentro del ancla |
| 20 | Visita las Termas de un amigo | Social | Altura 25 | 15 s |

### 5.3 Reglas de rotación

1. Cada día salen cuatro: **un ancla** (3, 4 o 5), **una hazaña** que se cumple dentro de ese mismo combate, **uno de Termas o social** y **uno de taller, progreso o libro**.
2. Nunca hay dos encargos que pidan combates distintos.
3. Solo salen encargos de sistemas ya desbloqueados.
4. Ningún encargo que no sea ancla se repite dos días seguidos.
5. El planeta del día sube la probabilidad de los encargos 4, 9 y 10 al 65 %.
6. Las hazañas cuentan hacia atrás dentro del día: si ya lograste el Spark antes de abrir el tablero, ya está cumplido.
7. Un cambio gratis al día.
8. Recolectar en la salina (§6.4) cuenta como ganar, pero no cumple hazañas; por eso la hazaña del día también se puede hacer en historia o en el Fresno.
9. Nunca existen encargos de "haz una tirada", "compra", "gasta Perlas" ni "entra X días seguidos".

**Un martes de ejemplo (Marte):** gana en la Mina Férrea (90 s) + provoca un Quiebre de Marte (dentro del combate) + conversa con una bruja (15 s) + prepara una Salmuera (20 s). Con la navegación, unos 3 minutos.

### 5.4 Encargos semanales

Seis por semana: tres fijos y tres al azar de la lista. Cada uno da 300 de Crecimiento, 1 000 de experiencia del Grimorio, 15 000 almitas y 10 Sal Cruda a elegir. **No dan Sales de Baño,** para no mover las 70 tiradas de la 16.3.

| # | Encargo semanal | Fijo o al azar |
| --- | --- | --- |
| 1 | Termina tus encargos diarios 5 días | Fijo |
| 2 | Termina 2 Ascensos cortos o 1 run completa del Fresno | Fijo (desde Altura 10) |
| 3 | Gana o recolecta 5 veces en las salinas | Fijo (desde Altura 12) |
| 4 | Logra 20 Spark | Al azar |
| 5 | Provoca 3 Reacciones Híbridas | Al azar |
| 6 | Intenta un Juicio de Pureza (no hace falta ganarlo) | Al azar |
| 7 | Ofrenda 100 000 almitas | Al azar |
| 8 | Gana combates con 3 brujas 3★ distintas | Al azar |
| 9 | Prepara 5 Salmueras | Al azar |
| 10 | Presta tu Invitada 10 veces (se cumple solo) | Al azar |
| 11 | Gana un combate con una Cadena de cinco eslabones | Al azar |
| 12 | Guarda almitas en 3 Santuarios o recupera una Mancha de Sal | Al azar |

Antes de que exista un sistema, su fijo se cambia por uno al azar.

### 5.5 Cómo se limita el farmeo sin energía

No hay energía (16.4). Cada fuente tiene su propio tope de recompensa, y pasado el tope se puede seguir jugando.

| Fuente | Tope | Qué pasa después |
| --- | --- | --- |
| Historia | Una vez por misión | Se repite sin botín, para practicar o releer |
| Salinas del día | 3 Cosechas al día, acumulables hasta 9 | Combate de práctica sin sal |
| Fresno | Tope semanal de almitas (10.4) | Las almitas se convierten en Savia a tasa menor |
| Jefes regionales | 3 victorias con materiales por semana | Solo almitas |
| Enemigos y barriles del mapa | Reaparecen en cada reinicio | — |
| Grieta de luna nueva | 1 al día durante la ventana | — |
| Desafío diario de evento | 1 al día, acumulable hasta 3 | — |

**Por qué funciona:** la energía obliga a entrar a horas fijas para no desperdiciarla y vende recargas. Un tope diario que se acumula da el mismo ritmo sin relojes ni ventas: el jugador que se salta dos días no pierde nada, y el que farmea todo el día no se adelanta más de lo que permiten los topes.

**Supuesto de economía:** las almitas de esta propuesta suponen la escala de D-02 (nivel a `40·n^1.5`), con unas 100 000 almitas al día para quien juega unos 30 minutos. Encargos y salinas dan alrededor de la mitad; el resto sale de historia, Fresno y eventos. Si cambia H-01, estas cantidades se escalan igual.

## 6. Mazmorra de sal del día planetario

### 6.1 Formato

- Cada día abre **la salina de su planeta** (7.7). Es un combate de dos oleadas más una veta de sal que se rompe al final: de 60 a 90 s con Auto y ×2.
- Los enemigos son débiles al planeta del día y a otro que rota. Así el +15 % del día importa, pero sigues necesitando una rompedora.
- El +15 % se aplica solo al planeta dominante de cada bruja (C-27).
- La entrada está en la proa de la Tortuga: "Hoy la Tortuga pasa por la Mina Férrea".

### 6.2 Las siete salinas

| Día | Planeta | Salina | Sal Cruda |
| --- | --- | --- | --- |
| Lunes | Luna | Pozo de Plata | Sal Argenta |
| Martes | Marte | Mina Férrea | Sal Férrea |
| Miércoles | Mercurio | Fuente de Azogue | Sal de Azogue |
| Jueves | Júpiter | Torre de Estaño | Sal Estánnea |
| Viernes | Venus | Huerto Cúprico | Sal Cúprica |
| Sábado | Saturno | Cripta de Plomo | Sal Plúmbea |
| Domingo | Sol | Salina Áurea | Sal Áurea |

### 6.3 Niveles y recompensas por Cosecha

| Nivel | Altura | Nivel de bruja sugerido | Sal Cruda del día | Almitas | Material de talento |
| --- | --- | --- | --- | --- | --- |
| I | 12 | 20 | 10 | 4 000 | — |
| II | 20 | 30 | 15 | 5 000 | — |
| III | 28 | 40 | 20 | 6 000 | 20 % |
| IV | 36 | 50 | 25 | 7 000 | 30 % |
| V | 44 | 60 | 32 | 8 000 | 40 % |
| VI | 52 | 70 | 40 | 10 000 | 50 % |

La primera victoria en cada nivel da 200 de Crecimiento y 2 Sal Destilada de ese planeta. Cuánta sal pide cada grado sigue abierto (H-11).

### 6.4 Cosechas y recolección

- **Cosechas:** recibes 3 en cada reinicio y se acumulan hasta 9. Cada victoria con botín gasta una. El contador siempre está a la vista: "Cosechas 7/9".
- **Recolección:** un nivel que ya ganaste se puede cosechar sin pelear. Cuesta lo mismo y tarda 5 segundos. Eso mantiene el encargo diario en 3 minutos.
- **Sin Cosechas,** el combate es práctica: sin sal, con 10 % de las almitas.
- **Nada de lo que se vende da Cosechas** (16.4).

### 6.5 El domingo

El domingo rige el Sol: abre la Salina Áurea y las brujas de Sol hacen +15 % (7.7). Además es **domingo de transmutación**. En el Altar del Árbol, 3 Sal Cruda de cualquier planeta se transmutan en 1 de otro; el domingo la tasa baja a 2 por 1. En alquimia el oro es el metal perfecto, así que el día del Sol perfecciona las sales.

Con eso, quien no pudo jugar el sábado no espera una semana por su Sal Plúmbea: guarda Cosechas, cosecha otra sal y la transmuta.

### 6.6 Hora de reinicio y zona horaria

- **El "día real" es el día del servidor.** Cambia a las **04:00 de la hora del servidor**. La astrología clásica cuenta el día planetario desde el amanecer, y las 04:00 es antes del alba.
- **Servidores por región,** cada uno con un desfase fijo y sin horario de verano:

| Servidor | Desfase | Quién juega ahí |
| --- | --- | --- |
| Américas | UTC−6 | México, EE. UU., Canadá y Latinoamérica |
| Asia | UTC+9 | Japón y Corea |
| Europa (si se lanza) | UTC+1 | Europa |

| Ciudad | Hora local del reinicio |
| --- | --- |
| Ciudad de México | 04:00 todo el año (México dejó el horario de verano en 2022) |
| Tijuana y Los Ángeles | 02:00 en invierno, 03:00 en verano |
| Nueva York | 05:00 en invierno, 06:00 en verano |
| Bogotá | 05:00 |
| Buenos Aires y São Paulo | 07:00 |
| Tokio y Seúl | 04:00 |

- **Por qué UTC−6:** el lanzamiento suave es en México (22.4) y así el reinicio cae a la misma hora todo el año ahí. En EE. UU. se mueve una hora con su horario de verano, siempre de madrugada.
- **Amigos:** solo dentro del mismo servidor.
- **En pantalla:** "Hoy rige Marte · cambia en 5 h 12 min", y el planeta de mañana.
- **Semana:** cambia el lunes a las 04:00 del servidor.

## 7. Fase lunar real

### 7.1 Cálculo

- **El servidor calcula** el instante exacto de cada luna nueva y llena con un algoritmo astronómico estándar (error menor a un minuto). Publica la tabla de los próximos 24 meses en la configuración remota. El cliente solo la lee (S-05).
- **Ventana:** el día de servidor que contiene el instante exacto, más el anterior y el siguiente. Son **3 días de luna llena y 3 de luna nueva** en cada ciclo de 29.5 días.
- **Cuartos de luna:** sin efecto. Menos reglas, más legibles.
- **Calendario visible:** el Libro Mayor muestra las fases de los próximos 30 días. Nadie se entera tarde.

### 7.2 Luna llena: caen más almitas

| Efecto | Valor |
| --- | --- |
| Almitas de combate (historia, salinas, Fresno, eventos) | +25 % |
| Confianza al conversar en las Termas | +50 % (la luna llena suelta la lengua) |
| Tope semanal del Fresno | Sin cambio: llegas antes, no más arriba |
| En las Termas | El cielo muestra la luna llena (solo visual) |

### 7.3 Luna nueva: eventos de Sal Negra

| Efecto | Valor |
| --- | --- |
| **Grieta de Sal Negra:** combate especial junto a la salina, 1 al día (3 por ventana), sin gastar Cosechas | 2 Sal Negra y 10 000 almitas por victoria |
| Enemigos de la Grieta | Sin Carta: los Aspectos no les afectan (6.5), así que obligan a pelear distinto |
| Paralaje | +5 por victoria en la Grieta |
| Élites del Fresno | +1 Sal Negra |
| Almita con nombre de luna nueva | Un fragmento de historia por ventana |

Suma unas 6 Sal Negra al mes desde la Grieta, que alcanzan para 6 Vislumbres. El Fresno y los élites siguen siendo la fuente principal (11.1).

### 7.4 Eclipses reales (opcional)

Hay de 2 a 5 eclipses reales al año. Propongo marcarlos solo con un cielo especial en las Termas y un folio del Libro Mayor, sin recompensas de poder. Cuesta poco y se comparte en redes. No se confunde con el reloj de eclipses de la historia (sección 6), que avanza por regiones.

## 8. Logros y Títulos

### 8.1 Reglas

- Unos **150 logros al lanzamiento**, con 4 800 SB en total, de una sola vez. Cada versión suma unos 15 logros con 320 SB, que salen de la línea "Historia y exploración nuevas" de la 16.3.
- **Ningún logro pide juntar ni tener brujas del gacha.** Lo que pide un planeta, un papel o una rareza se cumple con brujas reclutables por historia o con la Invitada.
- Para eso, las reclutables gratis deben cubrir los siete planetas como dominantes. Hoy faltan Sol y Júpiter: recomiendo hacer reclutables a la jaguar (Sol) y a la lince (Júpiter), además de Cempa (MP-02).
- Ningún logro pide tirar, pagar ni gastar Perlas.

### 8.2 Treinta logros de ejemplo

| # | Logro | Condición | SB | Título |
| --- | --- | --- | --- | --- |
| 1 | El orden es poder | Abre tu primera Cadena | 80 | — |
| 2 | Nota dorada | Logra tu primer Spark | 80 | — |
| 3 | Grieta en la Firmeza | Provoca tu primer Quiebre | 80 | — |
| 4 | Canto mayor | Lanza tu primera Ascensión | 80 | — |
| 5 | Sal en la lengua | Lleva a una bruja a la banda Salobre | 80 | — |
| 6 | Primera Salmuera | Prepara una Salmuera en el Caldero | 80 | — |
| 7 | Coro completo | Cadena de cinco eslabones con la Invitada como Cierre | 40 | — |
| 8 | Cuatro notas | Cuatro Spark en una sola Cadena | 80 | *Oído Absoluto* |
| 9 | Cuatro reacciones | Provoca Eclipse, Helada Negra, Espinas Rojas y Tempestad (se vale con la Invitada) | 80 | *Lectora de Aspectos* |
| 10 | Silencio ante el jefe | Vence a un jefe sin lanzar Ascensiones | 80 | *Silente* |
| 11 | Al borde | Gana un combate después de que una bruja entre en Éxtasis Abisal | 40 | De bruja: *Salada hasta el Alma* |
| 12 | Raíz pisada | Termina tu primer Ascenso corto | 80 | — |
| 13 | Vuelvo por lo mío | Recupera una Mancha de Sal | 40 | *Quien Volvió por sus Almas* |
| 14 | Dos veces muerta | Cae dos veces en Raíces Putrefactas con la misma bruja | 20 | De bruja: *Dos Veces Muerta en la Raíz* (13.1) |
| 15 | Por gusto | Termina una zona con cinco maldiciones | 80 | *Maldita por Gusto* |
| 16 | La poda | Vence a la Jardinera | 160 | *Quien Vio a la Jardinera* |
| 17 | Parias | Termina una run completa con tres brujas 3★ | 160 | *Amiga de las Parias* |
| 18 | Destilada | Sube a una bruja al grado B | 80 | — |
| 19 | Quintaesencia | Sube a una bruja al grado S | 160 | — |
| 20 | La aprendiza llega | Sube a una bruja 3★ al grado SS | 320 | *Maestra de Aprendizas* |
| 21 | Recetario | Anota diez recetas de Salmuera | 80 | — |
| 22 | Cocina turbia | Falla una mezcla y obtén una Salmuera Turbia | 20 | *Alquimista de Cocina* |
| 23 | Leyenda en el Yunque | Forja una legendaria | 160 | *Forjadora de Leyendas* |
| 24 | Salada de nacimiento | Termina la región de tu propio signo | 80 | *Salada de Nacimiento* (13.4) |
| 25 | Bajar al Hades | Termina la región de Escorpio | 80 | *La que Bajó al Hades* (13.4) |
| 26 | Mirar de vuelta | Llega a Paralaje Máximo | 80 | *Quien Miró de Vuelta* |
| 27 | Siete sales | Gana en las siete salinas | 80 | *Semana Completa* |
| 28 | Contadora sin alma | Ofrenda un millón de almitas al Fresno | 80 | *Contadora sin Alma* (13.4) |
| 29 | Mano prestada | Tu Invitada ayuda 50 veces | 80 | *Anfitriona de la Tortuga* |
| 30 | Dos lunas | Juega en una luna llena y en una luna nueva | 40 | — |

Estos 30 suman 2 680 SB. Los otros 120 son pequeños (de 0 a 20 SB, o materiales) y suman 2 120.

Los logros 8, 9, 17 y 20 empujan a jugar con 3★ y a pensar el orden. Mueven el KPI de equipos del Fresno con al menos una 3★ (22.3).

### 8.3 Títulos

- **Dos clases.** Los **Títulos de la Calculista** dan una pasiva pequeña y una etiqueta visible (13.4). Los **Títulos de bruja** aparecen en su ventana de estado (13.1), son de historia y no dan estadísticas (MC-08).
- **De dónde salen:** logros marcados, decisiones de historia (marcas de Destino), temporadas del Fresno (10.7), y Confianza 10 para los de bruja. Ninguno sale del gacha ni de la tienda.
- **Uno equipado.** La pasiva solo cuenta con el Título equipado. Los diálogos que abre un Título cuentan con solo tenerlo.
- **Tope de efecto:** ±5 % o +1 a una estadística, y siempre en un contexto concreto.
- **Pronombres:** cada Título de la Calculista se escribe en tres variantes (ella, él, elle) o en forma neutra. Ejemplo: *Salada de Nacimiento*, *Salado de Nacimiento*, *Salade de Nacimiento*; *La que Bajó al Hades*, *El que Bajó al Hades*, *Quien Bajó al Hades*.

| Título | Pasiva |
| --- | --- |
| *Recién Caída* | +1 Cálculo en chequeos durante el primer capítulo |
| *Silente* | +1 Cálculo en chequeos |
| *Oído Absoluto* | Empiezas cada combate con +5 de Resonancia |
| *Lectora de Aspectos* | +5 % al efecto de los Aspectos |
| *Maldita por Gusto* | +5 % de almitas en el Fresno con cinco maldiciones |
| *Amiga de las Parias* | +5 % de almitas por cada 3★ en el equipo |
| *Alquimista de Cocina* | Las Salmueras Turbias dan un efecto bueno más seguido |
| *Contadora sin Alma* | +5 % de Crecimiento por Ofrenda |
| *Quien Miró de Vuelta* | +1 Temple en chequeos de horror |

## 9. Social: amigos e Invitada

### 9.1 Qué es la Invitada

Según C-17, la Invitada no está en el campo: aporta su **Regencia** todo el combate y puede ser el **Cierre** de una Cadena por combate (9.1 y 9.5). No tiene vida, Salinidad ni Ánima.

### 9.2 De dónde sale

| Fuente | Quiénes | Desde |
| --- | --- | --- |
| **Invitadas de la caravana (NPC)** | Siete, una por planeta, con brujas del elenco en grado C y nivel acorde al contenido. Incluyen a las cuatro Regentes del banner estándar, así que sirven para probarlas | F17 |
| **Amigos** | Hasta 30, 40 o 50 según la Altura (§4.3) | Altura 15 |
| **Sugeridos** | 10 jugadores del mismo servidor, activos en las últimas 72 horas, con Altura parecida (±10). Primero los que prestan una bruja del planeta del día o de las debilidades del contenido que vas a jugar | Altura 15 |

Cada jugador expone **tres brujas** como Invitadas. Quien la pide elige una.

### 9.3 Estadísticas y límites

- **Se usan las de la dueña:** grado, arma y prefijo (sus frames cambian el Spark), Talento del Cierre, Dignidad y nivel.
- **El nivel tiene tope:** tu bruja de nivel más alto más 10. Así un amigo veterano ayuda, pero no juega por ti.
- **No se usan:** el Sendero, los Títulos ni las Raíces de la dueña.
- **Usos:** ilimitados. En el Fresno eliges una Invitada al empezar la run y se queda toda la run.
- **Dónde no:** Juicios de Pureza, que son la prueba personal de la bruja (12.4).
- **Bono de amistad:** la primera vez al día que usas a la Invitada de cada amigo, los dos reciben 1 punto de Gratitud para el logro 29 y el encargo semanal 10.

### 9.4 Recompensa por prestar

Cada vez que alguien usa a tu Invitada recibes **500 almitas y 20 de Crecimiento**, con tope de **20 usos al día** (10 000 almitas y 400 de Crecimiento). Se cobra en el Buzón con un solo botón. Nunca da Sales de Baño.

### 9.5 Comunicación y moderación

**Sin chat libre al lanzamiento.** Moderar texto en tres idiomas cuesta mucho. El fandom furry recibe acoso con frecuencia. Y un chat sin moderar es un riesgo para la clasificación 17+ y para la revisión de las tiendas.

- **Sellos:** 20 al lanzamiento, como "¡Gracias por la Invitada!" o "Buen Spark". Más sellos salen de eventos y de la tienda cosmética.
- **Perfil:** nombre con etiqueta, Título equipado, Altura, tres Invitadas, un sello favorito y un **lema elegido de una lista**, nunca escrito.
- **Nombres:** filtro en español, inglés y japonés, con variantes y caracteres sustituidos. Un nombre reportado tres veces se oculta hasta revisión; si se confirma, el jugador elige otro.
- **Bloquear:** oculta a esa persona en los dos sentidos: sugeridos, amigos y visitas.
- **Privacidad:** "No aparecer en sugeridos" y "Solo amigos pueden visitar mis Termas".
- **Solicitudes:** 20 enviadas al día como máximo.
- **Compartir:** la transformación y la hoja de estado se comparten con el menú del teléfono, fuera del juego (H-29).
- **Después:** revisar un chat con frases predefinidas o grupos (aquelarres) en la versión 1.3, con datos de reportes del lanzamiento suave (MP-07).

## 10. Termas como hub

### 10.1 Mapa de la Tortuga

Las Termas son la pantalla de inicio. Cada sistema tiene un lugar en el caparazón, con su atajo en el borde derecho, al alcance del pulgar (REFERENCIAS, mapa de pantallas).

| Lugar | Qué hay |
| --- | --- |
| Manantial | Gacha y baño |
| Caldero de Bruja | Salmueras, dulces y regalos |
| Yunque Astral | Armas y reforja |
| Altar del Árbol y esqueje del Fresno | Refinar, transmutar, fusiones y Ofrenda |
| Puesto de la Herbolaria | Qué puedes fabricar con lo que tienes (15.5) |
| Espejo | Vestidor y edición de la Calculista |
| Buzón | Correo y cobro de préstamos |
| Tablero de encargos | Diarios, semanales y logros |
| Proa | Mapa de regiones, Fresno y salina del día |
| Puesto de la Tortuga | Tienda |

### 10.2 Reposo, tocar y conversar

- **Reposo:** tu favorita y hasta tres brujas más descansan en lugares de la Tortuga: la orilla del manantial, la mesa de té o la sombra del esqueje. Rotan cada reinicio. Cada una tiene sus bucles de reposo (19.2).
- **Tocar:** tres zonas nada más. **Cabeza u orejas:** reacción según su especie. **Mano o pata:** saludo o choque. **Cualquier otro punto:** una reacción general de su carácter, que no depende de dónde tocaste. No hay reacciones ligadas a pecho, cadera, piernas ni cola.
- **Conversar:** una charla corta por bruja al día. Habla del planeta del día, de la luna, de lo último que hicieron juntas o de otra bruja. La favorita saluda al entrar. Estas frases enseñan el meta-juego sin menús: "Hoy es día de Marte; la Mina Férrea está abierta".

### 10.3 Confianza

Cada bruja tiene **Confianza** del 1 al 10. Es amistad y confianza con la Calculista, no romance. Las escenas son las mismas con cualquier pronombre. **Ninguna recompensa da estadísticas.**

| Fuente | Confianza |
| --- | --- |
| Conversar | 20 al día por bruja (30 en luna llena) |
| Regalo | 10, 20 o 40 según le guste; hasta 3 al día por bruja |
| Ganar un combate con ella | 5, tope de 20 al día |
| Su cumpleaños | 100 con el pastel de sal del correo (§13.1) |

Para llegar a 10 se necesitan 4 000. Jugando diario con una favorita, toma entre 25 y 70 días.

| Nivel | Recompensa |
| --- | --- |
| 2 | Línea de voz y su receta de dulce favorita |
| 3 | Folio personal en el Libro Mayor |
| 4 | Su lugar favorito de reposo en la Tortuga |
| 5 | Recuerdo 1: escena de 2 a 3 minutos con retratos |
| 6 | Expresiones nuevas en su retrato de inicio |
| 7 | Título de bruja *Confidente* |
| 8 | Recuerdo 2 y línea de voz |
| 9 | Variante de color de su pose de victoria |
| 10 | Recuerdo 3, carta escrita a mano y un color de sal exclusivo |

### 10.4 Regalos

- Salen del mundo (objetos en los puntos de interés) y del Caldero (dulces de flor de sal y agua termal). No gastan sal planetaria.
- Cada bruja tiene dos categorías favoritas según su especie y su planeta. La Herbolaria da pistas.
- **Ningún regalo se vende,** ni por Perlas ni por dinero.

### 10.5 El baño y el Paralaje

Se alinea con H-15: medidor de 0 a 100 con bandas en 25, 50 y 80.

| Banda | Paralaje |
| --- | --- |
| Bajo | 0 a 24 |
| Medio | 25 a 49 |
| Alto | 50 a 79 |
| Máximo | 80 a 100 |

| Fuente | Cambio |
| --- | --- |
| Pelear contra un Huésped | +5 |
| Leer un folio de saber prohibido (siempre opcional y avisado) | +10 |
| Entrar en Éxtasis Abisal | +3 |
| Ganar en la Grieta de luna nueva | +5 |
| **Bañarte en las Termas** | **−20, una vez al día** |
| Meter solo los pies | 0. Cuenta para el encargo 18 |
| Agua de Olvido (14.2) | −20, sin límite diario |

- **Tú decides cuánto ver** (6.5). El baño pregunta: "¿Quieres olvidar?" con dos botones: *Bañarte* o *Solo los pies*. Así el encargo diario nunca obliga a olvidar a quien busca el final secreto.
- **Escena del baño:** 10 segundos de vapor, agua y estrellas que se apagan. La Calculista aparece de hombros para arriba.
- **Ajuste de accesibilidad:** "No pasar de Paralaje Medio", para quien no quiere horror (MP-14).

### 10.6 Termas de amigos

Desde Altura 25 puedes visitar las Termas de un amigo: ves su decoración y a su favorita en reposo, y dejas un sello. Cada visita da 1 agua termal, base de Salmuera (14.1), con tope de 5 al día. Quien recibe visitas ve la lista al entrar.

### 10.7 Reglas de contenido en las Termas

- **Atuendo base del baño:** bata de baño de la Tortuga, gratis para todas. Nunca toallas solas, que insinúan desnudez.
- **Cámara libre con límites:** distancia mínima de plano medio, sin ángulos bajos y sin zoom al cuerpo. Tocar acerca la cámara a la cara.
- **Modo recatado:** yukata o bata cerrada para todas, cámara más lejos y física de ropa y pecho reducida (19.5). Las interacciones son las mismas.
- **Cosméticos de baño de pago:** ninguno muestra más piel que el atuendo gratis más revelador de esa bruja. Se vende estilo, no piel.
- **El fanservice no se usa como recompensa de gasto:** la Confianza, los Recuerdos y las reacciones se ganan jugando y no se venden.

## 11. Grimorio y Diezmo de Sal

### 11.1 Grimorio

| Campo | Valor |
| --- | --- |
| Duración | 42 días: una versión, alineada con el Tránsito (10.7) |
| Niveles | 50, de 1 000 de experiencia cada uno (50 000 en total) |
| Experiencia posible | Diarios 21 000 (500 × 42), semanales 36 000 (6 000 × 6) y misiones de temporada 6 000. Total: 63 000 |
| Margen | Llegas al 50 con el 79 % de lo posible. Con diarios 5 días por semana y todos los semanales, alcanza |
| Después del 50 | Cada 1 000 de experiencia: 10 000 almitas, solo en el carril gratuito |
| Fuentes | Solo encargos y misiones de temporada. El farmeo no da experiencia |

Las misiones de temporada son ocho por versión y de una sola vez. Ejemplos: termina el capítulo nuevo, llega a las Ramas del Eclipse con el Tránsito nuevo, gana el modo desafío del evento.

### 11.2 Recompensas

| Niveles | Carril gratuito | Carril premium |
| --- | --- | --- |
| 1 | 20 000 almitas | Marco del Libro Mayor de la temporada |
| 2 a 9 | Almitas, Sal Cruda a elegir y material de talento | 160 SB en el nivel 5 |
| 10 | 160 SB | 160 SB y color de sal de la temporada |
| 11 a 24 | Almitas, Sal Destilada y Savia | 160 SB en los niveles 15 y 20; emote en el 20 |
| 25 | 160 SB | 160 SB |
| 30 | 3 Sal Negra | **Atuendo de la temporada** y 160 SB |
| 31 a 39 | Almitas y material de talento | 160 SB en el nivel 35 |
| 40 | 160 SB | 160 SB y efecto de Spark |
| 45 | Sal Destilada | Decoración de las Termas |
| 50 | Marco de la temporada | Marco animado y pose de victoria |
| **Total en Sales** | **480 SB (3 tiradas)** | **1 280 SB (8 tiradas)** |

- **El carril premium solo trae Sales y cosméticos.** Las almitas, la sal y los materiales viven en el gratuito, que tienen todos (16.4).
- **No se venden niveles del pase.** Comprar niveles desbloquearía materiales del carril gratuito: sería vender progreso.
- **El atuendo del Grimorio es siempre de una 4★ o de una reclutable gratis,** nunca de la Regente del banner. El pase no es un anuncio del gacha, y las brujas modestas también reciben ropa.
- **Las 480 SB del carril gratuito** salen de la línea de eventos de la 16.3 (MC-01).

### 11.3 Diezmo de Sal

- **Qué da:** 300 Perlas al comprar y 90 SB diarias durante 30 días (16.4). En total, 3 000: **18.75 tiradas** por tarjeta.
- **Cobro:** las 90 del día se reciben al entrar. Un día sin entrar se pierde, y el contador de días restantes siempre está a la vista (MP-09).
- **Acumulable** hasta 180 días. No se puede comprar con más de 150 días restantes.
- **No da** Cosechas, Crecimiento, materiales ni ventajas. Solo Perlas y Sales.
- **En el libro contable,** las 90 diarias se asientan como **SB compradas** (C-25 y S-09).

### 11.4 Precios sugeridos

| Producto | USD | MXN |
| --- | --- | --- |
| Diezmo de Sal (30 días) | 4.99 | 99 |
| Grimorio premium (una temporada) | 9.99 | 199 |

Los precios en pesos siguen los niveles de las tiendas, que ya incluyen el IVA. Un Grimorio de lujo a 19.99 USD, con un segundo atuendo y sin niveles, queda fuera del lanzamiento (MP-13).

### 11.5 Cuenta contra las 70 tiradas gratis

| Producto | Sales por versión (42 días) | Tiradas extra por versión | Sobre las 70 gratis | Costo por tirada |
| --- | --- | --- | --- | --- |
| Grimorio gratuito | 480 | 0: ya está dentro de las 70 | — | Gratis |
| Grimorio premium | 1 280 | 8 | +11 % | 1.25 USD, más cosméticos |
| Diezmo de Sal, sin cortes (1.4 tarjetas) | 4 200 (420 en Perlas y 3 780 diarias) | 26.25 | +38 % | 0.27 USD |
| Los dos | 5 480 | 34.25 | +49 % | 0.50 USD (17 USD por versión) |

- **Las 70 gratis no cambian.** Ningún producto quita una fuente gratuita ni se cuenta dentro de ellas.
- **Con las 70 gratis,** el 78 % consigue a la destacada de su versión (HUECOS, sección 2). Con 104 (tarjeta y pase), casi siempre la consigue, y a veces también a la segunda Regente de la versión (22.1). Ese es el incentivo de quien gasta poco, sin romper la promesa gratuita.
- **Para comparar:** el paquete grande de Perlas cuesta 2.13 USD por tirada (§12.2).

### 11.6 Lo que nunca venden

Ni el Grimorio ni el Diezmo venden almitas, sal, materiales, Savia, Cosechas, niveles del pase, grados, Juicios, Eco ni ventajas del Fresno (16.4).

## 12. Tienda

### 12.1 Secciones

1. **Perlas:** paquetes (§12.2).
2. **Suscripciones:** Diezmo de Sal y Grimorio.
3. **Cosméticos directos,** sin azar (16.5).
4. **Polvo Astral:** tienda rotativa de cosméticos (§12.4).
5. **Tienda del evento,** con moneda ganada jugando (§13.3).
6. **Intercambio:** Perlas a Sales de Baño, 1 a 1. En Bélgica no aparece.

### 12.2 Paquetes de Perlas

La base es plana: 60 Perlas por dólar. El bono crece con el paquete.

| Paquete | USD | MXN | Perlas base | Bono | Total | Primera compra (base doble) | Tiradas del total |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Pizca | 0.99 | 19 | 60 | — | 60 | 120 | 0.4 |
| Puñado | 4.99 | 99 | 300 | 20 | 320 | 600 | 2 |
| Cuenco | 14.99 | 299 | 900 | 100 | 1 000 | 1 800 | 6.3 |
| Jarra | 29.99 | 599 | 1 800 | 300 | 2 100 | 3 600 | 13.1 |
| Tinaja | 49.99 | 999 | 3 000 | 600 | 3 600 | 6 000 | 22.5 |
| Aljibe | 99.99 | 1 999 | 6 000 | 1 500 | 7 500 | 12 000 | 46.9 |

- **Primer pago doble:** la primera compra de cada paquete da el doble de la base en lugar del bono. Se reinicia en cada aniversario.
- **Las Perlas no caducan** (S-13).
- **Precio real a la vista:** cada cosmético muestra su equivalente aproximado en la moneda local junto a las Perlas.

### 12.3 Cosméticos directos

| Cosmético | Perlas | Aproximado en USD |
| --- | --- | --- |
| Atuendo de bruja | 1 280 | 21 |
| Atuendo de Ascendente | 980 | 16 |
| Manantial | 1 980 | 33 |
| Ropa de baño o yukata | 980 | 16 |
| Ritual de sal alterno | 680 | 11 |
| Color de sal | 480 | 8 |
| Marco del Libro Mayor | 480 | 8 |
| Apariencia de la Calculista | 480 a 980 | 8 a 16 |
| Decoración de las Termas | 160 a 680 | 3 a 11 |
| Pose, emote, sello o efecto de Spark | 160 a 480 | 3 a 8 |

### 12.4 Ofertas y Polvo Astral

- **Escaparate semanal:** un cosmético con 20 % de descuento por semana. Nada más.
- **Sin presión:** ninguna oferta aparece después de una tirada; ninguna oferta dura menos de 24 horas; ninguna ventana emergente de venta en la primera sesión.
- **Tienda de Polvo Astral:** seis cosméticos que rotan cada semana, incluidos los de eventos pasados. El Polvo sale solo de tiradas sin brujas nuevas posibles y de los hitos de Ofrenda (11.1). No se vende.

### 12.5 Límite de gasto y recordatorios (16.6)

| Herramienta | Regla |
| --- | --- |
| Límite mensual opcional | Lo eliges en tu moneda. Bajarlo aplica al instante; subirlo, 72 horas después |
| Avisos del límite | Al 50 %, al 80 % y al 100 % |
| Aviso de 24 horas | Si gastas más del equivalente a 100 USD en un día (ajustable), el juego te lo dice antes de la siguiente compra |
| Resumen mensual | En la tienda: "Este mes gastaste…" con el historial de compras |
| Menores de 18, donde la tienda lo permita | Límite de 50 USD al mes por defecto |
| Menores de 20 en Japón | El tope por edad de la guía de la industria japonesa, a confirmar con legal (S-13) |

### 12.6 Bélgica

Las Perlas solo compran cosméticos y no se cambian por Sales. No se vende el Diezmo. El Grimorio premium cambia sus 1 280 SB por cosméticos. Los banners solo aceptan Sales ganadas jugando (C-25).

## 13. Correo, eventos y notificaciones

### 13.1 Correo

- Los mensajes del sistema caducan a los 30 días. Las compras nunca caducan.
- Tope de 200 mensajes y botón de cobrar todo.
- **Categorías:** sistema y compensaciones, recompensas, eventos y cartas de brujas.
- **Cumpleaños de bruja:** cada bruja tiene fecha en su carta natal. Ese día llega su carta y un pastel de sal (+100 de Confianza). Son dos cumpleaños al mes con 24 brujas: contenido barato que se comparte.

### 13.2 Compensaciones

La 16.3 reserva 440 SB por versión: 300 del mantenimiento de versión y 140 de margen para errores.

| Caso | Compensación |
| --- | --- |
| Mantenimiento programado de versión (hasta 5 h) | 300 SB |
| Mantenimiento que se alarga | +80 SB por cada hora extra |
| Caída no planeada de más de 1 hora | 160 SB por cada 2 horas, tope de 640 |
| Error que te quitó recursos | Se devuelve exacto, más 160 SB |
| Error visual o de texto | Nada |

- Las compensaciones van a las cuentas creadas antes del problema y con Altura 10 o más, para no premiar cuentas de repetición.
- Las emergencias grandes se pagan aparte y no cuentan dentro de las 70 tiradas.
- Las envía la herramienta interna de la Fase 3A (S-07).

### 13.3 Plantilla: evento de historia de 3 semanas

| Día | Qué pasa |
| --- | --- |
| 1 | Abren los actos 1 y 2, la tienda del evento y el desafío diario |
| 4 | Acto 3 |
| 8 | Acto 4 y modo desafío con tres dificultades |
| 12 | Acto 5, el final |
| 13 a 21 | Repetición libre; el desafío diario sigue |
| 21 | Termina el evento |
| 28 | Cierra la tienda. La moneda que sobra se convierte en almitas: nunca se pierde (pilar 4) |

- **Moneda temática,** una por evento (por ejemplo, Conchas de Marea). Se gana: actos 600 (una vez), desafío diario 2 100 (100 al día, acumulable hasta 3 días) y modo desafío 300. Total: 3 000.
- **Tiempo:** cada acto dura unos 15 minutos y el desafío diario unos 3.
- **Sin ventajas por pagar:** no hay bono de moneda por tener a la Regente nueva. El modo desafío se gana con brujas gratis y la Invitada.

| Objeto de la tienda | Precio | Límite | Total |
| --- | --- | --- | --- |
| 160 SB | 150 | 6 | 900 |
| Sal Destilada a elegir | 60 | 5 | 300 |
| Material de talento | 40 | 10 | 400 |
| 20 000 almitas | 30 | 10 | 300 |
| Sal Negra | 100 | 2 | 200 |
| Decoración del evento para las Termas | 300 | 1 | 300 |
| Receta del arma del evento (Alquímico) | 300 | 1 | 300 |
| **Total** | | | **2 700** |

Con el 90 % de la moneda compras todo. El evento da 1 440 SB: 480 por los actos y 960 de la tienda.

### 13.4 Eventos dentro de la versión

| Semana | Evento | SB |
| --- | --- | --- |
| 1 a 3 | Evento de historia (§13.3) | 1 440 |
| 3 | Evento de las Termas o minijuego (22.1) | 640 |
| 6 | Evento corto | 440 |
| **Total** | | **2 520** |

Con las 480 del Grimorio gratuito, la línea de eventos de la 16.3 queda en 3 000, igual que hoy.

### 13.5 Notificaciones push

| Categoría | Ejemplo | Por defecto | Frecuencia máxima |
| --- | --- | --- | --- |
| Diezmo (si lo tienes) | "Tus 90 Sales de Baño te esperan." | Encendida | 1 al día |
| Encargos | "Te faltan 2 encargos; el día cambia en 3 horas." | Encendida | 1 al día, 3 h antes del reinicio |
| Luna | "Esta noche hay luna llena: +25 % de almitas por tres días." | Encendida | 2 al mes |
| Mancha de Sal | "Tus almitas te esperan en Raíces Putrefactas." | Encendida | 1 por muerte, 24 h después |
| Cosechas llenas | "Tienes 9 Cosechas: ya no se acumulan más." | Encendida | 1 por semana |
| Versión y eventos | "Nueva región: Costa de las Conchas." | Encendida | 1 por versión y 1 por evento |
| Cierre de tienda de evento | "Te quedan 400 Conchas; la tienda cierra en 48 h." | Encendida | 1 por evento |
| Salina marcada | "Hoy es día de Saturno. Marcaste Sal Plúmbea." | Apagada | 1 por semana por planeta |
| Social | "Tu Invitada ayudó a 12 personas hoy." | Apagada | 1 al día |
| Banners | "La destacada se va en 48 h." | Apagada | 1 por banner |
| Ofertas | — | Apagada; nunca se enciende sola | — |

- **Máximo una notificación no transaccional al día.** Si coinciden varias, gana la de más arriba.
- **Silencio de 22:00 a 09:00** en la hora del teléfono.
- **Un interruptor por categoría y uno general,** en Ajustes (S-17 y S-19).
- **El permiso se pide en F18,** después de la primera 5★, con la lista de categorías a la vista. Nunca al abrir el juego.

## 14. Métricas

Los eventos van a la taxonomía de `docs/analitica.md` que escribe la Fase 3A. Los nombres siguen la convención de CLAUDE.md: identificadores en inglés.

| Sistema | Evento de analítica | Datos clave | KPI de la 22.3 que mueve |
| --- | --- | --- | --- |
| Primera sesión | `ftue_step` | Paso F01 a F21, tiempo de juego, día de calendario | D1 |
| Primera Cadena | `first_chain`, `first_spark` | Tiempo de juego, órdenes probados | Primera Cadena antes del minuto 3 |
| Creación | `calculista_created` | Especie, escala, signo, modo rápido o detallado, duración | D1 |
| Primera 5★ | `first_pull`, `first_5star`, `advance_granted` | Tiempo de juego, día, tiradas usadas, Regente elegida, monto adelantado | Primera 5★ antes de 2 horas |
| Desbloqueos | `system_unlocked` | Sistema, Altura, tiempo de juego | D1 y D7 |
| Altura y Ofrenda | `altura_up`, `ofrenda_made` | Nivel, fuentes de Crecimiento, almitas | D7 y D28 |
| Encargos | `daily_task_completed`, `daily_set_completed`, `daily_task_swapped`, `weekly_task_completed` | Encargo, duración, duración de la sesión | D7 y D28 |
| Salinas | `salina_run`, `harvest_overflow`, `transmute` | Planeta, nivel, resultado, recolección, Cosechas perdidas por tope | D7 |
| Luna | `lunar_window_started`, `grieta_run`, `black_salt_gained` | Fase, resultado, fuente | D28 |
| Logros y Títulos | `achievement_unlocked`, `title_equipped` | ID, tiempo de juego | D7, D28 y equipos con 3★ |
| Invitada | `guest_selected`, `guest_lent` | Fuente (amigo, sugerido, NPC), bruja, rareza, contenido | Jugadores gratis con la destacada (prueban antes de tirar) |
| Amigos y moderación | `friend_request`, `player_reported`, `player_blocked` | Enviada o aceptada, motivo | D28 |
| Termas | `termas_interaction`, `gift_given`, `trust_level_up` | Tipo, bruja, nivel | D7 y D28 |
| Baño y Paralaje | `bath_taken`, `paralaje_changed` | Antes y después, fuente, modo (baño o pies) | D28 |
| Grimorio | `pass_xp_gained`, `pass_level_up`, `pass_purchased` | Fuente, nivel, premium | Mezcla de ingresos y D28 |
| Diezmo | `card_purchased`, `card_claimed`, `card_expired` | Días restantes, día cobrado | Mezcla de ingresos y D28 |
| Tienda | `shop_viewed`, `iap_purchased`, `cosmetic_purchased` | Sección, producto, precio local, primera compra | Mezcla de ingresos |
| Límite de gasto | `spend_limit_set`, `spend_reminder_shown` | Monto, tipo de aviso | Cumplimiento (16.6) |
| Gacha | `pull`, `featured_obtained` | Banner, pity previo, resultados, ¿ha pagado alguna vez? | Jugadores gratis con la destacada: 50 % o más |
| Fresno | `fresno_run_ended` | Zonas, resultado, rarezas del equipo, Invitada | Equipos del Fresno con 3★: 20 % o más |
| Correo | `mail_claimed`, `compensation_sent` | Tipo, motivo, monto | — (salud) |
| Eventos | `event_act_completed`, `event_currency_earned`, `event_shop_purchase`, `event_currency_converted` | Evento, acto, monto | D7 y D28 |
| Notificaciones | `push_sent`, `push_opened`, `push_category_toggled` | Categoría, encendida o apagada | D7 y D28 |

**Métricas de salud** (no están en la 22.3; propongo sumarlas):

| Métrica | Meta |
| --- | --- |
| Mediana de tiempo para terminar los cuatro encargos | 3 a 5 minutos |
| Jugadores activos que terminan los cuatro encargos | 60 % o más |
| Cuentas que apagan todas las notificaciones | Menos de 30 % |
| Reportes por cada 1 000 jugadores activos al día | Seguir la tendencia; revisar si sube dos semanas seguidas |
| Cosechas perdidas por tope | Menos de 10 % de las generadas |
| Cuentas que llegan al nivel 50 del Grimorio sin pagar | 50 % o más de las que juegan toda la versión |

## 15. Qué fase construye cada parte

Las fases son las de [HOJA_DE_RUTA.md](../HOJA_DE_RUTA.md).

| Parte | Fase | Criterio de aceptación sugerido |
| --- | --- | --- |
| Reloj: reinicio, semana, día planetario y fase lunar por `IClock` | 2A y 3A | Cambiar la hora del teléfono no cambia el día ni da recompensas |
| Libro contable con SB ganadas y compradas | 2A | El Diezmo y el Grimorio premium se asientan como compradas |
| Creador de la Calculista y signo a región | 3B | Los 12 signos llevan a una región que existe |
| Paralaje y baño | 3B y 3C | El baño baja 20 una vez al día; "solo los pies" cumple el encargo sin bajar nada |
| Termas: reposo, tocar, conversar, Confianza y regalos | 3C | Una prueba de datos confirma que ninguna recompensa de Confianza cambia una estadística |
| Primera sesión F01 a F21, desbloqueos, banner de inicio y Adelanto | 4 | Primera Cadena antes del minuto 3; 3 200 SB antes del minuto 75 en una partida de prueba; el banner de inicio da una sola 5★ |
| Notificaciones y límite de gasto | 4 | Cada categoría se apaga sola; subir el límite tarda 72 horas |
| Taxonomía de analítica | 3A y 4 | Cada evento de la sección 14 está en `docs/analitica.md` y se emite desde el bus |
| Altura, Ofrenda, encargos, logros, Títulos, Grimorio, Diezmo, Perlas, correo, eventos, salinas, luna, amigos e Invitada | 5 | La versión simulada de 6 semanas da 70 tiradas gratis; una prueba de datos confirma que ningún logro ni encargo cita una bruja que solo sale del gacha |
| Contenido: textos de las siete salinas, 150 logros, charlas de las Termas, regalos y Recuerdos | 6 | El validador de datos pasa |

## 16. Valores TUNING, preguntas y contradicciones

### 16.1 Valores TUNING

| Valor | Propuesta | Sección de esta propuesta |
| --- | --- | --- |
| Minuto de la primera Cadena | 2:00 a 2:20 | 2.1 |
| Pistas de la Partitura en el tutorial | A los 15 s y a los 30 s | 2.4 |
| Correo de bienvenida | 1 600 SB | 2.6 |
| SB por misión del Capítulo de llegada | 160 (Prólogo y misiones 1 a 4) | 2.6 |
| SB por logro de *Primeros pasos* | 80 × 6 | 2.6 |
| SB por Altura 2 a 5 | 80 × 4 | 2.6 |
| Minuto objetivo de la tirada 20 | 65 | 2.7 |
| Adelanto máximo del día 2 | 1 040 SB | 2.7 |
| Preinscripción | Hasta 1 600 SB y cosméticos | 2.6 |
| Calendario de llegada | 640 SB en 7 días, dentro de 30 | 2.8 |
| SB por región de lanzamiento | 800 | 2.9 |
| Curva de Altura | 600, 2 400, 5 000, 8 000, 14 000 y 20 000 por nivel en cada tramo de 10 | 4.2 |
| Ofrenda | 100 almitas = 1 de Crecimiento; la primera del día ×2 hasta 20 000 | 4.1 |
| Bendición del Fresno | +1 % por cada 10 de Altura, tope +5 % | 4.3 |
| Hito de Ofrenda | Cada 500 000 almitas | 4.4 |
| Encargo diario | 15 SB, 100 de Crecimiento, 100 de experiencia y 5 000 almitas | 5.1 |
| Encargo semanal | 300 de Crecimiento, 1 000 de experiencia, 15 000 almitas y 10 Sal Cruda | 5.4 |
| Ingreso diario de almitas supuesto | ~100 000 para quien juega 30 minutos | 5.5 |
| Cosechas | 3 al día, tope 9 | 6.4 |
| Recompensa por Cosecha | 10 a 40 Sal Cruda y 4 000 a 10 000 almitas según el nivel | 6.3 |
| Transmutación | 3 por 1; domingo 2 por 1 | 6.5 |
| Reinicio | 04:00; Américas UTC−6 y Asia UTC+9 | 6.6 |
| Ventana lunar | 3 días por fase | 7.1 |
| Luna llena | +25 % de almitas y +50 % de Confianza al conversar | 7.2 |
| Grieta de luna nueva | 1 al día, 2 Sal Negra y 10 000 almitas, +5 Paralaje | 7.3 |
| SB de logros de lanzamiento | 4 800 en unos 150 logros | 8.1 |
| Pasivas de Títulos | ±5 % o +1 a una estadística | 8.3 |
| Tope de nivel de la Invitada | Tu bruja más alta + 10 | 9.3 |
| Recompensa por prestar | 500 almitas y 20 de Crecimiento por uso, tope 20 usos al día | 9.4 |
| Amigos | 30, 40 y 50 en Altura 15, 30 y 45; 20 solicitudes al día | 4.3 y 9.5 |
| Confianza | 4 000 para el nivel 10; conversar 20, regalo 10 a 40, combate 5 | 10.3 |
| Paralaje | Fuentes +3 a +10; baño −20 al día | 10.5 |
| Agua termal por visita | 1, tope 5 al día | 10.6 |
| Grimorio | 42 días, 50 niveles de 1 000; 480 SB gratis y 1 280 premium | 11.1 y 11.2 |
| Precios | Diezmo 4.99 USD (99 MXN); Grimorio 9.99 USD (199 MXN) | 11.4 |
| Perlas | 60 por dólar más bono del 0 al 25 %; seis paquetes | 12.2 |
| Precios de cosméticos | 160 a 1 980 Perlas | 12.3 |
| Límite para menores de 18 | 50 USD al mes por defecto | 12.5 |
| Compensaciones | 300 por versión; +80 por hora extra; 160 por cada 2 h de caída | 13.2 |
| Moneda de evento | 3 000 ganables; tienda de 2 700 | 13.3 |
| Notificaciones | 1 no transaccional al día; silencio de 22:00 a 09:00 | 13.5 |

### 16.2 Preguntas para el diseñador

| ID | Pregunta | Mi recomendación |
| --- | --- | --- |
| MP-01 | ¿Cuántas regiones salen al lanzamiento, y cómo se asigna la región inicial si son menos de 12? | Si son 12, tu signo es tu región. Si son menos, las cuatro cardinales (Aries, Cáncer, Libra y Capricornio) y el signo elige por elemento |
| MP-02 | ¿Qué 4★ se reclutan gratis por historia? | Cempa en el Prólogo, más la jaguar (Sol) y la lince (Júpiter), para que las gratis cubran los siete planetas |
| MP-03 | ¿La 5★ elegida sale en la tirada 20 (minuto 65) o en la 10 (minuto 17)? | En la 20: llega justo antes del jefe del capítulo y el jugador ya sabe qué vale. Probar la 10 con una prueba A/B en el lanzamiento suave, midiendo D1 y D7 |
| MP-04 | ¿Se aprueba el Adelanto del día 2? | Sí: hace real la promesa de la sección 2 sin regalar Sales nuevas |
| MP-05 | ¿El Grimorio gratuito da 480 SB, restadas de la línea de eventos? | Sí: el pase gratuito tiene que sentirse como tiradas, y el total se queda en 70 |
| MP-06 | ¿Hay Confianza por bruja? | Sí, sin estadísticas. Es el sistema de apego más barato de producir |
| MP-07 | ¿Chat libre? | No al lanzamiento. Revisar en la versión 1.3 con los datos de reportes |
| MP-08 | ¿Servidores Américas (UTC−6) y Asia (UTC+9) con reinicio a las 04:00? ¿Habrá Europa? | Sí a los dos. Europa con UTC+1 solo si hay lanzamiento ahí |
| MP-09 | ¿Los días del Diezmo sin cobrar se pierden? | Sí, como es costumbre en la industria, con contador visible y aviso. La alternativa amable es guardar 3 días |
| MP-10 | ¿Hay Marea de Sal (nivel de mundo)? | Sí: sin ella, el orden libre de regiones obliga a escalar cada región a mano |
| MP-11 | ¿Relectura al final de la historia o antes? | Al final, como dice la 13.5 |
| MP-12 | ¿Precios regionales más bajos en Latinoamérica? | En el lanzamiento suave, los niveles de cada tienda. Revisar después con datos de conversión |
| MP-13 | ¿Grimorio de lujo a 19.99 USD? | No al lanzamiento: menos productos se entienden mejor |
| MP-14 | ¿Ajuste "No pasar de Paralaje Medio"? | Sí, como accesibilidad para quien no quiere horror |
| MP-15 | ¿Las emergencias grandes se compensan aparte de las 440 de la 16.3? | Sí, y no cuentan como ingreso planeado |

### 16.3 Contradicciones del GDD

Nuevas, no listadas en HUECOS:

| ID | Secciones | Problema | Recomendación |
| --- | --- | --- | --- |
| MC-01 | 11.1 y 16.3 | La 11.1 dice que las Sales salen también de "logros" y del "pase gratuito", pero la tabla de la 16.3 no tiene esas filas | Pase gratuito: 480 SB dentro de las 70, restadas de eventos. Logros nuevos de cada versión: dentro de "Historia y exploración nuevas". Logros de lanzamiento: reserva de cuenta nueva (§2.9) |
| MC-02 | 7.7 y 17 contra 10.7 y 22.1 | La 7.7 y la 17 hablan de Tránsitos semanales que favorecen planetas distintos cada semana; la 10.7 y la 22.1 dicen que el Tránsito cambia cada seis semanas | Dos capas: el Tránsito de temporada (6 semanas) y un "cielo de la semana" pequeño que favorece dos planetas |
| MC-03 | 12.2 y 16.4 | Con todas las 3★, la tirada da recetas y Sal Cruda; pero las tiradas se compran y la 16.4 prohíbe vender sal | Recetas y Polvo Astral en lugar de Sal Cruda |
| MC-04 | 6 contra 22.1 y 23.3 | El signo elige entre 12 regiones iniciales, pero la 23.3 construye dos y la 22.1 suma regiones por versión | Decidir MP-01 |
| MC-05 | 9.9 contra 2 y 22.3 | "Combate automático desde el principio" deja que la primera Cadena la haga la máquina | Auto desde el final del combate 2 |
| MC-06 | 1 y 10.1 | La 1 habla de "un anillo del Fresno" de 15 a 25 min; la 10.1 llama Ascenso corto a una zona de 15 a 20 min | Usar "una zona (Ascenso corto), 15 a 20 min" |
| MC-07 | 10.6 y 10 | Si la Ofrenda fuera la única fuente del nivel de cuenta, la cuenta no subiría antes de abrir el Fresno, y la Ofrenda competiría con subir brujas | Crecimiento por jugar más la Ofrenda, con el esqueje en las Termas (§4) |
| MC-08 | 13.1 y 13.4 | La 13.1 pone Títulos en la ventana de una bruja; la 13.4 los describe con pasiva, como si fueran de la Calculista | Dos clases: de Calculista con pasiva y de bruja sin estadísticas (§8.3) |
| MC-09 | 1 y 11.3 | La 1 fija sesiones de 3 a 5 min (diarias) y de 15 a 25 (Fresno), pero la 11.3 pide "nivel 70 en siete días de juego diario" sin decir cuánto es. Con la escala actual son unas 230 000 almitas al día (H-01) | Definir "juego diario" como 30 minutos en la tabla de ingresos de H-01 |
| MC-10 | 16.4 | Se vende el Grimorio premium, pero no se pueden vender almitas, sal, materiales ni Savia. No es un error, pero obliga a que el premium solo tenga Sales y cosméticos, y prohíbe vender niveles | Reglas de §11.2 |

Ya están en HUECOS y esta propuesta las resuelve así:

| ID | Cómo queda |
| --- | --- |
| C-04 | Meta de 2 horas de juego y Adelanto del día 2 (§2.7) |
| C-06 | El Juicio I pide la bruja en nivel 40 (§3) |
| C-17 | La Invitada no está en el campo; se usan sus datos para la Regencia y el Cierre (§9.3) |
| C-24 | El banner de inicio tiene contador propio (§2.5) |
| C-25 | Diezmo y Grimorio premium se asientan como SB compradas; versión de Bélgica en §12.6 |
| C-27 | El +15 % del día solo para el planeta dominante (§6.1) |
| H-15 | Paralaje de 0 a 100 con bandas en 25, 50 y 80; el baño baja 20 al día (§10.5) |
