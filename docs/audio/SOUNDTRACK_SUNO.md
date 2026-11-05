# Banda sonora con Suno

Dirección musical, lista de cues y prompts de Suno para *Sal y Almas: La Odisea del Fresno*. El GDD manda sobre este documento; aquí mandan sus secciones 5, 6, 7.5, 9, 10, 12.5, 13.5, 16.1, 18, 20, 22 y 24.

Este documento cubre **qué música se genera y cómo se genera**. Cómo entra al juego (eventos de FMOD, buses, mixer, efectos, voces habladas, la Desafinación como DSP y la sonoridad final) está en [`INTEGRACION_AUDIO.md`](INTEGRACION_AUDIO.md). No se repite aquí.

**Contrato común con `INTEGRACION_AUDIO.md`**

| Tema | Acuerdo |
| --- | --- |
| Middleware | FMOD Studio en Unity |
| ID de cada cue | `mus_<contexto>_<nombre>`, en snake_case y sin acentos |
| Datos de cada cue | BPM, tonalidad, compás y longitud del bucle en compases |
| Capas del GDD 20.3 | Base, Percusión, Voz y Coro se entregan como stems alineados de **una misma generación**. La Desafinación no se genera: es un DSP en FMOD |
| Parámetros de FMOD | `Tension`, `Salinidad`, `Paralaje`, `EnCadena`, `AscendenteActivo`, `EsJefe` |
| IDs de bruja | `marisal`, `selenia`, `solenne`, `cervanda`, `tormentina`, `cupria`; Oficialas `cempa` (la coyota) y `jaguar` (provisional, aún sin nombre) |

---

## 1. Resumen

- **66 cues para el lanzamiento:** 22 del vertical slice y 44 de lanzamiento. Además hay 14 familias posteriores que se producen con *Cover* a partir de estas pistas.
- **60 fichas de prompt** con 63 prompts listos para copiar (la ficha P05 trae cuatro de *Sounds*). Cuatro fichas producen dos cues cada una desde la misma generación.
- **Modelo:** Suno v6 en plan Premier, con una cuenta de la empresa. El plan gratuito no sirve: lo que generes ahí no se puede usar comercialmente nunca.
- **Melodías humanas para lo que se repite:** el tema principal y los ocho leitmotivs conviene que los escriba una persona. Se suben a Suno como audio y se arreglan con *Cover*. Así los temas se reconocen entre pistas y la melodía sí tiene autor humano (sección 8).
- **Voz humana para lo cantado más importante:** tema principal, Termas de noche, finales, créditos y Ascensiones III de las Regentes. El GDD 20.1 ya planeaba una vocalista y un lingüista; mi recomendación es mantenerlos.

### Orden de producción

Primero lo que necesita el vertical slice (Fase 3C de `docs/HOJA_DE_RUTA.md`, con pistas temporales), después el lanzamiento (pistas finales antes de la Fase 4).

| Lote | Qué se genera | Por qué en este orden |
| --- | --- | --- |
| 0. Calibración (1 o 2 días) | Tres pruebas del ADN (sección 7.1) | Medir cómo responde v6 al ADN, a los BPM, a la separación de voz y coro y al interruptor de instrumental antes de gastar en serio |
| 1. Ancla | P01 `mus_titulo_tema` | Fija el sonido de todo el juego. De aquí sale la Persona de estilo y la referencia de mezcla |
| 2. Combate | P07 combate y Partitura, P08 y P09 jingles, P10 jefe de Casa | Es lo que más se escucha: un combate dura de 60 a 90 s y se juegan cientos |
| 3. Termas y tirada | P02, P03, P04, P05, P06 | Las Termas son la pantalla de inicio. La tirada es el momento de la compra y tiene que emocionar |
| 4. Regiones del slice | P11 Aries, P12 Capricornio | Las dos regiones jugables de la Fase 3B |
| 5. Fresno y Marisal | P13, P14, P15, P16, P17 | Raíces y su jefa cierran el bucle del slice. Marisal (Ascendente Aries) es la 5★ de muestra y su leitmotiv suena en su emergencia |
| 6. Lanzamiento | P18 a P60, por bloques: combate avanzado, Fresno, regiones, horror, crafting, leitmotivs, Ascensiones, finales | Contenido de la Fase 6. Los finales y créditos al final porque son variaciones del tema principal |
| 7. Posterior | Familias de la sección 5.3 | Se producen con *Cover* por versión de seis semanas |

---

## 2. Suno a noviembre de 2026: lo que verifiqué

Busqué en la web el 5 de noviembre de 2026. **No pude abrir suno.com ni sus páginas de ayuda desde esta sesión** (el dominio está bloqueado en la red del entorno), así que lo oficial lo confirmé por los resultados del buscador y lo demás por fuentes secundarias. Antes del lote 0, abre en la app la página de precios, los términos y las notas de versión, guárdalas en PDF con fecha y compáralas con esta tabla.

