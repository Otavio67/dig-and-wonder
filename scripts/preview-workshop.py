#!/usr/bin/env python3
"""Prévia de composição em HTML usando a arte real. Não executa nem captura o Unity.

python3 scripts/preview-workshop.py --capture
O modo de captura requer Playwright e /usr/lib/chromium/chromium no ambiente de nuvem.
"""
from pathlib import Path
import base64
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
ART = (ROOT/'UnityProject/Assets/_Game/Resources/Art/Workshop').as_uri()
FONTS = (ROOT/'UnityProject/Assets/_Game/Resources/Fonts/Workshop').as_uri()
SHEET = (ROOT/'UnityProject/Assets/_Game/Resources/Art/Drill/drill_pioneira.png').as_uri()
DATA = (ROOT/'UnityProject/Assets/_Game/Resources/Data/CombatBalance.asset').read_text()


def number(key):
    return re.search(r'^  '+key+r': (.+)$', DATA, re.M).group(1)


def icon(name, css=''):
    return f'<img class="icon" src="{ART}/{name}.png" style="{css}" alt="">'


def main():
    html = '''<!doctype html><html lang="pt-BR"><meta charset="utf-8"><style>
    @font-face {font-family:Title;src:url(FONT_TITLE)}
    @font-face {font-family:Body;src:url(FONT_BODY)}
    * {box-sizing:border-box} html,body{margin:0;width:1080px;height:1920px;overflow:hidden}
    body{background:#1B263B;color:#FFF8E7;font-family:Body, sans-serif}
    .title{font-family:Title,sans-serif;paint-order:stroke fill;-webkit-text-stroke:5px #0c1623;text-shadow:0 5px #091220}
    .plate{border:26px solid transparent;border-image:url(PLATE) 26 fill stretch;position:absolute}
    .button{border:26px solid transparent;border-image:url(BUTTON) 26 fill stretch;background:#ffb703;background-clip:padding-box;
    text-align:center;font-size:32px;line-height:1.18;display:flex;align-items:center;justify-content:center}
    .icon{object-fit:contain;position:absolute;width:100%;height:100%;pointer-events:none}
    .r{position:absolute} .center{display:flex;align-items:center;justify-content:center;text-align:center}
    .slot{position:absolute;width:205px;height:210px}.slot .caption{text-align:center;font:26px Title;margin-top:-4px}
    .slot .lock{position:absolute;right:0;bottom:18px;width:60px;height:60px}
    .stat{font-size:38px;line-height:1.4}.muted{color:#aec1c3}.tab{width:29%;height:130px;position:absolute;bottom:19px;
    border:19px solid transparent;border-image:url(BUTTON) 26 fill stretch;background:#1E6F5C;background-clip:padding-box}
    .tab span{position:absolute;bottom:-3px;left:0;right:0;text-align:center;font-size:28px}
    .plate,.button,.tab{isolation:isolate}
    .plate::before,.button::before,.tab::before{content:"";position:absolute;inset:-20px;z-index:-1;
    background-color:inherit;mix-blend-mode:multiply;border-radius:16px;pointer-events:none}
    body>.r{z-index:2}
    </style><body>
    <div class="plate" style="left:43px;top:27px;width:886px;height:117px;background:#536f82;background-clip:padding-box">
      <div class="title center" style="height:100%;font-size:68px;color:#ffb703">OFICINA</div></div>
    <div class="button r" style="left:950px;top:31px;width:98px;height:104px;background-color:#ff595e">CLOSE</div>
    <div class="r title" style="left:49px;top:157px;font-size:34px">BROCA PIONEIRA</div>
    <div class="r title" style="right:44px;top:157px;font-size:34px">GEAR 0</div>
    <div class="r muted" style="left:45px;top:209px;font-size:18px">PRÉVIA DE COMPOSIÇÃO • FORA DO UNITY</div>
    <div class="plate" style="left:38px;top:240px;width:1005px;height:515px;background:#1e6f5c;background-clip:padding-box">
      PEDESTAL
      <div class="r" style="left:202px;top:21px;width:552px;height:421px;background-image:url(SHEET);background-size:400% 300%"></div>
      SLOTS
    </div>
    <div class="plate center title" style="left:43px;top:764px;width:994px;height:79px;background:#1e6f5c;background-clip:padding-box;font-size:31px">REFORÇOS 0 / 20</div>
    <div class="plate" style="left:43px;top:858px;width:994px;height:154px;background:#536f82;background-clip:padding-box">
     <div class="r title" style="left:8px;top:-9px;font-size:24px">MÓDULO DE COMBATE</div>
     <div class="r title" style="right:44px;top:-9px;font-size:24px">PASSIVOS</div>
     MODULES
    </div>
    ROWS
    <div class="r center muted" style="left:54px;top:1508px;width:972px;height:76px;font-size:24px">Reparo: VIDA 10 • Crítico: ATQ 10<br>Melhorias em preparação</div>
    <div class="r" style="left:59px;top:1592px;width:580px;font-size:27px">Inicie uma expedição para liberar as compras.</div>
    <div class="r button title" style="left:691px;top:1580px;width:346px;height:98px;background-color:#00f5d4;font-size:32px">VOLTAR</div>
    <div class="r" style="left:16px;top:1705px;width:1048px;height:180px;border:26px solid transparent;border-image:url(TREAD) 24 fill stretch">
      TABS
    </div>
    </body></html>'''
    slots=''
    for name,art,x,y in [('BROCA','drill',-9,0),('CHASSI','chassis',-9,234),('MOTOR','motor',757,0),('NÚCLEO','core',757,234)]:
        slots+=f'<div class="slot" style="left:{x}px;top:{y}px"><div class="caption title">{name}</div><div class="r" style="left:12px;top:35px;width:180px;height:160px">{icon("slot")}{icon(art,"left:20px;top:20px;width:140px;height:120px")}</div><div class="lock">{icon("lock")}</div></div>'
    modules=f'<div class="r" style="left:10px;top:34px;width:100px;height:86px">{icon("drill")}</div><div class="r" style="left:128px;top:38px;font-size:27px">4 impactos<br>32 de dano base</div>'
    for i,name in enumerate(['shield','repair','bolt']):
        modules+=f'<div class="r" style="left:{608+i*105}px;top:34px;width:90px;height:85px">{icon(name,"opacity:.48")}{icon("lock","left:47px;top:48px;width:41px;height:41px")}</div>'
    rows=''
    for title,art,value,y in [('ATQ','attack',number('drillAttackDamage'),1029),('VIDA','health',number('playerBaseHealth'),1270)]:
        rows+=f'<div class="plate" style="left:43px;top:{y}px;width:994px;height:222px;background:#536f82;background-clip:padding-box"><div class="r" style="left:-5px;top:8px;width:150px;height:150px">{icon(art)}</div><div class="stat r" style="left:164px;top:11px">{title} {value}<br>Nv. 0 / 10</div><div class="button title r" style="right:7px;top:8px;width:350px;height:148px;background-color:#536f82">TURBINAR<br>{18 if title=="ATQ" else 20} OURO</div></div>'
    tabs=''
    for i,(name,art) in enumerate([('OFICINA','repair'),('PEÇAS','gear'),('DRONES','drone')]):
        tabs+=f'<div class="tab" style="left:{42+i*291}px;{ "background-color:#00f5d4;bottom:24px" if i==0 else ""}"><div class="r" style="left:76px;top:-1px;width:87px;height:72px">{icon(art)}</div><span class="title">{name}</span></div>'
    # Esta composição é uma referência de design, não uma captura de execução do jogo.
    substitutions={'FONT_TITLE':FONTS+'/TitanOne-Regular.ttf','FONT_BODY':FONTS+'/DWRounded-ExtraBold.ttf',
      'PLATE':ART+'/plate.png','BUTTON':ART+'/button.png','TREAD':ART+'/tread.png','SHEET':SHEET,
      'PEDESTAL':f'<div class="r" style="left:174px;top:318px;width:610px;height:153px">{icon("pedestal")}</div>',
      'SLOTS':slots,'MODULES':modules,'ROWS':rows,'TABS':tabs,'CLOSE':icon('close'),
      'GEAR':f'<img src="{ART}/gear.png" style="height:48px;vertical-align:middle" alt="Ouro">'}
    for key,value in substitutions.items(): html=html.replace(key,value)
    # Arquivo autossuficiente: renderizar não exige acesso a file:// nem a serviços externos.
    for directory in [ROOT/'UnityProject/Assets/_Game/Resources/Art/Workshop', ROOT/'UnityProject/Assets/_Game/Resources/Fonts/Workshop']:
        for asset in directory.iterdir():
            if asset.suffix not in ('.png','.ttf'):continue
            mime='image/png' if asset.suffix=='.png' else 'font/ttf'
            html=html.replace(asset.as_uri(),'data:'+mime+';base64,'+base64.b64encode(asset.read_bytes()).decode())
    html=html.replace(SHEET,'data:image/png;base64,'+base64.b64encode(Path(SHEET.removeprefix('file://')).read_bytes()).decode())
    out=ROOT/'.tools/review/workshop-0.0.15/index.html'
    out.parent.mkdir(parents=True,exist_ok=True)
    out.write_text(html)
    print(out)
    if '--capture' in sys.argv:
        from playwright.sync_api import sync_playwright
        with sync_playwright() as p:
            browser=p.chromium.launch(executable_path='/usr/lib/chromium/chromium',headless=True,args=['--no-sandbox','--disable-dev-shm-usage'])
            page=browser.new_page(viewport={'width':1080,'height':1920},device_scale_factor=1)
            page.set_content(html,wait_until='load')
            page.evaluate('document.fonts.ready')
            page.screenshot(path=str(out.with_name('preview.png')))
            browser.close()
        print(out.with_name('preview.png'))


if __name__=='__main__':main()
