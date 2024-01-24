using System.Collections.Generic;
using LegendAPI;

namespace Collect{
    static class CollectorOutfit{
        public static OutfitInfo info = new OutfitInfo();
        internal static NumVarStatMod discount;
        internal static NumVarStatMod completeDiscount;
        internal static OutfitModStat speed;

        internal static void Register(){
	    info.name = "Collection";
            speed = new OutfitModStat(OutfitModStat.OutfitModType.Speed,0,0,0,false);
            speed.hasMultiValue = true;
	    speed.isIncrease = true;
            var modlist = new List<OutfitModStat>{speed,new OutfitModStat(LegendAPI.Outfits.CustomModType,0,0,0,false),new OutfitModStat(OutfitModStat.OutfitModType.AllowUpgrade)};
            modlist[1].multiModifier.modValue = 0.25f;
            info.outfit = new Outfit("COLLECT::collector",18,modlist,true,false);
	    info.customMod = (p,b1,b2,mod) => {
		if(b1){
                    ItemStoreItem.spawnEventHandlers += ItemStore;
                    SkillStoreItem.spawnEventHandlers += SkillStore;
                    p.skillChangedEventHandlers += OnSkillChange;
                    On.Inventory.AnnounceItemEvent += OnInventoryChange;
                    OnInventoryChange((a,b,c) =>{},p.inventory,p.inventory.GetRandomItem(false),b1);
		}
                else{
                    ItemStoreItem.spawnEventHandlers -= ItemStore;
                    SkillStoreItem.spawnEventHandlers -= SkillStore;
                    p.skillChangedEventHandlers -= OnSkillChange;
		}
	    };
	    info.customDesc = (b,m) => {return "- Relics and Arcana that can <color=#00FFFF>combine</color> with ones you hold are cheaper.";};
	    Outfits.Register(info);
            discount = new NumVarStatMod("CollectorsDealItemSale",-0.25f,10,VarStatModType.Multiplicative);
            completeDiscount = new NumVarStatMod("CollectorsDealItemSaleToComplete",-0.5f,10,VarStatModType.Multiplicative);
        }

        internal static void OnOutfitChange(Player p,string outfitID){
            
        }

        internal static void ItemStore(ItemStoreItem storeItem){
            if(storeItem && storeItem.Cost != 0 &&  GameController.inGameScene){
                storeItem.costStat.RemoveMod(discount);
                Item item = LootManager.completeItemDict[storeItem.itemID];
                foreach(var player in GameController.activePlayers){
                   if(Collection.PlayerIsWearing(player,info.outfit.outfitID) && player.inventory.CheckItemCombine(item)){
                       Collect.Collection.logger.LogDebug($"Is that a collectors edition {storeItem.itemID}?!?");
                       if(item.isGroupItem && ((player.inventory.GetItem(item.parentGroupID) as GroupItem).Count == GroupItemManager.groupsDict[item.parentGroupID].Count - 1)){
                         storeItem.costStat.AddMod(completeDiscount);
                         storeItem.topText.text = "!!COLLECT!!";
                       }
                       else{
                         storeItem.costStat.AddMod(discount);
                         storeItem.topText.text = "COLLECT";
                       }
                       storeItem.topText.color = new UnityEngine.Color(0,1,1);
                       storeItem.UpdatePrice();
                   }
                }
            }
        }

        internal static void SkillStore(SkillStoreItem item){
            if(item && GameController.inGameScene){
               item.costStat.RemoveMod(discount);
               foreach(var player in GameController.activePlayers){
                  if(Collection.PlayerIsWearing(player,info.outfit.outfitID) && player.HasSkill(Player.UseDragonGrade.staticID) && item.skillID != Player.UseRisingDragon.staticID && (item.skillID == Player.ShootFireArc.staticID || item.skillID.Contains("Dragon"))){
                     item.costStat.AddMod(discount);
                     Collect.Collection.logger.LogDebug($"That {item.skillID} is exactly what I need for my Dragon Deck!");
                     item.topText.text = "COLLECT";
                     item.topText.color = new UnityEngine.Color(0,1,1);
                     item.UpdatePrice();
                  }
               }
            }
        }

        internal static void OnInventoryChange(On.Inventory.orig_AnnounceItemEvent orig,Inventory self,Item item,bool status){
            orig(self,item,status);
            if(self.parentEntity is Player && Collection.PlayerIsWearing((Player)self.parentEntity,info.outfit.outfitID)){
            foreach(var storeItem in ItemStoreItem.currentItems){
              ItemStore(storeItem);
            }
            speed = Outfit.OutfitDict[((Player)self.parentEntity).outfitID].modList[0];
            speed.isIncrease = true;
            speed.multiModifier.modValue = 0;
            foreach(var invItem in self.itemDict){
                if(invItem.Key == DamageUpClearInventory.staticID){
                   speed.multiModifier.modValue += (invItem.Value as DamageUpClearInventory).itemCount;
                }
                else if(invItem.Value.isGroupItem){
                   speed.multiModifier.modValue += (invItem.Value as GroupItem).Count;
                }
                else{
                   speed.multiModifier.modValue++;
                }
            }
            speed.multiModifier.modValue = UnityEngine.Mathf.Min(speed.multiModifier.modValue / 36,0.55f);
            speed.SetModStatus(item.parentPlayer,true);
            }
        }

        internal static void OnSkillChange(Player.SkillState givenSkill){
            if(givenSkill != null && givenSkill.skillID == Player.UseDragonGrade.staticID){
               foreach(var storeItem in SkillStoreItem.currentSkills){
                 SkillStore(storeItem);
               }
            }
        }

    }
}
