# Sal y Almas: La Odisea del Fresno

RPG gacha por turnos para móvil (iOS y Android, en horizontal), hecho con Unity 6 LTS y URP. Brujas furras adultas comen sal para lanzar magia, pelean con su Ascendente y cruzan el mundo para escalar el Fresno, el árbol adonde van todas las almas. *Sal y Almas* es título de trabajo.

## Estado

**Fase 1 (El Aquelarre) construida.** El núcleo de combate en C# está probado: 140 pruebas pasan con `dotnet test`. La escena de prueba de Unity compila, pero todavía hay que abrirla en Unity 6. Los detalles están en [docs/FASE1.md](docs/FASE1.md).

```bash
dotnet test Tests/Core.Tests
```

En Unity 6 LTS, abre la raíz del repositorio y usa el menú **Sal y Almas > Construir escena de combate**.

**Fase 2 (La Ascensión) en curso.** El núcleo de progreso, Fresno, crafting, gacha, economía y guardado está escrito y compila; faltan las pruebas completas, el piloto de runs, el balance del Fresno y la interfaz de Unity. El estado y lo que falta están en [docs/FASE2_PROGRESO.md](docs/FASE2_PROGRESO.md).

## Documentos

| Archivo | Contenido |
| --- | --- |
| [docs/GDD.md](docs/GDD.md) | Documento de diseño. Manda sobre cualquier otro documento |
| [docs/REFERENCIAS.md](docs/REFERENCIAS.md) | Referencias visuales, paleta, mapa de pantallas y antirreferencias |
| [docs/FASE1.md](docs/FASE1.md) | Fase 1: qué se construyó, cómo probarlo, valores // TUNING y huecos del GDD |
| [docs/FASE2_PROGRESO.md](docs/FASE2_PROGRESO.md) | Fase 2: qué está hecho, qué falta, decisiones y huecos del GDD (traspaso) |
| [docs/Retratos para Codex.md](docs/Retratos%20para%20Codex.md) | Prompts de los retratos de cuerpo completo de la primera ola |
| [docs/Bustos para Codex.md](docs/Bustos%20para%20Codex.md) | Análisis del arte de Codex, recortes en `art/recortes/` y prompts de los bustos de diálogo |

## Cómo se construye

El desarrollo se divide en tres fases, con un prompt por sesión de Claude Code (sección 23 del GDD). Cada una se revisa y se sube antes de pasar a la siguiente:

1. **El Aquelarre:** núcleo de combate y magia.
2. **La Ascensión:** progreso, Fresno, crafting y gacha.
3. **El Mundo:** historia, Termas, presentación y producto.

## Reglas que no se negocian

Todo el elenco es adulto en el lore y se ve adulto, sin desnudos ni actos sexuales (sección 5 del GDD). Nunca se vende progreso y no existen los duplicados (sección 16).
