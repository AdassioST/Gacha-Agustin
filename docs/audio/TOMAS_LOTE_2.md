# Tomas del lote 2: victoria, derrota, jefe y el tema de inicio

Revisión del 5 de noviembre de 2026 de las siete canciones del lote 2: dos de cada ficha P08, P09 y P10 de [SOUNDTRACK_SUNO.md](SOUNDTRACK_SUNO.md), más una canción extra que elegiste como tema principal. Las medí igual que el [lote 1](TOMAS_LOTE_1.md), con ffmpeg y librosa. **No las escuché.** Todas se generaron con plan Pro, que da derechos de uso comercial.

## Qué suena en cada pantalla

Tu decisión: el tema nuevo suena al inicio, las otras dos canciones del tema principal van al menú general, y en el menú las tres suenan al azar. Además se usan las dos tomas de cada canción.

| Pantalla | Modo | Pistas |
| --- | --- | --- |
| **Inicio** (`Title`) | Bucle | Tema de inicio (t03) |
| **Menú general** (`Menu`; en el prototipo, el hub y la formación) | Al azar, una tras otra | La primera vez suena el tema de inicio; después, al azar, tema t01, tema t02 y tema t03, sin repetir la misma dos veces seguidas |
| **Combate normal** (`Battle`) | Al azar, una tras otra | Combate t01 y t02 |
| **Jefe** (`Boss`) | Al azar, una tras otra | Jefe t01 y t02 |
| **Victoria** (`Victory`) | Al azar, una tras otra | Victoria t01 y t02 |
| **Derrota** (`Defeat`) | Una vez y silencio | Derrota t01 o t02, al azar |
| **Termas** (`Termas`, Fase 3C) | Bucle | Termas de noche t01 |
| **Santuario del Fresno** (`FresnoSantuario`, Fase 2A) | Bucle | Termas de noche t02 |

El prototipo todavía no tiene pantalla de inicio propia: el hub de la Fase 2 y la formación hacen de menú general y arrancan con el tema de inicio. La lista sigue sin cortarse al pasar de uno a otro. Cuando exista la pantalla de título, usará el contexto `Title` sin cambiar código. Todo se configura en `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/musica_temporal.json`.

## Medidas

| Toma | Duración | Sonoridad | Rango (LRA) | Pico real | Tempo | Tonalidad | Pedido en la ficha |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Tema de inicio (t03) | 3:47 | −16.3 LUFS | 7.5 LU | −3.2 dBTP | ≈129 o 65 | Fa♯ menor | Prompt tuyo, no de la ficha P01 |
| Victoria t01 | 3:08 | −16.2 LUFS | 5.0 LU | −3.0 dBTP | ≈96 | Re mayor | 96 BPM, Re mayor ✓ |
| Victoria t02 | 2:59 | −15.5 LUFS | 5.0 LU | −3.5 dBTP | ≈99 | Re mayor | 96 BPM, Re mayor ✓ |
| Derrota t01 | 0:25 | −17.2 LUFS | 4.1 LU | −4.1 dBTP | ≈61 | Re menor | 60 BPM, Re menor ✓ |
| Derrota t02 | 0:18 | −17.7 LUFS | 13.4 LU | −4.7 dBTP | ≈59 | Re menor | 60 BPM, Re menor ✓ |
| Jefe t01 | 1:01 | −16.1 LUFS | 8.5 LU | −3.6 dBTP | ≈144 | Re menor | 144 BPM ✓, Mi frigio ✗ |
| Jefe t02 | 1:40 | −15.7 LUFS | 7.7 LU | −3.7 dBTP | ≈144 | Re mayor | 144 BPM ✓, Mi frigio ✗ |

La ganancia de cada pista en `musica_temporal.json` la deja en unos −18 LUFS, para que todas suenen parejas.

## Notas

- **Tempo:** salió bien en las seis tomas de ficha. La corrección de "tempo estricto" funcionó.
- **Jefe:** la letra no es la de la ficha P10. Suno escribió la suya, con palabras latinas reales (*caligo*, *ferrum*, *luna*, *dominae*) en lugar del Salmódico, y las dos tomas quedaron en Re en vez de Mi frigio. Duran 1 y 1:40 min, cuando la ficha pedía unos 3. Sirven de temporales; para la final, vuelve a generar P10 pegando la letra de la ficha en el campo de letra.
- **Victoria:** son canciones completas de 3 min, no jingles. Se usan enteras en la pantalla de victoria. El jingle de 6 s se corta al editar los WAV finales.
- **Tema de inicio:** está en Fa♯ menor, fuera del Re que comparten las demás pistas. No es problema en el inicio ni en el menú, porque entre canciones hay fundido. Pero si después se usa para finales o créditos, que la ficha pedía en Re dórico, convendrá un *Cover* en Re. **Falta su prompt:** cópialo en `Audio/Licencias/suno/mus_titulo_tema.json` (toma t03) para la trazabilidad.

## Dónde quedaron

| Qué | Ruta |
| --- | --- |
| Las trece pistas del prototipo | `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/` |
| Qué suena en cada pantalla | `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/musica_temporal.json` |
| Fichas de trazabilidad | `Audio/Licencias/suno/mus_jingle_victoria.json`, `mus_jingle_derrota.json`, `mus_jefe_casa.json` y la t03 en `mus_titulo_tema.json` |

## Descargas

Con este lote van 13 descargas de Suno este mes. Si el límite de Pro es de 20, como encontramos al investigar, quedan unas 7. El lote 3 (P02, P04 y P06) son 6 canciones.
