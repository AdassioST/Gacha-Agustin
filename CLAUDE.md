# Guía para agentes: Sal y Almas

RPG gacha por turnos para móvil (iOS y Android, horizontal) en Unity 6 LTS con URP. Lee esto antes de tocar el repositorio.

## Fuentes de verdad

1. `docs/GDD.md` manda sobre todo lo demás. Si tu tarea lo contradice, gana el GDD y lo reportas.
2. Las propuestas de `docs/propuestas/` y `docs/audio/` solo cuentan cuando el GDD las cita o el usuario las aprueba. Mientras tanto son la mejor guía para llenar huecos, y cada valor que tomes de ellas lleva `// TUNING`.
3. `docs/HUECOS.md` lista contradicciones y huecos conocidos del GDD, y cómo los resolvió cada fase. Revisa ahí y en los reportes `docs/FASE*.md` antes de inventar un número.
4. `docs/HOJA_DE_RUTA.md` dice en qué fase va cada sistema y cómo se integra.

## Reglas que no se negocian

- Todo el elenco es adulto en el lore y se ve adulto. Sin desnudos ni actos sexuales (GDD sección 5).
- Nunca se vende progreso: almitas, sal, materiales, Savia, grados, Juicios, ranuras de Eco ni ventajas del Fresno (16.4).
- No existen los duplicados: una bruja que ya tienes no puede salir del gacha (12.2).
- Nada de *kompu gacha*: ningún sistema pide juntar un set obtenido con tiradas (sección 2).
- Nada de la palabra *Stand*, del tarot como nombres de Ascendentes ni de la iconografía de JoJo (8.3 y 24).

## Arquitectura

Siete piezas (GDD 21.2). La que más importa al programar:

- **Núcleo** (`Assets/SalYAlmas/Core/`): C# puro con `noEngineReferences: true`. Nada de `UnityEngine`. Toda regla de juego vive aquí.
- **Presentación** (`Assets/SalYAlmas/Presentation/`): consume los eventos tipados del núcleo y los reproduce. Nunca decide reglas.
- **Datos** (`Assets/SalYAlmas/Data/`): JSON cargado y validado por el núcleo.
- **Editor** (`Assets/SalYAlmas/Editor/`): herramientas y constructores de escena.
- **Pruebas:** `Tests/Core.Tests/` (dotnet, fuera de Unity) y `Assets/SalYAlmas/Tests/EditMode/`.

Los sistemas nuevos siguen el mismo patrón: reglas en el núcleo, servicios externos (servidor, guardado, tienda, analítica, reloj) detrás de una interfaz con una implementación local o falsa para pruebas.

## Determinismo del núcleo

Con la misma semilla y las mismas entradas, el núcleo da el mismo resultado evento por evento. Eso permite validar en servidor y repetir combates.

- El azar entra solo por un generador con semilla inyectada. Nunca `System.Random` sin semilla, `Guid.NewGuid`, `DateTime.Now` ni `UnityEngine.Random`.
- El tiempo real (día planetario, fase lunar, reinicio diario) entra por una interfaz de reloj, nunca leído directo.
- No dependas del orden de iteración de `Dictionary` o `HashSet`. Ordena antes de iterar si el orden afecta el resultado.
- Redondea a entero en los límites del estado (daño, vida, Firmeza, Salinidad). Los cálculos en `double` son deterministas en una misma máquina, pero el servidor (.NET en x64) y el teléfono (IL2CPP en ARM64) pueden diferir en el último bit.
- Toda regla nueva emite eventos. La presentación no lee el estado interno para decidir qué mostrar.

## Convenciones

- Identificadores de código en inglés; comentarios, documentos, registro de combate e interfaz en español de México.
- Los IDs de contenido van en `snake_case` sin acentos y son los mismos en datos, arte, audio y localización. Ejemplo: `marisal`, y su arte en `art/retratos/marisal/`.
- Todo número que no venga del GDD lleva `// TUNING` en código o en datos, y se lista en el reporte de la fase.
- Los textos visibles para el jugador no se escriben en el código: van en datos o en tablas de localización.

## Pruebas

Requiere el SDK de .NET 8:

```bash
dotnet test Tests/Core.Tests
dotnet run --project Tests/Core.Sim -- balance guardian 100
```

`Tests/Core.Lib` compila el núcleo como .NET Standard 2.1 con C# 9, el perfil de Unity. No uses rasgos de C# más nuevos en el núcleo. Las pruebas compartidas viven en `Assets/SalYAlmas/Tests/EditMode/`; las que solo corren en Unity, en su carpeta `Unity/`, desde el Test Runner. La CI (`.github/workflows/ci.yml`) corre las pruebas del núcleo en cada push y pull request.

Antes de entregar: todas las pruebas de las fases anteriores pasan, y si cambiaste reglas del núcleo, los combates grabados siguen dando el mismo resultado o explicas por qué cambiaron.

## Cómo se trabaja

- Una fase por sesión, en su propia rama. La sección 23 del GDD y `docs/HOJA_DE_RUTA.md` tienen los prompts.
- Al cerrar una fase, escribe su reporte en `docs/FASE<id>.md` (como `docs/FASE1.md`): qué se construyó, cómo correrlo, valores `// TUNING`, huecos y contradicciones del GDD.
- No reescribas sistemas de fases anteriores sin decirlo en el reporte.
- Los documentos se escriben en español de México, con tuteo, oraciones cortas, tablas Markdown y encabezados en tipo oración, como el GDD.
