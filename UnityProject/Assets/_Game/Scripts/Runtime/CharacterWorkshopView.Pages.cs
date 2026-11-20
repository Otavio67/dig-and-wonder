using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    public sealed partial class CharacterWorkshopView
    {
        private Image[] arsenalArt;
        private Image[] arsenalLocks;
        private Outline[] arsenalEdges;

        private void ApplyPageLayout(int index)
        {
            string[] showcaseNames={"Character showcase","Character XP track","Combat cartridge"};
            foreach(string name in showcaseNames)
            {
                Transform item=root.Find(name);
                if(item!=null)item.gameObject.SetActive(index==0);
            }
            foreach(RectTransform pane in panes)
            {
                if(pane==null)continue;
                pane.anchorMin=new Vector2(.04f,.218f);
                pane.anchorMax=new Vector2(.96f,index==0?.464f:.86f);
                pane.offsetMin=Vector2.zero;pane.offsetMax=Vector2.zero;
            }
            if(panes[index]!=null)
            {
                DOTween.Kill(panes[index]);
                panes[index].localScale=new Vector3(.97f,.97f,1);
                panes[index].DOScale(1,.18f).SetEase(Ease.OutCubic);
            }
        }

        // Cartões da garagem mostram a própria broca (folha da Pioneira tingida com a cor de identificação).
        // Bloqueadas aparecem como silhueta escura com cadeado; a equipada recebe borda ciano.
        private void BuildArsenalGrid()
        {
            RectTransform pane=panes[3];
            int count=driver.CharacterCatalog.Length;
            arsenalArt=new Image[count];arsenalLocks=new Image[count];arsenalEdges=new Outline[count];
            Sprite drillSprite=null;
            foreach(Sprite sprite in Resources.LoadAll<Sprite>("Art/Drill/drill_pioneira"))
                if(sprite!=null&&sprite.name=="idle_00"){drillSprite=sprite;break;}
            for(int i=0;i<count;i++)
            {
                int index=i;
                CharacterCombatData data=driver.CharacterCatalog[i];
                float x=.02f+(i%4)*.245f;
                float y=.41f-(i/4)*.145f;
                RectTransform card=Plate("Arsenal "+data.Id,pane,new Color(.07f,.15f,.21f,.96f),x,y,x+.23f,y+.135f);
                RectTransform artRegion=Region("Arsenal art",card,.10f,.30f,.90f,.97f);
                Image art=artRegion.gameObject.AddComponent<Image>();
                art.sprite=drillSprite!=null?drillSprite:SpriteFor("hud_drill");
                art.preserveAspect=true;art.raycastTarget=false;
                arsenalArt[i]=art;
                Label(card,data.DisplayName.Replace("Broca ",""),22,Cream,.04f,.03f,.96f,.30f,false);
                arsenalLocks[i]=Icon(card,"lock",.70f,.62f,.97f,.97f);
                Outline edge=card.gameObject.AddComponent<Outline>();
                edge.effectDistance=new Vector2(3f,-3f);
                arsenalEdges[i]=edge;
                card.GetComponent<Image>().raycastTarget=true;
                Button button=card.gameObject.AddComponent<Button>();
                button.targetGraphic=card.GetComponent<Image>();
                button.onClick.AddListener(()=>{garageIndex=index;Refresh();});
            }
            RefreshArsenal();
        }

        private void RefreshArsenal()
        {
            if(arsenalArt==null||driver.Profile==null)return;
            for(int i=0;i<arsenalArt.Length;i++)
            {
                CharacterCombatData data=driver.CharacterCatalog[i];
                bool owned=CharacterProgression.Find(driver.Profile,data.Id)!=null;
                bool equipped=driver.Profile.EquippedCharacterId==data.Id;
                bool selected=driver.CharacterCatalog.Length>0&&garageIndex%driver.CharacterCatalog.Length==i;
                arsenalArt[i].color=owned?CharacterColor(data):new Color(.10f,.16f,.20f,.9f);
                arsenalLocks[i].gameObject.SetActive(!owned);
                arsenalEdges[i].effectColor=equipped?Cyan:selected?Gold:new Color(0,0,0,0);
            }
        }
    }
}
