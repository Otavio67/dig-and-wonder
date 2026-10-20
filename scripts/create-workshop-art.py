#!/usr/bin/env python3
"""Gera arte original modular de UI. Requer Pillow; não transforma a arte da broca."""
from pathlib import Path
import math
import uuid
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'UnityProject/Assets/_Game/Resources/Art/Workshop'
INK, CREAM, GOLD, CYAN, TEAL = '#101925', '#FFF8E7', '#FFB703', '#00F5D4', '#1E6F5C'
S = 3


def canvas(w=192, h=192):
    im = Image.new('RGBA', (w*S, h*S))
    return im, ImageDraw.Draw(im)


def box(d, xy, fill, radius=16, outline=INK, width=4):
    d.rounded_rectangle(tuple(int(v*S) for v in xy), radius*S, fill, outline, width*S)


def line(d, pts, fill, width=5):
    d.line([(int(x*S), int(y*S)) for x, y in pts], fill, width*S, joint='curve')


def polygon(d, pts, fill, outline=INK, width=4):
    p = [(int(x*S), int(y*S)) for x, y in pts]
    d.polygon(p, fill)
    if outline:
        d.line(p+[p[0]], outline, width*S, joint='curve')


def ellipse(d, xy, fill, outline=INK, width=4):
    d.ellipse(tuple(int(v*S) for v in xy), fill, outline, width*S)


