# Interfaz · El home de las Termas, la tirada y el combate lateral

Rehechura de la interfaz de *Sal y Almas* sobre la Fase 3. Un home tipo gacha conecta todo, la tirada cae en agua simulada y el combate pasó a vista lateral, con las brujas a la izquierda y los enemigos a la derecha. Rama `ui/interfaz`, basada en `fase/3` (d4f6fb7). Nada de esto cambia reglas: `Core/` y `Data/` no se tocaron.

## Estado

| Parte | Estado |
| --- | --- |
| Kit visual (marcos, cuero, pergamino, iconos, sellos) | **Verificado en capturas** a 16:9, 20:9 y 4:3 |
| Capa uGUI: título, home, Aquelarre, Manantial | **Verificado en capturas** |
| Tirada con agua simulada y VFX | **Verificado en capturas**: sin el rectángulo brillante y con el reflejo de la bruja |
| Combate lateral con bioma, sombras y VFX | **Verificado en capturas**, con un combate completo en automático hasta la victoria |
| HUD de combate compacto | **Verificado en capturas** a 16:9, 20:9 y 4:3 |
| Pantallas IMGUI con la piel nueva | **Verificado en capturas**. Siguen siendo IMGUI |
| Pruebas | `dotnet test`: 288 de 288. EditMode en Unity: 290 de 290 |
| Teléfono real | **Sin probar**. Solo se simuló la proporción 20:9 en Windows |
| Sonidos de interfaz | **Nadie los ha oído** |

## Qué se construyó

### Capa uGUI

`Assets/SalYAlmas/Presentation/Ui/`, asmdef `SalYAlmas.Ui`. `UiApp` observa `MetaHub.Current`. Home, Termas, Manantial y Aquelarre son uGUI. Las demás pantallas siguen en IMGUI, con fondo, barra superior y navegación uGUI (`ExternalScreen`, `ExternalChrome`). Si `UiApp.Build` falla, el hub IMGUI sigue jugable.

### Tirada

`UiPullSequence` y `UiWater`. La ecuación de onda corre en GPU sobre una textura RGHalf. El vapor toma el color de la rareza real y nunca engaña. Saltar siempre está disponible. La cuadrícula final se titula con `summon.results`.

### Combate lateral

Inspiración pedida por el usuario: Honkai: Star Rail y Brave Frontier. Las piezas:

- `BattleStage`: cámara lateral fija, fondo del bioma que llena el encuadre sin deformarse, y puestos pensados en el lienzo de 1920×1080 que se proyectan al suelo. Oculta el suelo y los anillos de la escena de la Fase 1, y los devuelve al salir.
- Bioma: en el Fresno, la zona de la run (`raices`, `tronco`, `ramas`, `copa`); fuera, `salinas`.
- Brujas en dos filas escalonadas a la izquierda; enemigos a la derecha. Cada figura lleva sombra de contacto y el tinte de luz del bioma.
- Enemigos con arte (`Ui/enemigos/`). El ID se mapea por familia en `BattleStage.EnemyArt`. Sin arte, vuelve la cápsula.
- `UnitView`: la embestida va en X, el destello aclara el recorte, y al morir se deshace en sal.
- `BattleFx`: chispas al golpe, sal al morir y almitas al ganar. Densidad baja con Ahorro de batería y con Reducir destellos.
- Tocar una figura la elige como objetivo, aunque los recortes no lleven collider.

### HUD de combate

- Placa de bruja de 278 a 196 px, con busto suave, vida y Salinidad con texto sobre cinta legible, Ánima y Ascensiones. Los estados salen en una cinta encima de la placa.
- Registro plegado por defecto, con botón **Registro**.
- Granos y cinturón más compactos.
- Placas enemigas de 224 px (300 el jefe) que no se enciman: la de atrás sube. Los números de daño se dibujan encima.
- Botones de acción más altos: el Autohechizo y el Vislumbre ya no se cortan.

### Legibilidad y pixelado

- `PortraitBank.Bust` usa los bustos de 320 px con filtro suave de `Ui/brujas`. Los de 160 px con filtro Point quedan de respaldo. `Editor/BustImport.cs` no se tocó: es de otro agente.
- `GuiKit.Legible` aplica el piso de tamaño también a los `<size=N>` del texto enriquecido: más 2 puntos y mínimo 15.

### Ajustes conectados

- Ahorro de batería baja `UiParticles.Density` y `BattleFx.Density`.
- Reducir destellos llega a `UiTheme.ReduceFlashes` y baja los VFX de combate.
- La tecla N silencia también los sonidos de interfaz (`UiSfx.Muted`).

### Arte procedural

