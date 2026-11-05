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
- **60 fichas de prompt** con 63 prompts listos para copiar (la ficha P05 trae cuatro de *Sounds*). Tres fichas (P07, P08 y P39) producen dos cues cada una desde la misma generación.
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
| Jefes | Menores y frigios, de Re a La | 120 a 150 | 4/4 y 3/4 | Más tensos y rápidos que el combate |
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

**Ortografía para Suno:** escribe *sh* en vez de *x* (Suno leería "ks") y *ts* en vez de *tz*. Fuera de los sabores regionales de la sección 4.6, no uses *ñ*, *ll*, *th*, *w* ni *k*: Suno las pronuncia de formas distintas según el idioma que crea leer.

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
8. **Primera toma, primera revisión de pronunciación.** Si Suno lee mal una palabra, reescríbela como suena, aunque rompa la regla de ortografía (*cuetor* → *kuetor* solo en la letra, nunca en el glosario) y anota el cambio en un diccionario de pronunciación junto a los metadatos (sección 7.8).

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

### 6.10 Las otras diez regiones

Todas siguen el patrón de P11: **Bucle** de 24 a 48 compases, stems B, P y V, P con `Tension`, V desde la segunda vuelta o en escenas de historia, de 6 a 8 tomas. Cada una suma como máximo dos instrumentos invitados de su cultura. La voz canta con el sabor fonético de su región (sección 4.6). Si un grupo de voces aparece, se suma a V: el coro completo no entra en exploración.

#### P29 · `mus_region_tauro` · Llanuras de Cobre

**Título:** Istalijk
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for copper plains of windmills inspired by the Low Countries: sweet, sleepy, slightly hollow, a village that forgets. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Carillon bells play a gentle waltz melody, barrel-organ-like drone, lute and guitar, soft frame drum like turning sails. The voice hums a sweet tune that slowly loses its words. F major, 72 BPM, 3/4. Form: intro, 24-bar theme, 24 bars with voice, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, polka, accordion, circus
```

**Letra**

```text
[Intro]
[Instrumental: carillon and lute]

[Verse]
[Instrumental: carillon waltz]

[Chorus]
Olvaien istalijk, cochumen
Remelijk... olvaien...
Istalijk... mmm...
Mmm... mmm...

[Outro]
[End]
```

**Sentido:** Olvida, sal dulce, duerme. El recuerdo... olvida... Sal dulce... y las palabras se pierden. Es el pueblo de los lotófagos: la sal dulce borra recuerdos.

**Uso:** invitados carillón y organillo. **Bucle:** 48 compases (120 s).

#### P30 · `mus_region_geminis` · Ciudad Espejo

**Título:** Ermai ze Ermai
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for a mirrored canal city inspired by Venice: elegant, playful, secretive, two sister messengers and a bottled wind. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Barcarolle rocking in 6/8, mandolin tremolo, harpsichord, guitar, airy flute sweeps like wind escaping a bottle. Two female voices sing in canon and in mirrored thirds. G minor, 60 BPM in 6/8 (dotted quarter). Form: intro, 24-bar theme, 24 bars with two voices, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, opera, accordion, recorder
```

**Letra**

```text
[Intro]
[Instrumental: barcarolle, mandolin tremolo]

[Verse]
[Instrumental: harpsichord and flute]

[Chorus]
Ermai ze ermai, ishocae cuetor
(Ermai ze ermai)
Mareshae, mareshae, ilcel ze maresh
(Mareshae)

[Outro]
[End]
```

**Sentido:** Hermana es hermana: mira, regresa. Mar, mar: el cielo es mar.

**Uso:** invitados mandolina y clavecín. **Bucle:** 48 compases (96 s). La segunda voz va entre paréntesis y se suma a V. La flauta dulce está excluida aunque Tormentina tenga Ascendente Géminis: es su instrumento reservado.

#### P31 · `mus_region_cancer` · Costa de las Conchas

**Título:** Mareshenn
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for a stormy shell coast inspired by Brittany: tides, salt spray, danger under the surface. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Bombarde-like double-reed melody over a drone, guitar and low cello swelling like waves, ocean drum, deep frame drum hits like crab claws. Call-and-response between the lead voice and two answering voices. E Dorian, 80 BPM, 4/4. Form: intro, 16-bar theme, 16 bars of call and response, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, sea shanty, pub song, harp
```

**Letra**

```text
[Intro]
[Instrumental: ocean drum and drone]

[Verse]
[Instrumental: double-reed melody over waves]

[Chorus]
Mareshenn, istalek!
(Mareshenn, istalek!)
Cuetorou atlua, nelradenn
(Cuetorou atlua)

[Outro]
[End]
```

**Sentido:** ¡Mar, sal! El agua regresa, raíz.

**Uso:** invitados bombarda y tambor oceánico. **Bucle:** 32 compases (96 s). Las voces que responden se suman a V.

#### P32 · `mus_region_leo` · Desierto Áureo

**Título:** Alistal Jarqel
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for a golden desert inspired by Al-Andalus and the Maghreb: heat shimmer, sacred sun herds, forbidden beauty. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Qanun-like psaltery runs, breathy end-blown reed flute, frame drum and goblet drum groove, guitar, low cello drone, small gold bells. The voice sings melismatic, ornamented lines. D Phrygian dominant (Hijaz), 96 BPM, 4/4. Form: free intro, 16-bar groove theme, 16 bars with voice, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, oud, belly dance pop
```

**Letra**

```text
[Intro]
[Instrumental: free qanun-like psaltery]

[Verse]
[Instrumental: frame drum and goblet drum groove]

[Chorus]
Alistal jarqel, solatiu qetmaa
Aaah... alyolmah, cuetor
Solatiu, solatiu, tleugo jarqel
Aaah...

[Outro]
[End]
```

**Sentido:** La sal del rebaño, el sol que quema. Alma, vuelve. Sol, sol, fuego del rebaño. Es el ganado del Sol: tocarlo trae castigo.

**Uso:** invitados qanun (como salterio) y darbuka. **Bucle:** 32 compases (80 s). El laúd árabe está excluido: es de Solenne, aunque su Ascendente sea Leo.

#### P33 · `mus_region_virgo` · Abadía de las Herbolarias

**Título:** Cuicantum Istalis
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for an abbey of herbalist witches inspired by Central European monasteries: serene chant, incense, hidden menace. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Three women sing a plainchant-like line in unison, portative organ drone, plucked psaltery, guitar, low cello, hand bells. Slowly, small dissonances creep in under the beauty. G Dorian, 60 BPM, 4/4. Form: intro, 12-bar chant, 12-bar chant with the lead voice above, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, full choir, gospel, harp, drums
```

**Letra**

```text
[Intro]
[Instrumental: organ drone and hand bells]

[Verse]
Cuicantum istalis, nantrae lucetlum
Uarbis yolmae, cochumibus
Cuicantum istalis, nantrae lucetlum
Uarbis yolmae

[Chorus]
Cuicantum istalis, nantrae lucetlum
(Uarbis yolmae, cochumibus)

[Outro]
[End]
```

**Sentido:** Canto de la sal, luz de la madre. Almas del árbol, en sueños. Las disonancias que entran poco a poco anuncian a Circe, que vuelve bestias a los visitantes.

**Uso:** invitados órgano portativo y campanas de mano. **Bucle:** 24 compases (96 s). El arpa está excluida: es de Cupria, aunque su Ascendente sea Virgo. Las tres voces van en V.

#### P34 · `mus_region_libra` · Ducado de las Balanzas

**Título:** Istaletta
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for a rich merchant duchy inspired by Italian Renaissance city-states: courtly, polished, cold, everything has a price. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Stately pavane: lute and guitar, harpsichord, tambourine and clinking coin-like bells keeping time, low cello. Phrases alternate between A minor and C major like a scale weighing souls. 72 BPM, 4/4. Form: intro, 16-bar pavane, 16 bars with voice, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, opera, viola da gamba
```

**Letra**

```text
[Intro]
[Instrumental: pavane, coin-like bells]

[Verse]
[Instrumental: lute and harpsichord]

[Chorus]
Istaletta, yolmella, cosetlina
Amolibetto, na olvaio
Istaletta, yolmella
Cosetlina...

[Outro]
[End]
```

**Sentido:** Salecita, almita, pequeña cosecha. Librito, no olvido. En el ducado todo se paga con almas.

**Uso:** invitados clavecín y campanas de moneda. **Bucle:** 32 compases (107 s). La viola da gamba está excluida: es de Cervanda.

#### P35 · `mus_region_escorpio` · Pantanos de Azufre

