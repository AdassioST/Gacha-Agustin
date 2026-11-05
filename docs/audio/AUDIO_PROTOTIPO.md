# Audio del prototipo

Cómo suenan hoy la música y los efectos en la escena de combate de la Fase 1. Todo es **temporal**: la Fase 3C lo reemplaza por FMOD según [INTEGRACION_AUDIO.md](INTEGRACION_AUDIO.md), sin tocar el núcleo, porque las dos versiones se cuelgan de los mismos eventos.

## Teclas

| Tecla | Qué hace |
| --- | --- |
| **M** | Silencia o activa la música |
| **N** | Silencia o activa los efectos |

Las dos se recuerdan entre sesiones y no responden mientras escribes en un campo de texto, como el de la semilla.

## Música

- **Pistas:** las de Suno, en `Assets/SalYAlmas/Audio/Resources/MusicaTemporal/`.
- **Qué suena en cada pantalla:** lo decide `musica_temporal.json`. La asignación vigente está en [TOMAS_LOTE_2.md](TOMAS_LOTE_2.md).
- **Código:** `TempMusicPlayer` (`Assets/SalYAlmas/Presentation/`). Hace fundidos cruzados, alterna pistas al azar y baja el volumen con la Cadena abierta.

## Efectos

- **Archivos:** 470 WAV de 165 efectos en `Assets/SalYAlmas/Audio/Resources/SfxTemporal/`. Los genera `Tools/Audio/sfx/` con síntesis propia y foley CC0 de Kenney ([README del generador](../../Tools/Audio/sfx/README.md)). Suno no se usa para efectos.
- **Código:** `TempSfxPlayer` (`Assets/SalYAlmas/Presentation/`). Escucha `BattleDirector.EventPresented`, que se dispara cuando un evento **se muestra** en pantalla. Así el golpe suena cuando se ve, también en la Cadena, donde cada golpe cae en su frame.
- **Nombres:** el reproductor busca la variante más específica que exista (`id__valor__valor`, luego `id__valor`, luego `id`) y elige al azar entre sus variaciones `__vN`. Si cambias o agregas WAV, el índice `sfx_indice.json` se reescribe solo al correr el generador.
- **Importación:** `TempAudioImport` (Editor) descomprime los efectos al cargar y pone la música en streaming.

### Qué evento dispara cada efecto

| Evento | Efecto |
| --- | --- |
| Empieza el combate | `sfx_combate_entrada` |
| Turno | `sfx_combate_turno` (aliada o enemiga); si la bruja está en Cristalización Turbia, `sfx_salinidad_perdida_vida` |
| Ataque básico | `sfx_combate_ataque__natural` y, al pegar, `sfx_combate_golpe` (ligero, medio o pesado según el daño contra la vida máxima) |
| Hechizo | Ritual de la bruja (largo la primera vez del combate, corto después), crujido de sal y aparición del Ascendente por su signo, canalización y golpe astral del planeta dominante |
| Autohechizo, Vislumbre | `sfx_magia_autohechizo` (su forma; el Frenesí suma Sal Negra), `sfx_magia_vislumbre` |
| Poción | Cinturón, destapar (según la base), beber y el efecto (cura, escudo, velocidad, retraso, salinidad o Ánima) |
| Cadena | Abrir y cerrar la pantalla, confirmar, cálculo, un eslabón por papel, una nota de la Partitura por golpe y la campana del Spark, que sube con cada Spark de la misma Cadena; fin según cuántos Spark hubo |
| Crítico, escudo, daño a una aliada | `sfx_combate_critico`, `sfx_combate_escudo` (poner o romper), `sfx_combate_dano_recibido` según la especie |
| Aspectos y Reacciones | Un intervalo por Aspecto; un sonido por Reacción Híbrida |
| Quiebre | Rotura de Firmeza y el efecto del planeta |
| Salinidad | Un sonido por banda al subir, otro al bajar, otro al salir del Éxtasis y un bucle mientras alguien esté en Cristalización |
| Granos, Resonancia y Ánima | Granos que suben o bajan, Resonancia llena y carga de cada nivel de Ascensión |
| Ascensión | Lanzar, la técnica por planeta (la III de Marisal tiene la suya) e impacto |
| Jefe | Aviso de carga y cambio de fase |
| Caída | `sfx_combate_caida`; si es enemiga, almitas que se sueltan |
| Fin del combate | `sfx_combate_victoria` o `sfx_combate_derrota` |
| Cualquier botón | `sfx_ui_tocar` |

## Límites conocidos

- **IDs de las 3★:** los datos de la Fase 1 todavía dicen `zarigueya` y `topo`. El reproductor los traduce a `muertita` y `topacia` hasta que la Fase 1.5 los renombre (HUECOS C-26).
- **Lo que falta:** no hay sonidos de armas porque el prototipo no tiene armas, y los de Termas, Fresno, tirada y crafting existen pero esperan a sus pantallas.
- **Sin compilar en esta sesión:** el código no se compiló en Unity. Al abrir el proyecto, revisa la consola y haz commit de los `.meta` nuevos.
- **Volumen:** los volúmenes relativos se ajustaron con medidas, sin escuchar. Si algo sobresale, cambia el volumen en la llamada de `TempSfxPlayer` o vuelve a generar el efecto con otra ganancia en su receta.
