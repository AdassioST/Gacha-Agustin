# Fase 2: La Ascensión

El bucle completo de progreso: escalar el Fresno, perder y recuperar almas, refinar brujas sin duplicados, fabricar armas y Salmueras, y tirar en el gacha. Las reglas viven en el núcleo de C#, y la interfaz de prueba usa figuras simples (GDD 23.2).

## Cómo correrlo

```bash
dotnet test Tests/Core.Tests
dotnet run --project Tests/Core.Sim -- gacha
dotnet run --project Tests/Core.Sim -- fresno 200
dotnet run --project Tests/Core.Sim -- fresno-log 1
dotnet run --project Tests/Core.Sim -- juicio todas 20
```

En Unity, usa **Sal y Almas > Jugar combate**. La escena ahora abre en el hub, con estas pestañas:

| Pestaña | Qué hay |
| --- | --- |
| Fresno | Equipo, Eco, cinturón, Raíces, Ofrenda, mapa, maldiciones, Injertos, Santuario, Mercader, Caldero y Eco al terminar. Los botones del piloto juegan un paso o la run entera |
| Grados | Nivel, Dignidad, Talentos, Vía, vista previa de las tres fases y Juicios |
| Yunque | Armas, reforja, equipar, recetas, fusiones y Altar del Árbol |
| Caldero | Base, sal, reactivo, Recetario e inventario |
| Termas | Banners, termómetro de pity, vapor por rareza, selector e historial |
| Libro | Saldos y cada movimiento con su fuente o su sumidero |

Los combates del Fresno y de los Juicios usan el HUD y la Cadena de la Fase 1. El botón **Combate libre** abre la formación de la Fase 1. El perfil se guarda en `Application.persistentDataPath/guardado/perfil.json`, y **Perfil nuevo** lo reinicia.

## Qué se construyó

| Sistema | Archivo | Qué hace |
| --- | --- | --- |
| Ganchos de combate | `Core/Combat/Modifiers.cs` y `BattleSetup` | Nivel, grado, arma, prefijos, Injertos, Frutos, Mutaciones, maldiciones y Raíces. Sin ellos, el combate es idéntico al de la Fase 1 |
| Datos | `Data/*.json` y `Core/Meta/MetaLoader.cs` | 8 archivos con validación cruzada |
| Libro contable | `Core/Economy/Ledger.cs` | Rechaza flujos no declarados y saldos negativos |
| Guardado | `Core/Save/` | JSON detrás de `ISaveStore`, listo para apuntar a un servidor |
| Progreso | `Core/Progress/` | Nivel, grados, Juicio de Pureza, Vía, Dignidad y Talentos |
| Crafting | `Core/Crafting/` | Yunque, Altar, reforja y Caldero |
| Gacha | `Core/Gacha/Termas.cs` | Tasas, pity, 75/25, exclusión, banner de inicio y selector |
| Fresno | `Core/Fresno/` | Generador, runs, Injertos, Mancha, Eco, Raíces, Ofrenda y tope semanal |
| Piloto de runs | `Core/Fresno/AutoClimber.cs` | Juega runs completas para pruebas, balance y la interfaz |
| Simulador | `Tests/Core.Sim/MetaSim.cs` | Comandos `gacha`, `fresno`, `fresno-log` y `juicio` |
| Interfaz | `Presentation/Meta/` | El hub de arriba |

## Criterios de aceptación

| Criterio | Estado | Cómo se comprobó |
| --- | --- | --- |
| Todas las pruebas de las Fases 1 y 2 pasan | Cumplido | `dotnet test`: 250 de 250 |
| Tres runs seguidas, y la tercera se siente distinta | Cumplido por prueba | `AceptacionTresRunsSeguidasLaTerceraEsDistinta`: Eco, Mancha recuperada y una Raíz comprada cambian el inicio de la tercera. Falta que alguien juzgue la sensación jugando |
| Una bruja sube de D a B por sus Juicios | Cumplido por prueba | `AceptacionUnaBrujaSubeDeDaBPorSusJuicios` |
| La simulación del gacha cae dentro de ±1 tirada | Cumplido | 52.71 por 5★ y 65.72 por la destacada (semilla 2026) |

## Balance del Fresno

Se simularon 200 campañas de tres runs con el perfil de prueba y el equipo Marisal, Selenia, Solenne y Tormentina.

