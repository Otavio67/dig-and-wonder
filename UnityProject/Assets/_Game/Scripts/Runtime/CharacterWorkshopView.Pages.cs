using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DigAndWonder
{
    public sealed partial class CharacterWorkshopView
    {
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
        private void BuildArsenalGrid()
        {
            RectTransform pane=panes[3];
            for(int i=0;i<driver.CharacterCatalog.Length;i++)
            {
                int index=i;
                CharacterCombatData data=driver.CharacterCatalog[i];
                float x=.02f+(i%4)*.245f;
                float y=.41f-(i/4)*.145f;
                RectTransform card=Plate("Arsenal "+data.Id,pane,new Color(.17f,.29f,.35f),x,y,x+.23f,y+.135f);
                Image icon=Icon(card,"hud_drill",.22f,.30f,.78f,.94f);
                bool owned=CharacterProgression.Find(driver.Profile,data.Id)!=null;
                icon.color=owned?CharacterColor(data):Dim;
                Label(card,data.DisplayName,20,Cream,.04f,.02f,.96f,.30f,false);
                if(!owned)Icon(card,"lock",.73f,.60f,.95f,.95f);
                card.GetComponent<Image>().raycastTarget=true;
                Button button=card.gameObject.AddComponent<Button>();
                button.targetGraphic=card.GetComponent<Image>();
                button.onClick.AddListener(()=>{garageIndex=index;Refresh();});
            }
        }
    }
}