**Título:** Micmorka
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for sulfur swamps inspired by Eastern European folklore: mist, ghosts, the way down to the river of souls. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Hammered dulcimer tremolo, scratchy folk fiddle, drone, guitar, low cello, slow frame drum. The lead voice uses an open-throated folk style with sharp ornaments, answered by two voices in close dissonant harmony. E Dorian with a raised fourth, 64 BPM in 6/8 (dotted quarter). Form: intro, 24-bar theme, 24 bars with voices, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, full choir, polka, accordion, clarinet
```

**Letra**

```text
[Intro]
[Instrumental: hammered dulcimer and drone]

[Verse]
[Instrumental: folk fiddle over mist]

[Chorus]
Micmorka, atluya, nelradzhe
(Micmorka...)
Yolmiya cuetorov, nochualka
(Yolmiya...)

[Outro]
[End]
```

**Sentido:** Muerte, agua, raíz. Las almitas regresan, noche. Es el descenso al Río Lácteo.

**Uso:** invitados címbalo y violín popular. **Bucle:** 48 compases (90 s). Si quieres probar un 7/8 de la región, hazlo como toma aparte; para el bucle de lanzamiento usa el 6/8 (sección 3.7).

#### P36 · `mus_region_sagitario` · Estepas del Centauro

**Título:** Estlalaar
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for endless centaur steppes inspired by Central Asian nomads: galloping, wind, vast sky, a one-eyed giant on the horizon. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Bowed horsehead-fiddle-like melody, galloping guitar and frame drum rhythm, low overtone drone, plucked lute, wooden flute in the wind. The voice sings long open calls. A minor pentatonic, 120 BPM, 4/4. Form: intro, 24-bar gallop theme, 24 bars with voice, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, western, country, banjo
```

**Letra**

```text
[Intro]
[Instrumental: galloping frame drum and drone]

[Verse]
[Instrumental: bowed fiddle melody]

[Chorus]
Estlalaar, ilceluun, cuetoraar
Aaah...
Ishocuun huemag, ilceluun
Aaah...

[Outro]
[End]
```

**Sentido:** Estrellas, cielo, volver. El ojo inmenso en el cielo: el cíclope del paso, y lo que viene detrás.

**Uso:** invitados violín de cabeza de caballo y bordón de armónicos. **Bucle:** 48 compases (96 s).

#### P37 · `mus_region_acuario` · Archipiélago de Hielo

**Título:** Hvestlur
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for a frozen archipelago inspired by Scandinavia and Iceland: icy silence, northern light, sirens luring you into cold water. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Keyed-fiddle-like resonant strings, sparse guitar harmonics, low cello, glassy bowed bowls, slow frame drum like a heartbeat in the cold. The voice sings high, piercing herding calls that echo across the water. B Dorian, 75 BPM, 3/4. Form: intro, 20-bar theme, 20 bars with voice calls, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, viking metal, war drums, horns
```

**Letra**

```text
[Intro]
[Instrumental: glassy bowls and harmonics]

[Verse]
[Instrumental: keyed fiddle over ice]

[Chorus]
Hvestlur istaley, kvetor yolmir
Aa-ooo... aa-ooo...
Atluur, atluur, cochumir
Aa-ooo...

[Outro]
[End]
```

**Sentido:** Estrella de sal, vuelvan, almas. Agua, agua, duerman. Los llamados son el canto de las sirenas.

**Uso:** invitados nyckelharpa y cuencos de cristal. **Bucle:** 40 compases (96 s). Versión gaélica si mantienes el GDD 20.1: cambia la región de inspiración y la letra por *Istalach yolmean, cuetorach / Atluach, cochumean* (pregunta 3 de la sección 9).

#### P38 · `mus_region_piscis` · Isla de la Ninfa

**Título:** Cochumion
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Exploration theme for a nymph's island inspired by Byzantium and Greece: warm sea, golden mosaics, a sweet trap where time stops. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. A low sung drone held under the melody, bowed lyra-like fiddle, hammered dulcimer, guitar, gentle frame drum. The lead voice sings slow, ornamented, hypnotic phrases that never fully resolve. E Phrygian over a constant drone, 60 BPM, 4/4. Form: intro, 12-bar theme, 12 bars with voice, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, full choir, bouzouki dance, harmonium
```

**Letra**

```text
[Intro]
[Instrumental: sung drone and lyra]

[Verse]
[Instrumental: hammered dulcimer over the drone]

[Chorus]
Cochumion maresis, atluathos
Na cuetoros, na cuetoros
Ermaion, cochumion
Mmm...

[Outro]
[End]
```

**Sentido:** Sueño del mar, agua. No vuelvas, no vuelvas. Hermana, duerme. Es la isla de Calipso: quédate para siempre.

**Uso:** invitados lira de arco y santur. **Bucle:** 24 compases (96 s). El bordón cantado va en V. El armonio está excluido: es de Selenia, aunque su Ascendente sea Piscis.

### 6.11 Eclipse y horror

#### P39 · `mus_eclipse` y `mus_eclipse_aviso` · Eclipse

**Título:** Solatiu Micmor
**Ajustes:** Instrumental no · Weirdness 35 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Eclipse event: the sun is swallowed, time holds its breath, awe and dread. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Choir: small mixed chamber choir, soft and distant. Opens with one deep bell and a choir swell that turns dark: the warning. Then a slow loop: low gong-like bells, low cello drone, guitar harmonics, the choir holding a chord that shifts from C major to C minor, sparse frame drum. No lead vocal, choir only. C minor, 60 BPM, 4/4. Form: 2-bar warning sting, 16-bar slow loop, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, solo singer, fast tempo, jump scare
```

**Letra**

```text
[Intro]
Solatiu... micmor...

[Verse]
Ilcel... neshcen...
Solatiu... na...
Ilcel... neshcen...
Solatiu... na...

[Outro]
[End]
```

**Sentido:** El sol... muere. / Cielo... ceniza. Sol... no.

**Uso**

- 6 tomas. **Aviso:** compases 1 y 2 (8 s), sin bucle; suena cuando el reloj de eclipses se cumple (cada dos regiones, GDD 6). **Bucle:** 16 compases (64 s) mientras dura el evento.
- Stems: B, P y C. Como no hay voz solista, lo que Suno separe como voz va a C.

#### P40 · `mus_horror_capa_paralaje` · Capa de Paralaje

**Título:** Lo que Mira
**Ajustes:** Instrumental sí · Weirdness 60 · Style Influence 65 · Variety 0 · Max Mode sí · Duración auto · Modelo v6 (prueba también v6-wild)

**Estilo**

```text
Dark ambient horror texture, no melody, no pulse, no key: something huge watching from beyond the sky. Recorded live in a small stone chapel with natural reverb. Sub-bass rumble with a faint low-mid hum, breath-like air, singing bowls rubbed until they groan, bowed metal, distant reversed whispers, faint radio static and slow pulsar-like clicks, long silences between events. Sparse and quiet, never a jump scare. Free time. Form: one continuous evolving texture with long gaps.
```

**Excluir estilos**

```text
melody, chords, drums, beat, guitar, choir, singing, lyrics, jump scare, loud hits, EDM, synth pad, epic trailer music, braams, cinematic riser, heartbeat
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: low rumble and breath]
[Verse]
[Instrumental: groaning bowls, distant whispers]
[Break]
[Instrumental: near silence, faint static]
[Outro]
[End]
```

**Uso**

- Es la única pieza sin ADN de estilo completo: no puede tener ensamble ni tonalidad porque se suma encima de cualquier cue. Su ADN de exclusión también es propio.
- 6 tomas. Elige 60 a 90 s sin eventos fuertes y haz el bucle con un fundido cruzado de 4 s.
- Revisa en la bocina de un celular: si no oyes nada, sube el zumbido de 100 a 200 Hz en la edición (sección 3.5).
- Sube con `Paralaje`. **Nunca suena en las Termas.**

#### P41 · `mus_huesped_combate` · Combate contra Huéspedes

**Título:** Cohsi Gameuh
**Ajustes:** Instrumental no · Weirdness 55 · Style Influence 70 · Variety 0 · Max Mode sí · Duración auto · Modelo v6 (prueba también v6-wild)

**Estilo**

```text
Battle against a cosmic entity from beyond the sky: vast, indifferent, incomprehensible, slow dread. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Choir: small mixed chamber choir, soft and distant. Slow heavy pulse on deep frame drums, low cello clusters sliding in microtones, bowed bells, scraped psaltery, the choir holding dissonant clusters that drift out of tune, faint radio-signal static and pulsar-like clicks on top. No lead vocal. No clear key, centered on C sharp. 75 BPM, 4/4. Form: intro, 32-bar slow loop, 8 bars of near silence, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, vinyl crackle, solo singer, heroic, major key, jump scare
```

**Letra**

```text
[Intro]
[Instrumental: deep drums and bowed bells]

[Verse]
Cohsi gameuh... lecli...
Ilaltse amloy...
Cohsi gameuh... lecli...
Ilaltse amloy...

[Break]
[Instrumental: near silence]

