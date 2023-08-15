using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LegendQOL{

    public class UsefulItemPool : TweakBase{

        //ConfigEntry<bool> coopRing;
        public Dictionary<string,List<string>> toRemove = new Dictionary<string, List<string>>(){
          {nameof(RushRunMod),new List<string>(){SkillStoreLocatorItem.staticID,ItemStoreLocatorItem.staticID,MiscRoomLocatorItem.staticID,ExitRoomLocatorItem.staticID,TokenCursed.constID}}
        };

        public override void Init(ConfigFile config){
            base.Init(config);
            //coopRing = config.Bind<bool>(nameof(UsefulItemPool),"CoopItemInSP",false,"Enable to keep coop related items in singleplayer.");
        }
        public override void Activate(){
            if(!active){
              On.LootManager.ResetAvailableItems += ResetPoolHook;
            }
            base.Activate();
        }
        public override void Deactivate(){
            if(active){
              On.LootManager.ResetAvailableItems -= ResetPoolHook;
            }
            base.Deactivate();
        }

        public void ResetPoolHook(On.LootManager.orig_ResetAvailableItems orig){
            orig();
            bool outfitGrade = true;
            foreach(var player in GameController.activePlayers){
               if(player.outfitEnhanceAllowStat.CurrentValue)
                   outfitGrade = false;
            }
            if(outfitGrade){
              LootManager.RemoveIDFromAvailableList("TokenTailor");
            }
            if(RunModifier.Instance){
               foreach(var mod in RunModifier.Instance.runMods.Where((m) => m.Value != null && toRemove.ContainsKey(m.Key))){
                   foreach(var id in toRemove[mod.Key]){
                      LootManager.RemoveIDFromAvailableList(id);
                   }
               }
            }
        }


    }
}
