# Versões do Dig & Wonder

Versões seguem `0.0.patch` durante o protótipo. Cada checkpoint funcional recebe tag Git `v<versão>`; ajustes locais não devem trocar o número até outro marco verificável. O número também fica em `VERSION` e `project.godot`.

## 0.0.1 — Baús e oficina

- Chaves e baús grátis encontrados em marcos de profundidade, sem expiração.
- Catálogo de seis brocas com padrões diferentes e seis equipamentos passivos.
- Seis raridades, chance crescente com profundidade, proteção de sorte e conversão de repetidos em ouro.
- Oficina permite consultar efeitos e equipar achados.
- Save esquema 2 com migração da versão 1.
- Sem compras de baú ou anúncio para chaves/abertura.

Tag: `v0.0.1` (criada junto ao checkpoint Git deste marco).

## 0.0.2 — companheira-broca e coleção ampliada

- HUD separado em Baús, Equipamentos e Personagem.
- 60 equipamentos, dez em cada raridade: Ruim, Boa, Épica, Lendária, Única e Mística.
- Quatro atributos funcionais: poder, moedas por bloco, velocidade de escavação e sorte nos baús; itens podem ter contrapartidas.
- A broca é a personagem, com rosto, seis modelos/padrões de escavação e 13 skins com passivas próprias.
- Save esquema 3 guarda skins e migra os esquemas 1 e 2.
- Harnesses de catálogo, sorte, passivas, menus e migração executados com Godot 4.7.2.

Tag: `v0.0.2`.

## 0.0.3 — fases, minérios e chefes

- Fases finitas de 24 trechos com paleta, terreno e minérios exclusivos definidos em `content/phases/catalog.json`.
- Três regiões regulares em ciclo e Fenda Aurora surpresa (22% de chance ao derrotar um chefe); fase sorteada fica persistida.
- Um chefe diferente por fase bloqueia a descida; tocar e segurar ataca, e derrotá-lo paga moedas e abre a fase seguinte.
- Chaves e baús aparecem em blocos marcados para exploração, com limite por fase (2 de cada em regiões normais, 3 na Fenda), sem exigir coleta para vencer.
- Descobertas exclusivas por ambiente; save esquema 5 migra saves antigos para uma fase próxima da profundidade já alcançada.
- Harnesses verificam limite do terreno, limites de recompensas, combate, transição, sorteio e save.

Tag planejada: `v0.0.3` no checkpoint Git deste marco.
