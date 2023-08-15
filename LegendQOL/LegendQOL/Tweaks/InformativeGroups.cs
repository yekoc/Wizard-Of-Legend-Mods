using System;
using System.Linq;
using System.Reflection;
using BepInEx.Configuration;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Mono.Cecil.Cil;

namespace LegendQOL{

    public class InformativeGroups : TweakBase{

        ILHook hook;

        public override void Init(ConfigFile config){
            base.Init(config);
        }
        public override void Activate(){
            if(!active){
              hook = new ILHook(typeof(GroupItem).GetProperty("InfoStr",(BindingFlags)(-1)).GetGetMethod(),GroupInfoHook);
            }
            base.Activate();
        }
        public override void Deactivate(){
            if(active){
              hook.Free();
            }
            base.Deactivate();
        }
        internal void GroupInfoHook(ILContext il){
            ILCursor c = new ILCursor(il);
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<Action<GroupItem>>((self) => self.updateInfoStr = self.itemInven.itemDict.Values.Any((i) => i.ExtraInfo != String.Empty));
            if(c.TryGotoNext(x => x.MatchLdstr("\n"))){
              c.Emit(OpCodes.Ldarg_0);
              c.Emit(OpCodes.Ldloc_0);
              c.EmitDelegate<Func<string,GroupItem,string,string>>((orig,self,itemName) => orig + " ( " + self.itemInven.GetItem(itemName).ExtraInfo + " )");
            }
        }
    }
}
