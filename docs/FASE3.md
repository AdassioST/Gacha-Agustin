# Fase 3: El Mundo

Esta fase pide un vertical slice listo para un lanzamiento suave cerrado (GDD 23.3). **Todavía no está listo para eso.** La lógica y la interfaz funcionan en un jugador de Windows, con servicios locales. Los servicios reales, las compilaciones móviles y el arte y audio finales dependen de cuentas, licencias y producción que una sesión de programación no puede cerrar. Las tablas de abajo separan lo real de lo que falta.

## Cómo correrlo

```bash
dotnet test Tests/Core.Tests
dotnet run --project Tests/Core.Sim -- historia 20
```

En Unity, abre el proyecto y usa **Sal y Almas > Jugar combate**. El juego empieza con el creador de la Calculista. El botón **Perfil de prueba** salta directo al home con seis brujas y recursos de sobra.

Para revisar las pantallas sin abrir el Editor:

1. Usa **Sal y Almas > Compilar jugador de Windows**.
2. Corre `Builds/Windows/SalYAlmas.exe -recorrido <carpeta>`. Guarda una captura de cada pantalla y cierra el juego. Borra el perfil guardado, así que úsalo solo para revisar.

## El home

Todo sale del home y vuelve a él, con la estructura de un gacha clásico. Las capturas que mandó el usuario son verticales, pero el GDD pide horizontal, así que la estructura se adaptó a horizontal.

| Zona | Qué hay |
| --- | --- |
| Barra superior | Calculista, Altura, Paralaje (no hay energía), emblema y cuatro monedas |
| Izquierda | Las cuatro brujas del equipo con su arte, planeta, rareza, nivel y grado |
| Derecha | Carrusel de destinos: Historia, Fresno, Manantial, Juicios y Libro Mayor |
| Accesos | Ajustes, Probabilidades y Espejo |
| Barra inferior | Inicio, Aquelarre, Yunque, Caldero, Termas y Tienda, con avisos |

El Aquelarre es una grilla con marcos por planeta y rareza. La piel de oro y ciruela se genera por código y también se aplica al combate.

## Qué se construyó

| Alcance del GDD | Estado | Detalle |
| --- | --- | --- |
| Motor de historia | Hecho | Decisiones con umbral fijo, bono por bruja, marcas, Títulos, Almas digeridas, Senderos, Relectura. Todo en `Data/story.json` |
| La Odisea: cuatro regiones cardinales | Hecho, en borrador | Aries, Cáncer, Libra y Capricornio, cinco misiones cada una, más el prólogo. Empiezas en la cardinal de tu elemento y la Tortuga sigue el anillo en sentido Directo o Retrógrado ([ODISEA](propuestas/ODISEA.md)). El guion de Cáncer y Libra es nuevo y está sin aprobar (H-14) |
| Zonas, secciones y botín | Hecho | Cuatro zonas por región: orilla, subsuelo y corazón bajan; la EX sube. Cada combate de historia es una cadena de secciones que arrastra vida y Salinidad, con almitas, material regional, sal y tesoros |
| Reloj de eclipses y Marea de Sal | Hecho | Cada dos regiones sube la Marea. El nivel y la capa de horror de una región dependen de cuántas llevabas al llegar, no de la región ni del equipo. Desde la Grieta entran Cristalizados; en el Abismo, Sin Carta y Huéspedes |
| Expediciones | Hecho | Las zonas terminadas se repiten con la Marea que elijas. La misión en curso queda en pausa. Botín completo en 20 secciones al día y ×0.25 después |
| Kits de las 24 brujas | Hecho | Las 18 que faltaban salen de `Tools/Content/gen_kits.py`. Salitre explota su intoxicación y Axa revive una vez (GDD 17). `kits` en el simulador vigila la regla del 5 % |
| Enemigos con mecánica | Hecho | 25 enemigos regionales con rasgos: protector, reflejo, furia contra Hechizos, inmunes a Aspectos, ataque creciente, curanderos y efectos al caer. Cada uno con su pista en la placa |
| Encargos, logros y Altura | Hecho | 4 diarios rotativos, semanales, 27 logros y Altura por Crecimiento. Todos los sistemas reportan eventos. Nunca piden tirar ni pagar |
| Arte de ambientes y enemigos | En curso, por Codex | Fondos de las cuatro regiones, el vado y el mapa del anillo; enemigos en recorte. Encargo en [Ambientes para Codex](Ambientes%20para%20Codex.md) |
| Ventanas del Libro Mayor y falla por Paralaje | Hecho | Folio por bruja y glifos con Paralaje alto. Es animación de interfaz, no shader |
| Termas navegables | Parcial | Mapa 2D de la Tortuga con reposo, tocar, conversar, Confianza y baño. No son 3D |
| Tirada completa de la 16.1 | Parcial | Sales, vapor por rareza, subida sorpresa, constelación, emergencia, sacudida por especie, tarjeta, saltar y modo de diez. Es secuencia 2D por código, no Timeline |
| Vestidor | Parcial | Equipa atuendos, color de sal, manantiales y marcos. El atuendo solo tiñe el recorte: no hay modelos ni blendshapes |
| Perlas, Grimorio y Diezmo | Hecho, sin dinero real | `StoreService` con plataforma y servidor falsos, sin Unity IAP |
| Servidor autoritativo | Solo la interfaz | `IBackend` con `FakeBackend` en memoria, que se pierde al cerrar. Valida recibos, registra compras y tiradas. El gacha sigue decidiéndose en el cliente |
| Configuración remota | Solo la interfaz | `IRemoteConfig` local: bandera de país y banners apagados |
| Audio por capas con FMOD | Parcial | Música temporal de Suno por pantalla y efectos temporales en combate y en la tirada (`docs/audio/AUDIO_PROTOTIPO.md`). No hay FMOD, capas ni voces |
| Gamas baja y alta | Parcial | El ajuste cambia fps y nivel de calidad. No hay LOD ni pelaje por capas |
| Localización | Parcial | Tabla de la interfaz principal en español, inglés y japonés, sin revisar por una persona. La mayoría de los textos de pantalla y todo el guion siguen solo en español. No usa TextMeshPro ni fuentes CJK propias |
| Accesibilidad | Parcial | Modo recatado, texto grande, colores seguros, saltar cinemáticas y reducir destellos. Reducir destellos solo cubre la tirada y las ventanas, todavía no el combate (S-18) |
| Cumplimiento | Hecho en el núcleo | Página de probabilidades, historial de un año, límite de gasto, Sales ganadas y compradas, bandera de Bélgica |
| Analítica | Parcial | Primera Cadena, primera 5★, runs, tiradas, compras y creación de la Calculista, en memoria. No sale a ningún servicio |
| Guardado | Hecho | Versión 2 con migración desde la 1, y se puede pausar a media misión |

