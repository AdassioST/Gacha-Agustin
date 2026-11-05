# Handoff: interfaz de Sal y Almas

Fecha: 5 de noviembre de 2026. La sesión se cortó porque la cuenta se quedó sin crédito. **El trabajo no está terminado ni confirmado.**

## Empieza aquí

1. Trabaja en `C:\Arcanoria Master\Gusi Gaming\gacha-ui`, rama `ui/interfaz`, basada en `fase/3` (d4f6fb7).
2. **Nada de la interfaz nueva tiene commit:** está sin confirmar en ese worktree. El usuario no pidió commit. Pídele permiso, o confirma en esta rama si te lo autorizó. No pongas estos archivos en `main`.
3. Corre el ciclo de verificación (abajo) antes de tocar nada: los últimos cambios no se han compilado.
4. Lee `CLAUDE.md`, y la dirección en `docs/REFERENCIAS.md` (paleta, tipografía y antirreferencias).

## Lo que pidió el usuario

Palabras suyas, resumidas:

- Arreglar toda la interfaz para que se sienta humana, creativa, hecha con alma y lista para producción, con un home tipo gacha que conecte todo.
- Investigar qué adaptar, crear y quitar, con evidencia sobre sensación premium, jugabilidad y estética.
- Nada pixelado ni distorsionado. Nada plano ni de un solo color: degradados, relieve, texturas CC0 y lo demás editado por código. Todo animado y con personalidad.
- La animación del pull debe ser realista, con VFX.
- **Combate:** que se note un bioma real y que el posicionamiento sea claro. Inspiración en HSR y Brave Frontier: de un lado las brujas, del otro los enemigos.

## Estado

| Pieza | Estado | Dónde |
| --- | --- | --- |
| Kit visual procedural con texturas CC0 | Hecho | `scripts/ui_kit.py`, `art/ui/cc0/`, `Assets/SalYAlmas/Art/Ui/Resources/Ui/` |
| Sonidos sintetizados | Hechos, **nadie los ha oído** | `scripts/ui_sfx.py`, carpeta `sfx/` |
| Shaders propios | Hechos | `scripts/ui_shaders.py`, carpeta `Shaders/` |
| Capa uGUI (título, home, Aquelarre, Manantial) | Funciona, capturada | `Assets/SalYAlmas/Presentation/Ui/` |
| Tirada con agua simulada y VFX | Funciona; **último ajuste sin compilar** | `UiPullSequence.cs`, `UiWater.cs` |
| Pantallas IMGUI con la piel y barras nuevas | Funciona, capturada; el texto sigue chico | `Meta/Theme.cs`, `GuiKit.cs`, `Meta/MetaHub.Ui.cs` |
| Escenarios y enemigos de combate | Solo PNG generados, **sin revisar a ojo ni conectar** | carpetas `battle/` y `enemigos/` |
| Combate lado a lado, HUD compacto | **Sin empezar** | ver tareas |
| Auditoría escrita con evidencia | **Sin escribir** | notas abajo |
| Revisión del código de Fase 3 | **No hecha** | prometida al usuario y al otro agente |
| Pruebas `dotnet` y EditMode tras mis cambios | **Sin correr** | no toqué `Core/` ni `Data/` |

Lo que sí se comprobó: compilación de Windows y recorrido de capturas sin excepciones, hasta antes de `patch7` (ajuste del agua y reflejo de la bruja) y del arte de combate. No se ha probado en teléfono ni con otra proporción de pantalla.

## Tareas pendientes, en orden

1. **Compilar y mirar** la tirada: `pull_6c_cinco` debe verse sin el rectángulo brillante ni la luz quemada abajo, y con el reflejo de la bruja visible. Si persiste, ajusta `WaterScene` en `scripts/ui_shaders.py`.
2. **Revisar el arte de combate** con `python scripts/ui_kit.py battle enemies` ya generado. Abre `battle/*.png` y `enemigos/*.png`. Repinta lo que no convenza: no se ha visto ni una vez.
3. **Combate lado a lado** (lo último que pidió el usuario):
   - Brujas a la izquierda y enemigos a la derecha, en dos filas escalonadas como Brave Frontier. Cambia `Anchor()` en `SalYAlmasGame.cs`.
   - Cámara lateral fija, cerca de `(0, 2.2, -11)` con FOV 40. Valor sin probar.
   - Quad de fondo con el bioma, llenando el encuadre sin deformarse. Oculta suelo y anillos de `BattleSceneBuilder`. Sombra de contacto (`enemigos/sombra`) bajo cada figura.
   - Bioma: las cuatro zonas del Fresno (`raices`, `tronco`, `ramas`, `copa`) según la zona de la run, y `salinas` en lo demás.
   - Enemigos: en `SpawnUnits`, usa arte en vez de cápsula. Mapa sugerido: `cristalizado` a golem; `corneja`, `ave_eclipse` a cuervo; `guardian`, `madre_raiz` a raiz; `corteza_hueca`, `lenador_hueco` a hueco; `larva` a larva; `recuerdo_atrapado`, `savia_errante`, `viento_salino` a voluta. Revisa el resto en `Data/fresno_enemies.json`.
   - Brujas mirando a los enemigos: voltea el quad cuando el recorte mire a la izquierda. `UnitView.Lunge` hoy avanza en Z: debe ir en X.
   - HUD: panel de bruja de 278 a unos 196 px con busto; registro plegado por defecto (hoy tapa media pantalla); Granos y cinturón más chicos; placas enemigas más angostas para que no se encimen.
   - VFX: chispas al golpe, sal al morir y almitas al ganar, en el lienzo uGUI bajo el IMGUI. Un evento en `UnitView.Flash` y `Die` basta.
