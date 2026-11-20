#if UNITY_EDITOR
using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace DigAndWonder.Editor
{
    [InitializeOnLoad]
    public static class UiVisualReview
    {
        const string Key="DW.UIReview.";
        static double next,deadline;
        static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
        static UiVisualReview(){if(SessionState.GetBool(Key+"active",false)) Attach();}
        static void Attach(){next=EditorApplication.timeSinceStartup+2;deadline=next+150;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
        public static void Run()
        {
            string output=Environment.GetEnvironmentVariable("DW_UI_REVIEW_DIR");
            if(string.IsNullOrEmpty(output))throw new Exception("Defina a pasta de saída da revisão.");
            Directory.CreateDirectory(output);
            SessionState.SetBool(Key+"active",true);SessionState.SetInt(Key+"step",0);
            var viewType=typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.GameView");
            var view=EditorWindow.GetWindow(viewType);view.Show();
            try
            {
                var asm=typeof(UnityEditor.Editor).Assembly;
                var sizesType=asm.GetType("UnityEditor.GameViewSizes");
                object sizes=sizesType.BaseType.GetProperty("instance",BindingFlags.Public|BindingFlags.Static|BindingFlags.FlattenHierarchy).GetValue(null);
                var groupType=asm.GetType("UnityEditor.GameViewSizeGroupType");
                object group=sizesType.GetMethod("GetGroup").Invoke(sizes,new[]{Enum.Parse(groupType,"Android")});
                var sizeType=asm.GetType("UnityEditor.GameViewSize");
                var modeType=asm.GetType("UnityEditor.GameViewSizeType");
                object size=Activator.CreateInstance(sizeType,new[]{Enum.Parse(modeType,"FixedResolution"),(object)540,960,"DW portrait review"});
                group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
                int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
                viewType.GetProperty("selectedSizeIndex",BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).SetValue(view,count-1);
            }
            catch(Exception e){Debug.LogWarning("[UIReview] GameView size: "+e.Message);}
            EditorApplication.isPlaying=true;Attach();
        }
        static void Shot(string folder,string file){int s;ScreenCapture.CaptureScreenshot(Path.Combine(folder,file),int.TryParse(Environment.GetEnvironmentVariable("DW_UI_REVIEW_SUPERSIZE"),out s)&&s>1?s:1);}
        static void Call(PrototypeGameUI ui,string name,params object[] args)=>typeof(PrototypeGameUI).GetMethod(name,Private).Invoke(ui,args);
        static void Tick()
        {
            if(EditorApplication.timeSinceStartup>deadline){Finish(1,"Tempo esgotado");return;}
            if(EditorApplication.timeSinceStartup<next)return;
            int step=SessionState.GetInt(Key+"step",0);
            if(step==99){if(!EditorApplication.isPlaying)Finish(0,"Capturas e reinício após 10s verificados");return;}
            if(!EditorApplication.isPlaying)return;
            Application.runInBackground=true;
            EditorApplication.QueuePlayerLoopUpdate();
            var ui=UnityEngine.Object.FindFirstObjectByType<PrototypeGameUI>();
            if(ui==null || !(bool)typeof(PrototypeGameUI).GetField("startupComplete",Private).GetValue(ui))return;
            string folder=Environment.GetEnvironmentVariable("DW_UI_REVIEW_DIR");
            try
            {
                if(step==0){Call(ui,"BeginMining");Call(ui,"OpenTopMenu","perfil-info");}
                else if(step==1) Shot(folder,"perfil.png");
                else if(step==2) Call(ui,"OpenTopMenu","loja-cristais");
                else if(step==3) Shot(folder,"loja.png");
                else if(step==4) Call(ui,"OpenTopMenu","configuracoes");
                else if(step==5) Shot(folder,"configuracoes.png");
                else if(step==6) Call(ui,"CloseTopMenu");
                else if(step==7)
                {
                    Shot(folder,"hud.png");
                    // DW_UI_REVIEW_EXTRA=1 também captura Modos, Equipamentos e as quatro páginas da Oficina.
                    if(Environment.GetEnvironmentVariable("DW_UI_REVIEW_EXTRA")=="1"){SessionState.SetInt(Key+"step",20);next=EditorApplication.timeSinceStartup+1;return;}
                }
                else if(step>=20 && step<=38)
                {
                    var workshop=(CharacterWorkshopView)typeof(PrototypeGameUI).GetField("workshop",Private).GetValue(ui);
                    switch(step)
                    {
                        case 20: Call(ui,"SelectTab","MODOS"); break;
                        case 21: Shot(folder,"modos.png"); break;
                        case 22: Call(ui,"SelectTab","MINA"); Call(ui,"OpenEquipmentPage"); break;
                        case 23: Shot(folder,"equipamentos.png"); break;
                        case 24: Call(ui,"CloseTopMenu"); Call(ui,"OpenProgression"); break;
                        case 25: Shot(folder,"oficina-0.png"); break;
                        case 26: workshop.Show(1); break;
                        case 27: Shot(folder,"oficina-1.png"); break;
                        case 28: workshop.Show(2); break;
                        case 29: Shot(folder,"oficina-2.png"); break;
                        case 30: workshop.Show(3); break;
                        case 31: Shot(folder,"oficina-3.png"); break;
                        case 32: Call(ui,"CloseProgression"); break;
                        case 33: Call(ui,"OpenHelp"); break;
                        case 34: Shot(folder,"ajuda.png"); break;
                        case 35: ((UnityEngine.Component)typeof(PrototypeGameUI).GetField("helpPanel",Private).GetValue(ui)).gameObject.SetActive(false); Call(ui,"OpenAlbum",false); break;
                        case 36: Shot(folder,"album.png"); break;
                        case 37: ((UnityEngine.Component)typeof(PrototypeGameUI).GetField("albumPanel",Private).GetValue(ui)).gameObject.SetActive(false); Call(ui,"SelectTab","MINA"); break;
                        case 38: SessionState.SetInt(Key+"step",8); next=EditorApplication.timeSinceStartup+1; return;
                    }
                }
                else if(step==8)
                {
                    var driver=UnityEngine.Object.FindFirstObjectByType<AutoBattleDriver>();
                    driver.Simulation.Run.IsPlayerDefeated=true;driver.Simulation.Run.PlayerHealth=0;
                    Call(ui,"OpenDefeatPopup");
                }
                else if(step==9){Shot(folder,"derrota.png");next=EditorApplication.timeSinceStartup+11;SessionState.SetInt(Key+"step",10);return;}
                else if(step==10)
                {
                    var driver=UnityEngine.Object.FindFirstObjectByType<AutoBattleDriver>();
                    if(driver.Simulation.Run.IsPlayerDefeated)throw new Exception("Derrota não reiniciou após 10 segundos");
                    foreach(string name in new[]{"perfil","loja","configuracoes","hud","derrota"})if(!File.Exists(Path.Combine(folder,name+".png")))throw new Exception("Captura ausente: "+name);
                    SessionState.SetInt(Key+"step",99);EditorApplication.isPlaying=false;return;
                }
                SessionState.SetInt(Key+"step",step+1);next=EditorApplication.timeSinceStartup+1;
            }
            catch(Exception e){Finish(1,e.ToString());}
        }
                static void Finish(int code,string message)
        {
            EditorApplication.update-=Tick;
            SessionState.SetBool(Key+"active",false);
            Debug.Log("[UIReview] "+message);
#if UNITY_ANDROID
            if(code==0 && Environment.GetEnvironmentVariable("DW_UI_REVIEW_BUILD")=="1"){ApplicationIconSetup.BuildApk();return;}
#endif
            EditorApplication.Exit(code);
        }
    }
}
#endif