| Tema | Qué encontré | Confianza | Fuente |
| --- | --- | --- | --- |
| Modelo | **v6**, lanzado el 9 de septiembre de 2026, en tres versiones: v6 (principal) y v6-wild (experimental), solo en Pro y Premier; v6-mini, en el plan gratuito. Los modelos anteriores ya no generan. Según la prensa, v6 se entrenó con catálogo licenciado de Warner, BMG y Believe | Alta | [Suno: Introducing v6](https://suno.com/blog/introducing-v6), [DataNorth](https://datanorth.ai/news/suno-launches-v6-v6-wild-and-v6-mini), [Boing Boing](https://boingboing.net/2026/09/16/mixed-reviews-for-ai-music-generator-sunos-industry-approved-new-model.html) |
| Límites de texto | Estilo: 1,000 caracteres. Excluir estilos: 1,000. Letra: 5,000 | Media | [HookGenius](https://hookgenius.app/learn/suno-character-limits/), [MySoniq](https://www.mysoniq.com/blog/suno-prompts) |
| Duración | Hasta 8 min por generación según varias fuentes; lo normal son 2 a 4 min. Hay un control de Duración (Auto o Personalizada) | Baja | [GenMediaLab](https://www.genmedialab.com/news/suno-v6-launch/), [Jack Righteous](https://jackrighteous.com/en-us/blogs/guides-using-suno-ai-music-creation/inside-suno-v6-create-simple-advanced-sounds-guide) |
| Metaetiquetas | `[Intro]`, `[Verse]`, `[Pre-Chorus]`, `[Chorus]`, `[Bridge]`, `[Outro]`, `[Instrumental]` y `[Break]` funcionan como guía, no como orden: no hay una gramática oficial. Cada etiqueta va sola en su línea. **No encontré confirmación de que `[End]` funcione** | Media | [Jack Righteous: meta tags](https://jackrighteous.com/pages/suno-ai-meta-tags-guide), [HookGenius: metatags](https://hookgenius.app/learn/suno-metatags-complete-list/) |
| Controles | Weirdness (de Safe a Chaos) y Style Influence (de Loose a Strong), ambos en 50 por omisión. **Variety** es nuevo: reescribe tu estilo antes de generar, y el FAQ oficial dice bajarlo a 0 para conservar tus etiquetas. **Max Mode** gasta más créditos para acertar mejor; se recomienda en piezas de más de 2 min, covers fieles y voz constante. Audio Influence aparece con audio subido. Escribir "Weirdness 20 %" en el estilo no hace nada | Alta para Variety y Max Mode; media para lo demás | [Suno: v6 FAQ](https://help.suno.com/en/articles/13924481), [PopularAITools](https://popularaitools.ai/blog/suno-v6-variety-style-influence-weirdness-2026) |
| Excluir estilos | Sigue en Opciones avanzadas, con 1,000 caracteres | Media | [HookGenius](https://hookgenius.app/learn/suno-character-limits/) |
| Stems | **Auto Split:** hasta 12 stems por 50 créditos, en Pro y Premier. **Split from Mix:** un instrumento más su complemento, 10 créditos por stem. **Advanced Split:** eliges entre casi 100 instrumentos, solo Premier. Los stems se **regeneran** con el modelo: no son la multipista original y su suma puede no ser idéntica a la mezcla | Alta | [Suno: How do I get stems?](https://help.suno.com/en/articles/13925185), [Suno: Advanced Stem Separation](https://help.suno.com/en/articles/12702337) |
| Extend, Cover y Replace Section | Siguen en v6, dentro de Remix & Edit. Replace Section es solo para Pro y Premier | Alta | [Suno: What's new in v6?](https://help.suno.com/en/articles/13924801) |
| Persona | El botón Voices reemplazó a Personas; las **Personas de estilo** siguen dentro de Voices. Voices (clonar una voz, desde v5.5) pide verificar en vivo que la voz es tuya y no funciona en pistas instrumentales | Media | [Suno: What's new in v6?](https://help.suno.com/en/articles/13924801), [Sonilo](https://sonilo.com/blog/suno-v5-5-voices-how-to) |
| Sounds | Modo de Create para one-shots y loops: eliges tonalidad y BPM y describes el sonido. Sirve para stingers | Alta (la función); baja (sus límites) | [Suno: Sounds](https://suno.com/release-notes/make-loops-and-samples-from-scratch-with-sounds), [Kyeo](https://kyeo.ai/en/models/suno-sounds) |
| Suno Studio | Studio 2.0 salió el 13 de agosto de 2026: DAW en el navegador con MIDI, efectos y sintes. Solo Premier. Exporta multipistas y stems a 32 bits y 48 kHz sin tope de descargas | Media | [Suno: Studio 2.0](https://blog.suno.com/release-notes/studio-2), [Dubspot](https://blog.dubspot.com/suno-studio-2-0) |
| Subir audio | Hasta 60 s en el plan gratuito y hasta 8 min en Pro y Premier; luego se puede extender o hacer cover | Alta | [Suno: Audio Uploads](https://help.suno.com/en/articles/6141569) |
| Nombres de artistas | v6 bloquea nombres de artistas reales, marcas y términos protegidos en el prompt | Media | [dev.ua](https://dev.ua/en/news/suno-v6-1788958562), [Jack Righteous](https://jackrighteous.com/en-us/blogs/guides-using-suno-ai-music-creation/suno-prompt-contained-inappropriate-material) |
| BPM y tonalidad | El BPM del prompt orienta pero no se respeta exacto: suele caer unos BPM arriba o abajo, y en una prueba se pidieron 118 y salieron 117.5 | Media | [Jack Righteous: tempo](https://jackrighteous.com/en-gb/blogs/guides-using-suno-ai-music-creation/master-tempo-in-music-comprehensive-guide-to-tempo-tags), [Challenging Voice](https://www.challengingvoice.com/suno-v6-api-podcast-audio-kit/) |
| Planes | Pro: 10 USD al mes con 2,500 créditos. Premier: 30 USD al mes con 10,000 créditos. Una generación estándar da dos canciones por unos 10 créditos. Los créditos no se acumulan | Media | [DIYAI](https://diyai.io/ai-tools/audio-generation/suno-pricing/), [Krater](https://krater.ai/pricing/suno), [Suno: v6 FAQ](https://help.suno.com/en/articles/13924481) |
| Descargas | Desde el 3 de septiembre de 2026: gratis, 7 de por vida y solo para uso personal; Pro, 20 al mes; Premier, 60 al mes e ilimitadas desde Studio. El MP3, el WAV y los stems de una canción cuentan como una sola descarga | Media | [Jack Righteous: límites](https://jackrighteous.com/en-us/blogs/guides-using-suno-ai-music-creation/suno-premier-60-song-download-limit-september-3-2026), [Mubert](https://mubert.com/blog/suno-download-caps), [UseSuno](https://usesuno.com/articles/suno-download-limits-september-3-2026/) |
| Uso comercial | Solo lo generado **con plan de pago activo**. Con los términos del 3 de septiembre de 2026, además necesitas una **descarga permitida** por el canal de Suno. Lo hecho en el plan gratuito es no comercial y **no se vuelve comercial** al suscribirte. Tras el acuerdo con Warner, Suno cambió "eres dueño de tus canciones" por "tienes derechos de uso comercial" | Media | [Music in Africa](https://www.musicinafrica.net/magazine/suno-adjusts-ai-music-ownership-terms-after-warner-music-partnership), [Dynamoi](https://dynamoi.com/learn/ai-music-distribution/suno-commercial-rights-explained), [Veena](https://www.veena.studio/blog/suno-commercial-use-rules) |
| Disqueras | **Warner** llegó a un acuerdo y una alianza con Suno el 25 de noviembre de 2025: modelos licenciados en 2026 y descargas solo con cuenta de pago. **Universal y Sony** siguen en litigio en EE. UU.; en 2026 ampliaron la demanda a unas 60,000 grabaciones (las cifras de las fuentes no coinciden). **GEMA** ganó en primera instancia en Múnich el 31 de julio de 2026 y Suno evalúa apelar. **Koda** (Dinamarca) demandó en noviembre de 2025 | Media-alta | [Music Business Worldwide: Warner](https://www.musicbusinessworldwide.com/warner-music-group-settles-with-suno-strikes-first-of-its-kind-deal-with-ai-song-generator/), [Music Business Worldwide: GEMA](https://www.musicbusinessworldwide.com/suno-infringed-copyright-in-gema-case-german-court-rules/), [Forbes](https://www.forbes.com/sites/virginieberger/2026/08/05/suno-lost-to-gema-why-the-ruling-should-worry-ai-music-companies/), [CourtDocket](https://courtdocket.org/suno-lawsuit-explained-warner-settlement-umg-and-sony-next/), [ChartLex](https://www.chartlex.com/blog/business/music-industry-ai-lawsuits-tracker-2026) |
| Oficina de Derechos de Autor de EE. UU. | Sigue vigente la Parte 2 de su informe (29 de enero de 2025): hace falta autoría humana; **los prompts solos no bastan**; sí se protege lo humano que se percibe en el resultado y la selección, el arreglo o la modificación creativos; al registrar hay que declarar y excluir lo generado. No encontré guía nueva en 2026. La Corte Suprema se negó a revisar *Thaler v. Perlmutter* el 2 de marzo de 2026, así que el requisito de autoría humana sigue firme | Alta | [Copyright Office, Parte 2 (PDF)](https://www.copyright.gov/ai/Copyright-and-Artificial-Intelligence-Part-2-Copyrightability-Report.pdf), [Library of Congress](https://newsroom.loc.gov/news/copyright-office-releases-part-2-of-artificial-intelligence-report/s/f3959c36-d616-498d-b8f9-67641fd18bab), [SCOTUSblog](https://www.scotusblog.com/cases/case-files/thaler-v-perlmutter), [JD Supra](https://www.jdsupra.com/legalnews/supreme-court-denies-review-in-ai-2865518/) |

**Lo que no pude verificar**

- El texto vigente de los términos de Suno, su página de precios y sus notas de versión, leídos de primera mano.
- La duración máxima exacta en v6 y si `[End]` corta la canción.
- Cuánto cuesta Max Mode en créditos.
- Si Suno muestra una semilla por generación. No encontré nada; asumo que no, y por eso la trazabilidad usa el ID y el enlace de la canción.
- Si una exportación desde Studio cuenta como "descarga permitida" para los derechos comerciales.
- Si los términos de Suno te indemnizan ante una reclamación de terceros. Asume que no.
- Si Apple tiene una regla específica para audio generado con IA en juegos.
- El formato exacto del WAV que entrega Suno (frecuencia de muestreo y profundidad).

---

## 3. Dirección musical

### 3.1 La idea en una página

La banda sonora suena a **música de cámara acústica grabada en una capilla de piedra pequeña**. Pocos instrumentos, tocados por personas, cerca del micrófono. Es la melancolía hermosa del pilar 6: un mundo que se está salando hasta morir y brujas que se quieren.

Tres reglas sostienen todo:

1. **Acústico y cercano.** Guitarra, laúd, salterio, violonchelo, flautas, campanas y cuencos, coro y una voz. Nada de orquesta épica, sintes ni batería de rock.
2. **La voz es el alma.** Una mezzosoprano canta en Salmódico lo que es magia cantada: Ascensiones, temas de región, finales. La Cadena es magia silente (GDD 7.5): ahí la voz solo vocaliza, sin palabras.
3. **El metal es la sal y los planetas.** Las campanas y los cuencos dicen qué planeta suena (sección 3.4). La sal se oye como brillo: trémolo de salterio y campanas agudas.

**Lo que tomamos de NieR y lo que no** (GDD 5, 20 y 24)

| Tomamos la idea general | No tomamos nunca |
| --- | --- |
| Piezas acústicas y melancólicas con voz | Melodías, progresiones o arreglos reconocibles |
| Un idioma inventado que suena a lenguas reales gastadas por los siglos | El nombre de su idioma, sus letras o su fonética concreta |
| Cada canción parte de la lengua de su región | El timbre o el estilo de su cantante |
| Música por capas que reacciona al juego | Títulos, nombres de compositores o del juego en cualquier prompt |

Ningún prompt nombra artistas, juegos, compositores, cantantes ni franquicias. Suno los bloquea, y además el estilo se describe, no se copia.

### 3.2 Paleta e instrumentos reservados

La paleta base del GDD 20.2 es fija. Cada cue la usa con el mismo papel.

| Instrumento | Papel en todo el juego | Regla |
| --- | --- | --- |
| Guitarra de nylon | Armonía y pulso | Está en casi todo |
| Laúd | Contrapunto y melodías secundarias | Laúd europeo; el laúd árabe es de Solenne |
| Salterio (punteado o martillado) | Brillo, trémolos, la sal en el aire | En regiones puede sonar como su pariente local: qanun, cimbalom o santur |
| Violonchelo | Bajos, pedales y contramelodías graves | **Solo lleva la melodía en los cues de Marisal.** En el ADN se pide *low cello* |
| Flautas de madera | Melodías y aire | Flauta traversa de madera. **La flauta dulce es de Tormentina** |
| Campanas y cuencos de metal | Color planetario, finales de frase, transiciones | La campana del Spark es un efecto (ver `INTEGRACION_AUDIO.md`) |
| Coro mixto de cámara | Momentos cumbre | Ver la regla del coro abajo |
| Percusión | Tambores de marco, bombo grave, pandero, palmas | Sin batería, sin caja de rock, sin platillos de choque |
| Voz solista | Mezzosoprano adulta, timbre puro | Una sola voz para todo el juego (sección 7.3) |

**Instrumentos reservados.** Cada uno identifica a una bruja. No lleva la melodía en ningún otro cue.

| Bruja | Instrumento solista | Dónde puede sonar |
| --- | --- | --- |
| `marisal` | Violonchelo solista | Su leitmotiv, su Ascensión III, su Juicio y sus escenas |
| `selenia` | Armonio | Ídem |
| `solenne` | Laúd árabe (oud) | Ídem |
| `cervanda` | Viola da gamba | Ídem |
| `tormentina` | Flauta dulce | Ídem |
| `cupria` | Arpa | Ídem |
| `cempa` | Jarana jarocha | Ídem |
| `jaguar` | Ocarina y tambor de madera | Ídem. La región de Capricornio usa ocarinas como color, pero la melodía principal de la región no va en ocarina sola |

**Regla del coro.** El GDD 20.3 reserva el coro completo para la Ascensión III y los jefes. Lo extiendo a los momentos cumbre que no son combate: tema principal, emergencia 5★, transformación de fase, eclipse, revelaciones de horror, finales y créditos. **Nunca** suena coro en exploración, Termas ni menús; ahí canta la voz solista o un grupo de dos o tres voces.

**Invitados regionales.** Cada región suma como máximo dos instrumentos de su inspiración cultural (sección 6 del GDD). Están en la ficha de cada región.

### 3.3 Armonía y tono

- **Modal antes que tonal.** Dórico, eolio y frigio son la casa. El mayor es luz: Termas de día, victoria, 5★.
- **La tercera de Picardía es nuestra firma.** Lo menor que termina en mayor es esperanza: emergencia 5★, victoria, transformación a Quintaesencia.
- **Pedales y bordones.** Graves sostenidos de violonchelo, zanfona o armonio. Son el suelo de sal sobre el que todo camina.
- **La disonancia informa.** La segunda menor del frigio es amenaza. Los clústeres y la afinación inestable son solo del horror.
- **Nada de progresiones de pop** (I–V–vi–IV) ni de "épica de tráiler" con golpes y *braams*.
- **Melodías cantables**, de una octava como máximo, que alguien pueda tararear después de jugar.
- **Las frases respiran.** Silencios cortos en todo; silencios largos en el horror.

### 3.4 Cómo suena cada planeta

El planeta define el metal, el modo y el gesto. Se usa en Ascensiones, transformaciones, Jardinera de temporada, eventos del día planetario y en el color de cada Regente.

| Planeta | Metal | Timbre de metal | Modo | Gesto musical |
| --- | --- | --- | --- | --- |
| Sol | Oro | Campanas agudas y brillantes, crótalos | Lidio | Arpegios que suben, trémolo de salterio |
| Luna | Plata | Copas y cuencos de cristal frotados | Eolio | Arpegios lentos, armonía suspendida |
| Marte | Hierro | Yunque y campanas de hierro secas | Frigio | Ritmo marcado, arco duro de violonchelo |
| Mercurio | Azogue | Campanillas rápidas, platillos frotados | Dórico que cambia a mixolidio | Figuras rápidas y nerviosas, flauta |
| Venus | Cobre | Cuencos de cobre cálidos | Jónico con cuartas suspendidas | Arpegios suaves, voz cercana |
| Júpiter | Estaño | Lámina de trueno, gong medio | Mixolidio | Tambores graves, coro al unísono |
| Saturno | Plomo | Campanas graves y apagadas, gong grave | Eolio con quinta disminuida | Notas largas, silencios, tempo lento |
| Sal Negra | — | Cuencos frotados hasta que gimen | Sin centro | Pedal grave, clústeres, voz al revés |

### 3.5 Cómo suena el horror

El GDD 20.4 pide coro desafinado, Salmódico al revés, graves casi inaudibles y silencios largos, y que los Huéspedes suenen como señales de radiotelescopio. Cada cosa se produce en un lugar distinto:

| Elemento | Cómo se produce | Dónde |
| --- | --- | --- |
| Coro desafinado | Clústeres pedidos en el prompt más el DSP de Desafinación | Suno y FMOD |
| Salmódico al revés | Se escriben las palabras al revés en la letra (sección 4.6) y además se invierte el audio del stem de voz en la edición | Suno y edición |
| Graves casi inaudibles | Retumbo bajo 40 Hz más un armónico entre 100 y 200 Hz, porque **las bocinas de un celular no reproducen graves**; sin ese armónico, el jugador sin audífonos no oye nada | Suno y edición |
| Silencios largos | La música se apaga de verdad. El silencio es un estado de FMOD, no una pista | FMOD |
| Señales de radiotelescopio | Estática, clics lentos tipo púlsar, tonos de onda corta. Suena poco en la música y se refuerza con diseño sonoro | Suno (poco) y `INTEGRACION_AUDIO.md` |

**Reglas de tono:** pavor antes que susto. Nada de golpes fuertes repentinos (GDD 6.5). El horror nunca entra a las Termas. La sonoridad del horror va por debajo del resto, no por encima.

### 3.6 Cómo suenan las Termas

- Zona segura: **sin capas de tensión**, sin percusión fuerte, sin Desafinación y sin capa de Paralaje, aunque el Paralaje esté alto.
- De día: vals cálido en Fa mayor, guitarra, laúd y flauta, humor tierno.
- De noche: canción de cuna en Re menor que se abre a Fa mayor, voz casi susurrada en Salmódico. Su letra habla de agua que hace olvidar (el baño baja el Paralaje, GDD 6.5).
- Día y noche comparten armadura (Re menor es la relativa de Fa mayor), así que el cruce entre las dos no choca.
- El Yunque y el Caldero viven en la Tortuga: también son zona segura.

### 3.7 Tempos y tonalidades de referencia

**La casa es Re.** El tema principal, el combate y las Raíces del Fresno giran alrededor de Re. Las transiciones más frecuentes comparten tonalidad o son vecinas:

| Familia | Tonalidad | Tempo | Compás | Por qué |
| --- | --- | --- | --- | --- |
| Tema principal, finales y créditos | Re dórico | 72 | 4/4 | La casa |
| Termas | Fa mayor y Re menor | 90 de día, 50 de noche | 3/4 y 6/8 | Relativas: cruzan sin chocar |
| Tirada | Pedal de Re que resuelve | 80 | 4/4 | La 5★ resuelve en Re mayor (Picardía) |
| Combate, Partitura y Éxtasis | Re menor | 128 | 4/4 | Mismo tempo y tonalidad: FMOD cambia entre ellos en la barra de compás |
| Victoria y derrota | Re mayor y Re menor | 96 y 60 | 4/4 y 3/4 | Cierran el combate en la casa |
| Jefes | Frigio, de Mi a Sol | 120 a 150 | 4/4 y 3/4 | Más tensos y rápidos que el combate |
| Fresno | Re, Mi, Sol y La, de las raíces a la copa | 60 a 80 | Varía | La tonalidad sube contigo por el árbol |
| Regiones | Según su cultura | 60 a 120 | Varía | Cada región tiene su modo |
| Leitmotivs | Uno por bruja | 60 a 100 | Varía | Cada bruja tiene su tonalidad, y su Ascensión III la hereda |
| Menús | Sol dórico | 96 | 4/4 | Subdominante de Re: neutro entre Termas y combate |
| Horror | Sin centro | Libre o 75 | Libre o 4/4 | No debe sentirse en casa |

**BPM amigables.** Todos los tempos de la lista son números que dan una cantidad entera de muestras por pulso a 48 kHz (48,000 × 60 / BPM): 50, 60, 64, 72, 75, 80, 90, 96, 100, 120, 128, 144 y 150. Así cada compás cae en una muestra exacta y los bucles no se desfasan. En 6/8 el BPM indicado es de **negra con puntillo** (dos pulsos por compás).

**Compases.** 4/4, 3/4 y 6/8. Evita los compases irregulares (7/8, 5/4): Suno no los sostiene con fiabilidad y un bucle irregular se rompe en FMOD.

### 3.8 El ADN de los prompts

Para que 60 generaciones suenen al mismo juego, todos los prompts repiten **las mismas frases de producción, palabra por palabra**. No las parafrasees ni las reordenes: el ADN funciona porque es idéntico.

**ADN de estilo (va en todos los prompts musicales).** 270 caracteres.

```text
Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls.
```

**ADN de voz (en los prompts con voz).** 124 caracteres.

```text
Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels.
```

**ADN de coro (en los prompts con coro).** 51 caracteres.

```text
Choir: small mixed chamber choir, soft and distant.
```

**ADN de exclusión (va completo en Excluir estilos, más lo propio de cada cue).** 263 caracteres.

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle
```

**Cómo se arma cada estilo:** una frase propia del cue (qué es y qué siente) + ADN de estilo + ADN de voz y de coro si aplica + arreglo propio + tonalidad, BPM y compás + forma. Con los tres ADN quedan unos 550 caracteres para lo propio; todos los estilos de la sección 6 caben en el límite de 1,000.

**La sala también es ADN.** No agregues reverb a la música en FMOD. Si una toma sale más seca que las demás, iguálala en la edición con una sola reverb de convolución de capilla pequeña, la misma para todo el juego.

---

## 4. El Salmódico para Suno

El Salmódico es la lengua de los cantos de Ascensión y, en la capa de horror, la lengua de las estrellas (GDD 20.1). Suena a español antiguo, latín, ladino y náhuatl gastados por los siglos. Esta guía es un punto de partida para Suno; **el lingüista que pide el GDD la fija y la amplía**.

### 4.1 Inventario de sonidos

| Tipo | Inventario | De dónde viene |
| --- | --- | --- |
| Vocales | a, e, i, o, u, siempre puras | Español y latín |
| Diptongos | ai, au, ei, ue, ua, ui, iu | Español |
| Consonantes comunes | p, t, c/qu (k), b, d, g, m, n, l, r, s, f (rara), v (suena como b), z (suena como s) | Español y latín |
| Consonantes de sabor | **tl**, **ts**, **sh**, **hu** (suena *w*), **cu** (suena *kw*) | Náhuatl y ladino |
| Grupos permitidos | tl, tr, pl, cl, bl, fr al inicio de sílaba; -tl al final de palabra | Náhuatl y latín |
| Finales de palabra | vocal, -n, -s, -l, -r, -c, -tl | Mezcla |
| Acento | Reglas del español: palabra que acaba en vocal, n o s, grave; las demás, agudas. Sin tildes | Así Suno la lee bien |

**Ortografía para Suno:** escribe *sh* en vez de *x* (Suno leería "ks") y *ts* en vez de *tz*. No uses *ñ*, *ll*, *th*, *w* ni *k*: Suno las pronuncia de formas distintas según el idioma que crea leer.

### 4.2 Reglas

1. **Ninguna palabra real completa** en español, latín, náhuatl, ladino, árabe, gaélico, nórdico, inglés, japonés ni coreano. Las partículas de una sílaba (e, na, ma) pueden coincidir; no importa.
2. **Gasta las raíces:** mezcla dos raíces de lenguas distintas o quita sílabas (sal + iztatl = *istal*).
3. **Nada sagrado ni litúrgico real:** ni *amen*, *kyrie*, *ave*, *aleluya*, *dies irae*, *teotl* ni nombres de dioses de ninguna religión.
4. **Revisa cada palabra nueva** en el diccionario de cada lengua de lanzamiento (español, inglés y japonés) y en coreano, por si dice algo ridículo u ofensivo.
5. **Nada de nombres reales:** ni personas, ni marcas, ni lugares actuales.
6. **Coherencia:** una palabra significa siempre lo mismo, aunque el jugador no lo sepa. Así los fans podrán descifrarla, como pasa con los idiomas inventados que funcionan.

### 4.3 Glosario base

| Salmódico | Sentido | Raíces que lo inspiran |
| --- | --- | --- |
| istal | sal | náhuatl *iztatl* + latín *sal* |
| yolma | alma | náhuatl *yollotl* (corazón) + *alma* |
| yolmi | almita | *yolma* + diminutivo *-i* |
| frashna | el Fresno | latín *fraxinus*, gastado |
| uarbe | árbol | *árbol*, *arbor* |
| estlali | estrella | *estrella* + náhuatl *citlalli* |
| ilcel | cielo | náhuatl *ilhuicatl* + *cielo* |
| nantre | madre | náhuatl *nantli* + *madre* |
| ermai | hermana | *hermana*, gastada |
| cuetor | volver, regresar | náhuatl *-cuepa* + ladino *tornar* |
| atlua | agua | náhuatl *atl* + *agua* |
| maresh | mar | *mar* con *sh* ladina |
| metzlu | luna | náhuatl *metztli* + *luna* |
| solatiu | sol | *sol* + náhuatl *tonatiuh* |
| tleugo | fuego | náhuatl *tletl* + *fuego* |
| micmor | muerte, morir | náhuatl *miqui* + latín *mors* |
| cuicant | canto, cantar | náhuatl *cuicatl* + latín *cantus* |
| remel | recuerdo, recordar | *recordar* + *memoria* |
| olvai | olvido, olvidar | *olvidar*, gastado |
| cosetl | cosecha | *cosecha* + *-tl* |
| neshcen | ceniza | náhuatl *nextli* + *ceniza* |
| amolib | libro | náhuatl *amoxtli* + latín *liber* |
| blaztac | blanco, en blanco | *blanco* + náhuatl *iztac* |
| nochual | noche | *noche* + náhuatl *yohualli* |
| lucetl | luz | *luz* + *-tl* |
| cochum | sueño, dormir | náhuatl *cochi* + latín *somnus* |
| eztre | sangre | náhuatl *eztli* + *sangre* |
| nelradi | raíz | náhuatl *nelhuayotl* + latín *radix* |
| ishoc | ojo, mirar | náhuatl *ixtli* + latín *oculus* |
| huemag | inmenso | náhuatl *huey* + latín *magnus* |

**Pronombres y partículas:** *neyo* (yo), *tuel* (tú), *nosotl* (nosotras), *e* (y), *na* (no), *ma* (que, ojalá), *ai* (¡oh!).

### 4.4 Gramática mínima

- **Orden libre**, con el verbo al final cuando se pueda: *Istal yolma cuetor* (la sal, el alma, vuelve).
- **Plural:** *-me* (*yolmime*, las almitas).
- **Mío:** prefijo *no-* (*nonantre*, mi madre).
- **Diminutivo y cariño:** *-i* (*yolmi*), y en Capricornio *-tsi*.
- **Negación:** *na* antes de la palabra (*na olvai*, no olvides).
- **Imperativo:** la palabra sola, repetida (*cuetor, cuetor*: vuelve).

Ejemplo:

> *Istal yolma, cuetor frashna. Na olvai, ermai, neyo cuicant.*
> La sal es alma: vuelve al Fresno. No olvides, hermana: yo canto.

### 4.5 Cómo escribirlo en el campo de letra

1. **Una frase por línea.** De 4 a 8 sílabas en tempos lentos; de 3 a 5 en los rápidos.
2. **Cada etiqueta de sección va sola en su línea**, entre corchetes. Si no, Suno la canta.
3. **El coro o los ecos van entre paréntesis:** `(Istal yolma)`. Suno suele cantarlo como voz de fondo; eso ayuda a que el stem de coro salga separado.
4. **Vocalizaciones sin palabras:** `Aaah`, `Ooh`, `Mmm`. Son las de la Cadena y de los momentos sin texto.
5. **Repite las líneas** en las secciones que serán bucle. Un bucle con texto distinto en cada vuelta se nota.
6. **Melismas:** un guion separa sílabas que quieres alargar (`cue-e-tor`). Úsalo poco.
7. **El estilo dice que es un idioma inventado** (lo hace el ADN de voz). Sin eso, Suno puede tratar de leerlo como inglés.
8. **Primera toma, primera revisión de pronunciación.** Si Suno lee mal una palabra, reescríbela como suena (*cuetor* → *kuetor* solo en la letra, nunca en el glosario) y anota el cambio en un diccionario de pronunciación junto a los metadatos (sección 7.8).

### 4.6 Variantes regionales

Como pide el GDD 20.1, cada región tiñe el Salmódico con la fonética de su inspiración. Es **sabor**: terminaciones y sonidos, nunca palabras ni textos reales.

| Región | Sabor fonético | Cómo se aplica | Ejemplo inventado |
| --- | --- | --- | --- |
| Aries | Castellano antiguo y ladino | *f-* inicial, *-iello*, *-ade*, *sh* | *Fierristal, fiyolmade, cuetoriello* |
| Tauro | Neerlandés | *-ijk*, *-en*, vocales dobles | *Olvaien istalijk, cochumen* |
| Géminis | Veneciano | *ze* como enlace, *-ae* | *Ermai ze ermai, ishocae cuetor* |
| Cáncer | Bretón | *-ek*, *-enn*, *-ou* | *Mareshenn, istalek, cuetorou* |
| Leo | Árabe andalusí | *al-*, *j* gutural, *q*, vocales largas | *Alistal jarqel, solatiu qetmaa* |
| Virgo | Latín eclesiástico | *-um*, *-is*, *-ae*, *-ibus* | *Cuicantum istalis, nantrae lucetlum* |
| Libra | Italiano renacentista | *-etta*, *-ella*, *-etto* | *Istaletta, yolmella, amolibetto* |
| Escorpio | Eslavo oriental | *-ka*, *-ya*, *zh*, *-ov* | *Micmorka, atluya, cuetorov* |
| Sagitario | Lenguas de la estepa | *-aar*, *-uun* | *Estlalaar, ilceluun, cuetoraar* |
| Capricornio | Náhuatl | Más *tl*, *ts*, *sh*; *-tli*, *-tsi* | *Istaltli huemag, metzlutsi cuetor* |
| Acuario | Nórdico e islandés (ver sección 10) | *hv-*, *kv-*, *-ur*, *-ir*, *ey* | *Hvestlur istaley, kvetor yolmir* |
| Piscis | Griego bizantino | *-os*, *-is*, *-ion*, *th* | *Cochumion maresis, atluathos* |
| Horror | Salmódico al revés | Cada palabra escrita de atrás para adelante | *ishoc huemag* → *cohsi gameuh* |

Si decides mantener el gaélico que dice el GDD 20.1 para Acuario, su sabor sería *-ach*, *-ean* y *bh* que suena *v*: *Istalach yolmean, cuetorach*.

---

## 5. Lista de cues

### 5.1 Capas y parámetros

**Capas** (columna "Capas" de la tabla):

| Clave | Capa | Qué stems de Suno la forman | Cuándo suena (GDD 20.3) | Parámetro de FMOD |
| --- | --- | --- | --- | --- |
| B | Base | Guitar, Bass, Strings, Keyboard, Woodwinds y el resto de lo que no es percusión ni voz | Siempre | — |
| P | Percusión | Drums y Percussion | En combate siempre. En regiones y Fresno, al subir la tensión | `Tension` |
| V | Voz | Lead Vocals (y Backing Vocals en cues sin coro) | Durante la Cadena y con un Ascendente activo; en regiones, desde la segunda vuelta | `EnCadena`, `AscendenteActivo` |
| C | Coro | Backing Vocals | Ascensión III y jefes | `EsJefe` |
| D | Desafinación | No se genera: DSP en el bus de música | Salinidad de 100 o más, o Paralaje alto | `Salinidad`, `Paralaje` |
| Estéreo | Mezcla completa | La mezcla de Suno, editada | Pistas sin capas | — |
| +solista | Instrumento reservado aislado | Split from Mix o Advanced Split | Firma de la bruja en la tirada y en la tarjeta | — |

Cómo usa la música cada parámetro. Los rangos, el suavizado y la implementación los fija `INTEGRACION_AUDIO.md`.

| Parámetro | Efecto en la música |
| --- | --- |
| `Tension` | En regiones y en el Fresno, sube la capa P en dos compases. En combate no se usa: P ya está activa |
| `Salinidad` | De 100 en adelante activa la Desafinación. En 150 hay transición a `mus_extasis_abisal` en la siguiente barra de compás |
| `Paralaje` | Medio: entra `mus_horror_capa_paralaje` muy baja. Alto: sube esa capa y activa la Desafinación. Máximo: además, la capa V suena invertida (versión invertida del stem, preparada en la edición). **Nunca en las Termas** |
| `EnCadena` | Entra la capa V (vocalización sin palabras: la Cadena es silente) |
| `AscendenteActivo` | Entra la capa V |
| `EsJefe` | Entra la capa C y se usan las regiones de fase de los jefes |

### 5.2 Tabla de cues

Prioridad: **VS** vertical slice, **L** lanzamiento, **P** posterior. Bucle en compases y su duración aproximada.

| Ficha | ID | Uso en el juego | BPM | Tonalidad | Compás | Bucle | Capas | Prioridad |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| P01 | `mus_titulo_tema` | Pantalla de título y tema principal; pista ancla | 72 | Re dórico | 4/4 | 32 (≈107 s) | Estéreo + V | VS |
| P02 | `mus_termas_dia` | Termas de día | 90 | Fa mayor con toques lidios | 3/4 | 48 (96 s) | Estéreo | VS |
| P03 | `mus_termas_noche` | Termas de noche | 50 (n. con puntillo) | Re menor que se abre a Fa mayor | 6/8 | 40 (96 s) | Estéreo | VS |
| P04 | `mus_tirada_suspenso` | Tirada: las sales caen. Dos regiones: tibia y hirviendo (pity suave) | 80 | Pedal de Re sin resolver | 4/4 | 8 + 8 (24 s cada una) | Estéreo | VS |
| P05 | `mus_tirada_vapor_blanco` | Vapor blanco, 3★ | 80 | Re menor | — | 2 s, sin bucle | Estéreo | VS |
| P05 | `mus_tirada_vapor_violeta` | Vapor violeta, 4★ | 80 | Si♭ mayor → Fa mayor | — | 3 s, sin bucle | Estéreo | VS |
| P05 | `mus_tirada_vapor_dorado` | Vapor dorado, 5★ | 80 | Re mayor | — | 4 s, sin bucle | Estéreo | VS |
| P05 | `mus_tirada_subida_dorada` | Subida sorpresa de violeta a dorado | 80 | Si♭ mayor → Re mayor | — | 5 s, sin bucle | Estéreo | VS |
| P06 | `mus_tirada_emergencia_5` | La 5★ emerge del manantial | 120 | Re mayor | 4/4 | 6 a 7 s, sin bucle | Estéreo | VS |
| P07 | `mus_combate_normal` | Combate normal | 128 | Re menor con acentos frigios | 4/4 | 32 (60 s) | B P V C + D | VS |
| P07 | `mus_cadena_partitura` | Pantalla de la Partitura (corte del Break de P07) | 128 | Re menor | 4/4 | 8 (15 s) | B + V | VS |
| P08 | `mus_jingle_victoria` | Victoria | 96 | Re mayor | 4/4 | 2 + cola (≈6 s), sin bucle | Estéreo | VS |
| P08 | `mus_combate_resultados` | Pantalla de resultados | 96 | Re mayor | 4/4 | 8 (20 s) | Estéreo | VS |
| P09 | `mus_jingle_derrota` | Derrota | 60 | Re menor | 3/4 | 3 + cola (≈9 s), sin bucle | Estéreo | VS |
| P10 | `mus_jefe_casa` | Jefe de Casa, genérico para las doce regiones | 144 | Mi frigio | 4/4 | Fase 1: 32; fase 2: 32 (53 s cada una) | B P V C + D | VS |
| P11 | `mus_region_aries` | Montes de Hierro Rojo | 90 | La frigio | 4/4 | 32 (85 s) | B P V + D | VS |
| P12 | `mus_region_capricornio` | Salinas de Obsidiana | 90 | Re menor pentatónico | 4/4 | 32 (85 s) | B P V + D | VS |
| P13 | `mus_fresno_raices` | Fresno: Raíces Putrefactas | 72 | Re frigio | 4/4 | 32 (107 s) | B P V + D | VS |
| P14 | `mus_jefe_madre_raiz` | Jefa de zona: la Madre Raíz | 120 | Re frigio | 4/4 | 32 + 32 (64 s cada una) | B P V C + D | VS |
| P15 | `mus_leitmotiv_marisal` | Leitmotiv de Marisal (violonchelo) | 64 | Do menor | 4/4 | 16 (60 s); firma de 2 compases | Estéreo + solista | VS |
| P16 | `mus_ascension_iii_marisal` | Ascensión III de Marisal | 120 | Do frigio | 4/4 | 7 s; versión corta de 2 a 3 s | Estéreo | VS |
| P17 | `mus_libro_mayor` | Libro Mayor y menús | 96 | Sol dórico | 4/4 | 32 (80 s) | Estéreo + D | VS |
| P18 | `mus_ascension_iii_coro` | Ascensión III genérica de Oficialas y Aprendizas | 120 | Re dórico | 4/4 | 7 s; corta de 2 a 3 s | Estéreo | L |
| P19 | `mus_juicio_pureza` | Juicio de Pureza: pelear contra tu propio Ascendente | 120 | La menor | 4/4 | 32 (64 s) | B P V C + D | L |
| P20 | `mus_extasis_abisal` | Éxtasis Abisal (dos turnos) | 128 | Re menor | 4/4 | 16 (30 s) | B P V C + D fija | L |
| P21 | `mus_transformacion_destilada` | Transformación de fase al grado B | 120 | Re menor → Fa mayor | 4/4 | 4 + cola (9 a 10 s) | Estéreo | L |
| P22 | `mus_transformacion_quintaesencia` | Transformación de fase al grado S | 120 | Re mayor con cuarta lidia | 4/4 | 4 + cola (9 a 10 s) | Estéreo | L |
| P23 | `mus_fresno_tronco` | Tronco de Ceniza | 80 | Mi eolio | 3/4 | 48 (108 s) | B P V + D | L |
| P24 | `mus_fresno_ramas` | Ramas del Eclipse | 64 (n. con puntillo) | Sol dórico | 6/8 | 48 (90 s) | B P V + D | L |
| P25 | `mus_fresno_copa` | Copa Astral | 60 | La lidio | 4/4 | 24 (96 s) | B P V + D | L |
| P26 | `mus_jefe_lenador_hueco` | Jefe de zona: el Leñador Hueco | 128 | Mi menor | 4/4 | 32 + 32 (60 s cada una) | B P V C + D | L |
| P27 | `mus_jefe_corneja_eclipse` | Jefa de zona: la Corneja del Eclipse | 144 | Sol menor | 4/4 | 32 + 32 (53 s cada una) | B P V C + D | L |
| P28 | `mus_jefe_jardinera` | Jefa de la Copa: la Jardinera (tema base de temporada) | 150 | La menor ↔ La lidio | 3/4 | 48 + 48 (58 s cada una) | B P V C + D | L |
| P29 | `mus_region_tauro` | Llanuras de Cobre | 72 | Fa mayor | 3/4 | 48 (120 s) | B P V + D | L |
| P30 | `mus_region_geminis` | Ciudad Espejo | 60 (n. con puntillo) | Sol menor | 6/8 | 48 (96 s) | B P V + D | L |
| P31 | `mus_region_cancer` | Costa de las Conchas | 80 | Mi dórico | 4/4 | 32 (96 s) | B P V + D | L |
| P32 | `mus_region_leo` | Desierto Áureo | 96 | Re hiyaz (frigio dominante) | 4/4 | 32 (80 s) | B P V + D | L |
| P33 | `mus_region_virgo` | Abadía de las Herbolarias | 60 | Sol dórico | 4/4 | 24 (96 s) | B P V + D | L |
| P34 | `mus_region_libra` | Ducado de las Balanzas | 72 | La menor ↔ Do mayor | 4/4 | 32 (107 s) | B P V + D | L |
| P35 | `mus_region_escorpio` | Pantanos de Azufre | 64 (n. con puntillo) | Mi dórico con cuarta aumentada | 6/8 | 48 (90 s) | B P V + D | L |
| P36 | `mus_region_sagitario` | Estepas del Centauro | 120 | La menor pentatónico | 4/4 | 48 (96 s) | B P V + D | L |
| P37 | `mus_region_acuario` | Archipiélago de Hielo | 75 | Si dórico | 3/4 | 40 (96 s) | B P V + D | L |
| P38 | `mus_region_piscis` | Isla de la Ninfa | 60 | Mi frigio sobre bordón | 4/4 | 24 (96 s) | B P V + D | L |
| P39 | `mus_eclipse` | Evento de eclipse | 60 | Do menor (de Do mayor a menor) | 4/4 | 16 (64 s) | B P C + D | L |
| P39 | `mus_eclipse_aviso` | Aviso de eclipse (stinger) | 60 | Do | 4/4 | 2 (8 s), sin bucle | Estéreo | L |
| P40 | `mus_horror_capa_paralaje` | Capa de Paralaje sobre cualquier cue | — | Sin tono | Libre | 60 a 90 s con fundido cruzado | Estéreo | L |
| P41 | `mus_huesped_combate` | Combate contra Huéspedes | 75 | Sin centro, alrededor de Do♯ | 4/4 | 32 (102 s) | B P C + D | L |
| P42 | `mus_horror_revelacion` | Revelaciones de la Grieta y el Abismo (GDD 6.5) | 60 | Re menor → sin tono | 4/4 | Sin bucle (≈70 s) | Estéreo | L |
| P43 | `mus_crafting_yunque` | Yunque Astral | 100 | La menor | 4/4 | 32 (77 s) | Estéreo | L |
| P44 | `mus_crafting_caldero` | Caldero de Bruja (y nodos de Caldero del Fresno) | 90 | Mi menor | 3/4 | 48 (96 s) | Estéreo | L |
| P45 | `mus_leitmotiv_selenia` | Leitmotiv de Selenia (armonio) | 72 | Si menor | 3/4 | 16 (40 s) | Estéreo + solista | L |
| P46 | `mus_leitmotiv_solenne` | Leitmotiv de Solenne (laúd árabe) | 100 | Re hiyaz | 4/4 | 16 (38 s) | Estéreo + solista | L |
| P47 | `mus_leitmotiv_cervanda` | Leitmotiv de Cervanda (viola da gamba) | 60 | Sol menor | 3/4 | 16 (48 s) | Estéreo + solista | L |
| P48 | `mus_leitmotiv_tormentina` | Leitmotiv de Tormentina (flauta dulce) | 96 (n. con puntillo) | La mixolidio | 6/8 | 32 (40 s) | Estéreo + solista | L |
| P49 | `mus_leitmotiv_cupria` | Leitmotiv de Cupria (arpa) | 80 | Mi mayor con iv menor | 4/4 | 16 (48 s) | Estéreo + solista | L |
| P50 | `mus_leitmotiv_cempa` | Leitmotiv de Cempa, la coyota (jarana jarocha) | 96 (n. con puntillo) | Re mayor | 6/8 | 32 (40 s) | Estéreo + solista | L |
| P51 | `mus_leitmotiv_jaguar` | Leitmotiv de la jaguar (ocarina y tambor de madera) | 90 | La menor pentatónico | 4/4 | 16 (43 s) | Estéreo + solista | L |
| P52 | `mus_ascension_iii_selenia` | Ascensión III de Selenia | 120 | Si menor | 4/4 | 7 s; corta de 2 a 3 s | Estéreo | L |
| P53 | `mus_ascension_iii_solenne` | Ascensión III de Solenne | 120 | Re hiyaz | 4/4 | 7 s; corta de 2 a 3 s | Estéreo | L |
| P54 | `mus_ascension_iii_cervanda` | Ascensión III de Cervanda | 120 | Sol menor | 4/4 | 7 s; corta de 2 a 3 s | Estéreo | L |
| P55 | `mus_ascension_iii_tormentina` | Ascensión III de Tormentina | 120 | La mixolidio | 4/4 | 7 s; corta de 2 a 3 s | Estéreo | L |
| P56 | `mus_ascension_iii_cupria` | Ascensión III de Cupria | 120 | Mi mayor | 4/4 | 7 s; corta de 2 a 3 s | Estéreo | L |
| P57 | `mus_final_cosecha` | Final Cosecha | 72 | Re mayor con pedal disonante | 4/4 | Sin bucle (≈3 min) | Estéreo | L |
| P58 | `mus_final_ceniza` | Final Ceniza | 60 | Re menor | 4/4 | Sin bucle (≈3 min) | Estéreo | L |
| P59 | `mus_final_libro_en_blanco` | Final secreto Libro en Blanco | 72 | Re dórico, quintas vacías | 4/4 | Sin bucle (≈3 min) | Estéreo | L |
| P60 | `mus_creditos` | Créditos | 72 | Re dórico | 4/4 | Sin bucle (≈4 min) | Estéreo + V | L |

**Conteo:** 22 cues VS y 44 de lanzamiento. Las 4★ y 3★ sin leitmotiv propio usan `mus_ascension_iii_coro` y la firma genérica de la tirada (`mus_tirada_vapor_violeta` o `mus_tirada_vapor_blanco`).

### 5.3 Familias posteriores

No llevan prompt propio: se hacen con *Cover* o *Extend* de una pista ya aprobada, con el mismo estilo y un cambio de color. Así mantienen la melodía y el ADN.

| Familia de IDs | Uso | Cómo se produce | Cuándo |
| --- | --- | --- | --- |
| `mus_combate_<signo>` | Combate con color de cada región | Cover de P07 sumando los dos invitados de la región | Con cada región nueva |
| `mus_jefe_casa_<signo>` | Jefe propio de cada Casa | Cover de P10 con el sabor de la región en la letra | Con cada región nueva |
| `mus_combate_fresno_<zona>` | Combate con color de cada zona del Fresno | Cover de P07 con la instrumentación de la zona | Después del lanzamiento |
| `mus_juicio_<bruja>` | Juicio de Pureza de cada Regente | Cover de P19 con su instrumento solista | Con cada Regente |
| `mus_jefe_jardinera_<temporada>` | La Jardinera de cada temporada (GDD 10.1 y 10.7) | Cover de P28 con el metal del planeta del Tránsito (sección 3.4) | Cada seis semanas |
| `mus_termas_<manantial>` | Manantiales cosméticos: volcánico, lunar, de cerezos, de obsidiana | Cover de P02 y P03 | Con cada Manantial |
| `mus_evento_<nombre>` | Evento de la semana 3 (GDD 22.1) | Prompt nuevo con el ADN | Cada versión |
| `mus_evento_verano` | Verano en las Termas (GDD 22.2) | Cover de P02, más luminoso | Temporada de verano |
| `mus_aniversario_tema` | Aniversario | Cover de P01 con coro completo | Cada año |
| `mus_avance_<version>` | Transmisión de la semana 6 | Cover del leitmotiv de la siguiente Regente | Cada versión |
| `mus_leitmotiv_<bruja>` | Leitmotivs de nuevas Regentes y del resto de Oficialas | Plantilla de P15 con su instrumento (sección 9, pregunta 10) | Con cada bruja |
| `mus_ascension_iii_<bruja>` | Ascensiones III de nuevas Regentes | Plantilla de P16 | Con cada Regente |
| `mus_final_local_<signo>` | Finales locales de región (GDD 13.5) | Cover del tema de la región, solo voz y guitarra | Con cada región |
| `mus_fresno_santuario` y `mus_menu_vestidor` | Santuario de Sal; vestidor y tienda | Cover de P03 y de P17 | Después del lanzamiento |

**Por versión de seis semanas** (GDD 22.1) calcula: leitmotiv y Ascensión III de cada Regente nueva (dos fichas por Regente), región nueva si la hay (tema, combate y Casa), una Jardinera, un evento y, si sale, un Manantial. Son de 6 a 10 cues y unos 800 a 1,500 créditos por versión.

---

## 6. Prompts de Suno

### 6.0 Cómo usar las fichas

- **Estilo, Excluir estilos y Letra** se copian tal cual en sus campos de Create, modo Custom. Los prompts van en inglés porque Suno los sigue mejor; la letra va en Salmódico.
- **Ajustes** van en Opciones avanzadas. Variety siempre en 0 (lo pide el FAQ de v6 para conservar tus etiquetas). Modelo v6, nunca v6-mini.
- **Piezas instrumentales:** enciende Instrumental. Si tu app conserva el campo de letra con Instrumental encendido, pega la estructura de la ficha; si no, el estilo ya termina con la forma (*Form: ...*) y basta. El lote 0 sirve para saber cuál de las dos funciona mejor (sección 7.1).
- **Persona de estilo:** en los cues con voz, usa la que creaste desde la ancla P01 (sección 7.3). No sirve en pistas instrumentales.
- **Uso** dice cuántas tomas generar, qué compases forman el bucle, qué stems salen y cuándo entra cada capa. Los compases se cuentan desde el primer tiempo fuerte después de la intro, salvo que se diga otra cosa.
- **Sentido** es la traducción interna de la letra. No va en Suno; es para el lingüista y la vocalista.

### 6.1 Tema principal y menús

#### P01 · `mus_titulo_tema` · Tema principal y pantalla de título

**Título:** Istal Yolma
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3:30)

**Estilo**

```text
Main theme, slow hymn-like ballad, bittersweet and hopeful. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Fingerpicked guitar and lute open, low cello countermelody, psaltery shimmer, bells on phrase endings. Builds from solo voice to full choir, then falls back to voice and guitar. Soft frame drum only in the last chorus. Memorable, singable melody. D Dorian, 72 BPM, 4/4. Form: intro, verse, chorus, interlude, verse, chorus, bridge, outro.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, fast tempo, heavy drums, rock ballad, musical theater belting
```

**Letra**

```text
[Intro]
[Instrumental: guitar and lute, bells]

[Verse 1]
Istal yolma, cuetor frashna
Nantre ilcel, estlali nochual
Yolmime olvai, na remel
Na olvai, ermai, neyo cuicant

[Chorus]
Frashna, frashna, cuetor
(Istal yolma)
Nelradi e lucetl, cuetor
(Istal yolma)

[Instrumental: low cello and psaltery]

[Verse 2]
Atlua cochum, maresh olvai
Tleugo neshcen, eztre e micmor
Nosotl cuicant, nantre uarbe
Cuetor, cuetor, yolma

[Chorus]
Frashna, frashna, cuetor
(Istal yolma)
Nelradi e lucetl, cuetor
(Istal yolma)

[Bridge]
Huemag ishoc, ilcel blaztac
(Aaah)

[Outro]
Istal yolma... cuetor
[End]
```

**Sentido:** La sal es alma: vuelve al Fresno. Madre del cielo, estrella de la noche. Las almitas olvidan, no recuerdan. No olvides, hermana: yo canto. / Fresno, Fresno, vuelve. Raíz y luz, vuelve. / El agua duerme, el mar olvida. Fuego y ceniza, sangre y muerte. Nosotras cantamos, madre árbol. / Un ojo inmenso, el cielo en blanco.

**Uso**

- Es la **ancla**: genera de 10 a 12 tomas (5 o 6 pedidos) y elige con calma. Todo el juego se compara contra ella.
- Ruta recomendada: una persona compone la melodía, la sube como audio y se usa *Cover* con este estilo y Audio Influence alrededor de 70 (sección 7.4).
- Bucle de título: Verse 1 más Chorus (32 compases); la intro suena una vez.
- Saca el stem de voz (V) para tráileres y para crear la Persona de estilo.
- De esta pista salen por *Cover* P57 a P60 y el aniversario.

#### P17 · `mus_libro_mayor` · Libro Mayor y menús

**Título:** Folio Cuatrocientos Diecisiete
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode no · Duración auto

**Estilo**

```text
Menu music for an ancient ledger of souls: quiet, studious, candlelit, faintly uncanny. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lute and guitar in slow clockwork-like counterpoint, pizzicato low cello, soft psaltery, ticking small bells like an abacus, very soft bowls. No drums, no vocals. G Dorian, 96 BPM, 4/4. Form: short intro, 32-bar loopable theme, gentle ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, drums, horror, epic
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: lute and guitar]
[Verse]
[Instrumental: clockwork counterpoint, ticking bells]
[Chorus]
[Instrumental: psaltery melody]
[Outro]
[End]
```

**Uso**

- 6 tomas. Bucle: 32 compases (80 s). La intro suena al abrir el Libro.
- Con Paralaje medio o alto, FMOD suma `mus_horror_capa_paralaje` y la Desafinación: es el momento en que la interfaz falla (GDD 6.5 y 13.1).

### 6.2 Termas

#### P02 · `mus_termas_dia` · Termas de día

**Título:** Vapor y Caparazón
**Ajustes:** Instrumental sí · Weirdness 20 · Style Influence 80 · Variety 0 · Max Mode no · Duración auto

**Estilo**

```text
Peaceful daytime hot-spring theme: safe, cozy, gentle humor with a faint wistfulness. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Warm fingerpicked guitar and lute duet, wooden flute melody, soft pizzicato low cello, psaltery sparkles, bell drops like water. Light hand percussion only: shaker and soft frame drum. Lilting waltz. F major with Lydian color, 90 BPM, 3/4. No tension, no dark turns. Form: short intro, 48-bar main theme that can repeat, soft ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, horror, dissonance, heavy drums, tension
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: guitar and lute]
[Verse]
[Instrumental: flute melody, waltz]
[Chorus]
[Instrumental: flute and psaltery]
[Verse]
[Instrumental: lute melody]
[Outro]
[End]
```

**Uso**

- 6 a 8 tomas. Bucle: 48 compases (96 s). Sin capas: es zona segura.
- El cruce con P03 por hora del día es un fundido de 4 compases. Comparten armadura, así que no choca.

#### P03 · `mus_termas_noche` · Termas de noche

**Título:** Agua que Olvida
**Ajustes:** Instrumental no · Weirdness 20 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Night hot-spring lullaby: tender, sleepy, safe, water that makes you forget. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. The voice is close and almost whispered. Slow arpeggiated guitar, plucked psaltery, sustained low cello, softly bowed singing bowls. No percussion. D minor opening into F major, 50 BPM in 6/8 (dotted quarter), gentle rocking. Form: guitar intro, two soft verses, humming section, guitar outro.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, drums, percussion, belting, dramatic climax, horror
```

**Letra**

```text
[Intro]
[Instrumental: slow guitar arpeggios]

[Verse 1]
Atlua cochum, atlua olvai
Nochual metzlu, cochum, ermai
Na istal, na micmor
Atlua olvai

[Verse 2]
Yolmi cochum, nantre cuicant
Estlali ilcel, na ishoc
Cochum, cochum, ermai
Atlua olvai

[Bridge]
Mmm... mmm...
Mmm... olvai...

[Outro]
[Instrumental: guitar and singing bowls]
[End]
```

**Sentido:** El agua duerme, el agua olvida. Noche de luna, duerme, hermana. Ni sal ni muerte. El agua olvida. / Almita, duerme; la madre canta. Estrella del cielo, no mires. Duerme, duerme, hermana.

**Uso**

- 8 tomas. Si la voz sale cantada a plena voz en vez de susurrada, baja Style Influence a 70 y repite.
- Bucle: de Verse 1 a Bridge (40 compases, 96 s). La intro de guitarra suena al entrar desde el día.
- Es la mejor candidata para la vocalista humana: es la canción que el jugador más escucha en reposo.

### 6.3 Tirada

#### P04 · `mus_tirada_suspenso` · Suspenso de la tirada

**Título:** El Termómetro
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode no · Duración auto

**Estilo**

```text
Suspense loop for a summon ritual at a hot spring: anticipation and warm mystery, not scary. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Low cello pedal on D, slow psaltery tremolo rising, soft heartbeat frame drum, bubbling bell figures, bowed singing bowls. Harmony hovers without resolving, like held breath. Second half simmers to a boil: faster psaltery, higher cello, stronger heartbeat. D pedal, 80 BPM, 4/4. Form: 8 calm bars, 8 intense bars, held unresolved chord.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, horror, jump scare, catchy hook
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: low cello pedal, heartbeat drum]
[Verse]
[Instrumental: psaltery tremolo rising, bubbling bells]
[Bridge]
[Instrumental: faster, simmering to a boil]
[Outro]
[Instrumental: held unresolved chord]
[End]
```

**Uso**

- 6 tomas. Región **tibia**: compases 1 a 8 (24 s), en bucle. Región **hirviendo**: compases 9 a 16, cuando el termómetro está en pity suave (GDD 16.1). El paso entre regiones cae en barra de compás.
- Al revelarse el color del vapor, FMOD corta en el siguiente pulso y suena el stinger de P05.

#### P05 · Stingers de vapor (modo Sounds)

Cuatro cues cortos. Se hacen en **Sounds** (Create → Custom → Sounds), tipo one-shot, con **tonalidad Re y 80 BPM** para que encajen sobre P04. Si Sounds no da el sonido de la ancla, genera P06 dos veces y corta los acordes de ahí.

**`mus_tirada_vapor_blanco`** (3★) · tonalidad Re menor · 80 BPM

```text
Acoustic one-shot, about 2 seconds: a soft nylon guitar strum of a D minor chord and one small bell, gentle and plain, natural stone chapel reverb tail.
```

**`mus_tirada_vapor_violeta`** (4★) · tonalidad Si♭ mayor · 80 BPM

```text
Acoustic one-shot, about 3 seconds: a rising plucked psaltery arpeggio landing on B-flat major then F major, a soft wordless female choir swell, small bells, natural stone chapel reverb tail.
```

**`mus_tirada_vapor_dorado`** (5★) · tonalidad Re mayor · 80 BPM

```text
Acoustic one-shot, about 4 seconds: a radiant D major chord blooms, full wordless choir swell, cascading high bells, psaltery tremolo, warm cello root, tender and triumphant, natural stone chapel reverb tail.
```

**`mus_tirada_subida_dorada`** (de violeta a dorado) · tonalidad Re mayor · 80 BPM

```text
Acoustic one-shot, about 5 seconds: starts as a soft B-flat major psaltery arpeggio, then lifts and modulates into a bright D major choir and bell bloom, joyful surprise, natural stone chapel reverb tail.
```

**Uso**

- 4 a 6 tomas de cada uno. Recorta a la duración de la tabla con una cola de 1 s.
- En una tirada de diez, las 3★ solo tienen su stinger; las 4★ y 5★ siguen con la emergencia (GDD 16.1).
- La subida dorada se dispara en lugar del violeta cuando la presentación hace la subida sorpresa. Es solo presentación: el resultado ya estaba decidido.

#### P06 · `mus_tirada_emergencia_5` · Emergencia de la 5★

**Título:** Lo que Sube del Agua
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible (si no, auto y recorta)

**Estilo**

```text
Short cinematic choral revelation: a legendary witch rises from a hot spring. Awe, warmth, wonder. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Water-like bells and a held breath, psaltery tremolo rises, then choir and solo voice bloom together on a radiant D major chord with cascading bells, cello and guitar strums. No drums. D major with Lydian touches, 120 BPM, 4/4. Form: very short intro, one big choral bloom, long ringing ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, drums, long song, many verses
```

**Letra**

```text
[Intro]
[Instrumental: water-like bells and breath]

[Chorus]
Aaah... estlali!
(Aaah... lucetl!)

[Outro]
[Instrumental: bells ring out]
[End]
```

**Sentido:** ¡Estrella! ¡Luz!

**Uso**

- 6 a 8 tomas. Corta el florecimiento: un compás antes del coro, dos compases de coro y 2 s de cola. Da unos 7 s, lo que dura la emergencia (GDD 19.2).
- Al aparecer la tarjeta suena la **firma** de la bruja: los dos primeros compases de su leitmotiv, solo el instrumento solista. Las 5★ sin leitmotiv todavía usan `mus_tirada_vapor_dorado`.

### 6.4 Combate

#### P07 · `mus_combate_normal` y `mus_cadena_partitura` · Combate normal y Partitura

**Título:** Eztre Istal
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 2:30)

**Estilo**

```text
Driving folk battle music for turn-based combat: urgent, tense, beautiful. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Fast strummed guitar ostinato, lute, aggressive low cello riffs, psaltery runs, iron bells, flute countermelody. Frame drums, deep bass drum, tambourine, claps. Wordless vocalise on the main section; choir answers with short chants. D minor with Phrygian accents, 128 BPM, 4/4. Form: 4-bar intro, 32-bar main section, 8-bar break of psaltery and bowls only, main section, short ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, slow tempo, ballad, orchestral string section, timpani
```

**Letra**

```text
[Intro]
[Instrumental: guitar and frame drums]

[Verse]
Aaah... aaah...
(Eztre! Istal!)
Aaah... ooh...
(Tleugo! Eztre!)

[Chorus]
Aaah... aaah...
(Istal yolma, cuetor!)
Aaah... ooh...
(Istal yolma, cuetor!)

[Break]
[Instrumental: psaltery and singing bowls only]

[Verse]
Aaah... aaah...
(Eztre! Istal!)

[Chorus]
Aaah... ooh...
(Istal yolma, cuetor!)

[Outro]
[End]
```

**Sentido:** ¡Sangre! ¡Sal! ¡Fuego! / Sal y alma, ¡vuelve!

**Uso**

- Mínimo 8 tomas: es la pista más escuchada del juego. Descarta las que tengan una sección que se "canse" en menos de dos vueltas.
- **Intro:** 4 compases. **Bucle:** Verse más Chorus, 32 compases (60 s). **Break:** 8 compases (15 s), es `mus_cadena_partitura`. **Outro:** cola hacia la victoria.
- **Stems:** B (todo menos percusión y voces), P (Drums y Percussion), V (Lead Vocals: la vocalización) y C (Backing Vocals: los cantos cortos).
- **Capas:** B y P desde el primer compás. V entra con `EnCadena` o `AscendenteActivo`, en el siguiente compás y con fundido de un compás; la vocalización sin palabras respeta la magia silente (GDD 7.5). C entra 8 compases después de cada Ascensión III. D, con Salinidad de 100 o Paralaje alto.
- **Partitura:** al abrir la Cadena, transición al Break en la siguiente barra; al confirmar, regreso al bucle con V activa.
- **Éxtasis:** P20 tiene el mismo tempo y tonalidad; el cambio es en barra de compás.

#### P08 · `mus_jingle_victoria` y `mus_combate_resultados` · Victoria y resultados

**Título:** Sal Guardada
**Ajustes:** Instrumental sí · Weirdness 20 · Style Influence 85 · Variety 0 · Max Mode no · Duración auto

**Estilo**

```text
Victory fanfare and results screen: relief and quiet pride, warm. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A bright 2-bar flourish of strummed guitar and bells ending on D major, then a calm lilting loop: lute melody, soft low cello, flute, light tambourine. D major, 96 BPM, 4/4. Form: 2-bar fanfare, 8-bar calm loop, gentle ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, minor key, long intro
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: bright guitar and bell flourish]
[Verse]
[Instrumental: calm lute melody, light tambourine]
[Outro]
[End]
```

**Uso**

- 6 tomas. **Jingle:** compases 1 y 2 más la cola (unos 6 s). **Resultados:** compases 3 a 10 (20 s) en bucle, justo después del jingle.

#### P09 · `mus_jingle_derrota` · Derrota

**Título:** Mancha de Sal
**Ajustes:** Instrumental sí · Weirdness 20 · Style Influence 85 · Variety 0 · Max Mode no · Duración personalizada lo más corta posible

**Estilo**

```text
Short defeat jingle: sorrowful and gentle, never punishing. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A sighing low cello phrase over one soft guitar chord, a single low bell, a crystalline psaltery fading away like salt. D minor, 60 BPM, 3/4. Form: 3-bar phrase, long reverb tail, then silence.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, drums, comedy, cartoon
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: low cello sigh, one bell]
[Outro]
[End]
```

**Uso**

- 6 tomas. Corta 3 compases más la cola (unos 9 s), sin bucle. Después, silencio o el Libro Mayor.
- En el Fresno, la derrota deja una Mancha de Sal (GDD 10.4): el mismo jingle sirve; no la castigues con algo más duro.

#### P10 · `mus_jefe_casa` · Jefe de Casa

**Título:** La Casa del Signo
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Epic acoustic boss battle against a witch coven: grim, majestic, ritual intensity. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Phase one: relentless guitar and lute ostinato, heavy low cello, war frame drums, anvil and iron bells, psaltery runs. Phase two: same tempo, the full choir chants, the lead voice soars, drums double. E Phrygian, 144 BPM, 4/4. Form: 4-bar intro, 32-bar phase one, 4-bar drum-roll transition, 32-bar phase two, final hit.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, slow tempo, ballad, orchestral string section
```

**Letra**

```text
[Intro]
[Instrumental: war drums and anvil]

[Verse]
(Eztre! Istal! Micmor!)
Aaah... aaah...
(Eztre! Istal! Micmor!)
Aaah... aaah...

[Pre-Chorus]
[Instrumental: drum roll]

[Chorus]
Istal yolma, micmor cuetor!
(Eztre, eztre, tleugo!)
Frashna ishoc, huemag nelradi!
(Eztre, eztre, tleugo!)

[Outro]
[End]
```

**Sentido:** ¡Sangre, sal, muerte! / Sal y alma: la muerte regresa. El Fresno mira, la raíz inmensa. ¡Sangre, fuego!

**Uso**

- 8 tomas. **Fase 1:** Verse, 32 compases (53 s). **Transición:** Pre-Chorus, 4 compases, una sola vez. **Fase 2:** Chorus, 32 compases.
- El cambio de fase lo dispara el combate (vida o patrón del jefe) y cae en la siguiente barra.
- Con `EsJefe` el coro suena todo el combate; en la fase 2 pasa al frente. V sigue la regla de la Cadena.
- Las doce Casas usan esta pista al lanzamiento; después, cada una tiene su *Cover* (sección 5.3).

### 6.5 Regiones del vertical slice

#### P11 · `mus_region_aries` · Montes de Hierro Rojo

**Título:** Fierristal
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for red-iron mountain fortresses inspired by medieval Iberia: proud, weary, windswept. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Hurdy-gurdy drone and melody, guitar and lute with Andalusian cadences, square frame drum in a soft pulse, distant iron bells. The voice enters in the second half with a slow ornamented lament. A Phrygian, 90 BPM, 4/4. Form: intro, 16-bar instrumental theme, 16 bars with voice, instrumental return, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, flamenco, heavy drums
```

**Letra**

```text
[Intro]
[Instrumental: hurdy-gurdy and lute]

[Verse]
[Instrumental: hurdy-gurdy melody over guitar]

[Chorus]
Fierristal, fiyolmade
Montesh d'eztre, cuetoriello
Ferrosh nantre, na olvaide
Fiyolmade, cuetor

[Instrumental: hurdy-gurdy solo]

[Outro]
[End]
```

**Sentido:** Sal de hierro, almas hijas. Montes de sangre, el regreso. Madre de hierro, no olvides. Almas hijas, vuelvan.

**Uso**

- Invitados regionales: zanfona y pandero cuadrado.
- 8 tomas. **Bucle:** Verse más Chorus, 32 compases (85 s).
- **Stems:** B, P (pandero) y V. P sube con `Tension`. V suena desde la segunda vuelta del bucle o en escenas de historia, para que el jugador no se canse de la voz.

#### P12 · `mus_region_capricornio` · Salinas de Obsidiana

**Título:** Istaltli
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for obsidian salt flats inspired by ancient Mesoamerica: sun-bleached, sacred, eerie. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Clay flutes as color, wooden slit drum and deep skin drum in a steady pulse, salt rattles, guitar and cello drone, circular ostinatos like whirlpools. The voice sings slow earthy calls with Nahuatl-like sounds. D minor pentatonic, 90 BPM, 4/4. Form: intro, 16-bar theme, 16 bars with voice, ostinato return, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, mariachi, trumpet, pan flute, new age
```

**Letra**

```text
[Intro]
[Instrumental: slit drum and clay flutes]

[Verse]
[Instrumental: circular ostinato, whirlpool]

[Chorus]
Istaltli huemag, metzlutsi cuetor
Atluatli, atluatli, ishoc tleugo
Nantretsi cochumtli
Istaltli, istaltli

[Instrumental: ostinato returns]

[Outro]
[End]
```

**Sentido:** Sal inmensa, lunita, vuelve. Agua, agua, mira el fuego. Madrecita dormida. Sal, sal.

**Uso**

- Invitados regionales: flautas de barro y tambor de hendidura de madera. La ocarina **no** lleva la melodía: es de la jaguar (P51).
- 8 tomas. **Bucle:** Verse más Chorus, 32 compases (85 s). Stems y capas como en P11.
- Los remolinos de sal (Escila y Caribdis) se oyen en los ostinatos circulares; si una toma no los tiene, no es esta.

### 6.6 Fresno: Raíces y su jefa

#### P13 · `mus_fresno_raices` · Raíces Putrefactas

**Título:** Las que No Llegaron
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Dungeon theme for the rotten roots of a dying world tree: damp, sorrowful, oppressive, souls that never arrived. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Low cello drones and col legno taps, muted lute plucks, cracked low bells, slow heartbeat frame drum, breathy air. The distant voice sings almost wordless, like a lost soul. D Phrygian, 72 BPM, 4/4. Form: long dark intro, 32-bar slow theme, fading ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, fast tempo, bright major, jump scare
```

**Letra**

```text
[Intro]
[Instrumental: low cello drone and cracked bells]

[Verse]
Ooh... yolmime...
Nelradi... micmor...
Ooh... na cuetor...
Nelradi... micmor...

[Instrumental: heartbeat drum and muted lute]

[Outro]
[End]
```

**Sentido:** Las almitas... raíz, muerte... no vuelven.

**Uso**

- 6 a 8 tomas. **Bucle:** Verse más el instrumental, 32 compases (107 s).
- **Stems:** B, P y V. P sube con `Tension` al acercarse a un nodo de Élite o de Jefe. V suena bajo desde la primera vuelta.
- Tonalidad de zona: Re. El Fresno sube de tonalidad contigo (sección 3.7).

#### P14 · `mus_jefe_madre_raiz` · La Madre Raíz

**Título:** Cochum, Yolmi
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Boss battle with a monstrous mother of roots: sorrowful, heavy, motherly and terrifying. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Grinding low cello ostinato, deep drums like falling earth, cracked bells, lute tremolo. The voice sings a twisted lullaby; the choir answers in deep slow chords. Phase two: double-time drums and full choir. D Phrygian, 120 BPM, 4/4. Form: intro, 32-bar phase one, 4-bar transition, 32-bar phase two, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, bright major, ballad
```

**Letra**

```text
[Intro]
[Instrumental: falling-earth drums, cracked bells]

[Verse]
Cochum, yolmi, nantre nelradi
Cochum, cochum, na cuetor
(Micmor... micmor...)
Cochum, yolmi, na cuetor

[Pre-Chorus]
[Instrumental: drums double]

[Chorus]
Nantre! Nantre! Istal nelradi!
(Cochum, yolmime, cochum!)
Nantre! Nantre! Na cuetor!
(Cochum, yolmime, cochum!)

[Outro]
[End]
```

**Sentido:** Duerme, almita, madre raíz. Duerme, no vuelvas. Muerte. / ¡Madre! Sal de raíz. Duerman, almitas.

**Uso**

- 8 tomas. Fases y capas como en P10: fase 1 en el Verse, transición de 4 compases, fase 2 en el Chorus.
- La canción de cuna torcida es la misma idea que P03 vista desde el horror. Si una toma se parece demasiado a P03 en melodía, mejor: es intencional.

### 6.7 Marisal, la 5★ del vertical slice

#### P15 · `mus_leitmotiv_marisal` · Leitmotiv de Marisal

**Título:** Hojadelobo
**Ajustes:** Instrumental sí · Weirdness 20 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a stoic, protective she-wolf warrior witch: noble, wounded, steady. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A solo cello carries the whole melody with a broad singing tone in tenor register. Guitar and lute accompany with slow arpeggios, soft frame drum in the second half, low iron bell on strong beats, flute only as an echo. No vocals. C minor, 64 BPM, 4/4. Form: 2-bar intro, 8-bar main motif on solo cello, 8-bar development, motif returns stronger, quiet ending on solo cello.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, violin solo, piano
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: guitar arpeggios]
[Verse]
[Instrumental: solo cello main motif]
[Bridge]
[Instrumental: development, soft frame drum]
[Chorus]
[Instrumental: solo cello motif returns stronger]
[Outro]
[Instrumental: solo cello alone]
[End]
```

**Uso**

- **Ruta recomendada:** una persona compone el motivo de 8 compases, lo graba simple (violonchelo o piano) y lo sube; *Cover* con este estilo y Audio Influence de 70 a 80. Sin melodía humana: 10 tomas y elige la más cantable.
- **Firma:** los dos primeros compases del motivo, solo el stem de violonchelo (Split from Mix). Suena en la tarjeta de la tirada y en su ficha.
- **Bucle de escena:** del motivo a su regreso, 16 compases (60 s).
- Su Ascensión III (P16) y su Juicio posterior heredan esta tonalidad y este motivo.

#### P16 · `mus_ascension_iii_marisal` · Ascensión III de Marisal

**Título:** Eztre Istal, Cuetor
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a wolf witch and her iron ram spirit: brief, explosive, sacred. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and a low iron bell, then solo cello and the voice launch the invocation; full choir and war drums explode at the climax with anvil hits; one last impact, ringing bells. C Phrygian, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and low iron bell]

[Chorus]
Eztre istal, tleugo, cuetor!
(Aaah!)

[Outro]
[Instrumental: impact and ringing bells]
[End]
```

**Sentido:** Sangre y sal, fuego: ¡vuelve!

**Uso**

- 8 tomas. Suno dará una pieza más larga: corta 7 s alrededor del clímax.
- **Sincronía con la cámara (GDD 19.3):** a 120 BPM un pulso dura 0.5 s. Preparación de 0 a 1.5 s (3 pulsos), ritual de 1.5 a 2.5 s (2), manifestación cantada de 2.5 a 5.5 s (6) e impacto de 5.5 a 7 s (3). Marca los cortes del Timeline sobre esos pulsos.
- **Versión corta (2 a 3 s):** ritual más impacto, como la cámara corta.
- Candidata a vocalista humana: es el canto de firma de la Regente.

### 6.8 Combate avanzado y transformaciones

#### P18 · `mus_ascension_iii_coro` · Ascensión III genérica

**Título:** Estlali, Cuetor
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a witch and her astral spirit: brief, radiant, sacred. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and a bell, rising psaltery tremolo, then the voice and full choir burst together over frame drums and bells; one final impact and ring-out. D Dorian, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and bell]

[Chorus]
Estlali, yolma, cuetor!
(Aaah!)

[Outro]
[Instrumental: impact and ringing bells]
[End]
```

**Sentido:** ¡Estrella, alma, vuelve!

**Uso**

- Corte y sincronía como en P16.
- Haz dos variantes del mismo cue cambiando el metal: *bright gold bells* para las sectas diurnas (Sol, Júpiter, Saturno) y *silver and copper singing bowls* para las nocturnas (Luna, Marte, Venus). Mercurio usa cualquiera. En FMOD son dos instrumentos del mismo evento.

#### P19 · `mus_juicio_pureza` · Juicio de Pureza

**Título:** Neyo Tuel
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Battle against your own mirrored spirit: introspective, tense, tragic, like fighting your reflection. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Two guitars answer each other in mirrored phrases, lute and low cello in canon, psaltery echoes, frame drums, a ticking bell. The voice sings a phrase and the choir answers with the same phrase, lower and slower. A minor, 120 BPM, 4/4. Form: intro, 32-bar main section, 8-bar quiet reflection, 32-bar intensified return, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, ballad, bright major
```

**Letra**

```text
[Intro]
[Instrumental: two mirrored guitars]

[Verse]
Yolma istal, istal yolma
(Istal yolma, yolma istal)
Neyo tuel, tuel neyo
(Tuel neyo, neyo tuel)

[Break]
[Instrumental: quiet reflection, ticking bell]

[Chorus]
Ishoc cuetor, cuetor ishoc
(Cuetor ishoc, ishoc cuetor)
Neyo tuel, tuel neyo
(Aaah...)

[Outro]
[End]
```

**Sentido:** Alma y sal, sal y alma. Yo eres tú, tú eres yo. / Mira y vuelve, vuelve y mira. Las líneas son palíndromos de palabras: la bruja se pelea con su reflejo.

**Uso**

- 8 tomas. **Bucle principal:** Verse, 32 compases (64 s). **Reflexión:** Break, 8 compases; suena cuando el Juicio revela el recuerdo de la bruja (GDD 12.4). **Regreso:** Chorus, 32 compases, cuando el Ascendente baja de la mitad de su vida.
- Capas: el Juicio cuenta como jefe (`EsJefe`), así que el coro suena siempre.
- Posterior: un *Cover* por Regente con su instrumento solista (`mus_juicio_<bruja>`).

#### P20 · `mus_extasis_abisal` · Éxtasis Abisal

**Título:** Huemag Ishoc
**Ajustes:** Instrumental no · Weirdness 55 · Style Influence 70 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Possession state in battle: the spirit takes control, ecstatic, frightening, sacred chaos. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Same pulse as a folk battle theme but heavier: pounding frame drums, scraping low cello, clustered choir, the voice wailing in long glissandi, bells struck hard, guitar slapped like a drum. Unstable but rhythmic. D minor, 128 BPM, 4/4. Form: 16-bar intense loop, 16-bar variation, short ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, calm, ballad, major key
```

**Letra**

```text
[Verse]
Aaah! Aaah!
(Micmor! Istal! Neshcen!)
Aaah...
(Huemag ishoc!)

[Chorus]
Aaah! Aaah!
(Micmor! Micmor!)
Aaah...
(Huemag ishoc!)

[Outro]
[End]
```

**Sentido:** ¡Muerte, sal, ceniza! ¡El ojo inmenso!

**Uso**

- 8 tomas. Mismo tempo y tonalidad que P07: FMOD entra en la siguiente barra cuando la Salinidad llega a 150.
- **Bucle:** Verse, 16 compases (30 s); el Chorus es la segunda vuelta. Dura los dos turnos del Éxtasis (GDD 7.4) y regresa a P07 en barra.
- La Desafinación va **siempre activa** aquí. Prepara además una copia invertida del stem V (Salmódico al revés) y súmala unos 12 dB abajo.
- Si Suno sale demasiado caótico para sostener un bucle, baja Weirdness a 45.

#### P21 · `mus_transformacion_destilada` · Transformación a Destilada (grado B)

**Título:** Refinar es Transformarse
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Magical transformation sequence, alchemical refinement: wonder and rising elegance. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Psaltery and guitar arpeggios rise step by step, low cello swells, bells sparkle; the voice sings a rising vocalise and the choir joins on the last chord. D minor turning into F major, 120 BPM, 4/4. Form: four rising bars and one bright held chord.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, drums, long song, many verses
```

**Letra**

```text
[Intro]
[Instrumental: rising psaltery arpeggios]

[Verse]
Aaah... aaah... aaah...
(Lucetl!)

[Outro]
[Instrumental: bright held chord, bells]
[End]
```

**Sentido:** ¡Luz!

**Uso**

- 6 tomas. Corta los cuatro compases que suben (8 s) más la cola: de 9 a 10 s, lo que dura la transformación (GDD 12.5). Al terminar suena la firma de la bruja.
- Se comparte en redes: tiene que sonar bien sola, sin contexto.

#### P22 · `mus_transformacion_quintaesencia` · Transformación a Quintaesencia (grado S)

**Título:** Estlali Lucetl
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate magical transformation: celestial, radiant, almost overwhelming beauty. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Bells cascade like falling stars, psaltery tremolo, choir in high harmony, the voice soaring, frame drums swell, cello and guitar on a huge bright chord. D major with a Lydian raised fourth, 120 BPM, 4/4. Form: four rising bars and one luminous held chord.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long song, many verses, minor key ending
```

**Letra**

```text
[Intro]
[Instrumental: falling-star bells]

[Verse]
Aaah... estlali!
(Estlali, lucetl!)

[Outro]
[Instrumental: luminous held chord]
[End]
```

**Sentido:** ¡Estrella! ¡Estrella, luz!

**Uso**

- Igual que P21. Esta es la de máxima presencia: si una toma suena modesta, descártala.

### 6.9 El Fresno: zonas y jefes de lanzamiento

#### P23 · `mus_fresno_tronco` · Tronco de Ceniza

**Título:** Remel
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Dungeon theme inside the ashen trunk of a world tree: trapped memories, warm sap, dust, nostalgia that hurts. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Music-box-like psaltery melody, guitar waltz accompaniment, woody knocks on the cello body, soft frame drum, the voice faint and wordless like a memory. E Aeolian, 80 BPM, 3/4. Form: intro, 48-bar waltz theme, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, heavy drums, toy piano
```

**Letra**

```text
[Intro]
[Instrumental: music-box psaltery]

[Verse]
Ooh... remel...
Ooh... remel... olvai...
Ooh... remel...
Mmm...

[Instrumental: waltz continues]

[Outro]
[End]
```

**Sentido:** Recuerdo... recuerdo, olvido...

**Uso**

- 6 a 8 tomas. **Bucle:** 48 compases (108 s). Stems B, P y V; P con `Tension`. Tonalidad de zona: Mi.

#### P24 · `mus_fresno_ramas` · Ramas del Eclipse

**Título:** Vértigo de Ramas
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Dungeon theme in the high branches of a world tree during an eclipse: wind, birds, vertigo, eerie light. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Fluttering flute trills like birds, fast guitar tremolo, psaltery gusts of wind, low cello pulses, galloping frame drum, the voice floating wordless above. G Dorian, 64 BPM in 6/8 (dotted quarter), swaying. Form: intro, 48-bar theme, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, bird sound effects, slow ballad
```

**Letra**

```text
[Intro]
[Instrumental: flute trills and guitar tremolo]

[Verse]
Aaah...
Ooh... aaah...
Aaah...
Ooh...

[Instrumental: wind gusts on psaltery]

[Outro]
[End]
```

**Uso**

- 6 a 8 tomas. **Bucle:** 48 compases (90 s). Stems B, P y V. Tonalidad de zona: Sol.

#### P25 · `mus_fresno_copa` · Copa Astral

**Título:** Hojas que Son Almas
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Dungeon theme in the astral crown of a world tree where leaves are souls: vast, luminous, serene and deeply wrong. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Slow constellations of bells, sustained bowls, psaltery shimmer, guitar harmonics, distant low cello. The voice sings very slowly, almost a prayer, with a second voice in close harmony. A Lydian, 60 BPM, 4/4. Form: intro, 24-bar theme, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, full choir, heavy drums, new age
```

**Letra**

```text
[Intro]
[Instrumental: constellations of bells]

[Verse]
Yolmime... estlali...
Ilcel... cosetl...
Yolmime... estlali...
Ooh...

[Outro]
[End]
```

**Sentido:** Las almitas... estrellas... cielo... cosecha. Es la primera vez que suena *cosetl*: anuncia el final Cosecha.

**Uso**

- 6 a 8 tomas. **Bucle:** 24 compases (96 s). Stems B, P y V (las dos voces van juntas en V). Tonalidad de zona: La.

#### P26 · `mus_jefe_lenador_hueco` · El Leñador Hueco

**Título:** Uarbe Neshcen
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Boss battle with a hollow woodcutter giant: relentless, mechanical, grim. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Chopping rhythms: woodblocks, log drums, low cello col legno and hard spiccato, muted guitar strums, lute ostinato, anvil-like hits on beats two and four. The choir chants low; the voice cries above in phase two. E minor, 128 BPM, 4/4. Form: intro, 32-bar phase one, 4-bar transition, 32-bar phase two, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, slow tempo, ballad, bright major
```

**Letra**

```text
[Intro]
[Instrumental: woodblocks and log drums]

[Verse]
(Uarbe! Neshcen! Uarbe!)
(Uarbe! Neshcen! Uarbe!)
(Uarbe! Neshcen! Uarbe!)
(Uarbe! Neshcen!)

[Pre-Chorus]
[Instrumental: axe-like hits]

[Chorus]
Aaah... remel!
(Uarbe, uarbe, neshcen!)
Aaah... na olvai!
(Uarbe, uarbe, neshcen!)

[Outro]
[End]
```

**Sentido:** ¡Árbol! ¡Ceniza! / ¡Recuerda! ¡No olvides!

**Uso**

- 8 tomas. Fases y capas como en P10.

#### P27 · `mus_jefe_corneja_eclipse` · La Corneja del Eclipse

**Título:** Solatiu Na
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Boss battle with a giant crow that swallows the sun: frantic, airborne, ominous. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Shrieking flute trills, fast psaltery and guitar tremolo, frame drums beating like wings, low gong-like bells for the eclipse, the choir in dark held chords, the voice in sharp cries. G minor, 144 BPM, 4/4. Form: intro, 32-bar phase one, 4-bar transition, 32-bar phase two, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, slow tempo, ballad, bird sound effects
```

**Letra**

```text
[Intro]
[Instrumental: wing-beat drums]

[Verse]
(Solatiu! Micmor!)
Aaah!
(Solatiu! Micmor!)
Aaah!

[Pre-Chorus]
[Instrumental: gong-like bells, the sun goes dark]

[Chorus]
Solatiu, na! Solatiu, na!
(Ilcel neshcen!)
Solatiu, na! Cuetor!
(Ilcel neshcen!)

[Outro]
[End]
```

**Sentido:** ¡Sol, muerte! / ¡Sol, no! ¡Cielo de ceniza! ¡Vuelve!

**Uso**

- 8 tomas. Fases y capas como en P10. La transición coincide con el momento en que la Corneja se traga el sol: sincroniza la animación con los compases.

#### P28 · `mus_jefe_jardinera` · La Jardinera

**Título:** Cosetl
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3:30)

**Estilo**

```text
Final boss of a world tree: a celestial gardener who prunes souls. Elegant macabre waltz, serene and merciless. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Fast waltz on guitar and lute, low cello pulses, psaltery snipping like scissors, bells in constellations, full choir in sublime chords, the voice calm and terrible. Harmony shifts between A minor and A Lydian. 150 BPM, 3/4. Form: intro, 48-bar phase one, 4-bar transition, 48-bar phase two with full choir, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, slow tempo, ballad, circus
```

**Letra**

```text
[Intro]
[Instrumental: waltz and constellation bells]

[Verse]
Yolmime cosetl, estlali huemag
Cochum, cochum, na olvai
Yolmime cosetl, ilcel blaztac
Cochum, cochum, na olvai

[Pre-Chorus]
[Instrumental: scissors-like psaltery]

[Chorus]
(Cosetl! Cosetl!)
Yolmime, cuetor ilcel!
(Cosetl! Cosetl!)
Huemag ishoc, cuetor!

[Outro]
[End]
```

**Sentido:** Las almitas son cosecha, estrella inmensa. Duerman, no olviden. Cielo en blanco. / ¡Cosecha! Almitas, vuelvan al cielo. El ojo inmenso: vuelve.

**Uso**

- 10 tomas: es la jefa más importante del Fresno. Fase 1: Verse, 48 compases (58 s). Transición: 4. Fase 2: Chorus, 48 compases.
- **Tema base de temporada.** La Jardinera cambia cada seis semanas (GDD 10.1); su música no. Cada temporada se hace un *Cover* que conserva la melodía y cambia el metal y el modo al del planeta del Tránsito (sección 3.4): el jugador la reconoce aunque cambie de rostro.

<!-- CONTINUA -->