[Outro]
[End]
```

**Sentido:** Son *ishoc huemag ilcel* y *estlali yolma* escritos al revés: ojo inmenso, cielo; estrella, alma. Es la lengua de las estrellas.

**Uso**

- 8 tomas. **Bucle:** 32 compases (102 s). Los 8 compases de casi silencio son una segunda región que FMOD intercala: los silencios largos del GDD 20.4.
- Stems: B, P y C. Los Huéspedes no cantan solos: se les oye. En el ADN de exclusión se quitó *lo-fi* para dejar pasar la estática.
- Invierte además el stem C en la edición y súmalo bajo: es el Salmódico al revés de verdad.

#### P42 · `mus_horror_revelacion` · Revelación

**Título:** La Grieta
**Ajustes:** Instrumental no · Weirdness 45 · Style Influence 75 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Story revelation cue: a truth too big to hold, sorrow turning into cosmic dread. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Solo guitar and voice begin tender, then the harmony slips, the choir enters out of tune, bells ring too long, everything sinks into a deep rumble and silence. D minor dissolving into no key. 60 BPM, 4/4, free at the end. Form: 8 tender bars, 8 bars of slow collapse, long silence.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, drums, jump scare, loud hit
```

**Letra**

```text
[Verse]
Istal yolma, cuetor frashna
Nantre ilcel...
Istal yolma...

[Bridge]
Huemag ishoc...
(Aaah...)

[Outro]
[Instrumental: deep rumble, then silence]
[End]
```

**Sentido:** La sal es alma, vuelve al Fresno. Madre del cielo... / El ojo inmenso.

**Uso**

- **Ruta recomendada:** *Cover* de P01 con este estilo, para que la revelación arranque con la melodía del tema principal y se rompa.
- Sin bucle, unos 70 s. Suena en las revelaciones de la Grieta (regiones 5 a 8) y el Abismo (9 a 12), GDD 6.5.

### 6.12 Crafting

#### P43 · `mus_crafting_yunque` · Yunque Astral

**Título:** Chispa y Martillo
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode no · Duración auto

**Estilo**

```text
Forge and crafting theme: proud work, sparks, the rhythm of the hammer, cozy but strong. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Anvil and hammer strikes on beats two and four, strummed guitar and lute riff, low cello groove, frame drum, iron bells, wooden flute melody on top. A minor, 100 BPM, 4/4. Form: short intro, 32-bar working loop, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, horror, industrial
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: anvil and guitar]
[Verse]
[Instrumental: working rhythm, flute melody]
[Chorus]
[Instrumental: lute riff, iron bells]
[Outro]
[End]
```

**Uso:** 6 tomas. **Bucle:** 32 compases (77 s). Zona segura. Los golpes en 2 y 4 permiten sincronizar la animación de forja; el sonido de la forja es un efecto aparte.

#### P44 · `mus_crafting_caldero` · Caldero de Bruja

**Título:** Salmuera Turbia
**Ajustes:** Instrumental sí · Weirdness 30 · Style Influence 80 · Variety 0 · Max Mode no · Duración auto

**Estilo**

```text
Witch's cauldron brewing theme: playful, mischievous, bubbling, a pinch of danger. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Waltz with plucked lute and guitar, pizzicato low cello, bubbling psaltery runs, flute giggles, small bells and wooden spoon-like clicks. E minor with sly major turns, 90 BPM, 3/4. Form: short intro, 48-bar loop, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, circus, cartoon
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: bubbling psaltery]
[Verse]
[Instrumental: lute waltz, flute giggles]
[Chorus]
[Instrumental: sly major turn]
[Outro]
[End]
```

**Uso:** 6 tomas. **Bucle:** 48 compases (96 s). En las Termas es zona segura; en los nodos de Caldero del Fresno sí admite la capa de Paralaje.

### 6.13 Leitmotivs

Todos siguen el patrón de P15: **ruta recomendada** con melodía humana y *Cover*; si no, 10 tomas. **Firma:** los dos primeros compases del motivo, solo el stem solista (Split from Mix o Advanced Split). **Bucle de escena:** del motivo a su regreso. La Ascensión III de cada Regente hereda su tonalidad.

#### P45 · `mus_leitmotiv_selenia` · Selenia de Plata

**Título:** Polilla de Plata
**Ajustes:** Instrumental sí · Weirdness 20 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a melancholic moth witch poet who speaks with the dead: moonlit, fragile, devoted. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A solo harmonium carries the melody, breathing softly. Guitar arpeggios, low cello, softly bowed glass bowls, psaltery glints like moth wings. No drums, no vocals. B minor, 72 BPM, 3/4. Form: 2-bar intro, 8-bar main motif on harmonium, 8-bar development, motif returns, quiet ending on harmonium alone.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, drums, church organ, accordion
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: guitar arpeggios]
[Verse]
[Instrumental: solo harmonium main motif]
[Bridge]
[Instrumental: development, glass bowls]
[Chorus]
[Instrumental: harmonium motif returns]
[Outro]
[Instrumental: harmonium alone]
[End]
```

**Uso:** **Bucle de escena:** 16 compases (40 s). Selenia es limitada en la versión 1.0 (GDD 18.2): adelántala al principio del lote 6.

#### P46 · `mus_leitmotiv_solenne` · Solenne

**Título:** Gajo de Limón
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a teasing desert fox merchant witch: sly, warm, sun-drenched, playful swagger. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A solo oud carries the melody with quick ornaments and slides, frame drum and goblet drum groove, guitar, low cello, finger cymbals, psaltery flourishes. No vocals. D Phrygian dominant, 100 BPM, 4/4. Form: 2-bar intro, 8-bar main motif on oud, 8-bar development, motif returns with full groove, ending on oud alone.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, belly dance pop
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: oud flourish]
[Verse]
[Instrumental: solo oud main motif, frame drum groove]
[Bridge]
[Instrumental: development, finger cymbals]
[Chorus]
[Instrumental: oud motif returns with full groove]
[Outro]
[Instrumental: oud alone]
[End]
```

**Uso:** **Bucle de escena:** 16 compases (38 s).

#### P47 · `mus_leitmotiv_cervanda` · Cervanda

**Título:** Salero de Plata
**Ajustes:** Instrumental sí · Weirdness 20 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a regal deer witch, a sage who deserted the order of Saturn: courtly, grave, dignified, hiding guilt. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A solo viola da gamba carries the melody in a stately sarabande with weight on the second beat. Lute and guitar continuo, slow heavy low bells, psaltery ornaments. No drums, no vocals. G minor, 60 BPM, 3/4. Form: 2-bar intro, 8-bar main motif on viola da gamba, 8-bar development, motif returns, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, drums, violin solo, piano
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: lute continuo]
[Verse]
[Instrumental: solo viola da gamba main motif, sarabande]
[Bridge]
[Instrumental: development, heavy low bells]
[Chorus]
[Instrumental: viola da gamba motif returns]
[Outro]
[End]
```

**Uso:** **Bucle de escena:** 16 compases (48 s). Su Ascendente es Capricornio: si Cervanda sale en las escenas de esa región, su motivo puede sonar ahí en lugar de P12.

#### P48 · `mus_leitmotiv_tormentina` · Tormentina

**Título:** Mensajera
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a hyperactive hare messenger witch: fast, cheeky, breathless, lightning in her feet. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A solo alto recorder carries the melody with quick trills, galloping guitar strums, lute, bouncing pizzicato low cello, frame drum and tambourine, bells like sparks. No vocals. A Mixolydian, 96 BPM in 6/8 (dotted quarter), jig feel. Form: 1-bar intro, 8-bar main motif on recorder, 8-bar development, motif returns, quick ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, tin whistle, bagpipes, pub song
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: galloping guitar]
[Verse]
[Instrumental: solo alto recorder main motif, jig]
[Bridge]
[Instrumental: development, bells like sparks]
[Chorus]
[Instrumental: recorder motif returns]
[Outro]
[End]
```

**Uso:** **Bucle de escena:** 32 compases (40 s); a este tempo 16 compases se quedan cortos.

#### P49 · `mus_leitmotiv_cupria` · Cupria