También hay un combate con recortes de las brujas y bustos en sus placas. Los enemigos siguen siendo cápsulas de prueba.

## Criterios de aceptación

| Criterio | Estado | Evidencia |
| --- | --- | --- |
| Primera Cadena antes del minuto 3 | No medido | El prólogo la permite desde el primer turno con la Resonancia llena (prueba `ElPrologoGarantizaLaPrimeraCadena`). Falta medir con personas. La guía es un cartel, no el tutorial de la propuesta |
| Primera 5★ en menos de 2 horas sin pagar | Cumplido por diseño | El banner de inicio la garantiza justo después del prólogo. Prueba con 20 semillas |
| 60 fps en gama alta y 30 en baja | No verificado | No se ha corrido en un teléfono |
| Todas las pruebas de las tres fases pasan | Cumplido | Ver abajo |
| Una compra de prueba otorga lo correcto y queda en el servidor | Cumplido contra el servidor falso | `UnaCompraDePruebaOtorgaLoCorrectoYQuedaEnElServidor`. No hay servidor real |

## Pruebas y balance

- La suite de la Fase 3 está en `Tests/EditMode/Phase3/WorldTests.cs`.
- La Odisea tiene su suite en `OdysseyTests.cs` y los encargos en `QuestTests.cs`.
- `dotnet run --project Tests/Core.Sim -- historia 3` juega una cuenta nueva por cada signo cardinal y sentido, gastando sus almitas en niveles. Las ocho rutas terminan las cuatro regiones sin Expediciones extra, con el equipo de 1 a 4 niveles debajo de cada región.
- Los enemigos de historia ya no escalan con el equipo: subir de nivel sí sirve (antes la historia era imposible de mejorar y el Fresno, imposible de alcanzar).
- `dotnet run --project Tests/Core.Sim -- kits 10` mide el aporte de daño de las 24 brujas contra un maniquí y vigila el GDD 17.1.

## Cómo compilar para Android e iOS

**No se ha probado.** El Editor de esta máquina tiene el módulo de Android, pero no el de iOS.

1. En Unity Hub, agrega a la versión del proyecto los módulos de Android y de iOS.
2. Antes, fija la versión de Unity (D-14): el proyecto usa 6000.6.7f1, que no es LTS.
3. En **File > Build Profiles**, cambia de plataforma y agrega `Scenes/Combate.unity`.
4. Para iOS hace falta una Mac con Xcode y una cuenta de Apple Developer (D-10).

## Qué quedó con placeholders

