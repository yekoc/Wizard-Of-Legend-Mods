using System;
using System.IO;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using LegendAPI;

namespace Collect{
public class OniChain : Item{
        public static string staticID = "collect::LockedInHere";
        public static Sprite icon;
        public bool hooked = false;


        public OniChain(){
            ID = staticID;
            category = Category.Defense;
        }
        static OniChain(){

         var texture = new Texture2D(2,2);
         texture.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location),"Assets/Forged.png")));
         icon = Sprite.Create(texture,new Rect(0f,0f,texture.height,texture.width),new Vector2(0.5f,0.5f),55); 
         icon.name = staticID + "icon";
         icon.texture.name = staticID + "texture";
         icon.texture.filterMode = FilterMode.Point;

        }


        public static void Register(){
         var info = new ItemInfo();
         info.item = new OniChain();
         info.text.displayName = "Oni's Chains";
         info.text.itemID = info.item.ID;
         info.text.description = "While locked into a room,negate the first hit you would have taken.";
         info.icon = icon;
         info.tier = 3;
         info.priceMultiplier = 5;
         Items.Register(info);
        }

        public override void Activate(){
            if(SetParentAsPlayer()){
               parentPlayer.onSurvivalRoomEnterHandlers += OnEnterSurvival;
               SurvivalRoom.onSurvivalRoomClearEventHandlers += OnExitSurvival;
               UpdateItemBar(ItemStatusBar.ItemState.Ready);
               if(!hooked){
                 IL.BossRoomEventHandler.OnTriggerEnter2D += (il) =>{
                   ILCursor c = new ILCursor(il);
                   if(c.TryGotoNext(x => x.MatchLdfld(typeof(BossRoomEventHandler).GetField("triggerWall",(System.Reflection.BindingFlags)(-1))))){
                       c.EmitDelegate<Action>(() =>{
                          foreach(Player p in GameController.activePlayers){
                            if(p && p.inventory != null){
                              (p.inventory.GetItem(staticID) as OniChain)?.OnEnterSurvival(p.transform.position);
                            }
                          }
                       });
                   }
                 };
                 IL.BossRoomEventHandler.Update += BossCleanup;
                 IL.FinalBossRoomEventHandler.Update += BossCleanup;
                 hooked = true;
               }
            }
        }

        public override void Deactivate(){
            if(SetParentAsPlayer()){
               parentPlayer.onSurvivalRoomEnterHandlers -= OnEnterSurvival;
               SurvivalRoom.onSurvivalRoomClearEventHandlers -= OnExitSurvival;
               RemoveFromItemBar();
            }
        }

        public void OnEnterSurvival(Vector3 pos){
            UpdateItemBar(ItemStatusBar.ItemState.Active);
            parentPlayer.health.takeDamageEnterHandlers += OnTakeDamage;
        }

        public void OnExitSurvival(){
            parentPlayer.health.takeDamageEnterHandlers -= OnTakeDamage;
            UpdateItemBar(ItemStatusBar.ItemState.Ready);
        }

        public void OnTakeDamage(AttackInfo attack,Entity attacker){
            if(!parentPlayer.health.GuardNextAtk){
             parentPlayer.health.GuardNextAtk = true;
             UpdateItemBar(ItemStatusBar.ItemState.Disabled);
             parentPlayer.health.takeDamageEnterHandlers -= OnTakeDamage;
            }
        }

        public void BossCleanup(ILContext il){
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After,x => x.MatchStfld(typeof(BossRoomEventHandler).GetField("exitSpawned",(System.Reflection.BindingFlags)(-1))))){
              c.EmitDelegate<Action>(() =>{
              foreach(Player p in GameController.activePlayers){
                if(p && p.inventory != null){
                  (p.inventory.GetItem(staticID) as OniChain)?.OnExitSurvival();
                }
               }
              });
            }
        }

    }
}
