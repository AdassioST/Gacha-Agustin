# Revisión del código de la Fase 3

Revisión prometida al usuario y al agente de la Fase 3. Fecha: 5 de noviembre de 2026. Rama revisada: `fase/3` en 91e734e.

## Alcance

Revisé contra las reglas de `CLAUDE.md` que no se negocian: determinismo del núcleo, aislamiento de `Core/` y perfil de C# 9. Busqué patrones prohibidos en los 26 archivos de `Core/` que cambiaron desde la Fase 2, incluidos los cuatro nuevos de `Core/World/`, y leí cada coincidencia en contexto. Fue una revisión dirigida, no una lectura completa.

**No revisé** la lógica de juego línea por línea ni el balance. Tampoco revisé la presentación de la Fase 3: ya la reescribe la rama `ui/interfaz`.

## Resultado

| Regla | Resultado |
| --- | --- |
| Sin `DateTime.Now`, `System.Random` sin semilla, `Guid.NewGuid` ni `UnityEngine` en el núcleo | **Cumple**. El tiempo entra por `IClock` y `FixedClock` |
| Sin rasgos de C# más nuevos que la versión 9 | **Cumple**. Sin `record`, `init`, `required` ni patrones de lista |
| Pruebas | **Pasan** en la rama `ui/interfaz`, que contiene la Fase 3 hasta d4f6fb7: `dotnet test`, 288 de 288. EditMode en Unity, 290 de 290 |
| No depender del orden de iteración de `Dictionary` | **Un hallazgo**, abajo. Viene de la Fase 2 |

## Hallazgo

### El Caldero de la run elige la sal por el orden del diccionario

`Core/Fresno/FresnoService.cs`, en la preparación `"turbia"`:

```csharp
foreach (var kv in Run.Pouch) if (kv.Value > 0 && kv.Key.StartsWith("sal_cruda.", StringComparison.Ordinal)) { salt = kv.Key; break; }
```

`Run.Pouch` es un `Dictionary<string, int>`. Si la bolsa tiene dos sales crudas, la elegida depende del orden interno del diccionario. Ese orden suele ser el de inserción, pero cambia cuando se borran claves, y puede cambiar al guardar y cargar. `CLAUDE.md` lo prohíbe: "No dependas del orden de iteración de Dictionary".

- **Riesgo:** una run reanudada puede preparar otra Salmuera que la run original, y el servidor no podría validar la repetición.
- **Arreglo sugerido:** ordenar las claves antes de elegir, por ejemplo con el orden de `Planets.All` o por ID.
- **Origen:** commit 5e2926f, de la Fase 2. Sigue en `main` y en `fase/3`.
- **No lo arreglé:** es `Core/`, fuera del alcance de la interfaz. Lo decide la fase dueña del Fresno.

## Revisado y sin problemas

- `Battle.Actions.cs`: la ventana de Ascensiones encimadas recorre `_ascensionWindowHits`, que es una `List`. Es determinista.
- `FresnoService`: los `RunMods` se suman, así que su orden no importa.
- `StoryService`: las marcas y variantes se recorren en listas. El equipo entra ordenado.
- `WorldLoader`: `JsonNode.Properties` es una lista en el orden del archivo, que es fijo.
- `Services.cs`: las recompensas del Grimorio (`tier.Free`, `tier.Premium`) son listas. El orden de los asientos del Libro Mayor es fijo.