| Pieza | Hoy | Falta |
| --- | --- | --- |
| Marcos, iconos y fondo | Generados por código | Arte de interfaz |
| 12 brujas sin arte | Silueta de color | Sus retratos |
| Enemigos | Recortes de Codex donde ya existen; cápsula si falta el archivo | Los de las otras ocho regiones |
| Fases S y SS | Busto de fase 3 en 6 Regentes | El resto de los cuerpos y bustos |
| Títulos, Casas, Ferrán, Tecuani, Valdehierro | Propuestas mías | Biblia narrativa (H-14) |
| Música | Pistas temporales de Suno | Capas y FMOD (D-09, D-11, D-12) |

Los datos siguen usando `zarigueya` y `topo`, porque la Fase 1.5 no se ha corrido. `PortraitBank.ArtId` los traduce a `muertita` y `topacia` para el arte, y el diálogo ya usa los nombres nuevos.

## Valores // TUNING

| Valor | Actual | Archivo |
| --- | --- | --- |
| Base de la Calculista y bono por bruja | 4 y +2 | `story.json` |
| Experiencia de Sendero | 100 por nivel, 25 por victoria, +5 % por nivel | `story.json` |
| Fuerza de los combates de historia | 0.40 a 0.60 | `story.json` |
| Paralaje por escena | 2 a 15 | `story.json` |
| Recompensas por misión | 6 000 a 12 000 almitas; 80 a 160 Sales | `story.json` |
| Precios de Perlas y cosméticos | 0.99 a 29.99 USD; 200 a 980 Perlas | `services.json` |
| Grimorio | 1 000 de experiencia por nivel; 5 niveles de ejemplo | `services.json` |
| Confianza | 20 por charla; 400 por nivel | `services.json` |
| Tiempos de la tirada y subida sorpresa | 1 a 1.6 s por paso; 35 % | `PullSequence.cs` |
| Altura del recorte en combate | 2.7 | `SalYAlmasGame.cs` |
| Odisea: niveles, secciones, botín y Expediciones | Ver ODISEA 12 | `odyssey.json` (de `gen_odyssey.py`) |
| Encargos, logros y Altura | 15 Sales y 5 000 almitas por diario; curva de la META 4.2 | `quests.json` |
| Kits de las 18 brujas | Plantilla por papel; ×0.9 en 4★ y ×0.8 en 3★ | `witches_ext.json` (de `gen_kits.py`) |
| Tope de la explosión de Salitre | 6 veces su ATQ | `Battle.Effects.cs` |

## Decisiones tomadas

| Tema | Decisión |
| --- | --- |
| Orientación | Horizontal, como pide el GDD, aunque las referencias del home son verticales |
| Regiones | Las cuatro cardinales, una por elemento (la recomendación de D-13). El orden lo da el anillo, no una lista |
| Progresión | El lugar es libre (GDD 6) y el nivel es lineal (pedido del usuario): lo da la Marea al llegar ([ODISEA](propuestas/ODISEA.md)) |
| Escala de almitas | Sigue en `100·n^1.5` (GDD 11.3) hasta que se decida D-02. Las secciones dan `100·nivel^1.5` para que la historia alcance |
| Senderos | Alquimista da la ranura extra, sin las dos pociones por turno. Directora de Coro da daño de Spark, sin mover eslabones. Ambos faltantes se indican en `story.json` |
| Reembolso con Perlas gastadas | El saldo queda como deuda que descuenta la siguiente compra |
| Sales gastadas | Primero las ganadas; lo comprado solo baja cuando ya no alcanza |
| Relectura | Puede agregar marcas nuevas, pero no borra las anteriores ni repite recursos |

## Riesgos para el lanzamiento suave

1. **El gacha se decide en el cliente.** Sin servidor real se puede falsificar. Es el mayor riesgo y depende de D-04 y de las cuentas de UGS.
2. **Sin validar runs ni grabarlas.** El Fresno entrega recompensas sin revisión (S-04), y no se ha probado que los combates den lo mismo en ARM64 (S-25).
3. **Escala de almitas sin decidir (D-02).** La meta de nivel 70 en siete días no se puede cumplir con lo que da el juego hoy.
4. **Kits generados por plantilla.** Las 24 pelean, pero las 18 nuevas comparten plantilla por papel; falta darles mecánicas propias y probarlas con personas.
5. **Guion sin aprobar**, y sin revisión de tono ni legal.
6. **Falta de producto (Fase 4):** cuenta real, borrar cuenta en servidor, privacidad, notificaciones, tutorial guionizado y revisión de tiendas.
7. **Rendimiento desconocido.** Hay recortes de 977 × 1432 px por bruja sin Addressables, y nunca se ha medido en un teléfono.
8. **La hoja de ruta parte esta fase en 3A, 3B y 3C,** con más alcance que el prompt 23.2 y 23.3. Falta decidir si se sigue ese plan.
