
using System;
using System.IO;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using LegendAPI;

namespace Collect{
public class BlueBlood : Item{
        public static string staticID = "collect::HealthOrbSignature";
        public static Sprite icon;

        public BlueBlood(){
            ID = staticID;
            category = Category.Misc;
            isCursed = true;
        }
        static BlueBlood(){

         var texture = new Texture2D(2,2);
         texture.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location),"Assets/BlueBlood.png")));
         icon = Sprite.Create(texture,new Rect(0f,0f,texture.height,texture.width),new Vector2(0.5f,0.5f),55); 
         icon.name = staticID + "icon";
         icon.texture.name = staticID + "texture";
         icon.texture.filterMode = FilterMode.Point;
        }


        public static void Register(){
         var info = new ItemInfo();
         info.item = new BlueBlood();
         info.text.displayName = "BlueBlood Pen";
         info.text.itemID = info.item.ID;
         info.text.description = "Health Orb drops spawn Mana Orbs instead.";
         info.icon = icon;
         info.priceMultiplier = 0;
         Items.Register(info);
         IL.LootManager.DropHealth += (il) =>{
             ILCursor c = new ILCursor(il);
             if(c.TryGotoNext(MoveType.After,x => x.MatchLdsfld(out _),x => x.MatchLdcI4((int)ItemSpawner.PoolType.HealthPowerup))){
                 c.EmitDelegate<Func<ItemSpawner.PoolType,ItemSpawner.PoolType>>((h) => Inventory.EitherPlayerHasItem(staticID)?ItemSpawner.PoolType.ManaPowerup:h);
             }
         };
        }

	public override void Activate()
	{
	}

	public override void Deactivate()
	{
	}

    }
}
