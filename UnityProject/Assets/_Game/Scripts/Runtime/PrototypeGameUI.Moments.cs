using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    /// <summary>
    /// Faixas de momento (chefe à frente, fase concluída, nova broca) sobre o cenário.
    /// Só apresentação: aparecem depois do evento real da simulação e nunca alteram combate, recompensa ou save.
    /// </summary>
    public sealed partial class PrototypeGameUI
    {
        private struct BannerRequest
        {
            public string Title, Subtitle;
            public Color Accent;
            public float Hold;
            public Sprite Art;
            public Color ArtTint;
        }

        private readonly Queue<BannerRequest> bannerQueue = new Queue<BannerRequest>();
        private RectTransform bannerRoot;
        private CanvasGroup bannerGroup;
        private Image bannerArt;
        private Outline bannerEdge;
        private TextMeshProUGUI bannerTitle, bannerSubtitle;
        private bool bannerBusy;
        private int stageGoldEarned;
        private string pendingUnlockName;
        private Color pendingUnlockTint = Color.white;

        private const float BossBarStart = .135f, BossBarWidth = .83f;
        private readonly HashSet<Transform> pulsingTargets = new HashSet<Transform>();

        /// <summary>Reflexo claro no terço superior de um preenchimento de barra; acompanha a largura do pai.</summary>
        private void AddBarShine(RectTransform fill)
        {
            if (fill == null) return;
            MakePanel("Bar shine", fill, new Color(1f, 1f, 1f, .28f), new Vector2(0f, .58f), new Vector2(1f, .88f), Vector2.zero, Vector2.zero)
                .GetComponent<Image>().raycastTarget = false;
        }

        /// <summary>Entrada curta de painel (140–220 ms, guia seção 10).</summary>
        private static void OpenWithPop(RectTransform panel)
        {
            if (panel == null) return;
            panel.DOKill(false);
            panel.localScale = new Vector3(.94f, .94f, 1f);
            panel.DOScale(1f, .2f).SetEase(Ease.OutBack);
        }

        /// <summary>Compra concluída: a linha pulsa e o nível acende em ciano, ancorado ao item comprado.</summary>
        private void PlayPurchaseFeedback(RectTransform row, TextMeshProUGUI level)
        {
            if (row == null) return;
            row.DOKill(true);
            row.localScale = Vector3.one;
            row.DOPunchScale(new Vector3(.03f, .10f, 0f), .3f, 6, .6f);
            if (level == null) return;
            level.color = Cyan;
            level.transform.DOKill(true);
            level.transform.localScale = Vector3.one;
            level.transform.DOPunchScale(new Vector3(.35f, .35f, 0f), .35f, 5, .6f).OnComplete(() => level.color = Cream);
        }

        /// <summary>Ação disponível respira devagar para chamar atenção; para quando deixa de ser possível comprar.</summary>
        private void SetReadyPulse(Transform target, bool ready)
        {
            if (target == null) return;
            if (ready && pulsingTargets.Add(target))
                target.DOScale(1.08f, .6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutSine);
            else if (!ready && pulsingTargets.Remove(target))
            {
                target.DOKill(false);
                target.localScale = Vector3.one;
            }
        }

        private void BuildMomentBanner()
        {
            bannerRoot = Region("Moment banner", worldPanel, .03f, .22f, .97f, .56f);
            bannerGroup = bannerRoot.gameObject.AddComponent<CanvasGroup>();
            bannerGroup.interactable = false;
            bannerGroup.blocksRaycasts = false;
            RectTransform ribbon = MakeArtPanel("Banner ribbon", bannerRoot, new Color(.03f, .07f, .10f, .90f), "hud_panel", 0f, .10f, 1f, .66f);
            bannerEdge = ribbon.gameObject.AddComponent<Outline>();
            bannerEdge.effectDistance = new Vector2(4f, -4f);
            MakeArtPanel("Banner plaque", bannerRoot, Color.white, "stage_plate", .10f, .56f, .90f, 1f);
            bannerTitle = MakeLabel("Banner title", bannerRoot, "", 46, Gold, new Vector2(.17f, .63f), new Vector2(.83f, .94f), TextAlignmentOptions.Center);
            if (startFont != null) bannerTitle.font = startFont;
            bannerTitle.outlineWidth = .14f;
            bannerTitle.outlineColor = Ink;
            bannerArt = MakeIconSprite(bannerRoot, null, .03f, .14f, .25f, .62f, Color.white);
            bannerSubtitle = MakeLabel("Banner subtitle", bannerRoot, "", 30, Cream, new Vector2(.06f, .16f), new Vector2(.94f, .54f), TextAlignmentOptions.Center);
            if (readingFont != null) bannerSubtitle.font = readingFont;
            bannerRoot.gameObject.SetActive(false);
        }

        private void QueueBanner(string title, string subtitle, Color accent, float hold, Sprite art = null, Color? tint = null)
        {
            if (bannerRoot == null) return;
            bannerQueue.Enqueue(new BannerRequest { Title = title, Subtitle = subtitle, Accent = accent, Hold = hold, Art = art, ArtTint = tint ?? Color.white });
            if (!bannerBusy) ShowNextBanner();
        }

        private void ShowNextBanner()
        {
            if (bannerQueue.Count == 0)
            {
                bannerBusy = false;
                if (bannerRoot != null) bannerRoot.gameObject.SetActive(false);
                return;
            }
            bannerBusy = true;
            BannerRequest request = bannerQueue.Dequeue();
            bannerTitle.text = request.Title;
            bannerTitle.color = request.Accent;
            bannerEdge.effectColor = request.Accent;
            bannerSubtitle.text = request.Subtitle;
            bool hasArt = request.Art != null;
            bannerArt.gameObject.SetActive(hasArt);
            bannerArt.sprite = request.Art;
            bannerArt.color = request.ArtTint;
            // Com arte, o subtítulo abre espaço à esquerda para a broca liberada.
            bannerSubtitle.rectTransform.anchorMin = new Vector2(hasArt ? .27f : .06f, .16f);
            bannerRoot.gameObject.SetActive(true);
            bannerRoot.SetAsLastSibling();
            bannerRoot.DOKill(false);
            DOTween.Kill(bannerGroup);
            bannerRoot.localScale = new Vector3(.72f, .72f, 1f);
            bannerGroup.alpha = 0f;
            bannerRoot.DOScale(1f, .28f).SetEase(Ease.OutBack);
            bannerGroup.DOFade(1f, .16f);
            bannerGroup.DOFade(0f, .24f).SetDelay(.28f + request.Hold).OnComplete(ShowNextBanner);
        }

        private void ClearBanners()
        {
            bannerQueue.Clear();
            bannerBusy = false;
            pendingUnlockName = null;
            if (bannerRoot == null) return;
            bannerRoot.DOKill(false);
            DOTween.Kill(bannerGroup);
            bannerRoot.gameObject.SetActive(false);
        }

        /// <summary>Recebe os eventos reais da simulação e decide qual faixa mostrar.</summary>
        private void HandleMomentEvent(CombatEvent e)
        {
            switch (e.Type)
            {
                case CombatEventType.RunStarted:
                    stageGoldEarned = 0;
                    break;
                case CombatEventType.GoldAwarded:
                    stageGoldEarned += Mathf.CeilToInt(e.Amount);
                    break;
                case CombatEventType.BossSpawned:
                    bool mini = driver.Simulation.Run.EncounterVersion >= 1 && driver.Simulation.Run.CampaignStage % 10 == 5;
                    QueueBanner(mini ? "MINI-CHEFE!" : "GUARDIÃO!", mini ? "Vença o mini-chefe e a escolta" : "Derrote o guardião para avançar",
                        Coral, 1.2f, SpriteFor("critical_damage"));
                    break;
                case CombatEventType.CharacterUnlocked:
                    CharacterCombatData reward = System.Array.Find(driver.CharacterCatalog, item => item.Id == e.Text);
                    pendingUnlockName = reward == null ? e.Text : reward.DisplayName;
                    pendingUnlockTint = reward == null ? Color.white : new Color(reward.TintR, reward.TintG, reward.TintB);
                    break;
                case CombatEventType.StageCleared:
                    string next = string.IsNullOrEmpty(e.Text) ? "" : "  •  Próxima: " + e.Text;
                    QueueBanner("FASE CONCLUÍDA!", (stageGoldEarned > 0 ? "+" + GameplayHudModel.Number(stageGoldEarned) + " ouro" : "Vitória") + next, Gold, 1.5f, SpriteFor("coin"));
                    if (!string.IsNullOrEmpty(pendingUnlockName))
                    {
                        QueueBanner("NOVA BROCA!", pendingUnlockName + " liberada. Equipe na Oficina → Brocas.", Cyan, 2.4f,
                            FindResourceSprite("Art/Drill/drill_pioneira", "idle_00"), pendingUnlockTint);
                        pendingUnlockName = null;
                    }
                    break;
                case CombatEventType.PlayerDefeated:
                    ClearBanners();
                    break;
            }
        }
    }
}
