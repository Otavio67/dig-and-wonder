using UnityEngine;

namespace DigAndWonder
{
    public sealed partial class PrototypeGameUI
    {
        private void BuildIllustratedMenuArt()
        {
            foreach(string name in new[]{"menu_background","offer_frame","menu_avatar","defeat_stat_card_v3"})
            {
                Texture2D texture=Resources.Load<Texture2D>("Art/UI/Illustrated/"+name);
                if(texture!=null)uiAssetSprites[name]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);
            }
            AddIllustratedSkin("buttons","skin_tab",.005f,.08f,.49f,.36f);
            AddIllustratedSkin("buttons","skin_tab_active",.505f,.06f,.49f,.38f);
            AddIllustratedSkin("buttons","skin_action",.005f,.55f,.49f,.38f);
            AddIllustratedSkin("buttons","skin_close",.625f,.48f,.25f,.49f,false);
            AddIllustratedSkin("profile_fields","skin_avatar",.095f,0,.305f,.58f,false);
            AddIllustratedSkin("profile_fields","skin_field",.50f,.155f,.49f,.34f);
            AddIllustratedSkin("profile_fields","skin_stat",.005f,.59f,.49f,.35f);
            AddIllustratedSkin("profile_fields","skin_id",.50f,.60f,.49f,.34f);
            Texture2D stats=Resources.Load<Texture2D>("Art/UI/Illustrated/defeat_icons_v2");
            if(stats!=null)
            {
                string[] names={"stat_time","stat_projectile","stat_enemy","stat_waves"};
                int w=stats.width/2,h=stats.height/2;
                for(int i=0;i<4;i++)uiAssetSprites[names[i]]=Sprite.Create(stats,new Rect((i%2)*w,(1-i/2)*h,w,h),new Vector2(.5f,.5f),100);
            }
            Texture2D products=Resources.Load<Texture2D>("Art/UI/Illustrated/products");
            if(products==null)return;
            string[] productNames={"product_pouch","product_chest","product_safe","product_cart"};
            int width=products.width/2,height=products.height/2;
            for(int i=0;i<4;i++)uiAssetSprites[productNames[i]]=Sprite.Create(products,new Rect((i%2)*width,(1-i/2)*height,width,height),new Vector2(.5f,.5f),100);
        }

        private void AddIllustratedSkin(string sheet,string name,float x,float top,float width,float height,bool sliced=true)
        {
            Texture2D texture=Resources.Load<Texture2D>("Art/UI/Illustrated/"+sheet);
            if(texture==null)return;
            Rect rect=new Rect(x*texture.width,(1-top-height)*texture.height,width*texture.width,height*texture.height);
            uiAssetSprites[name]=sliced?Sprite.Create(texture,rect,new Vector2(.5f,.5f),texture.width/10.24f,0,SpriteMeshType.FullRect,new Vector4(rect.width*.07f,rect.height*.08f,rect.width*.07f,rect.height*.08f)):Sprite.Create(texture,rect,new Vector2(.5f,.5f),100);
        }

        private static string ProductArtFor(string title,string fallback)
        {
            if(title=="BOLSA DE CRISTAIS")return "product_pouch";
            if(title=="CAIXA DE CRISTAIS")return "product_chest";
            if(title=="COFRE DE CRISTAIS")return "product_safe";
            if(title=="RESERVA CRISTALINA")return "product_cart";
            if(title.StartsWith("KIT"))return "product_chest";
            return fallback;
        }
    }
}
