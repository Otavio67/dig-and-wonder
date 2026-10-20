> **Estado após a mudança de direção (20/10/2026):** o jogo principal agora é defesa com escavação, descrito em `DESENVOLVIMENTO.md`. Os resultados de APK/plataforma abaixo pertencem aos checkpoints 0.0.4/0.0.5 anteriores. Não comprovam exportação nem execução do Marco 1 atual em aparelho. SDK e prova AdMob permanecem isolados; não há anúncio no ciclo de defesa. O perfil atual é esquema 6 com importação confirmada e arquivo separado. Área segura/cancelamento/foco são reaproveitados.

# Revisão Android — versão 0.0.4

Data: 20/10/2026. Escopo: orientações oficiais aplicáveis ao protótipo Godot 4.7.2, Android ARM64, jogo 2D offline. A documentação cobre muitos produtos independentes; a presença de uma API no portal não torna sua integração obrigatória. Este diagnóstico confronta requisitos de plataforma/publicação, recomendações de experiência e ferramentas opcionais com o código e o APK reais.

## Análise e decisões

| Tema e fonte oficial | Resultado no Dig & Wonder |
| --- | --- |
| [API alvo do Google Play](https://developer.android.com/google/play/requirements/target-sdk?hl=pt-br) | A página consultada exige API 36 para novos apps/atualizações móveis desde 31/08/2026. O manifesto exportado já usa alvo 36 e mínimo 24; o verificador agora confere esses valores e a versão do jogo. Mínimo 24 vem do template escolhido, não de uma exigência universal da loja. |
| [Bibliotecas nativas e páginas de 16 KB](https://developer.android.com/guide/practices/page-sizes) | GDScript usa uma engine nativa: não basta verificar só o SDK instalado. `libgodot_android.so` e `libc++_shared.so` têm segmentos ELF alinhados em 16.384 bytes. O processo de exportação passou a conferir todos os PT_LOAD, ABI, integridade ZIP e `zipalign -c -P 16 4`. Execução em sistema 16 KB continua pendente. |
| [Ciclo de vida](https://developer.android.com/guide/components/activities/activity-lifecycle) | Corrigido toque que podia ficar segurado ao perder foco. Pausa/perda de foco cancela toque, ataque e arraste, grava o estado e suspende a lógica do jogo. Retomar exige um novo toque. A persistência acontece durante o jogo, sem depender apenas do fechamento da Activity. |
| [Armazenamento privado e cache](https://developer.android.com/training/data-storage/app-specific) | Mantido `user://` persistente privado do app. Cache pode ser descartado pelo Android, por isso não é o local do progresso. Corrigida a janela em que o save principal era removido antes do rename. Agora há temporários validados, backup substituído por rename e recuperação de temporário válido quando falta o principal. Limpar dados/desinstalar pode apagar o progresso; backup local não significa sincronização em nuvem. |
| [Navegação Voltar](https://developer.android.com/guide/navigation/custom-back) | Voltar/Escape fecha o menu/álbum, depois retorna à tela inicial e, a partir dela, encerra com save. A animação/integração do gesto preditivo da Activity precisa ser verificada no Android real. |
| [Edge-to-edge e insets](https://developer.android.com/develop/ui/views/layout/edge-to-edge) | Adicionada uma superfície que consulta a área segura fornecida pelo Godot, converte pixels físicos para coordenadas do viewport e centraliza o jogo sem deformar. Recalcula em resize/retomada e quando muda a área segura. O input usa a mesma transformação inversa. O conteúdo fica recortado no painel, sem vazar sobre as margens. Validar recorte de câmera, barras transitórias e gestos nos dispositivos. |
| [Jogos em diferentes telas](https://developer.android.com/games/develop/all-screens) | O painel mantém a proporção 540×960 e cabe também em janela estreita ou paisagem. A captura paisagem comprova o encaixe, mas o painel fica pequeno: trata-se de fallback, com otimização para tablets/paisagem ainda pendente. O manifesto já declara categoria game e Activity redimensionável. |
| [Controles e acessibilidade](https://developer.android.com/guide/topics/ui/accessibility/views/apps-views) | O Google recomenda alvos de pelo menos 48×48 dp. Corrigidos botões Vender/Melhorar cujas áreas tocáveis estavam trocadas/deslocadas. Geometria compartilhada entre desenho e toque, controles maiores, quatro equipamentos ou três modelos/skins por página, textos centralizados e descrições quebradas em linhas. Unidades do canvas não equivalem automaticamente a dp: medir o alvo efetivo em telas/densidades distintas. TalkBack, escala de fonte do sistema, foco acessível e revisão completa de contraste continuam pendentes; os controles ainda são desenhados em canvas. |
| [Android vitals](https://developer.android.com/google/play/vitals) e [profiling](https://developer.android.com/games/optimize) | Escavação agrupa gravações por até um segundo, descarrega ao soltar o toque/perder foco e mantém venda, compra, equipamento, baú e combate com gravação imediata. Limite configurado de 60 FPS, efeitos limitados e estilos de painel reutilizados. Isso reduz trabalho redundante por decisão de código; não é uma medição de desempenho. Medir frames, travamentos/ANR, memória, bateria e aquecimento em sessões prolongadas antes de afirmar resultado. |
| [Android App Bundle](https://developer.android.com/guide/app-bundle) | APK debug é adequado para instalação de teste. Publicação exige preparar AAB e assinatura de release/Play App Signing, conta e configuração da loja. Nenhuma chave de produção foi criada ou publicada. Ferramentas de captura e harnesses ficam excluídos do APK. |
| [Segurança dos dados](https://support.google.com/googleplay/android-developer/answer/10787469?hl=pt-br) e [público-alvo](https://support.google.com/googleplay/android-developer/answer/9867159?hl=pt-br) | O APK atual não declara permissões, não integra anúncios/analytics e salva progresso localmente. Para distribuir na loja: declarar público/classificação, política de privacidade e formulário correspondente à faixa de teste/publicação. Reavaliar coleta/compartilhamento ao adicionar qualquer SDK; o fato de o jogo ser cartoon não determina sozinho a idade do público. |
| [Rewarded ads](https://developers.google.com/admob/android/rewarded) e [UMP](https://developers.google.com/admob/android/privacy) | A versão 0.0.5 adiciona uma prova isolada com plugin AdMob Godot v7.0 e IDs oficiais de demonstração. A cena técnica só libera sua contagem pelo callback `rewarded_ad_user_earned_reward`, com no máximo uma recompensa por exibição. Isso comprova integração/exportação estática, não que um anúncio foi servido ou que o callback ocorreu em aparelho. Nenhum anúncio, permissão ou consentimento fictício foi adicionado ao jogo principal. UMP, elegibilidade/consentimento e ciclo real de pausa/retorno ficam pendentes antes de integrar uma oferta. |
| [Compras e itens aleatórios](https://support.google.com/googleplay/android-developer/answer/9858738?hl=pt-br) | Baús/chaves atuais são gratuitos e obtidos jogando. A política consultada exige divulgar chances antes/próximo da compra de itens aleatórios pagos; se houver monetização futura, mostrar probabilidades efetivas considerando sorte/garantias e revisar faturamento/políticas. O painel atual identifica explicitamente suas porcentagens como chances iniciais. |

O [programa Level Up](https://developer.android.com/games/guidelines) é voluntário segundo a página consultada. Suas condições de Vulkan, serviços Play Games, conquistas e cloud save não são automaticamente requisitos para este APK de teste. Preservado renderer Compatibility. ADPF, serviços de conquistas, login, Play Asset Delivery e compras devem entrar quando resolverem uma necessidade demonstrada; não foram adicionados ao protótipo.

Android Studio é útil para APK Analyzer, emuladores, profiler e Logcat. O código de gameplay continua em GDScript: Android Lint de Kotlin/Java não valida essa lógica. Usamos análise/importação do Godot, harnesses da lógica e ferramentas oficiais do SDK para o APK.

## Verificações executadas

- Importação e boot headless com Godot 4.7.2.
- Harnesses de economia, padrões das brocas, conteúdo/raridades, fases/chefes, migrações e save.
- Regressões específicas: tocar nos centros visíveis de Vender/Melhorar; input em painel escalado; segundo dedo; toque cancelado; pausa/foco; autosave periódico e descarregamento; acesso ao décimo equipamento/última skin por paginação; Voltar; encaixe geométrico em formatos diferentes.
- Save principal válido tem prioridade sobre temporário; temporário íntegro recupera uma gravação interrompida; backup válido anterior é mantido em substituições sucessivas. Esses testes simulam estados de arquivos; não simulam falha elétrica nem garantem durabilidade física do armazenamento.
- APK 0.0.4 exportado e assinatura, manifesto, ausência de permissões/harnesses, CRC, ZIP e bibliotecas ARM64/16 KB verificados por `scripts/verify-android-apk.py`.
- Capturas reais do Godot em X11 virtual (Xvfb), OpenGL Compatibility e Mesa llvmpipe, áudio Dummy. Revisados início, escavação, equipamentos, personagem, baús, álbum e fallback paisagem. Corrigidos cortes/centralização de texto, sobreposição do HUD e uso incorreto de `draw_rect` que emitia avisos e não arredondava os painéis. Houve aviso de V-Sync indisponível no driver virtual; essas capturas não são um benchmark Android.
- Versão 0.0.5: APK principal offline e APK de anúncio isolado exportados; assinatura, alvo/API, pacote, permissão somente no APK de anúncio, IDs de demonstração, plugin, ZIP e alinhamento de 16 KB verificados estaticamente. Importação/boot e harnesses repetidos após a inclusão do plugin.

Capturas de referência: [início](revisao-android/01-inicio.png), [escavação](revisao-android/02-escavacao.png), [equipamentos](revisao-android/03-equipamentos.png), [personagem](revisao-android/04-personagem.png), [baús](revisao-android/05-baus.png), [álbum](revisao-android/06-album.png) e [paisagem](revisao-android/07-paisagem.png). São estados locais de revisão, não imagens obtidas em telefone.

## Reproduzir

```bash
bash setup-cloud.sh
bash scripts/setup-android.sh
bash check-project.sh
bash scripts/run-harnesses.sh
bash scripts/export-android-debug.sh
# Prova AdMob separada; exige rede para dependências Gradle:
bash scripts/export-android-ad-test.sh
# Opcional, em Linux com Xvfb instalado:
xvfb-run -a bash scripts/capture-visual.sh
```

O APK fica em `builds/dig-and-wonder-android-debug.apk`; capturas em `builds/review/`. SDKs, chaves locais, saves de teste e APKs são ignorados pelo Git. O workflow GitHub executa importação, boot e harnesses, enquanto a exportação Android requer o SDK.

## Próxima validação no aparelho

1. Instalar o APK ARM64 em Android compatível, jogar/coletar/vender/equipar e vencer um chefe. Confirmar que os centros visuais acionam a ação correta.
2. Testar Home, Voltar por botão e gesto, Recents, bloqueio/desbloqueio, interrupção de foco e encerramento do processo. Confirmar retomada do save e ausência de toque preso. Encerramento abrupto durante escavação pode perder até o intervalo de autosave ainda em memória.
3. Testar navegação por gestos e três botões, recortes diferentes, orientação, multi-janela, tamanhos de tela e fontes; medir dp dos alvos e avaliar acessibilidade com TalkBack/Scanner.
4. Rodar em ambiente 16 KB (`adb shell getconf PAGE_SIZE` deve indicar `16384`) e em aparelho 4 KB. Alinhamento estático não substitui esse teste.
5. Medir sessões de pelo menos 15 minutos: estabilidade dos frames, memória, pausas de gravação com save maior, temperatura/bateria e retorno após processo encerrado.
6. Antes da loja: AAB/release assinado, análise do Play Console, política de privacidade/classificação/público e, quando integrado, anúncio exclusivamente de teste com recompensa confirmada e consentimento adequado.

Não havia dispositivo/emulador Android conectado nesta revisão. Nenhuma instalação Android, anúncio servido, aprovação do Play Console ou resultado de 60 FPS foi comprovado.
