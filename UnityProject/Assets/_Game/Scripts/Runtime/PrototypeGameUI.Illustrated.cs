using UnityEngine;

namespace DigAndWonder
{
    public sealed partial class PrototypeGameUI
    {
        private void BuildIllustratedMenuArt()
        {
            foreach(string name in new[]{"menu_background","offer_frame","menu_avatar"})
            {
                Texture2D texture=Resources.Load<Texture2D>("Art/UI/Illustrated/"+name);
                if(texture!=null)uiAssetSprites[name]=Sprite.Create(texture,new Rect(0,0,texture.width,texture.height),new Vector2(.5f,.5f),100);
            }
            Texture2D products=Resources.Load<Texture2D>("Art/UI/Illustrated/products");
            if(products==null)return;
            string[] productNames={"product_pouch","product_chest","product_safe","product_cart"};
            int width=products.width/2,height=products.height/2;
            for(int i=0;i<4;i++)uiAssetSprites[productNames[i]]=Sprite.Create(products,new Rect((i%2)*width,(1-i/2)*height,width,height),new Vector2(.5f,.5f),100);
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
