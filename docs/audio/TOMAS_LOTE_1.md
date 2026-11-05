# Tomas del lote 1: tema, combate y Termas de noche

Revisión del 5 de noviembre de 2026 de las seis canciones generadas con Suno a partir de las fichas P01, P07 y P03 de [SOUNDTRACK_SUNO.md](SOUNDTRACK_SUNO.md). Las medí con ffmpeg (sonoridad EBU R128 y pico real) y con librosa (tempo, tonalidad y percusión). **No las escuché:** las medidas dicen qué toma cumple la ficha, pero el gusto, la voz y la melodía los decides tú escuchándolas.

## Resumen

Por decisión del usuario se usan **las dos tomas de cada canción**: la recomendada como principal y la otra en otra pantalla (sección "Cómo se usan").

| Cue | Toma principal | Por qué |
| --- | --- | --- |
| `mus_titulo_tema` (P01) | **t01**, con reservas | Es la única centrada en Re, como pide la ficha (Re dórico). La t02 sale en Sol mayor. Pero es la pista ancla de todo el juego y la ficha pide de 10 a 12 tomas para elegirla: dos no bastan |
| `mus_combate_normal` (P07) | **t01** | Tempo estable en unos 129 BPM durante toda la canción (pedido: 128). La t02 se acelera a unos 136 BPM desde el segundo 40, y un bucle de combate necesita tempo fijo para que la Partitura caiga en el pulso |
| `mus_termas_noche` (P03) | **t01** | Casi sin percusión (la ficha dice "No percussion") y con dinámica de canción de cuna. La t02 tiene tanta percusión como las pistas de combate, suena 3.6 LU más fuerte y está más comprimida |

## Medidas contra la ficha

| Toma | Duración | Sonoridad | Rango (LRA) | Pico real | Tempo estimado | Tonalidad estimada | Percusión* |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Tema t01 | 3:19 | −15.5 LUFS | 5.7 LU | −3.0 dBTP | 72 | Re menor / Fa mayor (compatible con Re dórico) | 0.03 |
| Tema t02 | 3:23 | −14.9 LUFS | 7.4 LU | −2.5 dBTP | 72 o 108 (ambiguo) | **Sol mayor** | 0.03 |
| Combate t01 | 2:54 | −15.6 LUFS | 10.5 LU | −3.2 dBTP | **129, estable** | Re menor | 0.08 |
| Combate t02 | 2:58 | −15.7 LUFS | 6.5 LU | −3.7 dBTP | **129 que sube a 136** | Re menor | 0.11 |
| Termas t01 | 3:09 | −15.8 LUFS | 8.2 LU | −3.8 dBTP | ≈55 por negra con puntillo | Re menor | **0.01** |
| Termas t02 | 2:08 | **−12.2 LUFS** | **4.3 LU** | −2.2 dBTP | ≈54 por negra con puntillo | Re menor | **0.11** |

\* Proporción de energía percusiva tras separar armónicos y percusión. Como referencia, las tomas de combate dan 0.08 y 0.11.

Pedido en cada ficha: tema a 72 BPM en Re dórico; combate a 128 BPM en Re menor; Termas de noche a 50 BPM (negra con puntillo) en 6/8, de Re menor a Fa mayor y sin percusión.

Notas:

- Todas las letras coinciden con su ficha. La t01 del tema trae el `[Outro]` repetido al final.
- Ninguna toma pasa de −1 dBTP de pico real. Todas están más fuertes que el −18 LUFS de entrega de la sección 7.7: hay que bajarlas de 2.2 a 3.1 dB (la t02 de Termas, 5.8 dB) al editar el bucle.
- El estimador de tempo trabaja en saltos de 1.4 BPM cerca de 128, así que "129" puede ser 128 exacto. Lo que importa es que la t01 de combate no cambia y la t02 sí.
- El rango de 4.3 LU de la t02 de Termas está casi en el límite que la sección 7.7 marca como "muy aplastada" (menos de 4 LU) para una pieza que debería respirar.

## Qué revisar al escuchar

Las medidas no ven esto. Márcalo en cada toma antes de elegir:

- [ ] La voz canta el Salmódico sin palabras en inglés ni sílabas inventadas por Suno.
- [ ] La instrumentación es la del ADN: guitarra de nailon, laúd, salterio, violonchelo, flauta, campanas y cuencos. Nada de sintetizadores ni batería de rock.
- [ ] **Combate:** existe el break de 8 compases solo con salterio y cuencos. De ahí sale `mus_cadena_partitura`, y no lo pude localizar por energía.
- [ ] **Tema:** crece de una sola voz al coro y regresa a voz y guitarra; la melodía se te queda.
- [ ] **Termas:** voz cercana y casi susurrada; ningún clímax.
- [ ] Sin artefactos de IA: voces que se deshacen, colas metálicas, cortes de compás.
- [ ] No se parece a una canción o banda sonora existente (sección 8 de SOUNDTRACK_SUNO.md).

## Derechos: plan Pro

Las seis tomas se generaron entre las 07:29 y las 07:32 UTC del 5 de noviembre de 2026 con la cuenta `adassiost` y **plan Pro**. Según lo que verificó SOUNDTRACK_SUNO.md (sección 2), lo generado con un plan de pago activo trae **derechos de uso comercial**, así que estas tomas pueden entrar al juego.

