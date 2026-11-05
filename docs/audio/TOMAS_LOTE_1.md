# Tomas del lote 1: tema, combate y Termas de noche

Revisión del 5 de noviembre de 2026 de las seis canciones generadas con Suno a partir de las fichas P01, P07 y P03 de [SOUNDTRACK_SUNO.md](SOUNDTRACK_SUNO.md). Las medí con ffmpeg (sonoridad EBU R128 y pico real) y con librosa (tempo, tonalidad y percusión). **No las escuché:** las medidas dicen qué toma cumple la ficha, pero el gusto, la voz y la melodía los decides tú escuchándolas.

## Resumen

| Cue | Toma recomendada | Por qué |
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

## Pendiente legal: con qué plan se generaron

Las seis tomas se generaron entre las 07:29 y las 07:32 UTC del 5 de noviembre de 2026 con la cuenta `adassiost`. Según lo que verificó SOUNDTRACK_SUNO.md (sección 2), **solo lo que se genera con un plan de pago activo trae derechos de uso comercial, y lo del plan gratuito nunca los gana, aunque te suscribas después.**

- Si se generaron con Pro o Premier: sirven para el juego. Anota el plan en los metadatos.
- Si se generaron con el plan gratuito: úsalas solo como pistas temporales y como referencia de lo que quieres. Las finales se vuelven a generar con Premier (D-11 de la hoja de ruta).

## Dónde quedaron

| Qué | Ruta |
| --- | --- |
| Las seis tomas como pistas temporales de la Fase 3C | `Audio/FMOD/Assets/Temporal/TEMP_<cue_id>__t0N__<id8>.mp3` |
| Ficha de trazabilidad de cada cue: ID y enlace de cada toma en Suno, fecha, estilo y exclusiones exactos, letra usada y medidas | `Audio/Licencias/suno/<cue_id>.json` |

Los nombres siguen la sección 7.8 de SOUNDTRACK_SUNO.md. En las fichas quedan **por confirmar** el plan con el que se generaron, el modelo (v6 o v6-mini) y los ajustes que usaste. Llénalos tú, porque solo tú lo sabes.

## Siguientes pasos

1. Escucha las seis con la lista de arriba y elige una toma por cue. Anótala en `toma_elegida` de su ficha.
2. Confirma el plan de Suno. Si era el gratuito, contrata Premier antes de generar más.
3. Del tema principal, genera más tomas hasta tener de 10 a 12 (la ficha P01 lo pide porque es la ancla). Lo ideal es la ruta de la ficha: una persona compone la melodía, la subes y usas *Cover*.
4. De la toma elegida, descarga el WAV por el canal de Suno (no el MP3) y saca los stems: base, percusión, voz y coro (sección 7.5).
5. Edita el bucle en compases, ajusta la sonoridad a −18 LUFS y anota las regiones en la ficha (sección 7.6 y 7.7).
6. Las descargas crudas y las tomas descartadas van fuera de Git (sección 7.8). Estas seis se quedan como temporales hasta que existan los WAV editados.