| Run | Victorias | Zona media | Almitas ganadas | Guardadas | Savia | Tiempo estimado |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 4.5 % | 2.9 de 4 | 79 641 | 55 935 | 1 200 | 48 min |
| 2 | 23.5 % | 3.5 de 4 | 124 612 | 107 360 | 2 861 | 64 min |
| 3 | 33 % | 3.7 de 4 | 143 846 | 93 271 | 7 390 | 68 min |

- El GDD pide de 60 a 80 minutos por run completa. Las runs que llegan lejos caen en ese rango.
- La curva de victorias sube sola con el Eco, las Raíces y la Mancha.
- El único ajuste de valores fue `atkScale` 0.9 en las Raíces Putrefactas: 15 % de las primeras runs caían en la zona 1.
- El tiempo es una estimación: 7 s por turno, 30 s por Cadena, 25 s de reproducción por combate y 20 s por nodo sin combate.
- El piloto no juega como una persona. Falta validar el balance con jugadores.

## Decisiones tomadas

| Tema | Decisión |
| --- | --- |
| Pity suave | La tirada 60 ya lleva +7 %: p(n) = 0.006 + 0.07·(n − 59). Da los promedios del GDD (HUECOS, sección 2) |
| Exclusión (HUECOS C-23) | El 25 % solo existe si queda una 5★ estándar que no tengas. Si ya tienes a la destacada, la 5★ sale del estándar y el banner lo avisa |
| Contador (HUECOS C-24) | El banner de inicio lleva contador propio; la garantía pasa entre limitados |
| "4 zonas de 10 nodos" | Cada zona tiene 10 pisos y una ruta pasa por 10 nodos. Los pisos 2 a 9 tienen tres carriles |
| Costo(n) | Almitas para subir del nivel n − 1 al n |
| Nivel mínimo por grado | Igual al nivel del Ascendente de su Juicio: C 40, B 50, A 60, S 70, SS 80 |
| Cobro del grado | Se exige poder pagar antes del Juicio y se cobra solo al ganar |
| "En seis turnos" | Seis rondas (ciclos de AV) |
| "Solo con brujas 3★" | Las aliadas son 3★; la bruja del Juicio puede ser de cualquier rareza |
| Mancha | Pasa a la siguiente run como nodo. Si la run termina sin recuperarla, el Fresno la absorbe y devuelve 30 % como Savia |
| Abandonar | Cuenta como caer |
| Eco | No se gasta al usarlo; una run usa a lo más uno |
| Brujas caídas | Tras una victoria vuelven con 10 % de vida |
| Armas | La familia define el básico y, si tiene ecos, agrega golpes a la Cadena. El arma recién fabricada no tiene prefijo |
| Savia como ingrediente | Dos recetas de 14.2 piden "savia": se usa la Savia del Fresno, 50 por preparación |

## Valores // TUNING

Todos están marcados en los JSON. Estos son los que más pesan en el balance:

| Valor | Actual | Archivo |
| --- | --- | --- |
| Crecimiento de estadísticas por nivel | 5 % | `progression.json` |
| Sal refinada por grado | 5, 10, 5, 10 y 20 | `progression.json` |
| Dignidad: costos y daño | 15 000 a 180 000; +4 % a +16 % | `progression.json` |
| Talentos: costo y bono | 2 000·n^1.5; +4 % por nivel | `progression.json` |
| Ascendente: vida y ataque | 60 000 a nivel 52; ×0.85 | `progression.json` |
| ATQ por nivel de arma | 80, 180, 320, 500 y 720 | `weapons.json` |
| Afinidad de planeta | +8 % | `weapons.json` |
| Refinación | 5 a 1, con 1 000 y 5 000 almitas | `weapons.json` |
| Potencias de Salmuera | ×1, ×1.5 y ×2 | `brewing.json` |
| Almitas por zona | 1 500, 2 200, 3 000 y 4 000 | `fresno.json` |
| Multiplicadores de élite y jefe | ×2.5 y ×6 | `fresno.json` |
| Tope semanal y su tasa | 300 000 almitas; 10 % a Savia | `fresno.json` |
| Savia por zona | 400, 600, 800 y 1 200 | `fresno.json` |
| Sales de Baño por jefe | 125, con tope de 500 | `fresno.json` |
| Entrenar | +25 % al movimiento | `fresno.json` |
| Costos de Raíces y umbrales de Ofrenda | 50 000·n^1.5 | `fresno.json` |
| Polvo Astral por imposible | 40 y 8 | `gacha.json` |
| Ataque de la zona 1 | ×0.9 | `fresno.json` |

Los números de `AutoClimber` son del piloto, no del juego.

