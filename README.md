# Sal y Almas: La Odisea del Fresno

RPG gacha por turnos para móvil (iOS y Android, en horizontal), hecho con Unity 6 LTS y URP. Brujas furras adultas comen sal para lanzar magia, pelean con su Ascendente y cruzan el mundo para escalar el Fresno, el árbol adonde van todas las almas. *Sal y Almas* es título de trabajo.

## Estado

**Fase 1 (El Aquelarre) construida.** El núcleo de combate en C# está probado: 140 pruebas pasan con `dotnet test`. La escena de prueba de Unity compila, pero todavía hay que abrirla en Unity 6. Los detalles están en [docs/FASE1.md](docs/FASE1.md).

```bash
dotnet test Tests/Core.Tests
```

En Unity 6 LTS, abre la raíz del repositorio y usa el menú **Sal y Almas > Construir escena de combate**.

## Documentos

| Archivo | Contenido |
| --- | --- |
| [docs/GDD.md](docs/GDD.md) | Documento de diseño. Manda sobre cualquier otro documento |
| [docs/REFERENCIAS.md](docs/REFERENCIAS.md) | Referencias visuales, paleta, mapa de pantallas y antirreferencias |
| [docs/FASE1.md](docs/FASE1.md) | Fase 1: qué se construyó, cómo probarlo, valores // TUNING y huecos del GDD |
| [docs/HUECOS.md](docs/HUECOS.md) | Contradicciones del GDD, huecos de diseño y sistemas que faltan para un producto final |
| [docs/HOJA_DE_RUTA.md](docs/HOJA_DE_RUTA.md) | Cómo se integra cada sistema, plan de fases hasta el lanzamiento suave, decisiones pendientes y prompts |
| [docs/propuestas/](docs/propuestas/) | Propuestas de contenido por aprobar: Fresno, crafting, meta-juego y onboarding |
| [docs/audio/](docs/audio/) | Plan de integración de audio y banda sonora con prompts de Suno |
| [docs/Retratos para Codex.md](docs/Retratos%20para%20Codex.md) | Prompts y control de calidad de los retratos 2D |
| [CLAUDE.md](CLAUDE.md) | Reglas para los agentes que trabajan en el repositorio |

## Cómo se construye

El desarrollo se divide en fases, con un prompt por sesión de Claude Code. El GDD (sección 23) define tres; la [hoja de ruta](docs/HOJA_DE_RUTA.md) las parte en sesiones que caben y agrega las que faltan para publicar. Cada una se revisa y se sube antes de pasar a la siguiente:

1. **El Aquelarre:** núcleo de combate y magia. Después, un cierre con herramientas de contenido (1.5).
2. **La Ascensión:** Fresno y economía (2A); progreso, crafting y gacha (2B).
3. **El Mundo:** servicios y cuenta (3A); historia (3B); Termas, tirada, vestidor y audio (3C).
4. **Producto, servicio en vivo y contenido:** lo que piden las tiendas, el bucle de cada versión y el contenido del lanzamiento.

La CI corre las pruebas del núcleo en cada push (`.github/workflows/ci.yml`).

## Reglas que no se negocian

Todo el elenco es adulto en el lore y se ve adulto, sin desnudos ni actos sexuales (sección 5 del GDD). Nunca se vende progreso y no existen los duplicados (sección 16).