**Título:** Cardenillo
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a gentle serpent herbalist witch with ambiguous morals: soothing, seductive, healing with a hidden sting. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A solo harp carries the melody with flowing arpeggios. Guitar, low cello, soft copper singing bowls, wooden flute echo. Sweet E major with sudden minor chords that slither in. No drums, no vocals. 80 BPM, 4/4. Form: 2-bar intro, 8-bar main motif on harp, 8-bar development, motif returns, ending on harp alone.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, drums, piano, new age
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: harp arpeggios]
[Verse]
[Instrumental: solo harp main motif]
[Bridge]
[Instrumental: development, copper bowls, minor chords slither in]
[Chorus]
[Instrumental: harp motif returns]
[Outro]
[Instrumental: harp alone]
[End]
```

**Uso:** **Bucle de escena:** 16 compases (48 s).

#### P50 · `mus_leitmotiv_cempa` · Cempa, la coyota

**Título:** Cempasúchil
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a trickster coyote witch: mischievous, festive, wily, with a wink of sadness. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A jarana jarocha strums a son jarocho groove with shifting 6/8 and 3/4 accents and leads the melodic riffs, zapateado footwork on a wooden platform, guitar, low cello, quicksilver bells. No vocals. D major, 96 BPM in 6/8 (dotted quarter). Form: 1-bar intro, 8-bar main motif led by the jarana, 8-bar development, motif returns, quick ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, mariachi, trumpet, accordion
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: jarana strum]
[Verse]
[Instrumental: jarana main motif, zapateado]
[Bridge]
[Instrumental: development, quicksilver bells]
[Chorus]
[Instrumental: jarana motif returns]
[Outro]
[End]
```

**Uso:** **Bucle de escena:** 32 compases (40 s). La jarana es sobre todo rítmica: si Suno no le da melodía, deja que la guitarra cante el motivo y que la jarana lo acompañe; sigue siendo su instrumento. Escúchala con alguien que conozca el son jarocho antes de aprobar (sección 8).

#### P51 · `mus_leitmotiv_jaguar` · La jaguar

**Título:** Obsidiana
**Ajustes:** Instrumental sí · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración auto

**Estilo**

```text
Character theme for a fierce jaguar witch of the obsidian salt flats: solemn, sun-scorched, predatory grace. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. A clay ocarina carries the melody, wooden slit drum and deep skin drum in a stalking rhythm, guitar drone, low cello, dry obsidian-like clicks, gold bells. No vocals. A minor pentatonic, 90 BPM, 4/4. Form: 2-bar intro, 8-bar main motif on ocarina, 8-bar development, motif returns over drums, ending.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, vocals, choir, pan flute, new age, tribal cliche
```

**Letra (estructura)**

```text
[Intro]
[Instrumental: slit drum stalking rhythm]
[Verse]
[Instrumental: clay ocarina main motif]
[Bridge]
[Instrumental: development, obsidian clicks, gold bells]
[Chorus]
[Instrumental: ocarina motif returns over drums]
[Outro]
[End]
```

**Uso:** **Bucle de escena:** 16 compases (43 s). El ID `jaguar` es provisional: cuando tenga nombre, renombra el cue y sus archivos antes de integrarlos en FMOD.

### 6.14 Ascensiones III de las Regentes

Todas siguen el patrón de P16: 8 tomas, corte de 7 s sincronizado con los pulsos de la cámara (GDD 19.3), versión corta de 2 a 3 s y vocalista humana recomendada. Cada una toma el instrumento, el metal y el Ascendente de su Regente.

#### P52 · `mus_ascension_iii_selenia` · Selenia

**Título:** Metzlu Cochum
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a moth witch poet and her twin fish spirits of moonlight: dreamlike, hypnotic, haunting. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and a high silver bell, then a harmonium chord and the voice begin the invocation; the choir blooms in soft clusters like moonlit wings, glass bowls ring; one final hush and a shimmering impact. B minor, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and high silver bell]

[Chorus]
Metzlu, cochum, ishoc!
(Aaah...)

[Outro]
[Instrumental: shimmering impact]
[End]
```

**Sentido:** Luna, sueño: ¡mira!

#### P53 · `mus_ascension_iii_solenne` · Solenne

**Título:** Solatiu Tleugo
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a desert fox witch and her lion spirit with a solar crown: blazing, proud, roaring. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and bright gold bells, a fast oud flourish and the voice launch the invocation; frame drums roll and the choir bursts in a blazing bright chord like a roar; one final impact and sizzling bells. D Phrygian dominant, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and gold bells]

[Chorus]
Solatiu, tleugo, istal!
(Aaah!)

[Outro]
[Instrumental: impact and sizzling bells]
[End]
```

**Sentido:** Sol, fuego, ¡sal!

#### P54 · `mus_ascension_iii_cervanda` · Cervanda

**Título:** Huemag Nelradi
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a regal deer witch and her goat-fish spirit of gravity: grave, crushing, solemn. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and a deep heavy bell, a low viola da gamba stroke and the voice begin; a thunder sheet rumbles and the choir sinks into low grave chords as time slows; one final crushing impact and dull bells. G minor, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and deep heavy bell]

[Chorus]
Huemag nelradi, micmor!
(Ooh...)

[Outro]
[Instrumental: crushing impact and dull bells]
[End]
```

**Sentido:** Raíz inmensa: ¡muerte! El plomo de Saturno y el trueno de Júpiter, su mezcla.

#### P55 · `mus_ascension_iii_tormentina` · Tormentina

**Título:** Ermai Ermai
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a hare messenger witch and her twin mirrored spirits: lightning fast, dazzling, double strikes. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and quick tin bells, alto recorder trills doubled in echo and the voice begin; a thunder sheet cracks, frame drums hit twice, the choir answers in two mirrored halves; a double impact and ringing bells. A Mixolydian, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and quick tin bells]

[Chorus]
Ermai, ermai, cuetor!
(Ermai!)

[Outro]
[Instrumental: double impact and ringing bells]
[End]
```

**Sentido:** ¡Hermana, hermana, vuelve! Las dos figuras en espejo de su Ascendente Géminis.

#### P56 · `mus_ascension_iii_cupria` · Cupria

**Título:** Nantre Lucetl
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 85 · Variety 0 · Max Mode sí · Duración personalizada lo más corta posible

**Estilo**

```text
Ultimate attack sung invocation for a serpent herbalist witch and her wheat-maiden spirit: warm, precise, healing with a sting. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. A breath and warm copper bowls, a harp glissando and the voice begin; the choir wraps around in a sweet enveloping chord that turns suddenly minor; one precise impact and copper bells. E major, 120 BPM, 4/4. Form: 3-beat preparation, 2-beat ritual, 6-beat sung climax, 3-beat impact, ring-out.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, long intro, many verses, ballad
```

**Letra**

```text
[Intro]
[Instrumental: breath and copper bowls]

[Chorus]
Nantre, lucetl, yolmi!
(Aaah...)

[Outro]
[Instrumental: precise impact and copper bells]
[End]
```

**Sentido:** Madre, luz, ¡almita!

### 6.15 Finales y créditos

Los cuatro son variaciones del tema principal. **Ruta recomendada:** *Cover* de la toma aprobada de P01 con el estilo de cada ficha y Audio Influence de 60 a 70, para que la melodía sea la misma y cambie todo lo demás. Los cuatro son candidatos a vocalista humana.

#### P57 · `mus_final_cosecha` · Final Cosecha

**Título:** Cosetl
**Ajustes:** Instrumental no · Weirdness 30 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Ending theme where every soul is refined and harvested by something beyond the sky: overwhelming beauty with a wrong note that never leaves. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. The voice and then the full choir sing the main hymn in radiant D major, bells cascade, psaltery shimmers, but a low cello holds a dissonant pedal under everything. 72 BPM, 4/4. Form: voice and guitar intro, verse, choir chorus, bright climax, ending on an unresolved chord.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, fast tempo, heavy drums, happy ending
```

**Letra**

```text
[Intro]
[Instrumental: voice and guitar]

[Verse]
Istal yolma, cuetor frashna
Yolmime lucetl, ilcel cosetl
Nantre ilcel, estlali nochual
Yolmime lucetl, cuetor

[Chorus]
Cosetl, cosetl, frashna
(Istal yolma)
Huemag ishoc, cosetl
(Istal yolma)

[Bridge]
Aaah...
(Aaah...)

[Outro]
Cuetor... cosetl...
[End]
```

**Sentido:** La sal es alma, vuelve al Fresno. Las almitas son luz: el cielo es cosecha. / Cosecha, cosecha, Fresno. El ojo inmenso cosecha. Todo se refinó y todo alimentó a los Huéspedes: es hermoso y está mal.

#### P58 · `mus_final_ceniza` · Final Ceniza

**Título:** Neshcen
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Ending theme where the world tree dies and souls stop returning: ashes, silence, love that remains. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Only guitar, low cello and the voice, very sparse: the main hymn melody in D minor, slower and broken, a single bell, long silences between phrases. No choir, no drums. 60 BPM, 4/4. Form: guitar intro, one verse, a fragment of the chorus, voice alone, silence.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, choir, drums, major key, big climax
```

**Letra**

```text
[Intro]
[Instrumental: guitar alone]

[Verse]
Neshcen yolma, na cuetor
Frashna micmor, istal olvai
Nosotl cuicant... nosotl...
Na cuetor

[Chorus]
Frashna... frashna...
Na cuetor...

[Outro]
Mmm...
[End]
```