4. **Quitar pixelado restante:** `Editor/BustImport.cs` importa bustos de 160 px con filtro Point, y `PortraitBank` los usa en IMGUI y combate. Ese archivo es de otro agente: confirma antes si el Point era intencional, o usa `brujas/*_busto.png`.
5. **Texto chico en IMGUI:** subí 2 puntos con mínimo de 16, y sigue chico en Yunque y Fresno. Mide en teléfono. La salida de fondo es migrar esas pantallas a uGUI.
6. **Pantallas sin revisar con la piel nueva:** Creator, Scene, Witch, FresnoRun, Odds, Ledger, Mirror. `Scene` y `Creator` usan `Theme.Sky`, ahora abisal. `UiCapture` aún no las recorre ni entra a combate: usa `Hub.DebugBattle()`, `DebugNewPlayer()` y `DebugPrologue()`.
7. **Ajustes faltantes:** el burbuja del home en `-118` puede tapar la tarjeta en pantallas 4:3; barras en `UiShell` sin probar con notch; `MotionScale` no se conecta a ningún ajuste; volumen de `UiSfx` no se conecta a Ajustes ni a la tecla N; `UiParticles.Density` no responde a Ahorro de batería.
8. **Textos en código:** la tirada y las pantallas usan `ui_loc.json`, pero `summon.title` se usa como título de resultados. Japonés: no hay fuente con kanji, así que se verá en cuadros. Falta Noto Serif JP con subconjunto.
9. **Peso:** `Resources/Ui/brujas` son 35 MB y duplican `Art/Cuerpos`. Unifica, y pasa a Addressables antes de publicar.
10. **Escribir** `docs/FASE_UI.md` (formato de `docs/FASE1.md`), con los valores `// TUNING`, y sumar la fila al README. **Escribir la auditoría** con las notas de abajo. Enviar la revisión de Fase 3.

## Ciclo de verificación

Unity 6000.6.7f1. `-capturas` solo existe en compilaciones de desarrollo.

```bash
"/c/Program Files/Unity/Hub/Editor/6000.6.7f1/Editor/Unity.exe" -batchmode -quit -projectPath "$(pwd -W)" -executeMethod SalYAlmas.EditorTools.UiBuild.Player -buildTarget Win64 -logFile Logs/build.log
```

```bash
Build/Revision/SalYAlmas.exe -screen-fullscreen 0 -screen-width 1920 -screen-height 1080 -logFile capturas/log.txt -capturas capturas home coven summon pull legacy
```

Deja PNG y `errores.txt` en esa carpeta; sin secciones, recorre todo. Para regenerar arte: `python scripts/ui_kit.py <grupo>` (`bg chrome icons gold fx logo cards battle enemies witches`). El grupo `gold` depende de `icons`, y `bg` usa `art/ui/cc0`.

```bash
dotnet test Tests/Core.Tests
```

## Decisiones y por qué