`scripts/ui_kit.py`. Grupos `battle` (5 biomas) y `enemies` (golem, raíz, cuervo, hueco, voluta, larva y sombra). En esta sesión se rehicieron el golem (figura encorvada de cubos con brasa entre las juntas), el cuervo (perfil hacia las brujas, ala alzada) y la larva (segmentos sombreados uno por uno). También se calmó el bokeh de `raices`.

## Cómo correrlo

Unity 6000.6.7f1.

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe" -batchmode -quit -projectPath "$(pwd -W)" -executeMethod SalYAlmas.EditorTools.UiBuild.Player -buildTarget Win64 -logFile Logs/build.log
```

El recorrido de capturas necesita una ruta **absoluta**. Con una relativa, Unity la resuelve dentro de `SalYAlmas_Data` y no guarda nada:

```bash
Build/Revision/SalYAlmas.exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -capturas "$(pwd -W)/capturas/revision" home coven summon pull legacy combate
```

Secciones: `home`, `coven`, `summon`, `pull`, `legacy` y `combate`. Sin secciones, recorre todo. `combate` abre una run del Fresno, entra al primer combate, lo pasa a automático y captura hasta el resultado. Para el teléfono, usa `-screen-width 2400`.

Arte: `python scripts/ui_kit.py battle enemies`.

## Valores // TUNING

| Valor | Dónde | Por qué |
| --- | --- | --- |
| Puestos de brujas y enemigos en el lienzo | `BattleStage.WitchSlots`, `EnemySlots`, `BossSlot` | Dejan libre el HUD: franja de 100 a 870 y columna derecha desde 1590 |
| Cámara en (0, 3.4, -12), mira a (0, 1.2, 0), FOV 40 | `BattleStage` | La fila de atrás se lee sin volverse diminuta |
| Tinte de luz por bioma | `BattleStage.Tint` | Integra los recortes al fondo |
| Recortes que se voltean: `brea`, `selenia` | `BattleStage.FacesLeft` | Miran a la izquierda en su arte. Revisar con cada arte nuevo |
| Altura del recorte 2.05 m; enemigo ×0.92, jefe ×1.45 | `SalYAlmasGame.CreateFigure`, `SpawnUnits` | Proporción con las brujas |
| Densidad de VFX: 0.4 con batería, 0.6 sin destellos | `SalYAlmasGame.ApplySettings` | Gama baja y accesibilidad |
| Densidad de partículas uGUI 0.45 con batería | `UiApp.SyncSettings` | Gama baja |
| Piso de `<size=N>`: +2 y mínimo 15 | `GuiKit.Legible` | Segundas líneas legibles en teléfono |
| Agua de 384×256 | `UiWater` | Costo de GPU contra detalle |
| 0.14 s entre cubos que caen | `UiPullSequence` | Ritmo de la tirada |

## Huecos y decisiones pendientes

- **No hay ajuste de "reducir movimiento"** en `SettingsState`. `UiTheme.MotionScale` queda en 1. Agregarlo toca `Core/Save`, así que lo decide el usuario o la fase dueña del perfil.
- **El volumen de efectos no es un ajuste**: solo existe silenciar con N. `UiSfx.Volume` queda en 0.8.
- **IMGUI con franjas en 20:9**: el lienzo IMGUI mantiene 16:9 y deja bandas a los lados. Afecta al HUD de combate y a las pantallas viejas. La salida es migrarlas a uGUI.
- **Japonés**: no hay fuente con kanji. Hace falta Noto Serif JP con subconjunto.
- **Peso**: `Resources/Ui/brujas` son 35 MB y duplican `Art/Cuerpos`. Unificar y pasar a Addressables antes de publicar.
- **El arte de enemigos es procedural.** Se lee bien en combate, pero no está al nivel de las brujas pintadas. Faltan los enemigos propios de la Copa y del Tronco: hoy se reparten entre seis familias.
- **Un atasco sin reproducir**: el primer recorrido de combate en automático se quedó sin avanzar y lo cortó el tiempo límite. Los tres siguientes terminaron bien. `UiCapture.Trace` deja el estado del combate cada segundo en el log por si vuelve.
- **Pantallas sin revisar con la piel nueva**: Creator, Scene y Witch. `UiCapture` aún no entra a Creator ni a Scene. Odds, Ledger, Mirror y el mapa de la run sí se revisaron: los nodos del mapa pasaron de bloques planos a sellos de cuero, y las filas de Probabilidades crecen con el texto.

## Evidencia

En `docs/interfaz/`: `antes.png`, `despues.png`, `combate.jpg`, `combate_movil.jpg`, `home_movil.jpg`, `home_4x3.jpg`, `tirada_agua.jpg`, `tirada_5.png`, `yunque.jpg` y `fresno_mapa.jpg`. La auditoría está en [interfaz/AUDITORIA.md](interfaz/AUDITORIA.md) y la revisión de la Fase 3 en [interfaz/REVISION_FASE3.md](interfaz/REVISION_FASE3.md).