**Sentido:** Alma de ceniza, no vuelve. El Fresno muere, la sal olvida. Nosotras cantamos... nosotras... No vuelve. La magia turbia mató al árbol: ya no hay reencarnación, pero quedan ellas.

#### P59 · `mus_final_libro_en_blanco` · Final Libro en Blanco

**Título:** Amolib Blaztac
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode sí · Duración auto (unos 3 min)

**Estilo**

```text
Secret ending theme: a blank page, a soul that was never written, a new beginning. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. The voice alone, unaccompanied, sings the main hymn melody; guitar enters softly, then psaltery and bells like turning pages; the choir joins in open fifths without thirds, light and unfinished. D Dorian, 72 BPM, 4/4. Form: a cappella verse, verse with guitar, gentle choir chorus, ending on an open fifth.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, heavy drums, dark, horror
```

**Letra**

```text
[Verse 1]
Amolib blaztac, neyo e tuel
Na istal, na micmor
Amolib blaztac, neyo e tuel
Cuetor...

[Verse 2]
Amolib blaztac, cuetor
Nosotl remel
Na istal, na micmor
Nosotl remel

[Chorus]
Frashna, frashna, cuetor
(Aaah...)
Amolib blaztac
(Aaah...)

[Outro]
Amolib blaztac...
[End]
```

**Sentido:** Libro en blanco, tú y yo. Ni sal ni muerte. / Libro en blanco: vuelve. Nosotras recordamos. Es el final de la Calculista, el alma que nadie escribió.

#### P60 · `mus_creditos` · Créditos

**Título:** Istal Yolma (versión completa)
**Ajustes:** Instrumental no · Weirdness 25 · Style Influence 80 · Variety 0 · Max Mode sí · Duración personalizada unos 4 min (o auto y luego *Extend*)

**Estilo**

```text
End credits song: the main hymn as a complete ballad, gratitude and farewell, bittersweet hope. Melancholic acoustic chamber folk for a dark fantasy game. Recorded live in a small stone chapel: natural medium reverb, warm close mics, organic human timing. Core ensemble: nylon-string guitar, lute, psaltery, low cello, wooden flute, small metal bells, singing bowls. Lead vocal: adult female mezzo-soprano, pure airy tone, light vibrato, invented language sung with Spanish and Latin vowels. Choir: small mixed chamber choir, soft and distant. Guitar and lute intro, verses and choruses, a low cello and flute interlude, frame drum in the last chorus, choir and voice together at the end, bells ring out. D Dorian, 72 BPM, 4/4. Form: intro, verse, chorus, interlude, verse, chorus, bridge, final chorus, outro. About four minutes.
```

**Excluir estilos**

```text
synthesizer, synth pad, EDM, electronic drums, drum machine, 808, trap, dubstep, rock drum kit, electric guitar, distortion, brass section, epic trailer music, hybrid orchestral, braams, pop vocals, autotune, rap, vocal chops, English lyrics, lo-fi, vinyl crackle, fast tempo, rock ballad, musical theater belting
```

**Letra**

```text
[Intro]
[Instrumental: guitar and lute]

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

[Instrumental: low cello and flute]

[Verse 2]
Nosotl, ermai, nosotl cuicant
Istal e yolma, cuetor
Neyo remel, tuel remel
Na olvai, na olvai

[Chorus]
Frashna, frashna, cuetor
(Istal yolma)
Nelradi e lucetl, cuetor
(Istal yolma)

[Bridge]
Atlua cochum, estlali ishoc
Nosotl cuicant, cuetor
(Aaah...)

[Chorus]
Frashna, frashna, cuetor
(Istal yolma)
Nelradi e lucetl, cuetor
(Istal yolma, istal yolma)

[Outro]
Istal yolma... cuetor
[End]
```

**Sentido:** El verso 2 es nuevo: Nosotras, hermana, nosotras cantamos. Sal y alma, vuelve. Yo recuerdo, tú recuerdas. No olvides, no olvides. / Puente: El agua duerme, la estrella mira. Nosotras cantamos: vuelve.

**Uso:** sin bucle. Si sale corta, *Extend* desde el último coro. Es la versión que se publicaría como canción del juego (sección 8, riesgos).

---

## 7. Flujo de producción

### 7.1 Antes de empezar: cuenta y lote 0

- **Una cuenta Premier a nombre del estudio**, con el correo de la empresa y verificación en dos pasos. Nadie genera desde cuentas personales: los derechos de uso comercial van con la cuenta que generó y pagó.
- **Guarda en PDF, con fecha,** los términos, la página de precios y la de derechos de Suno. Repítelo cada mes y cada vez que Suno anuncie cambios.
- **Lote 0** (unos 300 créditos). Cuatro pruebas antes de producir:

| Prueba | Qué haces | Qué decides |
| --- | --- | --- |
| Instrumental | Genera P02 dos veces: con Instrumental encendido y la estructura en la letra, y con Instrumental encendido sin letra | Cuál respeta mejor la forma; esa es la regla para todas las instrumentales |
| Stems | Genera P07 y saca Auto Split | Si la vocalización cae en Lead y los cantos en Backing; si las campanas caen en Percussion; si la suma de stems suena como la mezcla |
| Tempo | Mide el BPM real de cuatro tomas de P07 y P10 | Si se desvían más de 2 %, agrega *steady tempo, no tempo changes* al final de los estilos de bucle |
| Pronunciación | Genera P03 y revisa cada palabra del Salmódico | El diccionario de pronunciación (sección 4.5) |

### 7.2 Cuántas tomas y cómo elegir

Suno da dos canciones por pedido; una "toma" es una canción.

| Tipo de cue | Tomas |
| --- | --- |
| Ancla (P01) | 10 a 12 |
| Leitmotivs | 10 sin melodía humana; 4 a 6 con *Cover* de una melodía humana |
| Combate, jefes, Juicio, Éxtasis | 8 |
| Regiones y Fresno | 6 a 8 |
| Ascensiones III | 8 |
| Jingles, stingers y transformaciones | 4 a 6 |
| Finales y créditos (*Cover* de la ancla) | 6 |

**Rúbrica.** Escuchan tres personas, sin saber cuál es cuál, con audífonos y luego en la bocina de un celular. La toma pasa si cumple todo:

| Criterio | Pasa si |
| --- | --- |
| ADN | Puesta junto a la ancla, al mismo volumen, suena a la misma sala y los mismos instrumentos |
| Bucle | Tiene una sección estable del largo pedido, sin cambios de tempo ni de instrumentación a la mitad |
| Tempo | Deriva menor de 1 % dentro del bucle |
| Tonalidad | La pedida, o una vecina que no choque con sus transiciones (sección 3.7) |
| Voz | Salmódico claro, sin palabras en inglés, sin timbre metálico |
| Artefactos | Sin chasquidos, saltos, finales cortados ni instrumentos que aparecen y desaparecen |
| Memoria | Al día siguiente alguien tararea la melodía (tema y leitmotivs) |
| Parecido | Nadie la asocia con una obra concreta (sección 8) |

**Reparar antes de regenerar.** *Replace Section* corrige un tramo malo. *Extend* alarga una sección corta o agrega un final. *Cover* hace variaciones con la misma melodía.

### 7.3 Coherencia entre pistas

1. **ADN idéntico** en todos los prompts (sección 3.8).
2. **La ancla primero.** P01 se aprueba antes que todo lo demás. Cada toma nueva se compara lado a lado con ella.
3. **Persona de estilo.** Créala desde la toma aprobada de P01 (menú Voices) y úsala en todos los cues con voz. Si arrastra demasiado el estilo del himno (un combate que suena a balada), quítala en ese cue y confía en el ADN de voz.
4. ***Cover* para variaciones:** finales, revelación, Jardinera de temporada y familias posteriores.
5. **Una sola sala** en la edición, si hace falta igualar (sección 3.8).
6. **Una sola persona aprueba el ADN:** quien dirija la música.

### 7.4 Leitmotivs con melodía humana

- **Quién:** una compositora o compositor, aunque sea por proyecto. Escribe nueve melodías: el tema principal y los ocho leitmotivs. Cada una de 8 compases, en la tonalidad, el compás y el tempo de la tabla.
- **Cómo se graba:** la melodía sola o con acordes mínimos, en un instrumento o un MIDI con sonido limpio, con metrónomo al BPM exacto. De 30 a 60 s.
- **En Suno:** sube el audio, elige *Cover*, pega el estilo de la ficha y prueba Audio Influence entre 60 y 80.
- **Qué ganas:** la misma melodía en el tema, la Ascensión III, el Juicio, la transformación y los finales; autoría humana de la melodía (sección 8); menos riesgo de parecido, porque sabes de dónde salió.
- **Qué guardas:** partitura, audio subido, fecha, autor y contrato de cesión de derechos al estudio, junto a los metadatos del cue.

### 7.5 Stems y alineación

