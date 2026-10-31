using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    public sealed partial class PrototypeGameUI
    {
        private bool openingStoreChest;
        private void StoreCard(string title,string detail,string icon,string price,float x0,float y0,float x1,float y1,UnityEngine.Events.UnityAction action)
        {
            RectTransform card=MakeArtPanel(title,topContent,Color.white,"offer_frame",x0,y0,x1,y1);
            TextMeshProUGUI heading=MakeLabel("Offer title",card,title,28,Gold,new Vector2(.12f,.70f),new Vector2(.88f,.83f),TextAlignmentOptions.Center);
            if(startFont!=null)heading.font=startFont;heading.outlineWidth=.07f;heading.outlineColor=Ink;

            Image product=MakeIcon(card,ProductArtFor(title,icon),.14f,.32f,.86f,.72f,Color.white);
            product.transform.DOScale(1.035f,1.8f).SetLoops(-1,LoopType.Yoyo).SetEase(Ease.InOutSine);
            MakeLabel("Offer detail",card,detail,22,Cream,new Vector2(.12f,.27f),new Vector2(.88f,.34f),TextAlignmentOptions.Center);
            MakeArtActionButton("Offer action",card,price,25,.14f,.14f,.86f,.26f,action,"hud_panel");
        }
        private void StoreMessage(string message)
        {
            ShowTopPage(topPage);
            TopText(message,.06f,.01f,.96f,.08f,22).color=Gold;
        }
        private void ShowChestOdds()
        {
            ClearTopContent();
            string odds="RECOMPENSAS DA CAIXA\n\n";
            foreach(ChestRewardRule rule in driver.Economy.ChestRewards)
                odds+=rule.Name+"  "+(rule.Weight/100f).ToString("0.##")+"%\nOuro ×"+rule.GoldMultiplier+" • "+rule.Crystals+" cristais\n\n";
            TopText(odds,.08f,.15f,.92f,.94f,25);
            TopButton("VOLTAR",.20f,.03f,.80f,.13f,()=>ShowTopPage(topPage));
        }
        private void OpenStoreChest(bool crystal)
        {
            if(openingStoreChest)return;
            ChestOpening reward;
            bool ok=crystal?driver.TryPurchaseCrystalChest(out reward):driver.TryOpenChest(out reward);
            if(!ok){StoreMessage(crystal?"Cristais insuficientes ou carteira cheia.":"Você precisa de um baú e uma chave.");return;}
            StartCoroutine(RevealStoreChest(reward));
        }
        private IEnumerator RevealStoreChest(ChestOpening reward)
        {
            openingStoreChest=true;
            ClearTopContent();
            Image chest=MakeIcon(topContent,"product_chest",.20f,.30f,.80f,.76f,Color.white);
            TopText("ABRINDO SUPRIMENTOS...",.12f,.15f,.88f,.25f,30);
            chest.transform.DOPunchScale(new Vector3(.22f,.22f,0),.65f,7,.6f);
            yield return new WaitForSecondsRealtime(.7f);
            if(topModal!=null && topModal.gameObject.activeSelf)
            {
                ClearTopContent();
                Image prize=MakeIcon(topContent,"gold_coin",.30f,.43f,.70f,.76f,Color.white);
                prize.transform.DOPunchScale(new Vector3(.18f,.18f,0),.4f,4,.5f);
                TopText(reward.Rarity.ToUpperInvariant()+"\n\n+"+reward.Gold+" OURO\n+"+reward.Crystals+" CRISTAIS",.12f,.18f,.90f,.43f,32).color=Gold;
                TopButton("RECEBIDO",.20f,.04f,.80f,.15f,()=>ShowTopPage(topPage));
            }
            openingStoreChest=false;
        }
        private void OpenEquipmentPage()
        {
            OpenTopMenu("equipamentos");
            ClearTopContent();
            topTitle.text="EQUIPAMENTOS";
            string[] names={"BROCA","CHASSI","MOTOR","NÚCLEO"};
            string[] icons={"attack","shield","bolt","core"};
            for(int i=0;i<4;i++)
            {
                float x=.05f+(i%2)*.46f,y=i<2?.51f:.14f;
                StoreCard(names[i],"Sistema da broca",icons[i],"VER NA OFICINA",x,y,x+.44f,y+.34f,()=>{topModal.gameObject.SetActive(false);OpenProgression();});
            }
            TopText("Os atributos atuais vêm das melhorias e da broca equipada. Troca de peças em preparação.",.07f,.02f,.93f,.12f,22);
        }
    }
}
