"""Pinta por código el kit visual de la interfaz de Sal y Almas (Libro Mayor + Termas).

Uso:  python scripts/ui_kit.py [grupo ...]     grupos: bg chrome icons fx logo cards witches all
Salida: Assets/SalYAlmas/Art/Ui/Resources/Ui/<grupo>/*.png
Todo es reproducible: cada imagen usa una semilla fija. No hay imágenes de terceros.
Paleta: docs/REFERENCIAS.md. Antirreferencias: nada de bisel dorado genérico, la sal es cúbica y no nieve.
"""
import math
import os
import shutil
import sys

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont
from scipy import ndimage as ndi

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
OUT = os.path.join(ROOT, "Assets", "SalYAlmas", "Art", "Ui", "Resources", "Ui")
FONTS = os.path.join(ROOT, "Assets", "SalYAlmas", "Art", "Ui", "Fonts")
RECORTES = os.path.join(ROOT, "art", "recortes")


def hx(s):
    s = s.lstrip("#")
    return np.array([int(s[i:i + 2], 16) / 255.0 for i in (0, 2, 4)])


PARCH, PARCH_S, INK = hx("EDE3CC"), hx("CFBE9A"), hx("2A2018")
ABYSS, PANEL = hx("0B0B14"), hx("1A1A28")
GOLD, GOLD_B, CRIM = hx("B8923A"), hx("E6C76E"), hx("8E2430")
SALT, SPARK, FIVE = hx("F6F4EE"), hx("FFF1B8"), hx("FFD84D")
VIOLET = hx("A25CF5")


def save(img, group, name):
    d = os.path.join(OUT, group)
    os.makedirs(d, exist_ok=True)
    if isinstance(img, np.ndarray):
        img = to_img(img)
    img.save(os.path.join(d, name + ".png"), optimize=True)
    print(f"  {group}/{name} {img.size}")


def to_img(a):
    a = np.clip(a, 0, 1)
    return Image.fromarray((a * 255 + 0.5).astype(np.uint8), "RGBA" if a.shape[2] == 4 else "RGB")


def noise(h, w, scale, seed, octaves=5, persistence=0.5):
    """fBm suave normalizado a 0..1."""
    rng = np.random.default_rng(seed)
    out = np.zeros((h, w))
    amp, total = 1.0, 0.0
    s = float(scale)
    for _ in range(octaves):
        gh, gw = max(2, int(h / s) + 2), max(2, int(w / s) + 2)
        g = rng.random((gh, gw))
        z = ndi.zoom(g, (h / (gh - 1) + 0.01, w / (gw - 1) + 0.01), order=3)[:h, :w]
        out += z * amp
        total += amp
        amp *= persistence
        s = max(2.0, s / 2)
    out /= total
    lo, hi = out.min(), out.max()
    return (out - lo) / (hi - lo + 1e-9)


def fit(a, h, w):
    """Reescala un mapa 2D a exactamente h x w."""
    return np.asarray(Image.fromarray(a.astype(np.float32), "F").resize((w, h), Image.BICUBIC)).astype(float)


def smooth(a, b, x):
    t = np.clip((x - a) / (b - a + 1e-9), 0, 1)
    return t * t * (3 - 2 * t)


def lerp(a, b, t):
    t = np.asarray(t)
    if t.ndim == 2:
        t = t[..., None]
    return a * (1 - t) + b * t


def grad(h, w, stops):
    """Degradado vertical. stops = [(0..1, color), ...]"""
    ys = np.linspace(0, 1, h)
    xs = [s[0] for s in stops]
    col = np.stack([np.interp(ys, xs, [s[1][i] for s in stops]) for i in range(3)], -1)
    return np.repeat(col[:, None, :], w, 1)


def font(name, size):
    return ImageFont.truetype(os.path.join(FONTS, name), size)


class SS:
    """Lienzo con superesmuestreo: se dibuja a 4x y se reduce."""

    def __init__(self, w, h, k=4):
        self.w, self.h, self.k = w, h, k
        self.im = Image.new("RGBA", (w * k, h * k), (0, 0, 0, 0))
        self.d = ImageDraw.Draw(self.im, "RGBA")

    def p(self, pts):
        return [(x * self.k, y * self.k) for x, y in pts]

    def line(self, pts, c, w=1.0):
        self.d.line(self.p(pts), fill=c, width=max(1, int(w * self.k)), joint="curve")

    def poly(self, pts, fill=None, outline=None, w=1.0):
        pp = self.p(pts)
        if fill is not None:
            self.d.polygon(pp, fill=fill)
        if outline is not None:
            self.d.line(pp + [pp[0]], fill=outline, width=max(1, int(w * self.k)), joint="curve")

    def circle(self, x, y, r, fill=None, outline=None, w=1.0):
        k = self.k
        self.d.ellipse([(x - r) * k, (y - r) * k, (x + r) * k, (y + r) * k], fill=fill, outline=outline, width=max(1, int(w * k)))

    def arc(self, x, y, r, a0, a1, c, w=1.0):
        k = self.k
        self.d.arc([(x - r) * k, (y - r) * k, (x + r) * k, (y + r) * k], a0, a1, fill=c, width=max(1, int(w * k)))

    def text(self, x, y, s, f, c, anchor="mm"):
        fk = ImageFont.truetype(f.path, f.size * self.k)
        self.d.text((x * self.k, y * self.k), s, font=fk, fill=c, anchor=anchor)

    def done(self):
        return self.im.resize((self.w, self.h), Image.LANCZOS)


def rgba(c, a=1.0):
    return tuple(int(v * 255) for v in c) + (int(a * 255),)


# ---------------------------------------------------------------------------------------------- Fondos