- **Extracción:** desde Studio (Premier, sin tope de descargas) o con Auto Split (12 stems, 50 créditos). Si la voz y el coro no se separan, usa Advanced Split y elige voz principal y coro.
- **Mapeo a capas:** tabla de la sección 5.1. Los stems Synth, Brass y FX deberían salir vacíos; si traen algo, la toma tiene instrumentos fuera del ADN: escúchala antes de aprobar.
- **Suma de comprobación:** suma B, P, V y C a 0 dB, invierte la fase de la mezcla original y súmala. Si lo que queda se oye como música, y no solo como brillo o ruido, regenera los stems o elige otra toma. Los stems de Suno se regeneran con el modelo: no esperes que sumen idéntico, pero sí muy cerca.
- **Alineación:** todos los stems de una generación empiezan en la muestra 0 y duran lo mismo. Si uno viene desplazado, alinéalo por correlación contra la mezcla antes de cortar.
- **Una sola sesión:** cada corte, estiramiento y fundido se aplica a todos los stems a la vez, en las mismas posiciones.

### 7.6 De canción a bucle sin cortes

1. Convierte a 48 kHz y 24 bits si la descarga viene en otro formato.
2. Mide el tempo real con el detector del DAW o con marcadores sobre 8 compases.
3. Si no coincide con la tabla, **estira todos los stems a la vez** hasta el BPM objetivo, solo si el cambio es menor de 2 %. Si es mayor, cambia el BPM del cue en la tabla en lugar de estirar más, y avísalo en `INTEGRACION_AUDIO.md`.
4. Pon la rejilla en el primer tiempo fuerte del bucle.
5. Elige N compases exactos (tabla 5.2) en una sección donde suenen todas las capas.
6. **Corta en compás.** Duración en muestras = N × pulsos por compás × 60 / BPM × 48,000. Ejemplo, P07: 32 × 4 × 60 / 128 × 48,000 = 2,880,000 muestras, 60 s exactos.
7. **Cola de reverb:** copia lo que suena después del final del bucle (de 1 a 3 s) y mézclalo sobre el inicio del bucle. Así, al repetirse, la reverb de la vuelta anterior sigue sonando.
8. **Fundido cruzado** de 5 a 20 ms en la unión, en cruce por cero si se puede; más largo si hay notas sostenidas.
9. **Exporta por separado** la intro (suena una vez), el bucle y el final, de cada stem, más la mezcla de referencia.
10. Escucha el bucle diez vueltas seguidas con audífonos. Si notas la unión, repite desde el paso 7.

### 7.7 Sonoridad y formato de entrega

| Parámetro | Valor |
| --- | --- |
| Formato | WAV, 48 kHz, 24 bits, estéreo. La compresión para móvil la define `INTEGRACION_AUDIO.md` |
| Bucles y pistas largas | **−18 LUFS integrados ±1 LU**, medidos sobre una vuelta del bucle con todas las capas sumadas a 0 dB |
| Stingers y jingles (menos de 10 s) | Sonoridad de corto plazo máxima de −16 LUFS |
| Capa de Paralaje (P40) | 12 dB abajo, unos −30 LUFS: es una capa que se suma |
| Pico real | **−1 dBTP** como máximo en todo |
| Stems | No se normalizan por separado: conservan su nivel relativo para que la suma reproduzca la mezcla |

