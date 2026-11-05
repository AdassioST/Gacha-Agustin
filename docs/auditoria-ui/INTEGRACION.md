# Integración de ramas en main

Fecha: 5 de noviembre de 2026. Repositorio `AdassioST/Gacha-Agustin`.

## Ramas funcionales

`d736b07` integra `fase/3` y Odisea. La resolución conservó los GUID y los ajustes PCM/mono de audio que ya tenía `main`; combinó los cambios de código de ambas líneas. `bb5fe13` integra `ui/interfaz`, con el hub uGUI y el escenario lateral. `e4ee1d2` incorpora el arte de Odisea que faltaba y prepara el recorrido de capturas con perfil en memoria.

También quedan incluidas por ascendencia `docs/gdd-y-referencias`, `fase/2-cierre` y las ramas remotas `claude/awesome-johnson-o7jyhk`, `claude/charming-mayer-j22ztu`, `claude/loving-edison-wtb25h` y `claude/youthful-dijkstra-hos1on`.

## Respaldo de GitHub Desktop

`respaldo/stash-github-desktop-0254` apunta a `640703f`, un snapshot de stash de una versión anterior. Se incorpora su historia con estrategia `ours` y recuperación explícita de sus archivos únicos. Esta excepción corresponde al respaldo; las ramas funcionales anteriores se integraron con sus cambios.

La comparación del snapshot con el árbol anterior a esta auditoría (`e4ee1d2`) dio 462 rutas modificadas desde su base: 205 ya idénticas, 174 diferentes en la línea actual y 83 sin esa ruta en el árbol actual. Entre las rutas ausentes:

- Siete fuentes y cuatro licencias ya existen con **el mismo blob** en `Resources/Ui/Fonts` y `Art/Ui/Licencias`. No se duplican los antiguos directorios y metadatos, para evitar GUID repetidos.
- Tres texturas antiguas se conservan como archivo en `art/archivo-ui/`, fuera del importador de Unity. La interfaz usa el kit actual.
- Se recupera `scripts/bustos_lote.py` y se incorporan los 50 PNG de bustos del snapshot, que coinciden byte por byte con las copias que ya existían en el disco. El manifiesto histórico se incorpora al índice de Git sin sobrescribir el archivo de trabajo.
- El código actual de combate, guardado, progreso y Odisea se mantiene. Restaurar en bloque el snapshot sustituiría sistemas posteriores y el rediseño auditado.

El manifiesto local tenía 39 imágenes registradas, frente a las 10 del respaldo. Se conserva **sin modificar** esa versión local y los archivos adicionales del lote. Por ello pueden seguir apareciendo cambios y archivos sin seguimiento en `art/bustos/`: son trabajo local previo, separado del contenido del snapshot integrado. No se borraron ramas, worktrees ni entradas del stash.

`integracion-respaldo.json` registra la comparación, equivalencias de fuentes y hashes de conservación del manifiesto local. Para comprobar las ramas:

```powershell
git merge-base --is-ancestor fase/3 main
git merge-base --is-ancestor ui/interfaz main
git merge-base --is-ancestor 640703f main  # el respaldo; su rama se retiró el 5 de noviembre
```

Los tres comandos deben terminar con código 0. La auditoría y sus pruebas están en [README.md](README.md).