Tres cosas para que siga así:

- **Guarda hoy en PDF** los términos, la página de precios y la de derechos de Suno. Suno cambió sus términos el 3 de septiembre de 2026, y lo que cuenta es lo que estaba vigente al generar.
- **Descargas:** Pro tiene un límite de unas 20 descargas al mes, y el lote 1 ya usó seis. Para los 66 cues con varias tomas cada uno, Premier (60 al mes) sigue siendo lo recomendado en la decisión D-11 de la hoja de ruta.
- **Derechos de autor:** Suno da derechos de uso, no autoría. Para que la música tenga protección hace falta intervención humana sustancial, como una melodía compuesta por una persona o una voz grabada (sección 8 de SOUNDTRACK_SUNO.md).

## Cómo se usan: las dos tomas de cada canción

Por decisión del usuario se usan las seis tomas. La t01 de cada canción es la principal de su cue; la t02 cubre otra pantalla hasta que exista su pista propia.

| Toma | Uso en el prototipo de la Fase 1 | Destino final |
| --- | --- | --- |
| Tema t01 | Pantalla de formación | `mus_titulo_tema` (pantalla de título) |
| Tema t02 | Pantalla de victoria | Por decidir al escuchar: `mus_creditos` o una variante del título. Está en Sol mayor |
| Combate t01 | Combates normales | `mus_combate_normal` |
| Combate t02 | Combates contra jefe (el Guardián): su aceleración de 129 a 136 BPM sirve como escalada | Temporal hasta que exista `mus_jefe_casa` (P10, lote 2) |
| Termas t01 | Todavía no hay pantalla de Termas; ya está en el catálogo con el contexto `Termas` | `mus_termas_noche` (Fase 3C) |
| Termas t02 | Pantalla de derrota | `mus_fresno_santuario`, que la lista de cues define como Cover de P03 con más cuerpo |

**Cómo funciona.** `TempMusicPlayer` (en `Assets/SalYAlmas/Presentation/`) lee `musica_temporal.json`, que asigna una pista y una ganancia a cada contexto. La ganancia baja cada toma a unos −18 LUFS para que todas suenen parejas. Cambia de pista con un fundido cruzado de 1.5 s, baja el volumen a 45 % mientras la pantalla de Cadena está abierta y la tecla **M** la silencia (se recuerda entre sesiones). La música vive en un `Resources` y se importa en streaming (`Assets/SalYAlmas/Editor/TempAudioImport.cs`). Todo esto es temporal: la Fase 3C lo reemplaza por FMOD sin tocar el núcleo.

Para cambiar qué suena en cada pantalla, edita `musica_temporal.json`: no hace falta tocar código.

## Dónde quedaron

| Qué | Ruta |
| --- | --- |
| Las seis tomas, listas para el prototipo | `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/TEMP_<cue_id>__t0N__<id8>.mp3` |
| Qué suena en cada pantalla | `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/musica_temporal.json` |
| Ficha de trazabilidad de cada cue: ID y enlace de cada toma en Suno, fecha, estilo y exclusiones exactos, letra usada, medidas y uso | `Audio/Licencias/suno/<cue_id>.json` |

Los nombres siguen la sección 7.8 de SOUNDTRACK_SUNO.md. En las fichas ya está el plan Pro. Quedan **por confirmar** el modelo (v6 o v6-wild) y los ajustes que usaste: llénalos tú, porque solo tú lo sabes.

## Lecciones para los siguientes lotes

- **Tempo:** la t02 de combate se aceleró aunque el estilo decía 128 BPM. En las pistas que se vuelven bucle, el estilo ahora pide tempo estricto y la lista de exclusiones lleva `accelerando, tempo change`.
- **Tonalidad:** la t02 del tema salió en Sol. El estilo ahora repite la tonalidad y pide quedarse en ella, y las exclusiones llevan `key change, modulation`.
- **Percusión:** la t02 de Termas trajo percusión aunque el estilo decía "No percussion". En las pistas sin percusión, el estilo lo dice al principio y las exclusiones nombran los instrumentos (`frame drum, hand percussion, shaker, tambourine`).
- **Efectos:** por decisión del usuario, Suno queda solo para canciones. Los stingers de vapor de la tirada (P05) ya no se hacen con Suno: se diseñan con los demás efectos.

## Siguientes pasos

1. Abre el proyecto en Unity, deja que importe y prueba la música en la escena. Haz commit de los `.meta` que genere.
2. Escucha las seis con la lista de arriba. Si prefieres otra asignación, cambia `musica_temporal.json`.
3. Guarda en PDF los términos de Suno de hoy y considera pasar a Premier antes del lote 2 por el límite de descargas.
4. Genera el lote 2 (P08, P09 y P10) con las fichas ya corregidas de SOUNDTRACK_SUNO.md.
5. Del tema principal, genera más tomas hasta tener de 10 a 12 (la ficha P01 lo pide porque es la ancla). Lo ideal es la ruta de la ficha: una persona compone la melodía, la subes y usas *Cover*.
6. De las tomas definitivas, descarga el WAV por el canal de Suno (no el MP3), saca los stems y edita el bucle a −18 LUFS (secciones 7.5 a 7.7).