**De dónde sale el valor.** El grupo de estándares de audio de Sony recomienda −18 LUFS para consolas portátiles, y la guía de niveles del IESD recomienda −16 LUFS integrados para juegos móviles como salida total ([IESD Mix Ref Levels](https://www.audiogang.org/wp-content/uploads/2015/04/IESD-Mix-Ref-Levels-v03.02.pdf), [Designing Sound](https://designingsound.org/2013/02/20/different-loudness-ranges-for-console-and-mobile-games)). Entregar la música 2 dB abajo del total deja espacio a efectos y voces. **El objetivo final de cada bus lo fija `INTEGRACION_AUDIO.md`; si difiere de este, manda aquel.**

Suno entrega audio ya masterizado. Si una toma llega muy aplastada (rango de sonoridad menor de 4 LU en una pieza que debería respirar), elige otra antes de intentar devolverle dinámica.

### 7.8 Nomenclatura y metadatos

Estructura propuesta. Los WAV van en Git LFS (decisión D-05 de `docs/HOJA_DE_RUTA.md`); los JSON van en Git normal. Coordina la ruta final con `INTEGRACION_AUDIO.md`.

```text
audio/
  fuentes/suno/<cue_id>/
    <cue_id>__t01__<id8>.wav               descarga original de Suno (t = toma)
    <cue_id>__t01__<id8>__stem_<nombre>.wav
    <cue_id>.json                          metadatos y trazabilidad
    melodia/                               partitura y audio humano subido, si aplica
  musica/<cue_id>/
    <cue_id>_intro_<capa>.wav
    <cue_id>_bucle_<capa>.wav
    <cue_id>_final_<capa>.wav
    <cue_id>_mix.wav                       referencia
```

`<id8>` son los primeros 8 caracteres del ID de la canción en Suno. `<capa>` es `base`, `perc`, `voz`, `coro`, `estereo` o `solista`. Los cues que se cortan de otra generación (como `mus_cadena_partitura`) tienen su propia carpeta en `musica/` y apuntan a la fuente en su JSON.

**Metadatos por cue** (`<cue_id>.json`):

```json
{
  "cue_id": "mus_combate_normal",
  "ficha": "P07",
  "toma_elegida": "t05",
  "suno": {
    "song_id": "id completo de la canción",
    "url": "enlace de la canción en Suno",
    "modelo": "v6",
    "cuenta": "cuenta del estudio",
    "plan_al_generar": "Premier",
    "fecha_generacion": "2026-11-12",
    "fecha_descarga": "2026-11-12",
    "canal_descarga": "Studio",
    "terminos_vigentes": "PDF de términos con fecha",
    "estilo": "texto exacto",
    "excluir": "texto exacto",
    "letra": "texto exacto",
    "ajustes": { "weirdness": 25, "style_influence": 80, "variety": 0, "max_mode": true, "instrumental": false, "duracion": "auto", "persona": "nombre o null" },
    "audio_subido": "ruta en melodia/ o null",
    "semilla": null,
    "operaciones": ["replace_section 0:45-1:05", "auto_split 12 stems"]
  },
  "tomas_descartadas": ["t01 id8", "t02 id8", "t03 id8"],
  "intervencion_humana": ["melodía de X", "edición de bucle de Y", "voz grabada por Z"],
  "revision_parecido": { "fecha": "2026-11-13", "metodo": "escucha ciega e identificación de covers", "resultado": "sin coincidencias" },
  "aprobado_por": "dirección musical"
}
```

- **La semilla queda en `null`:** no encontré que Suno la muestre. El ID y el enlace de la canción son la referencia.
- **Guarda las tomas descartadas** (solo su ID, no el audio): documentan la selección humana.
- **Nunca borres** una descarga original que llegó al juego.

### 7.9 Presupuesto de créditos y descargas

Estimado para los 66 cues de lanzamiento. Los costos de Max Mode y de las operaciones de edición no están verificados.

| Concepto | Cálculo | Créditos |
| --- | --- | --- |
| Lote 0 | Unas 30 canciones | 150 a 300 |
| Generaciones | 60 fichas × unos 4 pedidos × 10 | Unos 2,400 |
| Max Mode | Supón el doble en las fichas que lo usan | Unos 2,000 más |
| Stems | Unos 45 cues × 50 | Unos 2,250 |
| Reparaciones y *Cover* | Unas 100 operaciones | Unos 1,000 |
| **Total** | | **Unos 8,000** |

Premier (10,000 al mes) alcanza en uno o dos meses. Las descargas de canción también importan: son 66 cues y Premier da 60 al mes fuera de Studio, así que exporta desde Studio (sin tope) o reparte en dos meses. Pro (20 descargas al mes) no alcanza.

---

## 8. Riesgos legales y de calidad

Esto no es asesoría legal. La tabla resume lo que encontré; pide a un abogado de propiedad intelectual que la revise antes de la Fase 4.

| Riesgo | Qué puede pasar | Mitigación |
| --- | --- | --- |
| Derechos según el plan | Lo generado en el plan gratuito, o con la suscripción vencida, no se puede usar comercialmente, y suscribirte después no lo arregla | Premier del estudio activo durante toda la producción. Nunca usar v6-mini. Descargar cada toma por el canal de Suno. Registrar plan y fecha por toma |
| Los términos cambian | Ya cambiaron dos veces en un año: la redacción de propiedad tras el acuerdo con Warner y los límites de descarga de septiembre de 2026 | PDF de los términos con fecha en cada lote. Revisión mensual. Pregunta a tu abogado qué versión rige cada pista |
| Sin derechos de autor sobre música 100 % generada | En EE. UU. una pista hecha solo con prompts no tiene autor. Cualquiera podría reutilizarla y no podrías reclamar | Intervención humana sustancial y documentada: melodías humanas (7.4), letra en Salmódico escrita por personas (el texto sí se protege), voz e instrumentos solistas grabados por intérpretes, arreglo y edición. Al registrar, declara y excluye lo generado |
| Parecido accidental | Una toma recuerda una obra existente, incluida la de NieR | Nunca nombrar obras en los prompts. Escucha ciega con alguien que conozca bien bandas sonoras de JRPG. Pasar las tomas finales por un servicio de identificación de covers por melodía, como el de [ACRCloud](https://acrcloud.com/cover-song-identification). Ante la duda, descartar. Las melodías humanas bajan el riesgo |
| Litigios de Suno | Universal, Sony, GEMA y Koda siguen contra Suno. Un fallo puede cambiar el servicio o sus términos | No depender de Suno para lo más visible. Archivar fuentes y metadatos. Tener plan B: regrabar con músicos los cues más escuchados. Revisión legal antes de publicar en Alemania y Dinamarca |
| Tiendas | No encontré reglas de Apple ni de Google Play específicas para audio generado con IA en juegos; la política de Google Play apunta a apps que generan contenido dentro de la app ([Google Play](https://support.google.com/googleplay/android-developer/answer/14094294)). Steam (el puerto de PC) pide declarar el contenido generado con IA que el jugador consume, audio incluido ([PC Gamer](https://www.pcgamer.com/software/ai/steam-updates-ai-disclosure-form-to-specify-that-its-focused-on-ai-generated-content-that-is-consumed-by-players-not-efficiency-tools-used-behind-the-scenes/)) | Con los metadatos de la sección 7.8 tienes el inventario listo para el formulario de Steam. Revisa las guías vigentes antes de cada envío |
| Content ID y creadores | Un streamer recibe una reclamación porque alguien registró algo parecido; o tu distribuidora no acepta música generada | No registrar en Content ID pistas 100 % generadas. Publicar una política de uso para creadores. Si sale la banda sonora a la venta, solo con pistas con intervención humana y revisando las reglas de la distribuidora |
| Reputación | Parte del público de gacha y del fandom rechaza lo generado con IA | Voz humana en lo más visible, créditos claros a compositora, vocalista, lingüista e intérpretes, y transparencia |
| Representación cultural | Regiones y brujas toman culturas reales: Mesoamérica, Magreb, son jarocho, estepas | Excluir clichés en los prompts (ya están). Escuchar con personas de esas tradiciones antes de aprobar: un músico de son jarocho para Cempa, hablantes de náhuatl para Capricornio. Ninguna palabra real ni canto sagrado |
| Voz clonada | Si se crea un modelo de voz de la vocalista con Voices | Contrato explícito de uso de su voz en IA: alcance, pago, duración y revocación. Suno pide verificar en vivo que la voz es suya |
| Calidad | Artefactos, timbre metálico, tempo que deriva, stems que no suman | Rúbrica de 7.2, prueba en bocina de celular, reemplazo progresivo de lo más escuchado |

### ¿Conviene una intérprete humana?

**Sí, en un modelo híbrido.** El GDD 20.1 ya planeaba una vocalista principal y un lingüista, y es la mejor protección legal y de calidad al menor costo.

| Parte | Recomendación | Por qué |
| --- | --- | --- |
| Melodías del tema y de los 8 leitmotivs | Compositora o compositor humano | Coherencia entre pistas y autoría |
| Letras en Salmódico | Lingüista y equipo | El texto es protegible y el idioma se mantiene coherente |
| Voz del tema principal, Termas de noche, finales, créditos y las seis Ascensiones III de Regente | Vocalista humana, grabada sobre los stems de Suno en lugar de la capa V | Es lo más escuchado y lo más visible |
| Instrumentos solistas de los leitmotivs | Intérpretes humanos al menos para la firma de 2 compases; si alcanza, el leitmotiv completo | El timbre del solista es la identidad de la bruja |
| Combate, regiones, Fresno, horror, crafting y menús | Suno con edición humana | Son muchas horas de música; aquí Suno da la mejor relación costo y calidad |

Cómo se graba encima: exporta los stems sin V, la vocalista canta con metrónomo al BPM del cue y su toma sustituye a V. El resultado tiene melodía humana, interpretación humana y arreglo de Suno.

---

## 9. Preguntas para el diseñador

| # | Pregunta | Mi recomendación |
| --- | --- | --- |
| 1 | ¿Contratas a la vocalista y al lingüista del GDD 20.1, o todo con Suno? | Contrátalos. Vocalista para los cues de la sección 8; lingüista por horas para fijar el Salmódico a partir de la sección 4 |
| 2 | ¿Las melodías del tema y los leitmotivs las compone una persona? | Sí. Son nueve melodías de 8 compases; es el cambio que más mejora coherencia y derechos |
| 3 | Acuario: ¿nórdico (sección 6 del GDD) o gaélico (20.1)? | Nórdico en Acuario, que es lo que dice su inspiración cultural. Si quieres sabor celta, va mejor en Cáncer (Bretaña) |
| 4 | La Cadena es silente (7.5), pero la capa de voz suena en la Cadena (20.3). ¿Aceptas que esa voz sea una vocalización sin palabras? | Sí: la voz suena, pero nadie pronuncia hechizos. El Salmódico con palabras queda para las Ascensiones |
| 5 | ¿Suscribes Premier en una cuenta del estudio durante dos o tres meses? | Sí. Pro no alcanza por las descargas y no tiene Studio ni Advanced Split |
| 6 | ¿La Jardinera cambia de música cada temporada? | Sí, con un *Cover* del tema base: misma melodía, metal y modo del planeta del Tránsito |
| 7 | ¿Combate genérico al lanzamiento, o uno por región? | Genérico al lanzamiento (P07). Los regionales, por *Cover*, con cada región nueva |
| 8 | ¿Los Manantiales cosméticos cambian también la música de las Termas? | Sí. Es un cosmético más y suma valor al producto estrella de la tienda sin vender progreso |
| 9 | Los Huéspedes suenan "como un radiotelescopio" en un juego acústico. ¿Aceptas un toque de estática y clics? | Sí, pero poco en la música y reforzado como diseño sonoro en la capa de horror |
| 10 | ¿Llevamos un registro de instrumentos solistas reservados para las Regentes futuras? | Sí. Con una o dos Regentes por versión, los instrumentos se acaban rápido. Propongo reservar: tiorba, bandurria, nyckelharpa, kantele, duduk, marimba, santur y armónica de cristal. Si uno de ellos ya es invitado de una región, solo puede ser solista de una Regente con Ascendente en esa región |
| 11 | ¿Cómo se llama la jaguar? | Decídelo antes de generar P51 y su Ascensión. Mientras, el ID es `jaguar` |
| 12 | ¿Las Termas ignoran el Paralaje en la música aunque esté en máximo? | Sí. La regla de zona segura del GDD 6.5 pesa más |
| 13 | ¿Publicarás la banda sonora en plataformas o a la venta? | No al lanzamiento. Después, solo las pistas con intervención humana sustancial, y con revisión legal |
| 14 | ¿Agregamos al GDD 24 una regla de uso de IA en audio? | Sí: plan de pago, trazabilidad, intervención humana en lo principal y prohibición de nombrar obras o artistas en los prompts |

---

## 10. Contradicciones y huecos del GDD

| # | Dónde | Qué pasa | Cómo lo resolví aquí |
| --- | --- | --- | --- |
| 1 | 6 y 20.1 | La sección 6 dice que Acuario se inspira en Escandinavia e Islandia; la 20.1 le asigna gaélico (y está escrito "gélico"). El gaélico es celta, no nórdico | Acuario con sabor nórdico; la variante gaélica queda en la ficha P37. Pregunta 3 |
| 2 | 7.5 y 20.3 | La Cadena es magia silente "sin cantar", pero la capa de voz solista suena durante la Cadena | La voz de la Cadena vocaliza sin palabras. Pregunta 4 |
| 3 | 20.3 | Dice "FMOD o Wwise"; el contrato de audio fija FMOD | Uso FMOD. Hay que actualizar el GDD |
| 4 | 20.2 y 20.3 | La paleta base incluye violonchelo y flautas, pero el violonchelo es el solista de Marisal y la flauta dulce el de Tormentina | En la base, violonchelo grave y flauta traversa; la melodía de violonchelo y la flauta dulce quedan reservadas |
| 5 | 20.3 | "Base (cuerdas y guitarra)" deja fuera flautas, salterio, campanas y cuencos, que también están en la paleta. No dice en qué capa van | Todo lo que no es percusión ni voz va en la base. Las campanas que Suno separe como percusión se mueven a la base si hace falta |
| 6 | 20.3 | El coro completo es para Ascensión III y jefes; no dice nada del tema principal, los finales, el eclipse ni las regiones | Regla del coro de la sección 3.2: momentos cumbre sí; exploración, Termas y menús nunca |
| 7 | 10.1 | La Jardinera es la jefa de la Copa y cambia cada temporada: hay cuatro jefes de zona, no cuatro más la Jardinera | Cuatro jefes; la Jardinera tiene un tema base y variaciones por temporada |
| 8 | 20.4 y 20 | La música es acústica, pero los Huéspedes suenan "como señales de radiotelescopio", que es un sonido electrónico | Poco en la música, reforzado en diseño sonoro. Pregunta 9 |
| 9 | 20.3 | El parámetro `Tension` del contrato no existe en el GDD, que solo dice que la percusión entra "al entrar en combate" | `Tension` gobierna la percusión en regiones y Fresno; en combate siempre está activa |
| 10 | 6.5 y 20.3 | El Paralaje alto activa la Desafinación, y las Termas bajan el Paralaje y son zona segura. No dice qué pasa si entras a las Termas con Paralaje alto | La música de las Termas lo ignora siempre. Pregunta 12 |
| 11 | 18.3 | La coyota y la jaguar no tienen nombre en el GDD; el documento de retratos llama Cempa a la coyota (con Ascendente Sagitario), y la jaguar sigue sin nombre | IDs `cempa` y `jaguar` (provisional). Pregunta 11 |
| 12 | 20.2 y 22.1 | Cada Regente tiene un instrumento solista y salen una o dos Regentes por versión, pero no hay un plan de instrumentos futuros | Registro de instrumentos reservados. Pregunta 10 |
| 13 | 20.1 y 24 | El GDD planea vocalista y lingüista, pero no dice nada sobre música generada con IA, sus derechos ni su trazabilidad | Secciones 7.8 y 8. Pregunta 14 |
| 14 | 20 | El GDD nombra a los compositores y a la cantante de NieR como referencia | Correcto en un documento interno, pero esos nombres nunca entran a un prompt (sección 3.1) |

---

## 11. Fuentes

**Suno: producto** (consultadas el 5 de noviembre de 2026 por buscador; suno.com no se pudo abrir desde esta sesión)

- [Introducing v6 — Suno](https://suno.com/blog/introducing-v6) y [notas de versión](https://suno.com/release-notes/introducing-v6)
- [What's new in v6? — Suno Help](https://help.suno.com/en/articles/13924801)
- [v6 FAQ: Max Mode y Variety — Suno Help](https://help.suno.com/en/articles/13924481)
- [How do I get stems? — Suno Help](https://help.suno.com/en/articles/13925185)
- [Advanced Stem Separation — Suno Help](https://help.suno.com/en/articles/12702337)
- [How to Use: Audio Uploads — Suno Help](https://help.suno.com/en/articles/6141569)
- [Make loops and samples from scratch with Sounds — Suno](https://suno.com/release-notes/make-loops-and-samples-from-scratch-with-sounds)
- [Studio 2.0 — Suno](https://blog.suno.com/release-notes/studio-2) y [Dubspot](https://blog.dubspot.com/suno-studio-2-0)
- [Límites de caracteres — HookGenius](https://hookgenius.app/learn/suno-character-limits/) y [MySoniq](https://www.mysoniq.com/blog/suno-prompts)
- [Suno v6: canciones de 8 minutos — GenMediaLab](https://www.genmedialab.com/news/suno-v6-launch/)
- [Variety, Style Influence y Weirdness — PopularAITools](https://popularaitools.ai/blog/suno-v6-variety-style-influence-weirdness-2026)
- [Create en v6: Simple, Advanced y Sounds — Jack Righteous](https://jackrighteous.com/en-us/blogs/guides-using-suno-ai-music-creation/inside-suno-v6-create-simple-advanced-sounds-guide)
- [Guía de metaetiquetas — Jack Righteous](https://jackrighteous.com/pages/suno-ai-meta-tags-guide) y [HookGenius](https://hookgenius.app/learn/suno-metatags-complete-list/)
- [Tempo en Suno — Jack Righteous](https://jackrighteous.com/en-gb/blogs/guides-using-suno-ai-music-creation/master-tempo-in-music-comprehensive-guide-to-tempo-tags) y [prueba de la API de v6 — Challenging Voice](https://www.challengingvoice.com/suno-v6-api-podcast-audio-kit/)
- [Bloqueo de nombres de artistas — dev.ua](https://dev.ua/en/news/suno-v6-1788958562) y [Jack Righteous](https://jackrighteous.com/en-us/blogs/guides-using-suno-ai-music-creation/suno-prompt-contained-inappropriate-material)
- [Voices en v5.5 — Sonilo](https://sonilo.com/blog/suno-v5-5-voices-how-to)
- [Precios — DIYAI](https://diyai.io/ai-tools/audio-generation/suno-pricing/) y [Krater](https://krater.ai/pricing/suno)
- [Sounds: límites — Kyeo](https://kyeo.ai/en/models/suno-sounds)

**Suno: términos, descargas y licencias**

- [Límites de descarga desde el 3 de septiembre de 2026 — Jack Righteous](https://jackrighteous.com/en-us/blogs/guides-using-suno-ai-music-creation/suno-premier-60-song-download-limit-september-3-2026), [Mubert](https://mubert.com/blog/suno-download-caps) y [UseSuno](https://usesuno.com/articles/suno-download-limits-september-3-2026/)
- [Suno ajusta sus términos de propiedad tras el acuerdo con Warner — Music in Africa](https://www.musicinafrica.net/magazine/suno-adjusts-ai-music-ownership-terms-after-warner-music-partnership)
- [Derechos comerciales por plan — Dynamoi](https://dynamoi.com/learn/ai-music-distribution/suno-commercial-rights-explained) y [Veena](https://www.veena.studio/blog/suno-commercial-use-rules)
- [v6 con catálogo licenciado — DataNorth](https://datanorth.ai/news/suno-launches-v6-v6-wild-and-v6-mini) y [Boing Boing](https://boingboing.net/2026/09/16/mixed-reviews-for-ai-music-generator-sunos-industry-approved-new-model.html)

**Disqueras y tribunales**

- [Warner llega a un acuerdo con Suno — Music Business Worldwide](https://www.musicbusinessworldwide.com/warner-music-group-settles-with-suno-strikes-first-of-its-kind-deal-with-ai-song-generator/) y [TheWrap](https://www.thewrap.com/warner-music-group-partnership-suno-settles-lawsuit-ai-music-company)
- [GEMA gana contra Suno en Alemania — Music Business Worldwide](https://www.musicbusinessworldwide.com/suno-infringed-copyright-in-gema-case-german-court-rules/) y [Forbes](https://www.forbes.com/sites/virginieberger/2026/08/05/suno-lost-to-gema-why-the-ruling-should-worry-ai-music-companies/)
- [Universal y Sony contra Suno — CourtDocket](https://courtdocket.org/suno-lawsuit-explained-warner-settlement-umg-and-sony-next/) y [ChartLex](https://www.chartlex.com/blog/business/music-industry-ai-lawsuits-tracker-2026)
- [Suno (plataforma) — Wikipedia](https://en.wikipedia.org/wiki/Suno_(platform))

**Derechos de autor**

- [Copyright and Artificial Intelligence, Part 2: Copyrightability — U.S. Copyright Office (PDF)](https://www.copyright.gov/ai/Copyright-and-Artificial-Intelligence-Part-2-Copyrightability-Report.pdf)
- [Comunicado de la Library of Congress sobre la Parte 2](https://newsroom.loc.gov/news/copyright-office-releases-part-2-of-artificial-intelligence-report/s/f3959c36-d616-498d-b8f9-67641fd18bab)
- [Thaler v. Perlmutter — SCOTUSblog](https://www.scotusblog.com/cases/case-files/thaler-v-perlmutter) y [JD Supra](https://www.jdsupra.com/legalnews/supreme-court-denies-review-in-ai-2865518/)

**Tiendas, parecido y sonoridad**

- [Política de contenido generado con IA — Google Play](https://support.google.com/googleplay/android-developer/answer/14094294)
- [Steam y la divulgación de IA — PC Gamer](https://www.pcgamer.com/software/ai/steam-updates-ai-disclosure-form-to-specify-that-its-focused-on-ai-generated-content-that-is-consumed-by-players-not-efficiency-tools-used-behind-the-scenes/)
- [Cover Song Identification — ACRCloud](https://acrcloud.com/cover-song-identification)
- [IESD Mix Reference Levels (PDF)](https://www.audiogang.org/wp-content/uploads/2015/04/IESD-Mix-Ref-Levels-v03.02.pdf) y [rangos de sonoridad en consola y móvil — Designing Sound](https://designingsound.org/2013/02/20/different-loudness-ranges-for-console-and-mobile-games)

