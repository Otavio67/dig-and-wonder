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


def composition(width=1080, height=1920, safe_top=0, safe_bottom=0):
    # CanvasScaler matchWidthOrHeight=0.5 usa a média geométrica das escalas.
    scale = math.sqrt(width / 1080 * height / 1920)
    safe_height = height - safe_top - safe_bottom
    font = data(ROOT / "UnityProject/Assets/_Game/Resources/Fonts/Workshop/DWRounded-ExtraBold.ttf", "font/ttf")
    return f'''<!doctype html><html lang="pt-BR"><meta charset="utf-8">
<style>
@font-face{{font-family:DW;src:url({font})}}
*{{box-sizing:border-box}}html,body{{margin:0;width:{width}px;height:{height}px;overflow:hidden}}
body{{background:#1b263b;color:#fff8e7;font-family:DW,sans-serif}}
.background{{position:absolute;width:100%;height:100%;object-fit:cover}}
.safe{{position:absolute;top:{safe_top}px;left:0;width:100%;height:{safe_height}px}}
.logo{{position:absolute;left:6%;bottom:73.5%;width:88%;height:22.5%;object-fit:contain}}
.button{{position:absolute;left:9%;bottom:14%;width:82%;height:8.3%;border:none;padding:0;background:none;color:inherit;font-family:inherit;cursor:pointer}}
.base,.face{{position:absolute;inset:0;border: {26*scale}px solid transparent;border-image:url({data(ART/'Workshop/button.png')}) 26 fill stretch}}
.base{{height:95%;top:5%;filter:brightness(.24)}}
.face{{height:93%;bottom:7%;top:0;isolation:isolate}}
.face::before{{content:"";position:absolute;inset:-{20*scale}px;z-index:0;background:rgba(20,157,112,.73);mix-blend-mode:multiply;border-radius:{15*scale}px}}
.label{{position:absolute;inset:0;display:flex;align-items:center;justify-content:center;font-size:{47*scale}px;white-space:nowrap}}
.button:active .face{{transform:translateY({5*scale}px)}}
.status{{position:absolute;left:9%;bottom:8.4%;width:82%;height:4.9%;display:flex;align-items:center;justify-content:center;text-align:center;font-size:{26*scale}px}}
.version{{position:absolute;left:15%;bottom:2.2%;width:70%;height:3.3%;display:flex;align-items:center;justify-content:center;font-size:{22*scale}px;color:#9eb3bd}}
</style><body>
<img class="background" src="{data(ART/'StartMenu/mine-start-v1.png')}" alt="Mina, brocas e inimigos">
<div class="safe"><img class="logo" src="{data(ART/'StartMenu/dig-and-wonder-logo-v3.png')}" alt="Dig &amp; Wonder">
<button class="button"><span class="base"></span><span class="face"></span><span class="label">Iniciar a mineração</span></button>
<div class="status">Explore, evolua e descubra as profundezas.</div>
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
        browser.close()
    (OUTPUT / "composition.json").write_text(json.dumps(observations, indent=2))


if __name__ == "__main__":
    main()
