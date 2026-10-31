using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    public sealed partial class PrototypeGameUI
    {
        private RectTransform topModal, topContent;
        private TextMeshProUGUI topTitle;
        private string topPage;
        private string localPlayerId;

        private void BuildTopMenus()
        {
            topModal = MakePanel("Top menu shade", combatRoot, new Color(.01f, .03f, .05f, .90f), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            RectTransform card = MakeArtPanel("Top menu card", topModal, Color.white, "menu_background", .045f, .16f, .955f, .87f);
            MakeArtPanel("Menu title plaque",card,Color.white,"stage_plate",.06f,.905f,.81f,.985f);
            topTitle = MakeLabel("Top menu title", card, "", 38, Gold, new Vector2(.07f, .91f), new Vector2(.80f, .985f), TextAlignmentOptions.Center);
            MakeArtActionButton("Close top menu", card, "X", 32, .82f, .91f, .97f, .985f, CloseTopMenu, "slot");
            if(startFont!=null)topTitle.font=startFont;
            topTitle.outlineWidth=.10f;topTitle.outlineColor=Ink;
            topContent = Region("Top menu content", card, .075f, .055f, .925f, .87f);
            topModal.gameObject.SetActive(false);
            AudioListener.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("dw.masterVolume", 1f));
            localPlayerId = PlayerPrefs.GetString("dw.localProfileId", "");
            if (string.IsNullOrEmpty(localPlayerId))
            {
                localPlayerId = Guid.NewGuid().ToString("N").Substring(0, 12).ToUpperInvariant();
                PlayerPrefs.SetString("dw.localProfileId", localPlayerId);
                PlayerPrefs.Save();
            }
        }

        private void OpenTopMenu(string page)
        {
            driver.SetMenuPaused(true);
            StopCaveMotion();
            DOTween.Kill(topModal);
            topModal.gameObject.SetActive(true);
            topModal.SetAsLastSibling();
            RectTransform card = (RectTransform)topContent.parent;
            bool fullPage=page.StartsWith("loja") || page=="equipamentos";
            card.anchorMin=new Vector2(fullPage?.015f:.045f,fullPage?.015f:.16f);
            card.anchorMax=new Vector2(fullPage?.985f:.955f,fullPage?.985f:.87f);
            ShowTopPage(page);
            topModal.localScale = new Vector3(.96f, .96f, 1f);
            topModal.DOScale(1f, .18f).SetEase(Ease.OutCubic);
        }

        private void CloseTopMenu()
        {
            DOTween.Kill(topModal);
            topModal.DOScale(.96f,.12f).OnComplete(()=>topModal.gameObject.SetActive(false));
            bool resume = currentTab == "MINA" || currentTab == "MELHORIAS";
            driver.SetMenuPaused(!resume);
            if (resume && driver.Simulation != null && driver.Simulation.IsActive && !driver.IsSaveBlocked) StartCaveMotion();
        }

        private void ClearTopContent()
        {
            for (int i = topContent.childCount - 1; i >= 0; i--)
            {
                GameObject child = topContent.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private void TopButton(string label, float x0, float y0, float x1, float y1, UnityEngine.Events.UnityAction action, bool selected = false)
        {
            RectTransform button = MakeArtActionButton(label, topContent, label, 25, x0, y0, x1, y1, action, "hud_panel");
            button.GetComponent<Image>().color = selected ? new Color(.13f, .44f, .43f) : new Color(.12f,.22f,.32f);
        }

        private TextMeshProUGUI TopText(string text, float x0, float y0, float x1, float y1, int size = 25)
        {
            TextMeshProUGUI label = MakeLabel("Menu detail", topContent, text, size, Cream, new Vector2(x0, y0), new Vector2(x1, y1), TextAlignmentOptions.Left);
            if (readingFont != null) label.font = readingFont;
            return label;
        }

        private void ShowTopPage(string page)
        {
            topPage = page;
            DOTween.Kill(topContent);
            ClearTopContent();
            topContent.localScale = new Vector3(.98f,.98f,1);
            topContent.DOScale(1f,.16f).SetEase(Ease.OutCubic);
            if (page.StartsWith("perfil")) ShowProfilePage(page);
            else if (page.StartsWith("loja")) ShowStorePage(page);
            else if (page == "equipamentos") { topTitle.text = "EQUIPAMENTOS"; TopText("EQUIPAMENTOS\n\nOs espaços de equipamento estão em preparação. Acesse a Oficina para acompanhar sua broca e seus poderes.", .08f, .25f, .92f, .75f, 28); }
            else ShowSettingsPage();
        }

        private void ShowProfilePage(string page)
        {
            topTitle.text = "MEU PERFIL";
            TopButton("INFORMAÇÕES", .01f, .88f, .33f, .98f, () => ShowTopPage("perfil-info"), page == "perfil-info");
            TopButton("PREDEFINIÇÃO", .34f, .88f, .66f, .98f, () => ShowTopPage("perfil-equip"), page == "perfil-equip");
            TopButton("APARÊNCIA", .67f, .88f, .99f, .98f, () => ShowTopPage("perfil-visual"), page == "perfil-visual");
            string name = PlayerPrefs.GetString("dw.playerName", "Explorador");
            if (page == "perfil-info")
            {
                MakeIconSprite(topContent, SpriteFor("menu_avatar"), .03f, .62f, .29f, .84f, Color.white);
                TopText("NOME DO EXPLORADOR", .34f, .78f, .96f, .84f, 21);
                RectTransform inputPanel = MakeArtPanel("Name input", topContent, new Color(.08f,.18f,.26f), "hud_panel", .34f, .68f, .96f, .78f);
                inputPanel.GetComponent<Image>().raycastTarget = true;
                TMP_InputField input = inputPanel.gameObject.AddComponent<TMP_InputField>();
                TextMeshProUGUI inputText = MakeLabel("Name text", inputPanel, name, 28, Cream, new Vector2(.06f, .08f), new Vector2(.94f, .92f), TextAlignmentOptions.Left);
                input.textViewport = inputPanel; input.textComponent = inputText; input.characterLimit = 20; input.text = name;
                input.onEndEdit.AddListener(value => { string clean = value.Trim(); if (clean.Length == 0) clean = "Explorador"; PlayerPrefs.SetString("dw.playerName", clean); PlayerPrefs.Save(); input.text = clean; });
                TopText("ID LOCAL  " + localPlayerId + "\nEste identificador pertence a este aparelho.", .05f, .51f, .95f, .64f, 21);
                TopButton("COPIAR ID", .58f, .43f, .96f, .51f, () => GUIUtility.systemCopyBuffer = localPlayerId);
                var profile = driver.Profile;
                CharacterLevelState level = driver.CurrentCharacterLevel;
                string xp = level.IsMaximum ? "Nível máximo" : level.ExperienceInLevel + " / " + level.ExperienceToNextLevel + " XP";
                string[] names={"EXPEDIÇÃO","BROCA","DESCOBERTAS","FROTA"};
                string[] values={GameplayHudModel.CampaignPosition(profile.CampaignStage),"Nv. "+level.Level+" • "+xp,profile.DiscoveryStages.Count+" / "+DiscoveryCatalog.Entries.Count,profile.Characters.Count+" brocas conquistadas"};
                string[] icons={"nav_modes","attack","nav_album","nav_workshop"};
                for(int i=0;i<4;i++)
                {
                    float y=.33f-i*.09f;
                    RectTransform row=MakeArtPanel("Profile stat "+i,topContent,new Color(.08f,.16f,.24f),"hud_panel",.04f,y,.96f,y+.08f);
                    MakeIcon(row,icons[i],.025f,.16f,.15f,.84f,Color.white);
                    MakeLabel("Profile stat name",row,names[i],19,Gold,new Vector2(.19f,.52f),new Vector2(.96f,.92f),TextAlignmentOptions.Left);
                    MakeLabel("Profile stat value",row,values[i],26,Cream,new Vector2(.19f,.08f),new Vector2(.96f,.54f),TextAlignmentOptions.Left);
                }
            }
            else if (page == "perfil-equip")
            {
                var profile = driver.Profile;
                string drill = driver.EquippedCharacter == null ? "Broca" : driver.EquippedCharacter.DisplayName;
                string powers = "";
                foreach (AttackPowerRule rule in driver.Economy.Powers)
                {
                    int rank = EconomyProgression.PowerRank(profile, profile.EquippedCharacterId, rule.Id);
                    powers += "\n" + rule.Name + "  Nv. " + rank;
                }
                TopText("BROCA EQUIPADA\n" + drill + "  •  Nv. " + driver.CurrentCharacterLevel.Level + "\n\nMELHORIAS\nATQ  Nv. " + profile.AttackLevel + "    VIDA  Nv. " + profile.HealthLevel + "\nCADÊNCIA  Nv. " + profile.AttackSpeedLevel + "    REPARO  Nv. " + profile.RepairLevel + "\n\nPODERES DA BROCA" + powers + "\n\nDESCOBERTAS  " + profile.DiscoveryStages.Count + " / " + DiscoveryCatalog.Entries.Count, .06f, .16f, .94f, .84f, 25);
                TopButton("ABRIR OFICINA", .20f, .04f, .80f, .13f, () => { topModal.gameObject.SetActive(false); OpenProgression(); });
            }
            else
            {
                MakeIconSprite(topContent, FindResourceSprite("Art/Drill/drill_pioneira", "idle_00"), .25f, .46f, .75f, .84f, Color.white);
                TopText("RETRATO PIONEIRA\nO retrato inicial está selecionado. Novas opções de personalização poderão ser desbloqueadas futuramente.", .08f, .17f, .92f, .44f, 26);
            }
        }

        private void ShowStorePage(string page)
        {
            topTitle.text = "EMPÓRIO CRISTALINO";
            string[] tabs = { "OFERTAS", "CRISTAIS", "PACOTES", "BAÚS", "CAIXAS" };
            string[] pages = { "loja-suprimentos", "loja-cristais", "loja-pacotes", "loja-baus", "loja-loot" };
            for(int i=0;i<tabs.Length;i++){ string target=pages[i]; TopButton(tabs[i], .01f+i*.198f,.89f,.197f+i*.198f,.99f,()=>ShowTopPage(target),page==target); }
            TopText("CARTEIRA  " + driver.Profile.Crystals + " cristais  •  " + driver.Profile.Gold + " ouro", .05f,.81f,.95f,.88f,26);
            if(page=="loja-baus" || page=="loja-loot")
            {
                Image chest=MakeIcon(topContent,"product_chest",.18f,.43f,.82f,.79f,Color.white);
                chest.transform.DOScale(1.04f,1.1f).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.InOutSine);
                bool loot=page=="loja-loot";
                TopText(loot?"CAIXA DE SUPRIMENTOS\n"+driver.Economy.GoldPackCrystalCost+" cristais • recompensa aleatória":"SEUS BAÚS  "+driver.Profile.Chests+"   CHAVES  "+driver.Profile.Keys,.08f,.31f,.94f,.43f,26);
                TopButton(loot?"ABRIR COM CRISTAIS":"ABRIR BAÚ",.16f,.17f,.84f,.29f,()=>OpenStoreChest(loot));
                TopButton("VER RECOMPENSAS E CHANCES",.10f,.04f,.90f,.14f,ShowChestOdds);
                return;
            }
            if(page=="loja-suprimentos")
            {
                StoreCard("CHAVE DE BAÚ","Uma chave para seus baús", "nav_chests",driver.Economy.KeyCrystalCost+" cristais",.05f,.41f,.49f,.78f,()=>{bool ok=driver.TryPurchaseKey(); StoreMessage(ok?"Chave recebida!":"Cristais insuficientes ou compra indisponível.");});
                StoreCard("RESERVA DE OURO","Melhore sua expedição", "gold_coin",driver.Economy.GoldPackCrystalCost+" cristais",.51f,.41f,.95f,.78f,()=>{bool ok=driver.TryPurchaseGoldPack();StoreMessage(ok?"Ouro recebido!":"Cristais insuficientes ou compra indisponível.");});
                TopText("Cristais são conquistados nas fases. As trocas usam o saldo da sua expedição.",.08f,.14f,.92f,.34f,25);
                return;
            }
            string[] labels=page=="loja-cristais"?new[]{"BOLSA DE CRISTAIS","CAIXA DE CRISTAIS","COFRE DE CRISTAIS","RESERVA CRISTALINA"}:new[]{"KIT EXPLORADOR","KIT OFICINA","KIT DESCOBERTA","KIT EXPEDIÇÃO"};
            for(int i=0;i<4;i++){float x=.05f+(i%2)*.46f;float y=i<2?.43f:.05f;StoreCard(labels[i],"Prévia de oferta",page=="loja-cristais"?"core":"nav_chests","EM BREVE",x,y,x+.44f,y+.35f,()=>StoreMessage("Esta oferta ainda não está disponível nesta versão."));}
        }

        private void ShowSettingsPage()
        {
            topTitle.text = "CONFIGURAÇÕES";
            int volume = Mathf.RoundToInt(AudioListener.volume * 100f);
            TopText("VOLUME GERAL\n" + volume + "%", .08f, .70f, .55f, .86f, 29);
            TopButton("−", .56f, .72f, .72f, .83f, () => ChangeVolume(-.1f));
            TopButton("+", .77f, .72f, .93f, .83f, () => ChangeVolume(.1f));
            TopText("Sua preferência de volume é salva neste aparelho.", .08f, .60f, .92f, .68f, 23);
            TopButton("MEU PERFIL",.09f,.47f,.91f,.58f,()=>OpenTopMenu("perfil-info"));
            TopButton("COMO JOGAR", .09f, .34f, .91f, .45f, () => { topModal.gameObject.SetActive(false); OpenHelp(); });
            TopText("IDIOMA  Português (Brasil)\n\nVERSÃO  " + Application.version + "\n\nID LOCAL  " + localPlayerId, .08f, .09f, .92f, .30f, 23);
        }

        private void ChangeVolume(float amount)
        {
            AudioListener.volume = Mathf.Clamp01(AudioListener.volume + amount);
            PlayerPrefs.SetFloat("dw.masterVolume", AudioListener.volume);
            PlayerPrefs.Save();
            ShowTopPage("configuracoes");
        }
    }
}