def save(im, name, border=0):
    OUT.mkdir(parents=True, exist_ok=True)
    im.resize((im.width//S, im.height//S), Image.Resampling.LANCZOS).save(OUT/(name+'.png'))
    meta = OUT/(name+'.png.meta')
    if not meta.exists():
        meta.write_text(f'''fileFormatVersion: 2
guid: {uuid.uuid5(uuid.NAMESPACE_URL, 'dig-and-wonder/workshop/'+name).hex}
TextureImporter:
  serializedVersion: 12
  internalIDToNameTable: []
  externalObjects: {{}}
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  textureSettings:
    serializedVersion: 2
    filterMode: 1
    aniso: 1
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  textureType: 8
  textureShape: 1
  spriteMode: 1
  spriteMeshType: 1
  spritePixelsToUnits: 100
  spritePivot: {{x: 0.5, y: 0.5}}
  spriteBorder: {{x: {border}, y: {border}, z: {border}, w: {border}}}
  alphaIsTransparency: 1
  maxTextureSize: 1024
  platformSettings:
  - serializedVersion: 3
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 1024
    textureCompression: 1
    overridden: 0
  spriteSheet:
    serializedVersion: 2
    sprites: []
    outline: []
    physicsShape: []
    bones: []
    spriteID: {uuid.uuid5(uuid.NAMESPACE_URL, name).hex}
    internalID: 0
    vertices: []
    indices:
    edges: []
    weights: []
    secondaryTextures: []
    nameFileIdTable: {{}}
''')


def generate():
    # Painel neutro e botão neutro recebem a cor da paleta pelo Image.color.
    for name, gloss in [('plate', False), ('button', True)]:
        im, d = canvas(128, 128)
        box(d, (2, 9, 126, 126), '#171C27', 22)
        box(d, (2, 2, 126, 117), '#FFFFFF', 22)
        box(d, (10, 10, 118, 108), '#BBC5CA' if not gloss else '#E0E2D6', 16, '#697780', 2)
        line(d, [(22, 17), (106, 17)], '#FFFFFF', 4)
        if not gloss:
            for x, y in [(17, 18), (111, 18), (17, 100), (111, 100)]:
                ellipse(d, (x-4,y-4,x+4,y+4), '#AABCC3', '#394954', 1)
        save(im, name, 26)

    im, d = canvas()
    pts = [(96+85*math.cos(math.pi/3*i-math.pi/6), 96+85*math.sin(math.pi/3*i-math.pi/6)) for i in range(6)]
    polygon(d, pts, '#567E7D', '#101925', 6)
    pts = [(96+70*math.cos(math.pi/3*i-math.pi/6), 96+70*math.sin(math.pi/3*i-math.pi/6)) for i in range(6)]
    polygon(d, pts, TEAL, '#84CAB2', 4)
    save(im, 'slot')

    im, d = canvas()
    gear = []
    for i in range(48):
        a=math.pi*2*i/48
        r=85 if i%4 in (1,2) else 72
        gear.append((96+r*math.cos(a),96+r*math.sin(a)))
    polygon(d, gear, GOLD, INK, 5)
    ellipse(d,(39,39,153,153),'#BA7611','#FFDB6E',5)
    ellipse(d,(50,50,142,142),'#273745',INK,4)
    save(im,'gear')

    im,d=canvas(512,192)
    ellipse(d,(14,38,498,186),'#101925',INK,5)
    ellipse(d,(14,25,498,162),'#C39129',INK,5)
    # Nervuras inclinadas, limitadas ao meio do pedestal.
    for x in range(50,465,50):
        line(d,[(x,139),(x+18,156)],INK,14)
    ellipse(d,(30,9,482,136),'#406376',INK,5)
    ellipse(d,(48,15,464,120),CYAN,'#A6FFF0',5)
    ellipse(d,(69,24,443,106),'#1E6F5C','#24D9CE',3)
    save(im,'pedestal')

    im,d=canvas(128,128)
    box(d,(3,4,125,124),'#5E4A43',18)
    box(d,(12,11,116,113),'#26313E',12,'#9B7C64',3)
    line(d,[(19,40),(109,40)],'#846B57',5)
    line(d,[(19,85),(109,85)],'#846B57',5)
    save(im,'tread',24)

    # Ícones originais com geometria simples e contorno consistente.
    for name in ['drill','chassis','motor','core','shield','repair','bolt','lock','close','attack','health','drone']:
        im,d=canvas()
        if name in ('drill','attack'):
            polygon(d,[(35,49),(61,24),(159,121),(130,158)],'#95CED2',INK,6)
            for off in [0,28,56]: line(d,[(55+off,52+off),(85+off,30+off)],'#386B78',6)
            box(d,(27,126,77,166),GOLD,7)
            line(d,[(53,132),(104,81)],'#FFE6A5',12)
        elif name=='chassis':
            box(d,(18,113,175,165),'#4B3B38',20)
            for x in [46,97,148]: ellipse(d,(x-16,121,x+16,153),GOLD,INK,4)
            box(d,(36,41,158,125),TEAL,23,'#C39330',6)
            box(d,(57,55,140,91),'#152E3B',12)
            for x in [76,118]: ellipse(d,(x-6,62,x+6,82),CYAN,None)
        elif name=='motor':
            box(d,(28,51,157,147),'#C48525',20)
            box(d,(45,38,139,143),GOLD,14)
            box(d,(13,73,61,122),'#8DC3C8',10)
            box(d,(137,71,177,124),'#94C7CA',9)
            for y in [63,87,111]: line(d,[(78,y),(123,y)],INK,7)
        elif name=='core':
            polygon(d,[(65,25),(127,25),(157,83),(96,167),(36,83)],CYAN,INK,6)
            line(d,[(65,25),(80,85),(96,167)],'#A6FFF0',7)
            line(d,[(127,25),(112,85),(96,167)],'#27A59F',7)
            line(d,[(36,83),(157,83)],'#8BFFEE',5)
        elif name=='shield':
            polygon(d,[(96,23),(159,51),(150,117),(96,171),(42,117),(33,51)],'#75BDCF',INK,6)
            polygon(d,[(96,42),(139,61),(130,110),(96,146),(63,110),(54,61)],'#236884','#D1F5E9',4)
        elif name=='repair':
            polygon(d,[(137,24),(158,48),(124,76),(140,94),(174,69),(160,109),(126,116),(54,174),(25,144),(99,81),(96,49)],'#B5D2CD',INK,6)
        elif name=='bolt':
            polygon(d,[(92,18),(152,18),(116,81),(162,81),(64,176),(80,107),(37,107)],GOLD,INK,6)
            line(d,[(103,29),(79,80)],'#FFF8E7',6)
        elif name=='lock':
            box(d,(53,19,139,118),'#AEC4C8',39)
            box(d,(72,40,120,115),'#263644',22,None,0)
            box(d,(37,85,155,174),'#627982',14)
            ellipse(d,(86,110,108,132),INK,None)
            box(d,(92,126,102,148),INK,2,None,0)
        elif name=='close':
            line(d,[(50,50),(142,142)],INK,26)
            line(d,[(142,50),(50,142)],INK,26)
            line(d,[(50,50),(142,142)],CREAM,17)
            line(d,[(142,50),(50,142)],CREAM,17)
        elif name=='health':
            ellipse(d,(27,41,101,116),'#FF595E',INK,5)
            ellipse(d,(91,41,165,116),'#FF595E',INK,5)
            polygon(d,[(29,80),(163,80),(146,127),(96,173),(46,127)],'#FF595E',None)
            line(d,[(29,80),(46,127),(96,173),(146,127),(163,80)],INK,5)
            line(d,[(49,62),(73,57)],'#FFCDC4',7)
        else:
            for x in [30,128]: ellipse(d,(x-15,44,x+50,72),'#8BDBD0',INK,4)
            box(d,(56,69,137,133),GOLD,20)
            ellipse(d,(79,88,116,122),CYAN,INK,4)
            line(d,[(71,133),(50,163),(33,163)],INK,7)
            line(d,[(126,133),(147,163),(164,163)],INK,7)
        save(im,name)


if __name__ == '__main__':
    generate()
    print(f'{len(list(OUT.glob("*.png")))} peças originais em {OUT}')
