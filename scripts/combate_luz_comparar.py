"""Hoja de comparación del combate antes y después de la luz y los gestos.

Solo escala y pega capturas reales del juego (PNG de UiCapture); no pinta nada encima salvo los rótulos.
Uso: python scripts/combate_luz_comparar.py <antes.png> <después.png> <salida.png> [rótulo antes] [rótulo después]
Requiere Pillow.
"""
import sys
from PIL import Image, ImageDraw, ImageFont


def font(size):
    for name in ("C:/Windows/Fonts/georgia.ttf", "C:/Windows/Fonts/arial.ttf"):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


def main():
    if len(sys.argv) < 4:
        print(__doc__)
        sys.exit(1)
    before, after, out = sys.argv[1:4]
    labels = (sys.argv[4] if len(sys.argv) > 4 else "Antes", sys.argv[5] if len(sys.argv) > 5 else "Después")
    imgs = [Image.open(p).convert("RGB") for p in (before, after)]
    w = 1280
    imgs = [im.resize((w, round(im.height * w / im.width)), Image.LANCZOS) for im in imgs]
    band = 56
    sheet = Image.new("RGB", (w, sum(im.height + band for im in imgs)), (11, 11, 20))
    draw = ImageDraw.Draw(sheet)
    y = 0
    for im, label in zip(imgs, labels):
        draw.text((18, y + 12), label, fill=(230, 199, 110), font=font(30))
        sheet.paste(im, (0, y + band))
        y += im.height + band
    sheet.save(out)
    print(out)


if __name__ == "__main__":
    main()
