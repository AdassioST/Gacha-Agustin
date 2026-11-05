# Auditoría de la interfaz

Qué se quitó, qué se creó y qué se adaptó al rehacer la interfaz, y con qué evidencia. Acompaña a [../FASE_UI.md](../FASE_UI.md).

## Cuánto pesa cada evidencia

La investigación sobre otros gachas dio poco. No encontré análisis de diseñadores sobre homes de gacha, y `docs/REFERENCIAS.md` ya advierte lo mismo. Por eso cada fila dice de qué tipo es su evidencia:

| Tipo | Qué significa |
| --- | --- |
| **Medido** | Captura propia del juego antes y después, o una prueba |
| **Fuente** | Un análisis o guía publicada, citada abajo |
| **Pedido** | Lo pidió el usuario |
| **Criterio** | Opinión de diseño sin datos. Se puede discutir |

Nada se midió en un teléfono real.

## Quitar

| Qué | Por qué | Evidencia |
| --- | --- | --- |
| Bisel dorado genérico y degradados de un color | Se ve plano y de plantilla. Es una antirreferencia de `REFERENCIAS.md` | Medido: `antes.png`. Pedido: "nada plano ni de un solo color" |
| Cuatro cartas y carrusel en el home | Ocultaban destinos. El destino principal debe verse primero | Medido: `antes.png`. Fuente: análisis de interfaz de Clash Royale |
| Puntos rojos sin explicación | Avisan sin decir de qué | Criterio, con quejas de jugadores en r/gachagaming como apoyo débil |
| Bustos de 160 px con filtro Point en la interfaz | Se ven pixelados al escalar a tamaños no enteros | Medido: capturas del home. Pedido: "nada pixelado" |
| Combate de frente con cápsulas | No se leía el bioma ni quién estaba de qué lado | Pedido: bioma real y posicionamiento claro |
| Registro de combate abierto | Tapaba media pantalla | Medido: capturas de combate |
| Nodos del Fresno como bloques planos de color | Contradicen la regla de no usar rellenos planos | Medido: `fresno_run` |

## Crear

| Qué | Por qué | Evidencia |
| --- | --- | --- |
| Home de las Termas con destinos al borde derecho | Un solo lugar que conecta todo, con la favorita al centro | Pedido. `docs/propuestas/META_Y_ONBOARDING.md` 10.1 |
| Tirada con anticipación, saltable y corta | La espera crea emoción solo si se puede saltar | Fuente: N3TWORK (solo el resumen; la página dio 403) y guías de gacha |
| Agua simulada que forma anillos reales | La tirada debe sentirse física | Pedido: "realista, con VFX" |
| Vapor del color de la rareza real | Anticipa sin engañar | Criterio. Coincide con la regla de honestidad del GDD 12 |
| Combate lateral con bioma y dos filas escalonadas | Bioma visible y lados claros | Pedido: HSR y Brave Frontier |
| Sombra de contacto y luz del bioma en cada figura | Sin ellas las figuras flotan sobre el fondo | Medido: capturas de combate antes y después del ajuste |
| VFX de golpe, muerte y victoria | El golpe tiene que sentirse | Pedido: "todo animado y con personalidad" |
| Piso de tamaño en el texto enriquecido | Las segundas líneas quedaban en 13 px | Medido: recorte de `hub_Forge` a tamaño real |

## Adaptar

| Qué | Cómo | Evidencia |
| --- | --- | --- |
| Carta natal, Libro Mayor y pergamino | Como identidad del kit: marcos repujados, cuero, pergamino | GDD 19.1. `REFERENCIAS.md` 7.x |
| Placa de bruja del HUD | De 278 a 196 px, con busto y Ánima a la derecha | Medido: `combate.jpg` |
| Placas enemigas | Más angostas, sin encimarse, con números de daño encima | Medido: `combate.jpg` |
| Pantallas IMGUI | Con la piel y las barras nuevas mientras se migran | Medido: `yunque.jpg` |

## Pendiente de medir

| Qué | Cómo medirlo |
| --- | --- |
| Toques desde el título hasta el primer combate | Contar en un teléfono con un perfil nuevo |
| Tamaño real del texto | Captura en un teléfono de 6.1" y de 6.7", comparar con 2.5 mm de altura de x |
| FPS en gama baja | Profiler en un Android de gama baja con Ahorro de batería encendido y apagado |
| Sonidos de interfaz | Que alguien los oiga |

## Fuentes

- [Clash Royale: análisis de interfaz](https://www.gamegrin.com/articles/what-can-online-apps-learn-from-clash-royales-user-interface)
- [N3TWORK: revelado de cartas](https://medium.com/n3twork/choreographed-emotion-6-steps-to-a-great-card-reveal-ux-a6e6bb8487dd), solo el resumen de búsqueda
- [Interface In Game: Honkai Star Rail](https://interfaceingame.com/games/honkai-star-rail/), lista de capturas, sin ver imágenes
- [Guía de layout de lobby](https://wikidocs.net/346302) y [estudio coreano de lobby UI](https://koreascience.kr/article/CFKO201831342440777.pdf)
- [Wiki de Brave Frontier](https://r.datuan.dev/r/bravefrontier/wiki/welcome)
- Texturas CC0 de [ambientCG](https://ambientcg.com) y fuentes OFL de [google/fonts](https://github.com/google/fonts)
