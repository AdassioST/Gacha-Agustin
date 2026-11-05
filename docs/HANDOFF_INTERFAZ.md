# Handoff: interfaz de Sal y Almas

Fecha: 5 de noviembre de 2026, segunda sesión. Lo construido y cómo correrlo está en [FASE_UI.md](FASE_UI.md). Este documento solo dice dónde quedó y qué falta.

## Empieza aquí

1. Trabaja en `C:\Arcanoria Master\Gusi Gaming\gacha-ui`, rama `ui/interfaz`, basada en `fase/3` (d4f6fb7).
2. Todo está confirmado en `ui/interfaz`, sin push ni merge. No lo pongas en `main` sin que lo decida el usuario.
3. Lee `CLAUDE.md` y la dirección visual en `docs/REFERENCIAS.md`.
4. Revisa con capturas reales, a 16:9 y a 20:9. El ciclo está en FASE_UI.md, "Cómo correrlo".

## Dónde quedó

La primera sesión dejó el kit visual, la capa uGUI y la tirada, sin compilar el último ajuste del agua. Se cortó por falta de crédito.

La segunda sesión compiló y verificó el agua, construyó el combate lateral, compactó el HUD de combate, quitó el pixelado de los bustos y conectó los ajustes. También rehízo tres enemigos, escribió el reporte, la auditoría y la revisión de la Fase 3, y corrió todas las pruebas.

| Pieza | Estado | Evidencia |
| --- | --- | --- |
| Kit visual, home, Aquelarre, Manantial | Verificado en capturas | `docs/interfaz/despues.png`, `home_movil.jpg`, `home_4x3.jpg` |
| Tirada con agua y reflejo | Verificado en capturas | `docs/interfaz/tirada_agua.jpg` |
| Combate lateral, bioma, VFX y HUD compacto | Verificado en capturas, con un combate completo en automático | `docs/interfaz/combate.jpg`, `combate_movil.jpg` |
| Bustos suaves en IMGUI y combate | Verificado en capturas | `combate.jpg` |
| Texto enriquecido con piso de tamaño | Verificado en capturas | `docs/interfaz/yunque.jpg` |
| `dotnet test Tests/Core.Tests` | 288 de 288 | Corrido en esta sesión |
| EditMode en Unity | 290 de 290 | `Logs/ui-20261105/editmode.xml` (ignorado por git) |
| Auditoría | Escrita | `docs/interfaz/AUDITORIA.md` |
| Revisión de la Fase 3 | Escrita, con un hallazgo de determinismo en `Core/` | `docs/interfaz/REVISION_FASE3.md` |
| Teléfono real | **Sin probar** | Solo se simuló 20:9 en Windows |
| Sonidos de interfaz | **Nadie los ha oído** | |

## Qué falta, en orden

1. **Probar en un teléfono.** Medir texto, toques hasta el primer combate y FPS con Ahorro de batería. La tabla está en AUDITORIA.md, "Pendiente de medir".
2. **Oír los sonidos** de `scripts/ui_sfx.py` y ajustar.
3. **Decisiones del usuario:**
   - Agregar "reducir movimiento" y volumen de efectos a `SettingsState`. Toca `Core/Save`; la UI ya los espera (`UiTheme.MotionScale`, `UiSfx.Volume`).
   - El hallazgo de REVISION_FASE3.md: la elección de sal en el Caldero de la run depende del orden de un `Dictionary`.
4. **Migrar a uGUI** las pantallas IMGUI. En 20:9 dejan franjas a los lados, y el HUD de combate también. Empieza por el combate y el Fresno.
5. **Arte de enemigos** propio para el Tronco y la Copa. Hoy seis familias cubren a todos (`BattleStage.EnemyArt`). El procedural se lee bien, pero no está al nivel de las brujas.
6. **Revisar a ojo** Creator, Scene y Witch con la piel nueva. `UiCapture` no entra a Creator ni a Scene: usa `Hub.DebugNewPlayer()` y `DebugPrologue()`.
7. **Recortes que se voltean**: `BattleStage.FacesLeft` solo voltea a `brea` y `selenia`. Revisa cada arte nuevo.
8. **Fuente japonesa**: Noto Serif JP con subconjunto.
9. **Peso**: `Resources/Ui/brujas` duplica `Art/Cuerpos` (35 MB). Unificar y pasar a Addressables.
10. **Valores sin `// TUNING`** en la capa uGUI: muchos tiempos y tamaños de `Ui/*.cs` no llevan la marca. FASE_UI.md lista los principales.

## Riesgos en el repositorio

- **No sueltes ni borres `stash@{0}`.** A las 02:54, GitHub Desktop guardó ahí trabajo de la Fase 3 y de Codex. La rama `respaldo/stash-github-desktop-0254` es su ancla, y sobra en cuanto el usuario confirme que no falta nada.
- **Choques al integrar.** `main` tiene cambios ajenos en `MetaHub.cs`, `SalYAlmasGame.cs` y `TempSfxPlayer`. Esta rama también los toca. Además, `fase/3` avanzó a 91e734e después de la base de esta rama. Decide el usuario.
- **Unity reescribe `Assets/UniversalRenderPipelineGlobalSettings.asset`** en cada compilación. Es ruido: no lo confirmes.
- **Un atasco sin reproducir.** El primer recorrido de combate en automático no avanzó y lo cortó el tiempo límite. Los tres siguientes terminaron. `UiCapture.Trace` deja el estado en el log por si vuelve.
- El agente de la Fase 3 trabaja en `C:\Arcanoria Master\Gusi Gaming\gacha-fase3`, y Codex en `art/` y `Art/Bustos`. `Editor/BustImport.cs` es de otro agente y no se tocó.

## Memoria del proyecto

`C:\Users\Adass\.claude\projects\C--Arcanoria-Master-Gusi-Gaming\memory\`:

- `sal-y-almas-home-ui.md`: estructura del home, en horizontal.
- `sal-y-almas-ui-calidad.md`: las exigencias visuales del usuario y dónde sigue el trabajo.
