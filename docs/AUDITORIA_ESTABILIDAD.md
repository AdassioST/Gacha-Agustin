# Auditoría de estabilidad

Revisión del 5 de noviembre de 2026 sobre `main` (`5826537`), después de que el agente de interfaz integró `fase/3`, `ui/interfaz` y el respaldo de GitHub Desktop. Los arreglos viven en la rama `auditoria/estabilizacion`.

## Cómo se revisó

| Revisión | Resultado |
|---|---|
| Pruebas del núcleo (`dotnet test Tests/Core.Tests`) | 311 de 311 antes; 316 de 316 después |
| Pruebas EditMode en Unity 6000.6.7f1 | 323 de 323 antes; 328 de 328 después, sin advertencias del compilador |
| Prueba de estrés del meta-juego | 300 sesiones de 1 500 acciones al azar (≈450 000 operaciones, ≈25 000 combates): sin excepciones, sin saldos negativos, guardado y carga idénticos, mismo resultado con la misma semilla |
| Compilación de producción (sin Development) | Windows y Android (IL2CPP, ARM64): 0 errores |
| Lectura de código | Guardado, gacha, tienda, encargos, historia, Odisea, hub, capa uGUI y los últimos commits del agente de interfaz |

La prueba de estrés no está en el repositorio: es un programa de un solo uso que llama al núcleo real con tiradas, crafteo, Juicios, historia, Expediciones, runs del Fresno, tienda, encargos, Termas, cambios de día y guardado y carga cada pocas acciones.

## Errores corregidos

| # | Gravedad | Dónde | Qué pasaba | Arreglo |
|---|---|---|---|---|
| 1 | Crítica | `Core/Save/SaveStore.cs` | El guardado borraba el perfil y después movía el temporal. Un cierre entre los dos pasos dejaba sin perfil, y al abrir se creaba uno nuevo encima del temporal: se perdía toda la partida | Temporal escrito y forzado a disco, renombres, respaldo `.bak`. `Load` recupera el temporal completo o el respaldo |
| 2 | Crítica | `Presentation/Meta/MetaHub.cs` | Un perfil dañado dejaba el juego atorado en la pantalla de error para siempre | Si el perfil no se puede leer, se carga la copia anterior y se avisa. Un guardado de una versión más nueva nunca se sustituye (`NewerSaveException`) |
| 3 | Crítica | `MetaHub.Base.cs` y `MetaHub.Home.cs` | «Cargar perfil de prueba» existía en producción: borraba la partida real y regalaba 16 000 Sales de Baño, 1.5 millones de almitas y seis brujas | Perfil de prueba, combate libre, reembolso de prueba y datos de analítica solo en el Editor y en compilaciones de desarrollo (`MetaHub.DevTools`) |
| 4 | Crítica | `Core/World/Services.cs` | La tienda usa la plataforma de pruebas: en producción, tocar un precio entregaba Perlas, Grimorio o Diezmo sin cobrar | `UnavailableStorePlatform` en producción. `CanBuy` lo explica y los botones dicen «Pronto». En desarrollo la tienda de pruebas sigue igual |
| 5 | Alta | `MetaHub.Base.cs` | «Borrar mi cuenta» borraba todo con un solo toque | Pide un segundo toque en 4 s, con aviso. Borra también el temporal y el respaldo |
| 6 | Alta | `MetaHub.cs` y `SalYAlmasGame.cs` | El día solo cambiaba al abrir el juego. Con la app abierta pasada la medianoche no se renovaban encargos, Diezmo, charlas ni baño | `CheckDay` cada 5 s y al volver a primer plano. Solo avanza |
| 7 | Alta | `Core/Gacha/Termas.cs` | El banner de inicio podía dar una segunda Regente 5★, aunque su página de probabilidades dice que las tiradas restantes ya no dan otra | Tras la Regente, una 5★ natural del banner de inicio sale como 4★ |
| 8 | Media | `SalYAlmasGame.cs` y `MetaHub.cs` | El botón atrás de Android no hacía nada | Vuelve a la pantalla anterior del hub y sale de la formación. No actúa en el inicio, la creación, una escena, una run del Fresno, una tirada ni una transición |
| 9 | Media | `ProjectSettings.asset` | La orientación estaba fija en un solo lado horizontal | Giro automático entre las dos horizontales; vertical desactivada |
| 10 | Media | `MetaHub.Story.cs` | «Pelear» y «Combate automático» seguían activos sin equipo y lanzaban una excepción dentro de `OnGUI` | Se desactivan si el combate no trae equipo fijo y no tienes brujas |
| 11 | Media | `Core/Save/SaveStore.cs` | Un bloque en `null` en el guardado (`Settings`, `Store`…) rompía la carga más adelante | Vuelve a sus valores por defecto al cargar |
| 12 | Baja | `SalYAlmasGame.cs` | El recorrido de desarrollo `-recorrido` creaba un jugador y lo guardaba sobre el perfil real | Usa guardado en memoria, como `-capturas` |
| 13 | Baja | `MetaHub.Base.cs` | Advertencia CS0219 por una variable sin uso | Quitada |

