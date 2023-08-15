using System;
using BepInEx.Configuration;
using MonoMod.Cil;
using Mono.Cecil.Cil;

namespace LegendQOL{

    public class BetterItemDrop : TweakBase{

        public override void Init(ConfigFile config){
            base.Init(config);
        }
        public override void Activate(){
            if(!active){
              IL.Inventory.DropItem += ItemDropHook;
            }
            base.Activate();
        }
        public override void Deactivate(){
            if(active){
              IL.Inventory.DropItem -= ItemDropHook;
            }
            base.Deactivate();
        }
        internal void ItemDropHook(ILContext il){
            ILCursor c = new ILCursor(il);
            ILLabel lab = c.DefineLabel();
            if(c.TryGotoNext(MoveType.After,x => x.MatchLdfld(typeof(Inventory).GetField("currentItem",(System.Reflection.BindingFlags)(-1))),x => x.MatchBrfalse(out lab))){
                c.MoveAfterLabels();
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<Inventory,bool>>((inv) => inv.currentItem.destroyOnDrop);
                c.Emit(OpCodes.Brtrue,lab);
            }
        }
    }
}
