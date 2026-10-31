using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    public sealed partial class PrototypeGameUI
    {
        private Texture2D roundPowerTexture;
        private RectTransform defeatPopup;
        private TextMeshProUGUI defeatCountdown;
        private float defeatRemaining;

        private void BuildRoundPowerSkin()
        {
            const int size=128;
            roundPowerTexture=new Texture2D(size,size,TextureFormat.RGBA32,false);
            var pixels=new Color32[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                double dx=x-63.5,dy=y-63.5,r=Math.Sqrt(dx*dx+dy*dy);
                Color32 c=r>62?new Color32(0,0,0,0):r>56?new Color32(12,24,38,255):r>49?new Color32(180,204,209,255):r>45?new Color32(15,28,42,255):new Color32(255,255,255,255);
                pixels[y*size+x]=c;
            }
            roundPowerTexture.SetPixels32(pixels);roundPowerTexture.Apply();
            Sprite sprite=Sprite.Create(roundPowerTexture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100);
            uiAssetSprites["power_round"]=sprite;
        }

        private void OpenDefeatPopup()
        {
            if(defeatPopup!=null && defeatPopup.gameObject.activeSelf)return;
            if(defeatPopup!=null)Destroy(defeatPopup.gameObject);
            defeatRemaining=10;
            defeatPopup=MakePanel("Defeat popup",combatRoot,new Color(.015f,.025f,.05f,.90f),Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero);
            RectTransform card=MakeArtPanel("Defeat summary",defeatPopup,Color.white,"menu_background",.055f,.20f,.945f,.80f);
            TextMeshProUGUI title=MakeLabel("Defeat title",card,"BROCA AVARIADA!",42,Gold,new Vector2(.09f,.83f),new Vector2(.91f,.94f),TextAlignmentOptions.Center);
            if(startFont!=null)title.font=startFont;title.outlineWidth=.10f;title.outlineColor=Ink;
            MakeIcon(card,"repair",.40f,.71f,.60f,.85f,Color.white);
            var run=driver.Simulation.Run;
            int seconds=(int)run.ElapsedSeconds;
            string[] names={"TEMPO","TIROS","INIMIGOS","ONDAS"};
            string[] values={(seconds/60).ToString("00")+":"+(seconds%60).ToString("00"),run.ShotsFired.ToString(),run.EnemiesDefeated.ToString(),run.WavesCleared.ToString()};
            string[] icons={"cadence","attack","nav_mine","nav_modes"};
            for(int i=0;i<4;i++)
            {
                float x=.09f+(i%2)*.42f,y=i<2?.56f:.43f;
                RectTransform stat=MakeArtPanel("Statistic "+names[i],card,new Color(.11f,.22f,.31f),"hud_panel",x,y,x+.40f,y+.12f);
                MakeIcon(stat,icons[i],.04f,.18f,.25f,.82f,Color.white);
                MakeLabel("Stat name",stat,names[i],20,Gold,new Vector2(.29f,.54f),new Vector2(.95f,.90f),TextAlignmentOptions.Left);
                MakeLabel("Stat value",stat,values[i],36,Cream,new Vector2(.29f,.08f),new Vector2(.95f,.59f),TextAlignmentOptions.Left);
            }
            MakeLabel("Revive question",card,"Deseja voltar de onde parou?",29,Cream,new Vector2(.07f,.32f),new Vector2(.93f,.41f),TextAlignmentOptions.Center);
            RectTransform revive=MakeArtActionButton("Continue with ad",card,"CONTINUAR\nAnúncio em breve",25,.07f,.17f,.49f,.30f,()=>{},"hud_panel");
            revive.GetComponent<Button>().interactable=false;
            revive.GetComponent<Image>().color=new Color(.18f,.23f,.29f);
            MakeArtActionButton("Accept defeat",card,"MORRER\nReiniciar fase",25,.51f,.17f,.93f,.30f,RestartAfterDefeat,"hud_panel");
            defeatCountdown=MakeLabel("Defeat timer",card,"",24,Gold,new Vector2(.08f,.05f),new Vector2(.92f,.15f),TextAlignmentOptions.Center);
            defeatPopup.SetAsLastSibling();
            card.localScale=new Vector3(.90f,.90f,1);
            card.DOScale(1,.22f).SetEase(Ease.OutBack);
        }

        private void UpdateDefeatPopup()
        {
            if(defeatPopup==null || !defeatPopup.gameObject.activeSelf)return;
            if(driver.IsSaveBlocked){defeatCountdown.text="Não foi possível salvar. Seu progresso está preservado.";return;}
            defeatRemaining=Math.Max(0,defeatRemaining-Time.unscaledDeltaTime);
            defeatCountdown.text="Reiniciando em "+Mathf.CeilToInt(defeatRemaining)+"s\nOuro e XP mantidos";
            if(defeatRemaining<=0)RestartAfterDefeat();
        }

        private void RestartAfterDefeat()
        {
            if(driver.IsSaveBlocked)return;
            driver.SetMenuPaused(false);
            if(!driver.TryStartNewGame())return;
            defeatPopup.gameObject.SetActive(false);
            EnterGame(false);
        }
    }
}