Pruebas nuevas: banner de inicio sin segunda Regente, corte entre renombres, temporal a medio escribir, bloques en `null`, tienda sin plataforma.

También se revisó una nota vieja de `HANDOFF_ODISEA.md`: los rasgos de enemigos (reflejo, inmunidad a Aspectos, protección, enfurecer, crecer, curar, efectos al morir) y las pasivas nuevas ya están conectados en el combate.

## Revisión del trabajo del agente de interfaz

La integración en `main` es correcta: `fase/3`, `ui/interfaz` y el respaldo de Desktop son ancestros de `main`, las pruebas pasan y los registros de sus recorridos no tienen excepciones. Se encontraron dos defectos en su código y se corrigieron:

- `BattleHud.DrawEnemyPlates` tomaba el puesto de cada placa de `BattleHudLayout.ResolveEnemyPlates`, que solo tiene seis puestos, sin comprobar el índice. Un encuentro personalizado con más enemigos rompía el HUD. Ahora se omite la placa sobrante.
- Si la capa uGUI falla al construirse, `UiApp.Boot` quitaba `ExternalScreen` pero dejaba `ExternalChrome`. El hub IMGUI dejaba de dibujar fondo y barras porque creía que la capa destruida lo hacía. Ahora se limpian los tres ganchos.

Sus pendientes siguen abiertos y están en [auditoria-ui/README.md](auditoria-ui/README.md): prueba con personas, dispositivo real, estados densos y accesibilidad completa.

## Pendientes que necesitan tu decisión

| Tema | Estado | Qué falta |
|---|---|---|
| Identidad de la app | `productName` «Gacha Agustin», `companyName` «DefaultCompany», sin identificador de paquete (sale `com.DefaultCompany.GachaAgustin`) | Elegir nombre, compañía e identificador antes de publicar. En PC, cambiar nombre o compañía mueve la carpeta del guardado |
| Compras reales y servidor | No hay integración de tienda (Unity IAP) ni backend (D-04). En producción la tienda queda en «Pronto» | Integrar la plataforma real detrás de `IStorePlatform` y el servidor detrás de `IBackend` |
| Reloj del teléfono (S-05) | Adelantar la fecha del teléfono regala reinicios diarios | Reloj de servidor |
| Límite de gasto mensual | Cuenta bloques de 30 días desde el 1 de enero de 2026, no meses de calendario | Definir el corte cuando exista la tienda real |
| Arte fuente sin versionar | `art/ambientes` y `art/enemigos` (361 MB) solo existen en el disco de `gacha-fase3`. `main` tiene cambios locales y archivos nuevos en `art/bustos` | Decidir Git LFS o un almacenamiento aparte. El arte que usa el juego (`Assets/SalYAlmas/Art`) sí está en `main` |
| Colores seguros | Solo los aplica el hub IMGUI; la capa uGUI no | Llevar el ajuste a `UiTheme` |
| Tamaño y memoria en Android | El APK de producción pesa 290 MB. Las texturas suman 518 MB sin comprimir el paquete (90 % del juego), casi todas en carpetas `Resources`, y los retratos se cargan todos al abrir | Google Play pide AAB con un módulo base menor de 200 MB: hace falta Play Asset Delivery o Addressables. Medir memoria en un teléfono de gama media antes de publicar |
| Versión de Unity (D-14) | 6000.6.7f1 no es LTS | Decidir si se publica en una LTS |
| Volumen de música | `SettingsState.MusicVolume` se guarda pero no se usa; el volumen vive en `PlayerPrefs` | Unificar |

## Cómo reproducir

```powershell
dotnet test Tests/Core.Tests
Unity -batchmode -projectPath . -runTests -testPlatform EditMode -testResults Logs/editmode.xml
Unity -batchmode -quit -projectPath . -buildTarget Win64 -executeMethod SalYAlmas.EditorTools.UiBuild.Release
```

`UiBuild.Release` compila sin la opción Development y deja el juego en `Build/Produccion/<plataforma>/`. Sirve para comprobar que compilan las ramas que solo existen en producción.