def sky(W, H):
    g = grad(H, W, [(0, hx("1d1830")), (0.26, hx("3b2748")), (0.48, hx("7e4f66")), (0.62, hx("cc8579")), (0.70, hx("f6bd94")), (1, hx("f6bd94"))])
    ys = np.linspace(0, 1, H)
    # nubes estiradas, suaves, con vientre oscuro y filo encendido por el sol bajo
    n = fit(noise(H // 2, W // 8, 70, 11, 6), H, W)
    n2 = fit(noise(H // 2, W // 3, 60, 12, 5), H, W)
    band = (smooth(0.16, 0.36, ys) * (1 - smooth(0.56, 0.70, ys)))[:, None]
    cloud = ndi.gaussian_filter(smooth(0.50, 0.74, n * 0.7 + n2 * 0.3), 3) * band
    soft = ndi.gaussian_filter(cloud, 3)
    lit = np.clip(soft - np.roll(soft, -12, axis=0), 0, 1) * 5
    g = lerp(g, hx("4a2c48"), cloud * 0.62)
    g = lerp(g, hx("f7ae8e"), np.clip(lit, 0, 1) * 0.55)
    rng = np.random.default_rng(13)
    stars = np.zeros((H, W))
    ys_ = (rng.random(1500) ** 2.2 * H * 0.42).astype(int)
    xs = rng.integers(0, W, 1500)
    stars[ys_, xs] = rng.random(1500) ** 4 * 2.0
    stars = ndi.gaussian_filter(stars, 0.8) * 3 + ndi.gaussian_filter(stars, 2.4) * 2.2
    g = g + stars[..., None] * hx("ffe9c4") * (1 - cloud[..., None])
    yy, xx = np.mgrid[0:H, 0:W]
    glow = np.exp(-(((xx - W * 0.66) / (W * 0.30)) ** 2 + ((yy - H * 0.71) / (H * 0.13)) ** 2))
    g = g + glow[..., None] * hx("ffb078") * 0.38
    return np.clip(g, 0, 1)


def constellations(W, H):
    c = SS(W, H, 2)
    rng = np.random.default_rng(21)
    for _ in range(9):
        x, y = rng.random() * W, rng.random() * H * 0.34
        pts = [(x, y)]
        for _ in range(rng.integers(4, 8)):
            a = rng.random() * math.tau
            r = 40 + rng.random() * 90
            pts.append((pts[-1][0] + math.cos(a) * r, pts[-1][1] + math.sin(a) * r * 0.7))
        c.line(pts, rgba(GOLD_B, 0.34), 1.2)
        for p in pts:
            c.circle(p[0], p[1], 2 + rng.random() * 2.4, fill=rgba(SPARK, 0.95))
    return c.done()


HOR = 0.71


def cellular(u, v, seed):
    """Voronoi en coordenadas de mundo (aristas rectas: costra de sal cúbica). Devuelve (F2-F1, id de celda)."""
    iu, iv = np.floor(u).astype(np.int64), np.floor(v).astype(np.int64)
    f1 = np.full(u.shape, 9.0)
    f2 = np.full(u.shape, 9.0)
    cid = np.zeros(u.shape)

    def h(a, b, k):
        x = (a * 374761393 + b * 668265263 + k * 1274126177 + seed * 15485863) & 0xFFFFFFFF
        x = ((x ^ (x >> 13)) * 1274126177) & 0xFFFFFFFF
        return ((x ^ (x >> 16)) & 0xFFFF) / 65535.0

    for dv in (-1, 0, 1):
        for du in (-1, 0, 1):
            cu, cv = iu + du, iv + dv
            px, py = cu + h(cu, cv, 1), cv + h(cu, cv, 2)
            d = np.hypot(px - u, py - v)
            better = d < f1
            f2 = np.where(better, f1, np.minimum(f2, d))
            cid = np.where(better, h(cu, cv, 3), cid)
            f1 = np.where(better, d, f1)
    return f2 - f1, cid


def persp(W, H, hor_y, c=26.0):
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    dy = np.maximum(yy - hor_y, 0.0)
    z = 1000.0 / (dy + c)
    u = (xx - W / 2) * z / 1000.0
    return u, z, yy, xx


def far_ruins(W, H):
    """Sierras en bruma y el viaducto en ruinas de los retratos, con arcos de verdad."""
    hor = int(H * HOR)
    prof = noise(1, W, 340, 31, 5)[0]
    top = hor - 26 - prof * 120
    yy = np.arange(H)[:, None]
    a = np.zeros((H, W, 4))
    a[..., :3] = lerp(hx("7c5266"), hx("d58f80"), smooth(hor - 150, hor, np.repeat(yy, W, 1).astype(float)))
    a[..., 3] = smooth(-1.5, 1.5, yy - top[None, :]) * (yy[:, 0] < hor + 4)[:, None]
    im = to_img(a)
    k = 2
    wall = Image.new("L", (W * k, H * k), 0)
    d = ImageDraw.Draw(wall)
    rng = np.random.default_rng(32)
    for x0, x1, topy, ah, aw in [(0.02, 0.27, hor - 104, 74, 62), (0.72, 1.0, hor - 124, 90, 74)]:
        x = W * x0
        while x < W * x1:
            brk = rng.random()
            h_ = topy + (40 + rng.random() * 50 if brk < 0.22 else 0)
            d.rectangle([x * k, h_ * k, (x + 20) * k, (hor + 4) * k], fill=255)
            if brk >= 0.22:
                span = aw
                d.rectangle([x * k, topy * k, (x + 20 + span) * k, (topy + ah * 0.55) * k], fill=255)
                d.ellipse([(x + 20) * k, (topy + ah * 0.34) * k, (x + 20 + span) * k, (topy + ah * 0.34 + span) * k], fill=0)
            x += 20 + aw
    wall = wall.resize((W, H), Image.LANCZOS).filter(ImageFilter.GaussianBlur(0.8))
    v = np.zeros((H, W, 4))
    v[..., :3] = hx("5b3f56")
    v[..., 3] = np.asarray(wall) / 255.0 * 0.92
    im.alpha_composite(to_img(v))
    return im


def salt_water(W, H):
    """Salina al atardecer en perspectiva: costra blanca-rosa y agua carmesí que refleja el cielo."""
    hor = H * HOR
    u, z, yy, xx = persp(W, H, hor)
    t = np.clip((yy - hor) / (H - hor), 0, 1)
    sk = grad(int(hor) + 1, 1, [(0, hx("1d1830")), (0.26, hx("3b2748")), (0.48, hx("7e4f66")), (0.62, hx("cc8579")), (0.70, hx("f6bd94")), (1, hx("f6bd94"))])[:, 0]
    # el reflejo toma la fila del cielo a la misma distancia del horizonte
    refl = sk[np.clip((hor - (yy - hor) * 0.9).astype(int), 0, int(hor))]
    water = lerp(refl, hx("6e1a26"), 0.34 + 0.34 * t)
    ripple = noise(H, W // 2, 8, 44, 2)
    water = water + (fit(ripple, H, W) - 0.5)[..., None] * 0.07 * (0.3 + t[..., None])
    gap, cid = cellular(u * 2.6, z * 0.62, 41)
    crack = smooth(0.0, 0.11, gap)
    big = fit(noise(H // 2, W // 2, 110, 42, 5), H, W)
    world = noise(H // 2, W // 2, 60, 45, 5)
    pools = smooth(0.46, 0.54, big * 0.55 + fit(world, H, W) * 0.45)
    crust = lerp(hx("e9cfca"), hx("fbf2ea"), cid * 0.7 + 0.15)
    crust = crust * (0.62 + 0.38 * crack[..., None])
    crust = lerp(crust, hx("f5b89a"), (1 - smooth(0, 0.3, t)) * 0.62)
    rgb = lerp(crust, water, pools)
    rgb = lerp(rgb, hx("f6bd94"), (1 - smooth(0, 0.14, t)) * 0.7)
    a = smooth(hor - 2, hor + 2, yy)
    return np.dstack([np.clip(rgb, 0, 1), a])


def shell_terrace(W, H):
    """Terraza del caparazón de la Tortuga en perspectiva, con escudos hexagonales, y la poza de agua termal."""
    hor = H * HOR
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    edge = H * 0.815 + 34 * np.sin(xx / W * math.pi * 1.1 + 0.3) + (noise(1, W, 70, 51, 4)[0] - 0.5)[None, :] * 14
    z = 1000.0 / (yy - hor + 60)
    u = (xx - W * 0.42) * z / 1000.0
    v = z * 1.5
    s = 0.42
    q = (u * 0.57735 - v * 0.33333) / s
    r_ = (v * 0.66667) / s
    y_ = -q - r_
    rq, rr, ry = np.round(q), np.round(r_), np.round(y_)
    dq, dr, dy = np.abs(rq - q), np.abs(rr - r_), np.abs(ry - y_)
    fix_q = (dq > dr) & (dq > dy)
    fix_r = (~fix_q) & (dr > dy)
    rq = np.where(fix_q, -rr - ry, rq)
    rr = np.where(fix_r, -rq - ry, rr)
    fq, fr = q - rq, r_ - rr
    hexd = np.maximum.reduce([np.abs(fq), np.abs(fr), np.abs(fq + fr)])
    seam = smooth(0.40, 0.49, hexd)
    idv = (np.sin(rq * 12.9 + rr * 78.2) * 43758.5) % 1.0
    grain = noise(H, W, 26, 52, 5)[..., None]
    stone = lerp(hx("5a454a"), hx("7c5f55"), idv * 0.8 + 0.1)
    stone = stone * (0.74 + 0.5 * grain)
    dome = np.clip(1 - hexd * 2, 0, 1) ** 0.8
    stone = stone * (0.66 + 0.52 * dome[..., None])
    stone = stone + (np.clip(fq * 0.8 - fr, -1, 1) * 0.5 + 0.5)[..., None] * hx("ffb48a") * 0.09
    stone = stone * (1 - 0.68 * seam[..., None])
    crust = smooth(0.54, 0.70, noise(H, W, 34, 53, 5)) * seam
    gap, cid = cellular(xx / 11.0, yy / 9.0, 55)
    cubes = lerp(hx("c8b1ac"), hx("fff6ee"), cid)
    stone = lerp(stone, cubes, crust * smooth(0.0, 0.1, gap))
    px, py, prx, pry = W * 0.50, H * 1.02, W * 0.42, H * 0.135
    pd = np.sqrt(((xx - px) / prx) ** 2 + ((yy - py) / pry) ** 2)
    pool = 1 - smooth(0.975, 0.99, pd)
    rim = (1 - smooth(0.99, 1.075, pd)) * smooth(0.97, 0.99, pd)
    rimlit = np.clip((py - yy) / (pry * 1.1), 0, 1)
    wt = np.clip(pd, 0, 1)
    water = lerp(hx("ffcdb0"), hx("b45a5e"), smooth(0.2, 0.98, wt))
    ond = np.sin((pd * 54) + (noise(H, W, 70, 56, 3) * 8))
    water = water + (ond * 0.5 + 0.5)[..., None] * hx("fff0dc") * 0.10
    rimcol = lerp(hx("a37e6c"), hx("f1dcc6"), rimlit)
    rgb = lerp(stone, rimcol, rim)
    rgb = lerp(rgb, water, pool)
    rgb = rgb * (0.7 + 0.3 * smooth(0, 46, yy - edge))[..., None]
    return np.dstack([np.clip(rgb, 0, 1), smooth(-1.6, 1.6, yy - edge)])


def make_bg():
    W, H = 2560, 1440
    s = sky(W, H)
    save(s, "bg", "home_sky")
    save(constellations(W, H), "bg", "home_stars")
    save(far_ruins(W, H), "bg", "home_far")
    mid = salt_water(W, H)
    save(mid, "bg", "home_mid")
    save(shell_terrace(W, H), "bg", "home_near")
    # Fondo oscuro de pantallas de contenido: abisal con pergamino apenas sugerido y carta natal al fondo.
    g = grad(H // 2, W // 2, [(0, hx("15111f")), (0.6, hx("1a1422")), (1, hx("0b0b14"))])
    n = noise(H // 2, W // 2, 300, 61, 6)
    g = lerp(g, hx("3a2036"), smooth(0.5, 0.85, n) * 0.5)
    yy, xx = np.mgrid[0:H // 2, 0:W // 2]
    v = ((xx / (W / 2) - 0.5) ** 2 + (yy / (H / 2) - 0.5) ** 2)
    g = g * (1 - smooth(0.1, 0.55, v)[..., None] * 0.55)
    save(np.clip(g, 0, 1), "bg", "abyss")
    # Agua del Manantial, vista desde arriba: rosa profunda con ondas.
    W2, H2 = 1600, 900
    yy, xx = np.mgrid[0:H2, 0:W2].astype(float)
    r = np.hypot((xx - W2 / 2) / W2 * 1.6, (yy - H2 * 0.55) / H2)
    base = lerp(hx("c7705f"), hx("1a0c1c"), smooth(0.05, 0.75, r))
    n = noise(H2, W2, 90, 71, 5)
    caust = np.abs(n - 0.5) * 2
    caust = (1 - smooth(0.0, 0.1, caust)) * (1 - smooth(0.2, 0.9, r))
    base = base + caust[..., None] * hx("ffd9b8") * 0.28
    save(np.clip(base, 0, 1), "bg", "spring")


# ---------------------------------------------------------------------------------------------- Astrolabio


def astrolabe(size, rings=3, glyphs=True, seed=3):
    c = SS(size, size, 2)
    cx = cy = size / 2
    R = size / 2 - 6
    gold = rgba(GOLD_B, 0.95)
    thin = rgba(GOLD_B, 0.55)
    c.circle(cx, cy, R, outline=gold, w=3)
    c.circle(cx, cy, R - 10, outline=thin, w=1.4)
    for i in range(360):
        a = math.radians(i)
        l = 16 if i % 30 == 0 else 8 if i % 5 == 0 else 4
        r0, r1 = R - 10, R - 10 - l
        c.line([(cx + math.cos(a) * r0, cy + math.sin(a) * r0), (cx + math.cos(a) * r1, cy + math.sin(a) * r1)], gold if i % 30 == 0 else thin, 1.4 if i % 5 == 0 else 0.9)
    r2 = R * 0.80
    c.circle(cx, cy, r2, outline=gold, w=2)
    if glyphs:
        f = font("NotoSansSymbols-Regular.ttf", int(size * 0.052))
        for i, g in enumerate("♈♉♊♋♌♍♎♏♐♑♒♓"):
            a = math.radians(i * 30 + 15)
            rr = (R - 26 + r2) / 2
            c.text(cx + math.cos(a) * rr, cy + math.sin(a) * rr, g, f, gold)
        for i in range(12):
            a = math.radians(i * 30)
            c.line([(cx + math.cos(a) * r2, cy + math.sin(a) * r2), (cx + math.cos(a) * (R - 26), cy + math.sin(a) * (R - 26))], thin, 1.1)
    r3 = R * 0.56
    c.circle(cx, cy, r3, outline=thin, w=1.6)
    if rings >= 3:
        for i in range(12):
            a = math.radians(i * 30)
            c.line([(cx, cy), (cx + math.cos(a) * r2, cy + math.sin(a) * r2)], rgba(GOLD_B, 0.25), 1)
        rng = np.random.default_rng(seed)
        pts = [(cx + math.cos(a) * r3 * 0.92, cy + math.sin(a) * r3 * 0.92) for a in rng.random(7) * math.tau]
        for i in range(7):
            for j in range(i + 1, 7):
                if rng.random() < 0.3:
                    c.line([pts[i], pts[j]], rgba(GOLD_B, 0.35), 1.1)
    c.circle(cx, cy, 8, fill=gold)
    return c.done()


def ring_only(size, dots=72):
    c = SS(size, size, 2)
    cx = cy = size / 2
    R = size / 2 - 8
    c.circle(cx, cy, R, outline=rgba(GOLD_B, 0.9), w=2)
    for i in range(dots):
        a = i / dots * math.tau
        c.circle(cx + math.cos(a) * (R - 12), cy + math.sin(a) * (R - 12), 2.4 if i % 6 == 0 else 1.2, fill=rgba(GOLD_B, 0.9))
    return c.done()


# ---------------------------------------------------------------------------------------------- Marcos


def corner_flourish(c, x, y, sx, sy, s, col, col2):
    """Esquina del Libro Mayor: cuarto de astrolabio con rombo; nada de bisel."""
    c.arc(x, y, s, 0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270, (0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270) + 90, col, 1.6)
    c.arc(x, y, s * 0.68, 0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270, (0 if sx > 0 and sy > 0 else 90 if sx < 0 and sy > 0 else 180 if sx < 0 else 270) + 90, col2, 1.1)
    d = s * 0.24
    cx, cy = x + sx * s * 0.34, y + sy * s * 0.34
    c.poly([(cx - d, cy), (cx, cy - d), (cx + d, cy), (cx, cy + d)], fill=col)
    c.circle(x + sx * s * 0.86, y, 2.2, fill=col)
    c.circle(x, y + sy * s * 0.86, 2.2, fill=col)


def panel_abyss(size=240):
    a = np.zeros((size, size, 4))
    base = lerp(hx("191522"), hx("120f1a"), np.linspace(0, 1, size)[:, None].repeat(size, 1))
    n = noise(size, size, 40, 81, 4)[..., None]
    a[..., :3] = base * (0.92 + 0.16 * n)
    a[..., 3] = 0.96
    im = to_img(a)
    c = SS(size, size, 4)
    gold, thin = rgba(GOLD, 0.95), rgba(GOLD_B, 0.45)
    c.poly([(2, 2), (size - 2, 2), (size - 2, size - 2), (2, size - 2)], outline=gold, w=1.5)
    c.poly([(7, 7), (size - 7, 7), (size - 7, size - 7), (7, size - 7)], outline=thin, w=1)
    s = 34
    corner_flourish(c, 7, 7, 1, 1, s, gold, thin)
    corner_flourish(c, size - 7, 7, -1, 1, s, gold, thin)
    corner_flourish(c, 7, size - 7, 1, -1, s, gold, thin)
    corner_flourish(c, size - 7, size - 7, -1, -1, s, gold, thin)
    im.alpha_composite(c.done())
    return im


def deckle_mask(h, w, seed, m=10):
    """Borde de papel rasgado."""
    n = noise(h, w, 22, seed, 4)
    yy, xx = np.mgrid[0:h, 0:w]
    d = np.minimum.reduce([xx, yy, w - 1 - xx, h - 1 - yy]).astype(float)
    return smooth(0, 1.6, d - (n * m))


def panel_parchment(size=320):
    n1 = noise(size, size, 90, 91, 6)
    n2 = noise(size, size, 14, 92, 3)
    rgb = lerp(hx("ddcba6"), hx("f0e6d0"), smooth(0.2, 0.75, n1))
    rgb = rgb * (0.95 + 0.09 * n2[..., None])
    fib = ndi.gaussian_filter(np.random.default_rng(93).random((size, size)), (0.6, 4))
    fib = (fib - fib.mean()) / fib.std()
    rgb = rgb * (1 + 0.012 * fib[..., None])
    yy, xx = np.mgrid[0:size, 0:size]
    d = np.minimum.reduce([xx, yy, size - 1 - xx, size - 1 - yy]).astype(float)
    burn = 1 - smooth(4, 54, d + (n1 - 0.5) * 34)
    rgb = lerp(rgb, hx("7e5a35"), burn * 0.62)
    a = np.dstack([rgb, deckle_mask(size, size, 94)])
    return a


def tile_parchment(size=512):
    n1 = noise(size, size, 150, 95, 6)
    n2 = noise(size, size, 18, 96, 3)
    rgb = lerp(hx("e2d3b1"), hx("efe5cf"), smooth(0.2, 0.75, n1))
    return rgb * (0.95 + 0.09 * n2[..., None])


def button(w, h, fill_top, fill_bot, line, seed, wax=False):
    a = np.zeros((h, w, 3))
    a[:] = lerp(fill_top, fill_bot, np.linspace(0, 1, h)[:, None].repeat(w, 1))
    n = noise(h, w, 34, seed, 4)[..., None]
    a = a * (0.86 + 0.28 * n)
    if wax:
        spot = smooth(0.62, 0.8, noise(h, w, 26, seed + 1, 3))
        a = a + spot[..., None] * 0.06
    c = SS(w, h, 4)
    cut = 10
    pts = [(cut, 1), (w - cut, 1), (w - 1, cut), (w - 1, h - cut), (w - cut, h - 1), (cut, h - 1), (1, h - cut), (1, cut)]
    c.poly(pts, fill=(255, 255, 255, 255))
    alpha = np.asarray(c.done())[..., 3] / 255.0
    im = to_img(np.dstack([a, alpha]))
    c2 = SS(w, h, 4)
    c2.poly(pts, outline=rgba(line, 1), w=1.6)
    i = 5
    pts2 = [(cut + i * .6, 1 + i), (w - cut - i * .6, 1 + i), (w - 1 - i, cut + i * .6), (w - 1 - i, h - cut - i * .6), (w - cut - i * .6, h - 1 - i), (cut + i * .6, h - 1 - i), (1 + i, h - cut - i * .6), (1 + i, cut + i * .6)]
    c2.poly(pts2, outline=rgba(line, 0.42), w=1)
    im.alpha_composite(c2.done())
    return im


def card_frame(size, col, col2, double=True, flourish=True, seed=1):
    c = SS(size, size, 4)
    c.poly([(2, 2), (size - 2, 2), (size - 2, size - 2), (2, size - 2)], outline=rgba(col, 1), w=3)
    if double:
        c.poly([(8, 8), (size - 8, 8), (size - 8, size - 8), (8, size - 8)], outline=rgba(col2, 0.65), w=1.2)
    if flourish:
        s = 30
        for sx, sy, x, y in [(1, 1, 8, 8), (-1, 1, size - 8, 8), (1, -1, 8, size - 8), (-1, -1, size - 8, size - 8)]:
            corner_flourish(c, x, y, sx, sy, s, rgba(col, 1), rgba(col2, 0.7))
    return c.done()


def make_chrome():
    save(panel_abyss(), "chrome", "panel_abyss")
    save(panel_parchment(), "chrome", "panel_parchment")
    save(tile_parchment(), "chrome", "parchment_tile")
    save(button(240, 96, hx("a22c3a"), hx("6e1825"), GOLD_B, 101, True), "chrome", "btn_primary")
    save(button(240, 96, hx("262030"), hx("15111d"), GOLD, 102), "chrome", "btn_secondary")
    save(button(240, 96, hx("3a3440"), hx("24202a"), hx("6d6256"), 103), "chrome", "btn_disabled")
    save(button(240, 96, hx("f0e6d0"), hx("d5c29c"), hx("7e5a35"), 104), "chrome", "btn_parchment")
    save(card_frame(200, hx("D8D3C4"), hx("8A867B"), True, False), "chrome", "frame_r3")
    save(card_frame(200, hx("A25CF5"), hx("D6B6FF"), True, True), "chrome", "frame_r4")
    save(card_frame(200, hx("FFD84D"), hx("FFF1B8"), True, True), "chrome", "frame_r5")
    # Barras: arriba y abajo. Tinta casi negra que se funde con el fondo, filo de oro y dientes de pergamino rasgado.
    w, h = 512, 120
    yy = np.linspace(0, 1, h)[:, None].repeat(w, 1)
    a = np.zeros((h, w, 4))
    a[..., :3] = hx("120e18")
    a[..., 3] = 0.94 * (1 - smooth(0.52, 1.0, yy))
    im = to_img(a)
    save(im, "chrome", "bar_top")
    a2 = np.zeros((h, w, 4))
    a2[..., :3] = hx("120e18")
    a2[..., 3] = 0.97 * smooth(0, 0.5, yy)
    save(to_img(a2), "chrome", "bar_bottom")
    # Filete ornamental: línea que se afina con rombo central.
    c = SS(640, 28, 4)
    c.poly([(0, 14), (300, 12.6), (320, 14), (300, 15.4)], fill=rgba(GOLD_B, 0.95))
    c.poly([(640, 14), (340, 12.6), (320, 14), (340, 15.4)], fill=rgba(GOLD_B, 0.95))
    c.poly([(320, 5), (329, 14), (320, 23), (311, 14)], fill=rgba(GOLD_B, 1))
    c.poly([(298, 10), (302, 14), (298, 18), (294, 14)], fill=rgba(GOLD, 1))
    c.poly([(342, 10), (346, 14), (342, 18), (338, 14)], fill=rgba(GOLD, 1))
    save(c.done(), "chrome", "divider")
    # Sello carmesí redondo (números de aviso, nuevo) y sello oscuro de planeta
    c = SS(96, 96, 4)
    rng = np.random.default_rng(5)
    pts = []
    for i in range(44):
        a_ = i / 44 * math.tau
        r = 42 + rng.random() * 4
        pts.append((48 + math.cos(a_) * r, 48 + math.sin(a_) * r))
    c.poly(pts, fill=rgba(hx("9b2533"), 1))
    c.circle(48, 48, 33, outline=rgba(hx("c9505a"), 0.8), w=1.6)
    save(c.done(), "chrome", "seal_wax")
    c = SS(96, 96, 4)
    c.circle(48, 48, 44, fill=rgba(hx("15111d"), 1), outline=rgba(GOLD, 1), w=2)
    c.circle(48, 48, 38, outline=rgba(GOLD_B, 0.35), w=1)
    save(c.done(), "chrome", "seal_dark")
    # Barras de valor: riel y relleno en blanco para teñir.
    c = SS(64, 24, 4)
    c.poly([(6, 1), (58, 1), (63, 12), (58, 23), (6, 23), (1, 12)], fill=rgba(hx("0d0a12"), 0.95), outline=rgba(GOLD, 0.8), w=1.2)
    save(c.done(), "chrome", "bar_rail")
    c = SS(64, 24, 4)
    c.poly([(6, 1), (58, 1), (63, 12), (58, 23), (6, 23), (1, 12)], fill=(255, 255, 255, 255))
    save(c.done(), "chrome", "bar_fill")
    # Globo de diálogo de pergamino: 9-slice con cola aparte
    c = SS(44, 30, 4)
    c.poly([(0, 0), (44, 0), (8, 30)], fill=rgba(hx("e5d8bb"), 1))
    save(c.done(), "chrome", "bubble_tail")


# ---------------------------------------------------------------------------------------------- Iconos


def icon_canvas(s=128):
    return SS(s, s, 4)


def icons():
    W = (255, 255, 255, 255)
    res = {}

    def stroke(c, pts, w=5):
        c.line(pts, W, w)

    # Inicio: caparazón de la Tortuga
    c = icon_canvas()
    pts = [(14, 88), (30, 44), (64, 26), (98, 44), (114, 88)]
    c.line(pts, W, 6)
    c.line([(14, 88), (114, 88)], W, 6)
    c.poly([(64, 42), (82, 54), (82, 74), (64, 86), (46, 74), (46, 54)], outline=W, w=4.5)
    c.line([(64, 42), (64, 28)], W, 3.5)
    c.line([(82, 54), (98, 45)], W, 3.5)
    c.line([(46, 54), (30, 45)], W, 3.5)
    c.line([(114, 88), (122, 84), (124, 76)], W, 6)
    c.line([(32, 88), (28, 104)], W, 7)
    c.line([(96, 88), (100, 104)], W, 7)
    res["home"] = c
    # Aquelarre: sombrero de bruja
    c = icon_canvas()
    c.poly([(58, 12), (84, 40), (94, 84), (34, 84), (46, 40)], outline=W, w=5.5)
    c.line([(10, 94), (34, 84), (94, 84), (118, 94), (64, 106), (10, 94)], W, 5.5)
    c.line([(38, 72), (90, 72)], W, 6)
    c.poly([(64, 62), (70, 72), (64, 82), (58, 72)], fill=W)
    res["coven"] = c
    # Yunque
    c = icon_canvas()
    c.poly([(12, 40), (98, 40), (118, 48), (96, 62), (84, 62), (80, 80), (92, 92), (92, 104), (36, 104), (36, 92), (48, 80), (44, 62), (28, 62), (12, 52)], outline=W, w=5.5)
    c.circle(112, 22, 3.5, fill=W)
    c.circle(98, 16, 2.5, fill=W)
    c.circle(120, 34, 2.5, fill=W)
    res["forge"] = c
    # Caldero
    c = icon_canvas()
    c.line([(18, 54), (110, 54)], W, 7)
    c.d.arc([24 * 4, 34 * 4, 104 * 4, 110 * 4], 0, 180, fill=W, width=24)
    c.line([(24, 54), (24, 72)], W, 6)
    c.line([(104, 54), (104, 72)], W, 6)
    c.line([(42, 106), (34, 118)], W, 6)
    c.line([(86, 106), (94, 118)], W, 6)
    for x, y in [(50, 34), (64, 24), (80, 36)]:
        c.circle(x, y, 5, outline=W, w=3)
    res["cauldron"] = c
    # Manantial: gota y ondas con vapor
    c = icon_canvas()
    c.d.arc([14 * 4, 78 * 4, 114 * 4, 118 * 4], 0, 360, fill=W, width=20)
    c.d.arc([38 * 4, 88 * 4, 90 * 4, 108 * 4], 0, 360, fill=W, width=14)
    for x in (42, 64, 86):
        pts = [(x + math.sin(i * 0.7) * 6, 72 - i * 7) for i in range(9)]
        c.line(pts, W, 4.5)
    res["spring"] = c
    # Tienda: balanza contable
    c = icon_canvas()
    c.line([(64, 14), (64, 110)], W, 6)
    c.line([(40, 112), (88, 112)], W, 6)
    c.line([(18, 32), (110, 32)], W, 5)
    for x in (24, 104):
        c.line([(x, 32), (x - 14, 72)], W, 3)
        c.line([(x, 32), (x + 14, 72)], W, 3)
        c.d.arc([(x - 18) * 4, 54 * 4, (x + 18) * 4, 90 * 4], 0, 180, fill=W, width=18)
    c.circle(64, 14, 5, fill=W)
    res["shop"] = c
    # Libro abierto
    c = icon_canvas()
    c.poly([(64, 36), (40, 24), (12, 28), (12, 96), (40, 92), (64, 104), (88, 92), (116, 96), (116, 28), (88, 24)], outline=W, w=5)
    c.line([(64, 36), (64, 104)], W, 4.5)
    for y in (46, 60, 74):
        c.line([(26, y), (50, y + 3)], W, 3)
        c.line([(78, y + 3), (102, y)], W, 3)
    res["book"] = c
    # Fresno
    c = icon_canvas()
    c.line([(64, 118), (64, 66)], W, 8)

    def br(x, y, a, l, d):
        x2, y2 = x + math.cos(a) * l, y + math.sin(a) * l
        c.line([(x, y), (x2, y2)], W, max(2, d * 1.4))
        if d > 0:
            br(x2, y2, a - 0.48, l * 0.74, d - 1)
            br(x2, y2, a + 0.48, l * 0.74, d - 1)
        else:
            c.circle(x2, y2, 3.2, fill=W)

    br(64, 70, -math.pi / 2, 26, 3)
    res["tree"] = c
    # Sobre
    c = icon_canvas()
    c.poly([(14, 34), (114, 34), (114, 98), (14, 98)], outline=W, w=5.5)
    c.line([(14, 34), (64, 72), (114, 34)], W, 5)
    c.circle(64, 72, 9, fill=W)
    res["mail"] = c
    # Encargos: rollo con marcas
    c = icon_canvas()
    c.poly([(28, 20), (100, 20), (100, 108), (28, 108)], outline=W, w=5.5)
    for y in (42, 64, 86):
        c.line([(40, y), (48, y + 7), (58, y - 7)], W, 4)
        c.line([(66, y), (90, y)], W, 4)
    res["tasks"] = c
    # Ajustes: rueda de astrolabio
    c = icon_canvas()
    c.circle(64, 64, 26, outline=W, w=6)
    c.circle(64, 64, 8, fill=W)
    for i in range(8):
        a = i / 8 * math.tau
        c.line([(64 + math.cos(a) * 28, 64 + math.sin(a) * 28), (64 + math.cos(a) * 46, 64 + math.sin(a) * 46)], W, 9)
    res["settings"] = c
    # Atrás
    c = icon_canvas()
    c.line([(78, 22), (36, 64), (78, 106)], W, 10)
    res["back"] = c
    # Juicios: balanza inclinada / espada corta
    c = icon_canvas()
    c.poly([(64, 12), (76, 34), (70, 88), (58, 88), (52, 34)], outline=W, w=5)
    c.line([(36, 90), (92, 90)], W, 7)
    c.line([(64, 92), (64, 116)], W, 7)
    res["trial"] = c
    # Lupa/probabilidades
    c = icon_canvas()
    c.circle(54, 54, 32, outline=W, w=6.5)
    c.line([(78, 78), (112, 112)], W, 10)
    c.text(54, 54, "%", font("Alegreya-Bold.ttf", 38), W)
    res["odds"] = c
    # Plus
    c = icon_canvas()
    c.line([(64, 26), (64, 102)], W, 10)
    c.line([(26, 64), (102, 64)], W, 10)
    res["plus"] = c
    # Candado
    c = icon_canvas()
    c.poly([(30, 58), (98, 58), (98, 112), (30, 112)], fill=W)
    c.d.arc([40 * 4, 18 * 4, 88 * 4, 78 * 4], 180, 360, fill=W, width=28)
    res["lock"] = c
    # Altavoz
    c = icon_canvas()
    c.poly([(18, 50), (42, 50), (68, 26), (68, 102), (42, 78), (18, 78)], fill=W)
    c.d.arc([58 * 4, 38 * 4, 96 * 4, 90 * 4], 300, 60, fill=W, width=20)
    c.d.arc([58 * 4, 22 * 4, 114 * 4, 106 * 4], 310, 50, fill=W, width=20)
    res["sound"] = c
    # Espejo
    c = icon_canvas()
    c.d.ellipse([34 * 4, 12 * 4, 94 * 4, 88 * 4], outline=W, width=24)
    c.line([(64, 88), (64, 118)], W, 8)
    c.line([(48, 118), (80, 118)], W, 6)
    c.line([(52, 36), (66, 22)], W, 4)
    res["mirror"] = c
    for k, v in res.items():
        save(v.done(), "icons", k)

    # Planetas: glifos blancos para teñir
    f = font("NotoSansSymbols-Regular.ttf", 92)
    c = icon_canvas()
    c.circle(64, 64, 40, outline=W, w=8)
    c.circle(64, 64, 9, fill=W)
    save(c.done(), "planets", "sol")
    for pid, g in [("luna", "☽"), ("marte", "♂"), ("mercurio", "☿"), ("venus", "♀"), ("jupiter", "♃"), ("saturno", "♄")]:
        c = icon_canvas()
        c.text(64, 62, g, f, W)
        save(c.done(), "planets", pid)

    # Monedas, con color propio: la identidad de cada recurso se lee sin texto
    def crystal(c, cx, cy, s, col, seed=0):
        """Cubo de sal en perspectiva: tres caras; aristas rectas."""
        t = lerp(col, np.array([1, 1, 1]), 0.45)
        l = col * 0.82
        r = col * 0.55
        top = [(cx, cy - s), (cx + s * .9, cy - s * .5), (cx, cy), (cx - s * .9, cy - s * .5)]
        left = [(cx - s * .9, cy - s * .5), (cx, cy), (cx, cy + s), (cx - s * .9, cy + s * .5)]
        right = [(cx + s * .9, cy - s * .5), (cx, cy), (cx, cy + s), (cx + s * .9, cy + s * .5)]
        c.poly(top, fill=rgba(t))
        c.poly(left, fill=rgba(l))
        c.poly(right, fill=rgba(r))
        for p in (top, left, right):
            c.poly(p, outline=rgba(np.array([1, 1, 1]), 0.5), w=1.4)

    c = icon_canvas()
    crystal(c, 66, 44, 26, hx("f0a9a0"))
    crystal(c, 40, 78, 22, hx("f7c8bb"))
    crystal(c, 88, 86, 18, hx("d97f7c"))
    save(c.done(), "currency", "sales_de_bano")
    # Almita: llama-alma, núcleo blanco, halo dorado
    s = 128
    yy, xx = np.mgrid[0:s, 0:s].astype(float)
    dy = (yy - 74) / 40
    dx = (xx - 64) / (30 * np.clip(1.15 - (np.clip(-dy, -1, 1) + 1) / 2, 0.12, 1))
    d = np.hypot(dx, dy)
    core = 1 - smooth(0.25, 1.0, d)
    halo = np.exp(-(((xx - 64) ** 2 + (yy - 68) ** 2) / 1500))
    rgb = lerp(hx("ffd98a"), hx("fffbe8"), core[..., None] ** 0.8)
    alpha = np.clip(core * 1.6 + halo * 0.42, 0, 1)
    save(to_img(np.dstack([rgb, alpha])), "currency", "almitas")
    # Savia: gota verde dorada
    c = icon_canvas()
    pts = [(64 + (34 * math.sin(a)) * (1 - 0.72 * (1 + math.cos(a)) / 2 * 0.85), 80 - 52 * math.cos(a) * (0.9 if math.cos(a) > 0 else 0.62)) for a in np.linspace(0, math.tau, 60)]
    c.poly(pts, fill=rgba(hx("9fb35a")), outline=rgba(hx("e6e39a")), w=3)
    c.circle(54, 78, 7, fill=(255, 255, 255, 160))
    save(c.done(), "currency", "savia")
    # Polvo astral: estrellas violáceas
    c = icon_canvas()
    for x, y, r in [(62, 56, 34), (98, 90, 16), (32, 96, 14), (102, 30, 10)]:
        pts = []
        for i in range(8):
            a = i / 8 * math.tau - math.pi / 2
            rr = r if i % 2 == 0 else r * 0.26
            pts.append((x + math.cos(a) * rr, y + math.sin(a) * rr))
        c.poly(pts, fill=rgba(hx("cdb4ff")))
    save(c.done(), "currency", "polvo_astral")
    # Perla: orbe nacarado
    yy, xx = np.mgrid[0:s, 0:s].astype(float)
    d = np.hypot(xx - 64, yy - 64) / 46
    n_ = np.clip(1 - d ** 2, 0, 1) ** 0.5
    lx = np.clip(((xx - 64) * -0.45 + (yy - 64) * -0.6) / 46 + n_ * 0.66, 0, 1)
    rgb = lerp(hx("c9b6c4"), hx("fff8f2"), lx ** 1.4) + (np.sin(xx * 0.09 + yy * 0.06) * 0.03)[..., None]
    alpha = 1 - smooth(0.97, 1.02, d)
    save(to_img(np.dstack([np.clip(rgb, 0, 1), alpha])), "currency", "perlas")


# ---------------------------------------------------------------------------------------------- Efectos


def make_fx():
    # Punto suave (partículas, almitas, brillos)
    s = 128
    yy, xx = np.mgrid[0:s, 0:s].astype(float) - 63.5
    r = np.hypot(xx, yy) / 64
    save(to_img(np.dstack([np.ones((s, s, 3)), np.clip(1 - r, 0, 1) ** 2.4])), "fx", "soft_dot")
    # Destello de cuatro puntas
    a = np.exp(-np.abs(xx) / 3.4) * np.exp(-np.abs(yy) / 34) + np.exp(-np.abs(yy) / 3.4) * np.exp(-np.abs(xx) / 34) + np.exp(-(r * 5) ** 2) * 0.6
    save(to_img(np.dstack([np.ones((s, s, 3)), np.clip(a * (1 - smooth(0.8, 1, r)), 0, 1)])), "fx", "sparkle")
    # Vapor: cuatro bocanadas de ruido
    for i in range(4):
        s2 = 512
        yy2, xx2 = np.mgrid[0:s2, 0:s2].astype(float) - 255.5
        rr = np.hypot(xx2, yy2) / 256
        n = noise(s2, s2, 120, 200 + i, 6)
        puff = np.clip(1 - rr * (1.1 - n * 0.5), 0, 1)
        puff = smooth(0.0, 0.85, puff) * smooth(0.25, 0.8, n + 0.25) * (1 - smooth(0.82, 1.0, rr))
        save(to_img(np.dstack([np.ones((s2, s2, 3)), puff])), "fx", f"steam_{i}")
    # Rayos de luz radiales
    s3 = 1024
    yy3, xx3 = np.mgrid[0:s3, 0:s3].astype(float) - 511.5
    ang = np.arctan2(yy3, xx3)
    rr = np.hypot(xx3, yy3) / 512
    rng = np.random.default_rng(301)
    rays = np.zeros_like(ang)
    for f, w_ in zip(rng.integers(5, 40, 14), rng.random(14)):
        rays += w_ * np.cos(ang * f + rng.random() * 6.28)
    rays = (rays - rays.min()) / (rays.max() - rays.min())
    a = smooth(0.48, 0.85, rays) * (1 - smooth(0.05, 1.0, rr)) ** 1.3
    save(to_img(np.dstack([np.ones((s3, s3, 3)), a])), "fx", "rays")
    # Onda en el agua
    s4 = 512
    yy4, xx4 = np.mgrid[0:s4, 0:s4].astype(float) - 255.5
    rr = np.hypot(xx4, yy4) / 256
    a = np.exp(-((rr - 0.9) / 0.03) ** 2) + 0.4 * np.exp(-((rr - 0.78) / 0.02) ** 2)
    save(to_img(np.dstack([np.ones((s4, s4, 3)), np.clip(a, 0, 1) * (1 - smooth(0.97, 1, rr))])), "fx", "ripple")
    # Cristales de sal (cúbicos: la arista recta distingue la sal de la nieve)
    for i, (col, sz) in enumerate([(hx("ffffff"), 40), (hx("ffe5dc"), 34), (hx("f7c8bb"), 28)]):
        c = SS(128, 128, 4)
        cx, cy, s_ = 64, 64, sz
        t, l, r_ = lerp(col, np.ones(3), 0.4), col * 0.85, col * 0.58
        top = [(cx, cy - s_), (cx + s_ * .9, cy - s_ * .5), (cx, cy), (cx - s_ * .9, cy - s_ * .5)]
        left = [(cx - s_ * .9, cy - s_ * .5), (cx, cy), (cx, cy + s_), (cx - s_ * .9, cy + s_ * .5)]
        right = [(cx + s_ * .9, cy - s_ * .5), (cx, cy), (cx, cy + s_), (cx + s_ * .9, cy + s_ * .5)]
        for p_, q_ in ((top, t), (left, l), (right, r_)):
            c.poly(p_, fill=rgba(q_), outline=rgba(np.ones(3), 0.55), w=1.2)
        save(c.done(), "fx", f"crystal_{i}")
    # Viñeta y degradado
    s5 = 512
    yy5, xx5 = np.mgrid[0:s5, 0:s5].astype(float) / (s5 - 1) - 0.5
    v = smooth(0.35, 0.78, np.hypot(xx5, yy5))
    save(to_img(np.dstack([np.zeros((s5, s5, 3)), v])), "fx", "vignette")
    w = np.linspace(0, 1, 256)[:, None].repeat(4, 1)
    save(to_img(np.dstack([np.ones((256, 4, 3)), w])), "fx", "grad_v")
    # Ruido teselable para shaders (R: fBm grande, G: fBm chico, B: voronoi, A: 1)
    s6 = 512

    def tile_noise(scale, seed, oct_):
        rng = np.random.default_rng(seed)
        out = np.zeros((s6, s6))
        amp = 1.0
        for o in range(oct_):
            n_ = 2 ** (o + scale)
            g = rng.random((n_, n_))
            big = ndi.zoom(np.pad(g, 3, mode="wrap"), s6 / n_, order=3)
            off = int(3 * s6 / n_)
            out += big[off:off + s6, off:off + s6] * amp
            amp *= 0.55
        return (out - out.min()) / (out.max() - out.min())

    r_ = tile_noise(3, 401, 5)
    g_ = tile_noise(5, 402, 4)
    b_ = tile_noise(2, 403, 6)
    arr = (np.dstack([r_, g_, b_]) * 255).astype(np.uint8)
    d = os.path.join(OUT, "fx")
    Image.fromarray(arr, "RGB").save(os.path.join(d, "noise.png"))
    print("  fx/noise")
    # Grietas doradas (kintsugi), para Paralaje alto y para el quiebre
    W_, H_ = 1600, 900
    c = SS(W_, H_, 1)
    rng = np.random.default_rng(405)

    def crack(x, y, a, l, w_):
        n = int(l / 26)
        pts = [(x, y)]
        for i in range(n):
            a += (rng.random() - 0.5) * 0.9
            x += math.cos(a) * 26
            y += math.sin(a) * 26
            pts.append((x, y))
            if rng.random() < 0.14 and w_ > 1.4:
                crack(x, y, a + (rng.random() - 0.5) * 2.4, l * 0.42, w_ * 0.6)
        c.line(pts, rgba(GOLD_B, 1), w_)

    for sx, sy, a in [(0, 120, 0.35), (W_, 90, 2.7), (0, 760, -0.4), (W_, 820, 3.5), (720, 0, 1.4), (1020, H_, -1.7)]:
        crack(sx, sy, a, 900, 3.6)
    save(c.done().filter(ImageFilter.GaussianBlur(0.6)), "fx", "cracks")
    # Astrolabios
    save(astrolabe(1024), "fx", "astrolabe")
    save(ring_only(1024), "fx", "ring")
    # Silueta genérica de bruja para quien aún no tiene retrato: sombrero y hombros en blanco para teñir
    c = SS(512, 768, 2)
    c.poly([(246, 40), (312, 130), (340, 270), (172, 270), (206, 130)], fill=(255, 255, 255, 255))
    c.d.ellipse([60 * 2, 250 * 2, 452 * 2, 310 * 2], fill=(255, 255, 255, 255))
    c.d.ellipse([190 * 2, 270 * 2, 322 * 2, 420 * 2], fill=(255, 255, 255, 255))
    c.poly([(198, 390), (314, 390), (420, 760), (92, 760)], fill=(255, 255, 255, 255))
    save(c.done().filter(ImageFilter.GaussianBlur(2)), "fx", "silhouette")


# ---------------------------------------------------------------------------------------------- Logo


def make_logo():
    W, H = 1400, 520
    k = 2
    f = ImageFont.truetype(os.path.join(FONTS, "PirataOne-Regular.ttf"), 250 * k)
    fs = ImageFont.truetype(os.path.join(FONTS, "Texturina-SemiBold.ttf"), 46 * k)
    m = Image.new("L", (W * k, H * k), 0)
    d = ImageDraw.Draw(m)
    d.text((W * k // 2, 215 * k), "Sal y Almas", font=f, fill=255, anchor="mm")
    mask = np.asarray(m) / 255.0
    # relieve: pan de oro con vetas y luz arriba
    blur = ndi.gaussian_filter(mask, 5 * k * 0.6)
    gy, gx = np.gradient(blur)
    light = np.clip((-gy * 0.9 - gx * 0.4) * 22, -1, 1)
    h_, w_ = mask.shape
    veta = noise(h_, w_, 120, 501, 5)
    vy = np.linspace(0, 1, h_)[:, None].repeat(w_, 1)
    col = lerp(hx("fff0b8"), hx("b8802a"), smooth(0.28, 0.62, vy + (veta - 0.5) * 0.25))
    col = col + light[..., None] * 0.4
    # costra de sal sobre la parte alta de las letras: cubitos blancos
    crust = smooth(0.62, 0.78, noise(h_, w_, 30, 502, 3)) * (1 - smooth(0.24, 0.42, vy))
    col = lerp(col, hx("fbf6ee"), crust * 0.8)
    alpha = mask
    # contorno de tinta y sombra
    out = ndi.gaussian_filter(mask, 7.0 * k * 0.5)
    out = smooth(0.05, 0.25, out)
    shadow = np.roll(ndi.gaussian_filter(mask, 12 * k), 10 * k, axis=0) * 0.75
    base_a = np.clip(out + shadow, 0, 1)
    rgb = lerp(hx("170d14"), col, alpha)
    a = np.clip(alpha + base_a * (1 - alpha), 0, 1)
    im = to_img(np.dstack([rgb, a]))
    # subtítulo con filete
    c = Image.new("RGBA", (W * k, H * k), (0, 0, 0, 0))
    dc = ImageDraw.Draw(c, "RGBA")
    dc.text((W * k // 2, 402 * k), "La Odisea del Fresno", font=fs, fill=rgba(hx("f2e6cd"), 1), anchor="mm", stroke_width=3, stroke_fill=(20, 12, 18, 230))
    for sx in (-1, 1):
        x0 = W * k // 2 + sx * 300 * k
        dc.line([(x0, 402 * k), (x0 + sx * 200 * k, 402 * k)], fill=rgba(GOLD_B, 0.9), width=2 * k)
        dc.polygon([(x0, 394 * k), (x0 + 8 * k, 402 * k), (x0, 410 * k), (x0 - 8 * k, 402 * k)], fill=rgba(GOLD_B, 1))
    im.alpha_composite(c)
    save(im.resize((W, H), Image.LANCZOS), "logo", "logo")


# ---------------------------------------------------------------------------------------------- Tarjetas de destino


def engraved_card(W, H, seed, tint_top, tint_bot, draw):
    g = grad(H, W, [(0, tint_top), (1, tint_bot)])
    n = noise(H, W, 120, seed, 5)[..., None]
    g = g * (0.82 + 0.34 * n)
    yy, xx = np.mgrid[0:H, 0:W].astype(float)
    v = np.hypot((xx - W / 2) / W, (yy - H / 2) / H)
    g = g * (1 - smooth(0.32, 0.78, v)[..., None] * 0.7)
    im = to_img(np.dstack([g, np.ones((H, W))]))
    c = SS(W, H, 2)
    draw(c, W, H)
    layer = c.done()
    glow = layer.filter(ImageFilter.GaussianBlur(7))
    im.alpha_composite(glow)
    im.alpha_composite(layer)
    return im


def draw_tree(c, W, H):
    rng = np.random.default_rng(601)
    gold = rgba(GOLD_B, 0.92)
    # anillos de zona
    for i, r in enumerate([0.26, 0.42, 0.58, 0.74]):
        c.arc(W * 0.5, H * 1.02, H * r * 1.25, 180, 360, rgba(GOLD_B, 0.25), 1.2)

    def br(x, y, a, l, d):
        x2 = x + math.cos(a) * l
        y2 = y + math.sin(a) * l
        c.line([(x, y), (x2, y2)], gold, max(1.0, d * 1.05))
        if d > 0:
            n = 2 if rng.random() < 0.8 else 3
            for i in range(n):
                br(x2, y2, a + (i - (n - 1) / 2) * 0.62 + (rng.random() - 0.5) * 0.3, l * (0.68 + rng.random() * 0.14), d - 1)
        elif rng.random() < 0.55:
            c.circle(x2, y2, 1.8 + rng.random() * 1.6, fill=rgba(SPARK, 0.95))

    br(W * 0.5, H * 0.96, -math.pi / 2, H * 0.22, 8)
    # raíces
    for a in (0.3, 0.9, 2.2, 2.85):
        br(W * 0.5, H * 0.96, a, H * 0.07, 2)


def draw_book(c, W, H):
    gold = rgba(GOLD_B, 0.92)
    cx, cy = W * 0.5, H * 0.62
    s = H * 0.3
    c.poly([(cx, cy - s * .3), (cx - s * 1.4, cy - s * .75), (cx - s * 1.5, cy + s * .55), (cx, cy + s), (cx + s * 1.5, cy + s * .55), (cx + s * 1.4, cy - s * .75)], fill=rgba(hx("efe3c8"), 0.11), outline=gold, w=2.2)
    c.line([(cx, cy - s * .3), (cx, cy + s)], gold, 2)
    for i in range(5):
        t = i / 5
        for sg in (-1, 1):
            c.line([(cx + sg * s * .14, cy - s * .18 + t * s * .9), (cx + sg * s * 1.24, cy - s * .55 + t * s * .9)], rgba(GOLD_B, 0.5), 1.1)
    rng = np.random.default_rng(611)
    pts = [(cx + (rng.random() - .5) * W * .8, H * .07 + rng.random() * H * .3) for _ in range(9)]
    pts.sort()
    c.line(pts, rgba(GOLD_B, 0.5), 1.1)
    for p in pts:
        c.circle(p[0], p[1], 2.8, fill=rgba(SPARK, 1))
    # hilo de almas que sube de la página
    for i in range(10):
        x = cx + (rng.random() - .5) * s * 1.6
        y = cy - s * .35 - rng.random() * s * 0.9
        c.circle(x, y, 1.4 + rng.random() * 2, fill=rgba(SPARK, .85))


def draw_spring(c, W, H):
    gold = rgba(GOLD_B, 0.92)
    cx, cy = W * 0.5, H * 0.8
    for i, r in enumerate([0.17, 0.3, 0.45, 0.62, 0.8]):
        c.d.arc([(cx - W * r) * 2, (cy - H * r * .26) * 2, (cx + W * r) * 2, (cy + H * r * .26) * 2], 0, 360, fill=rgba(GOLD_B, .75 - i * .12), width=3)
    for i, x in enumerate((-.22, -.07, .08, .24)):
        pts = [(cx + W * x + math.sin(t * .9 + i) * 11, cy - H * .1 - t * H * .055) for t in range(10)]
        c.line(pts, rgba(np.array([1, .93, .85]), .55), 2.4)
    # cubo de sal que cae
    s = H * .085
    x0, y0 = cx, H * .24
    top = [(x0, y0 - s), (x0 + s * .9, y0 - s * .5), (x0, y0), (x0 - s * .9, y0 - s * .5)]
    left = [(x0 - s * .9, y0 - s * .5), (x0, y0), (x0, y0 + s), (x0 - s * .9, y0 + s * .5)]
    right = [(x0 + s * .9, y0 - s * .5), (x0, y0), (x0, y0 + s), (x0 + s * .9, y0 + s * .5)]
    c.poly(top, fill=rgba(SALT), outline=gold, w=1.4)
    c.poly(left, fill=rgba(hx("f0d3c8")), outline=gold, w=1.4)
    c.poly(right, fill=rgba(hx("c99a92")), outline=gold, w=1.4)


def draw_scale(c, W, H):
    gold = rgba(GOLD_B, 0.92)
    cx = W * .5
    c.line([(cx, H * .14), (cx, H * .86)], gold, 3)
    c.line([(cx - W * .12, H * .86), (cx + W * .12, H * .86)], gold, 3)
    c.line([(cx - W * .3, H * .32), (cx + W * .3, H * .22)], gold, 2.6)
    for sx, y in ((-.3, .32), (.3, .22)):
        x = cx + W * sx
        c.line([(x, H * y), (x - W * .07, H * (y + .25))], rgba(GOLD_B, .7), 1.4)
        c.line([(x, H * y), (x + W * .07, H * (y + .25))], rgba(GOLD_B, .7), 1.4)
        c.d.arc([(x - W * .08) * 2, (H * (y + .15)) * 2, (x + W * .08) * 2, (H * (y + .36)) * 2], 0, 180, fill=gold, width=4)


def make_cards():
    save(engraved_card(900, 520, 701, hx("3a2236"), hx("170f1d"), draw_tree), "cards", "fresno")
    save(engraved_card(900, 520, 702, hx("4a2a30"), hx("1c1016"), draw_book), "cards", "story")
    save(engraved_card(900, 520, 703, hx("7d3d44"), hx("220f18"), draw_spring), "cards", "spring")
    save(engraved_card(900, 520, 704, hx("2c2638"), hx("120e18"), draw_scale), "cards", "trial")


# ---------------------------------------------------------------------------------------------- Brujas


# id de datos -> carpeta de arte. Los nombres de arte son los de docs/Retratos para Codex.md.
ART_IDS = {"zarigueya": "muertita", "topo": "topacia"}


def make_witches():
    """Copia los recortes al juego. Si Codex los regenera, basta volver a correr este grupo."""
    d = os.path.join(OUT, "brujas")
    os.makedirs(d, exist_ok=True)
    n = 0
    for art in sorted(os.listdir(RECORTES)):
        folder = os.path.join(RECORTES, art)
        if not os.path.isdir(folder):
            continue
        rid = {v: k for k, v in ART_IDS.items()}.get(art, art)
        for ph in ("f1", "f3"):
            for kind, src in (("cuerpo", f"{art}_{ph}_cuerpo.png"), ("busto", f"{art}_{ph}_busto_320.png")):
                p = os.path.join(folder, src)
                if os.path.exists(p):
                    shutil.copyfile(p, os.path.join(d, f"{rid}_{ph}_{kind}.png"))
                    n += 1
    print(f"  brujas: {n} archivos")


GROUPS = {"bg": make_bg, "chrome": make_chrome, "icons": icons, "fx": make_fx, "logo": make_logo, "cards": make_cards, "witches": make_witches}

if __name__ == "__main__":
    want = sys.argv[1:] or ["all"]
    for g, fn in GROUPS.items():
        if "all" in want or g in want:
            print(g)
            fn()
