#!/usr/bin/env python3
"""Prévia de composição fora do Unity; não comprova execução do jogo.

Usa os mesmos PNGs, fonte, âncoras e CanvasScaler da tela inicial.
python3 scripts/preview-start-menu.py --capture
"""
from pathlib import Path
import base64
import json
import math
import sys

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / "UnityProject/Assets/_Game/Resources/Art"
OUTPUT = ROOT / ".tools/review/start-menu"


def data(path, mime="image/png"):
    return "data:" + mime + ";base64," + base64.b64encode(path.read_bytes()).decode()


def composition(width=1080, height=1920, safe_top=0, safe_bottom=0, loading=None):
    # CanvasScaler matchWidthOrHeight=0.5 usa a média geométrica das escalas.
    scale = math.sqrt(width / 1080 * height / 1920)
    safe_height = height - safe_top - safe_bottom
    font = data(ROOT / "UnityProject/Assets/_Game/Resources/Fonts/Workshop/TitanOne-Regular.ttf", "font/ttf")
    action = ('<button class="button"><span class="label">INICIAR MINERAÇÃO</span></button>' if loading is None
              else f'<div class="loading"><div class="loading-label">CARREGANDO...</div><div class="track"><div class="fill" style="width:{max(0,min(1,loading))*100}%"></div></div></div>')
    message = "Explore, evolua e descubra as profundezas." if loading is None else ""
    return f'''<!doctype html><html lang="pt-BR"><meta charset="utf-8">
<style>
@font-face{{font-family:DW;src:url({font})}}
*{{box-sizing:border-box}}html,body{{margin:0;width:{width}px;height:{height}px;overflow:hidden}}
body{{background:#1b263b;color:#ffffff;font-family:DW,sans-serif}}
.background{{position:absolute;width:100%;height:100%;object-fit:cover}}
.safe{{position:absolute;top:{safe_top}px;left:0;width:100%;height:{safe_height}px}}
.logo{{position:absolute;left:6%;bottom:73.5%;width:88%;height:22.5%;object-fit:contain}}
.button{{position:absolute;left:9%;bottom:14%;width:82%;height:8.3%;border:none;padding:0;background:none;color:inherit;font-family:inherit;cursor:pointer}}
.label{{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:{42*scale}px;white-space:nowrap}}
.button:active .label{{transform:translateY({5*scale}px)}}
.loading{{position:absolute;left:9%;bottom:14%;width:82%;height:8.3%}}
.loading-label{{position:absolute;bottom:55%;width:100%;height:45%;display:flex;align-items:center;justify-content:center;font-size:{30*scale}px}}
.track{{position:absolute;left:10%;bottom:18%;width:80%;height:12%;background:rgba(255,255,255,.2)}}
.fill{{height:100%;background:white}}
.status{{position:absolute;left:9%;bottom:8.4%;width:82%;height:4.9%;display:flex;align-items:center;justify-content:center;text-align:center;font-size:{26*scale}px}}
.version{{position:absolute;left:15%;bottom:2.2%;width:70%;height:3.3%;display:flex;align-items:center;justify-content:center;font-size:{22*scale}px;color:#9eb3bd}}
</style><body>
<img class="background" src="{data(ART/'StartMenu/mine-start-v2.png')}" alt="Mina, brocas e inimigos">
<div class="safe"><img class="logo" src="{data(ART/'StartMenu/dig-and-wonder-logo-v3.png')}" alt="Dig &amp; Wonder">
{action}
<div class="status">{message}</div>
<div class="version">v{(ROOT/'VERSION').read_text().strip()}</div></div></body></html>'''


def main():
    OUTPUT.mkdir(parents=True, exist_ok=True)
    (OUTPUT / "index.html").write_text(composition())
    print(OUTPUT / "index.html")
    if "--capture" not in sys.argv:
        return
    from playwright.sync_api import sync_playwright
    cases = [(1080, 1920, 0, 0), (360, 640, 0, 0), (390, 844, 32, 24), (360, 800, 24, 24)]
    with sync_playwright() as playwright:
        browser = playwright.chromium.launch(executable_path="/usr/lib/chromium/chromium", headless=True,
                                             args=["--no-sandbox", "--disable-dev-shm-usage"])
        observations = []
        for width, height, top, bottom in cases:
            page = browser.new_page(viewport={"width": width, "height": height}, device_scale_factor=1)
            page.set_content(composition(width, height, top, bottom), wait_until="load")
            page.evaluate("document.fonts.ready")
            path = OUTPUT / f"preview-{width}x{height}.png"
            page.screenshot(path=str(path))
            bounds = page.locator(".button").bounding_box()
            observations.append({"size": [width, height], "safe_top": top, "safe_bottom": bottom, "button": bounds})
            page.close()
            print(path)
        page = browser.new_page(viewport={"width": 1080, "height": 1920}, device_scale_factor=1)
        page.set_content(composition(loading=0.6), wait_until="load")
        page.evaluate("document.fonts.ready")
        page.screenshot(path=str(OUTPUT / "preview-loading-1080x1920.png"))
        page.close()
        browser.close()
    (OUTPUT / "composition.json").write_text(json.dumps(observations, indent=2))


if __name__ == "__main__":
    main()
