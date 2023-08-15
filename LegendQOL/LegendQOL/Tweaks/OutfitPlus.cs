using System;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LegendQOL{

    public class OutfitPlus : TweakBase{

        public override void Init(ConfigFile config){
            base.Init(config);
        }
        public override void Activate(){
            if(!active){
              On.OutfitMenu.LoadMenu += OutfitPlusHook;
            }
            base.Activate();
        }
        public override void Deactivate(){
            if(active){
              On.OutfitMenu.LoadMenu -= OutfitPlusHook;
            }
            base.Deactivate();
        }
        internal void OutfitPlusHook(On.OutfitMenu.orig_LoadMenu orig,OutfitMenu self,Player p){
            orig(self,p);
            self.outfitText.text += p.outfitEnhanced? "+" : "";
        }
    }
}