## Injertos pendientes

Están en datos con `"pending": true`. No se ofrecen ni cuentan para el Fruto.

| Rama | Injertos |
| --- | --- |
| Rama Áurea | Espejismo de Arena, Oro Viejo, Lengua de Fuego, Sudor de León, Ámbar Solar, Hoguera Lenta, Ojo del Mediodía, Rey sin Sombra, Sal Áurea Pura, Ceniza de Ofrenda |
| Rama Argenta | Cuarto Menguante, Polilla Blanca, Agua Dormida, Lago de Plata, Reflejo Infiel, Canción de Cuna, Ojo de Gato, Llanto de Sal, Velo de Novia, Noche sin Fin |
| Rama Férrea | Yunque Interior, Cicatriz, Grito de Guerra, Carnero Ciego, Mina Profunda, Martillo de Mina, Hierro Viejo, Óxido Rojo, Guardia de Mina, Fuerza Bruta |
| Rama de Azogue | Gota de Plata, Lengua Bífida, Mercado Negro, Caduceo, Paso Doble, Recado, Alquimia Rápida, Espejo Líquido, Chisme, Pie Alado |
| Rama Cúprica | Rosa de Sal, Miel Amarga, Enredadera, Beso de Cobre, Fruta Prohibida, Perfume, Lazo Verde, Espejo de Venus, Hiedra, Pétalo |
| Rama Estánnea | Cetro, Nube Negra, Decreto, Relámpago Quieto, Toga de Estaño, Águila, Tormenta Seca, Rayo Partido, Banquete, Tronar |
| Rama Plúmbea | Anillo de Polvo, Tiempo Muerto, Cronos, Plomo Fundido, Ceniza Fría, Último Grano, Lápida, Herencia, Péndulo Quieto, Hoz Vieja |

También quedan pendientes las recetas de 6 legendarias: solo el Reloj del Eclipse y el Péndulo tienen receta.

## Huecos del GDD

- **Escala de almitas (D-02, H-01).** Es la decisión que más urge. Llegar al nivel 70 cuesta 1.64 millones de almitas, y la meta de siete días pide unas 234 000 diarias. Una run da de 55 000 a 110 000, y el tope semanal es de 300 000. Se respetó el `100·n^1.5` del GDD. La hoja de ruta propone `40·n^1.5`, que es un solo número en `progression.json`.
- **Sal Negra.** Existe como recurso y como asignación por combate. No está definido si el Vislumbre gasta la del inventario.
- **Grados B y A.** "Nuevo efecto de Hechizo" y "mejora el Autohechizo" dependen de cada kit (H-12). Se aproximaron con +10 % al Hechizo y −20 % al costo de los Hilos.
- **Kits.** Solo 6 de las 24 brujas pelean. Las otras 18 se obtienen, pero no entran a runs ni a Juicios (H-27).
- **Condiciones de Juicio.** La plantilla es común, con pocos casos por bruja (H-08). El Juicio V pide aliadas 3★, y hoy solo hay dos 3★ con kit.
- **Nombres.** Las 4★ y 3★ que el GDD no nombra son propuestas (Grajilla, Madroña, Risca, Nacarina, Centella, Véspera, Ixbalam, Estrige, Migaja, Vellón, Púa, Charca).
- **IDs.** Los datos siguen usando `zarigueya` y `topo`. El renombre a `muertita` y `topacia` (D-03) es de la Fase 1.5, que no se ha corrido.
- **Calculista.** Sus estadísticas son de la Fase 3. Los eventos del Fresno usaban una base provisional de 10 más 2 por bruja afín.

## Qué no entra en esta fase

La hoja de ruta parte esta fase en 2A y 2B, y les suma cosas que el prompt 23.2 del GDD no pide. Estas no se hicieron en la Fase 2:

| Pieza | Dónde quedó |
| --- | --- |
| Reloj `IClock`, migraciones de guardado y `IGachaService` | Fase 3 |
| Grabar cada run con `Core/Replay` | Fase 3A; hoy el piloto da una huella por run |
| Sales de Baño ganadas y compradas por separado (C-25) | Fase 3 |
| Los frames y prefijos de `docs/propuestas/CRAFTING.md` y el catálogo de `FRESNO.md` | Sin aprobar (D-07). Los datos actuales vienen del GDD y de valores propios |

## Qué falta verificar a mano

- Las pruebas no juzgan la interfaz: hay que abrir el hub y jugar una run con el HUD.
- La vista de fases usa siluetas de prueba, no los retratos.
