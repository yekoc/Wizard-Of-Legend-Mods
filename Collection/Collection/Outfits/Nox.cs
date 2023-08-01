using System;
using System.Collections.Generic;
using System.Reflection;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using LegendAPI;

namespace Collect{
    static class NoxOutfit{
        public static OutfitInfo info = new OutfitInfo();

        internal static On.Item.hook_IsUnlocked unquestioner = (orig,s,b) => {return orig(s,b) || LootManager.completeItemDict[s].isCursed;};
        internal static On.Inventory.hook_RemoveItem dropper = (orig,self,s,b3,b4) => {if(!b3 && LootManager.completeItemDict[s].isCursed && (self.parentEntity is Player && Collection.PlayerIsWearing((Player)self.parentEntity,"collect::nox"))) b3 = true; return orig(self,s,b3,b4);};
        internal static ILContext.Manipulator giver = (il) => {
                ILCursor c = new ILCursor(il);
                if(c.TryGotoNext(x => x.MatchLdfld(typeof(Item).GetField("isCursed",(BindingFlags)(-1))),x => x.MatchBrfalse(out _))){
                    c.Index++;
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<bool,Player,bool>>((b,p) => b && (p && !Collection.PlayerIsWearing(p,"collect::nox")));
                }
        };
        internal static ILContext.Manipulator bossRush = (il) => {
                ILCursor c = new ILCursor(il);
                int playerIndex = -1;
                if(c.TryGotoNext(x=> x.MatchLdloc(out playerIndex),x=> x.MatchLdfld(typeof(Entity),nameof(Entity.inventory))) && c.TryGotoNext(x => x.MatchLdfld(typeof(Item),nameof(Item.isCursed)),x=> x.MatchBrfalse(out _)) ){
                   c.Emit(OpCodes.Dup);
                   c.Index++;
                   c.Emit(OpCodes.Ldloc,playerIndex);
                   c.EmitDelegate<Func<Item,bool,Player,bool>>((item,cursed,player) => {return cursed && (!Collection.PlayerIsWearing(player,"collect::nox") || item.ID != player.designatedItemID); });
                } 
        };



        internal static void Register(){
	    info.name = "Risk";
	    info.outfit = new Outfit("collect::nox",21,new List<OutfitModStat>{new OutfitModStat(LegendAPI.Outfits.CustomModType,0,0,0,false),new OutfitModStat(OutfitModStat.OutfitModType.AllowUpgrade)},true,false);
	    info.customMod = (p,b1,b2,modifier) => {
		if(b1){
                        On.RelicChestUI.LoadPlayerRelics += NoxHook; 
			On.Item.IsUnlocked += unquestioner; 
			On.Inventory.RemoveItem += dropper;
			IL.RushRunMod.OnLevelWasLoaded += bossRush;
                        IL.Player.GiveDesignatedItem += NoxOutfit.giver;
			LootManager.cursedItemIDList.Remove(BankLoan.staticID);
			Player.outfitEquipActualEventHandlers = (Player.OutfitEquipActualEventHandler)Delegate.Combine(Player.outfitEquipActualEventHandlers, new Player.OutfitEquipActualEventHandler(OnOutfitChange));
		}
                else{
                     On.RelicChestUI.LoadPlayerRelics -= NoxHook;
		     On.Item.IsUnlocked -= unquestioner;
		     On.Inventory.RemoveItem -= dropper;
		     IL.RushRunMod.OnLevelWasLoaded -= bossRush;
                     IL.Player.GiveDesignatedItem -= NoxOutfit.giver;
		     LootManager.cursedItemIDList.Add(BankLoan.staticID);
		     Player.outfitEquipActualEventHandlers = (Player.OutfitEquipActualEventHandler)Delegate.Remove(Player.outfitEquipActualEventHandlers, new Player.OutfitEquipActualEventHandler(OnOutfitChange));
		}
	    };
	    info.customDesc = (b,m) => {return "Allows you to choose <color=#FF0000>cursed</color> relics from Mimi and also drop <color=#FF0000>cursed</color> relics at will!";};
	    Outfits.Register(info);
        }

        internal static void OnOutfitChange(Player p,string outfitID){
             if(Item.IsCursedItem(p.designatedItemID) && outfitID != info.outfit.outfitID  && !RunData.runStarted){
              p.inventory.RemoveItem(p.designatedItemID,true,true);
              p.GiveDesignatedItem("default");
              p.newItemNoticeUI.Display(TextManager.GetItemName(p.designatedItemID), IconManager.GetItemIcon(p.designatedItemID), null, false, false, false, true);
             }
        }

	internal static void NoxHook(On.RelicChestUI.orig_LoadPlayerRelics orig,RelicChestUI self){
	   orig(self);
	   if(!Collection.PlayerIsWearing(self.currentPlayer,"collect::nox"))
		   return;
	   foreach(string item in LootManager.cursedItemIDList){
		Item.Category cat = LootManager.completeItemDict[item].category;
		if(!self.categoryInfoDict[cat].idList.Contains(item)){
		  self.categoryInfoDict[cat].idList.Add(item);
		  self.categoryInfoDict[cat].unlockedCount++;
		}
	   }
	   foreach(string item in LootManager.cursedHubOnlyItemIDList){
		Item.Category cat = LootManager.completeItemDict[item].category;
		if(!self.categoryInfoDict[cat].idList.Contains(item)){
		  self.categoryInfoDict[cat].idList.Add(item);
		  self.categoryInfoDict[cat].unlockedCount++;
		}
	   }
	}
    }
}