- **uGUI por código, no UI Toolkit:** permite materiales propios en cada imagen, que es lo que da vapor, agua y brillo. Va en su propio asmdef, `SalYAlmas.Ui`.
- **Capa híbrida:** `UiApp` observa `MetaHub.Current`. Home, Termas, Summon y Coven son uGUI. Lo demás sigue en IMGUI con fondo y barras uGUI (`ExternalScreen`, `ExternalChrome`). Si `UiApp.Build` falla, el hub IMGUI sigue jugable. IMGUI no es de producción: migra por pantalla.
- **Termas es el home** (`docs/propuestas/META_Y_ONBOARDING.md` 10.1) con destinos al borde derecho. Queda por encima de la estructura de Brave Frontier guardada en memoria. Bañarse y conversar pasaron al home.
- **Agua:** ecuación de onda en GPU en una textura RGHalf de 384×256. El vapor anuncia la rareza real y nunca engaña. Saltar siempre está disponible. `UiCapture.DemoPull` fabrica resultados solo para capturar y no toca el generador.
- **Sin duplicar reglas:** todo pasa por `GachaService`, `TermasBase` y el hub. El core no se tocó. Solo edité en `Presentation/`: `MetaHub.cs`, `SalYAlmasGame.cs` (`InHub`), `Theme.cs`, `GuiKit.cs`, y agregué `UiBuild.cs`.
- **Respiración de los retratos** por shader (`Hero`): puente hasta Live2D o Spine (GDD 19).

## Riesgos en el repositorio

- A las 02:54, GitHub Desktop hizo stash y pull en el checkout principal. `stash@{0}` guarda trabajo de Fase 3 y de Codex. **No lo sueltes ni lo borres.** Creé la rama `respaldo/stash-github-desktop-0254` como ancla. Ya contiene una versión vieja de mi `Art/Ui`, y es innecesaria en cuanto el usuario confirme que no falta nada.
- `main` tiene cambios ajenos sin confirmar en `MetaHub.cs`, `.Craft`, `.Termas`, `SalYAlmasGame` y `TempSfxPlayer`, según el agente de Fase 3. Habrá choques al integrar. Decide el usuario; nadie ha hecho push ni merge.
- La carpeta `Assets/TextMesh Pro` es nueva y debe confirmarse con la rama.
- No compartas el stash global: usa commits temporales.
- El agente de Fase 3 trabaja en `C:\Arcanoria Master\Gusi Gaming\gacha-fase3`, y Codex en `art/` y `Art/Bustos`.

## Auditoría: notas para el documento

Evidencia: `docs/interfaz/antes.png` y `docs/interfaz/despues.png`. **La investigación fue floja:** no encontré análisis de diseñadores sobre homes de gacha, y `docs/REFERENCIAS.md` ya advierte lo mismo. Solo cuenta como opinión de diseño lo que no está medido, y nada se midió en teléfono.

| Tipo | Qué | Evidencia |
| --- | --- | --- |
| Quitar | Bisel dorado genérico, degradados por color | Antirreferencia en REFERENCIAS y captura de antes |
| Quitar | Cuatro cartas y carrusel que ocultan destinos | Captura de antes; el destino principal debe ir primero (análisis de Clash Royale) |
| Quitar | Puntos rojos mudos | Quejas de jugadores en r/gachagaming |
| Crear | Tirada con anticipación, saltable y corta | Análisis de N3TWORK (solo snippet; la página dio 403), guías de gacha |
| Crear | Bioma y formación lateral | Pedido del usuario, HSR 6.1 |
| Adaptar | Carta natal, Libro Mayor y pergamino como identidad | GDD 19.1, REFERENCIAS 7.x |
| Pendiente | Mediciones: toques hasta combate, tamaño de texto, FPS en gama baja | Sin datos |

## Fuentes

- [ambientCG](https://ambientcg.com), texturas CC0: Paper001, Paper005, Leather030, Leather037, Metal042A, Rock030, Rock022. Copias en `art/ui/cc0/`.
- [google/fonts](https://github.com/google/fonts), OFL: Alegreya, Pirata One, Texturina, Noto Sans Symbols 1 y 2. Licencias en `Assets/SalYAlmas/Art/Ui/Licencias/`.
- [Interface In Game: Honkai Star Rail](https://interfaceingame.com/games/honkai-star-rail/), lista de capturas, sin ver imágenes.
- [Clash Royale: análisis de interfaz](https://www.gamegrin.com/articles/what-can-online-apps-learn-from-clash-royales-user-interface).
- [N3TWORK: revelado de cartas](https://medium.com/n3twork/choreographed-emotion-6-steps-to-a-great-card-reveal-ux-a6e6bb8487dd), solo el resumen de búsqueda.
- [Guía de layout de lobby](https://wikidocs.net/346302), [estudio coreano de lobby UI](https://koreascience.kr/article/CFKO201831342440777.pdf), [wiki de Brave Frontier](https://r.datuan.dev/r/bravefrontier/wiki/welcome).

## Memoria del proyecto

`C:\Users\Adass\.claude\projects\C--Arcanoria-Master-Gusi-Gaming\memory\`:

- `sal-y-almas-home-ui.md`: estructura de home tipo Brave Frontier, en horizontal, con la estética propia.
- `sal-y-almas-ui-calidad.md`: las exigencias visuales del usuario y dónde sigue el trabajo.
