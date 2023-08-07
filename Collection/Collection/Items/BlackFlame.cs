using System;
using System.IO;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using LegendAPI;

namespace Collect{
public class BlackFlame : Item{
        public static string staticID = "collect::FlameAttackBisect";
        public static Sprite icon;
        public NumVarStatMod kbMod = new NumVarStatMod(staticID,1.5f,10,VarStatModType.LateMultiplicative);
        private AttackInfo phyAtkInf;

        public override string ExtraInfo{
            get{
                return Globals.PercentToStr(0.5f);
            }
        }
        public BlackFlame(){
            ID = staticID;
            category = Category.Offense;
        }
        static BlackFlame(){

         var texture = new Texture2D(2,2);
         texture.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location),"Assets/BlackFlame.png")));
         icon = Sprite.Create(texture,new Rect(0f,0f,texture.height,texture.width),new Vector2(0.5f,0.5f),55); 
         icon.name = staticID + "icon";
         icon.texture.name = staticID + "texture";
         icon.texture.filterMode = FilterMode.Point;

         IL.OverheadNumberPool.SumAllNumbers += (il) => {
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After,x => x.MatchLdfld(typeof(OverheadNumberPool.DisplayQueueObject).GetField("fillColor")),x => x.MatchLdsfld(out _),x => x.MatchLdfld(typeof(OverheadNumberPool.DisplayQueueObject).GetField("fillColor")))){
                c.Index++;
                c.Emit(OpCodes.Ldarg_0);
                c.EmitDelegate<Func<bool,OverheadNumberPool,bool>>( (b,pool) => {
                    if(b){
                     if(OverheadNumberPool.currentNum.fillColor == Color.white){
                       OverheadNumberPool.currentNum.fillColor = OverheadNumberPool.incomingNum.fillColor;
                       OverheadNumberPool.currentNum.icon = OverheadNumberPool.incomingNum.icon;
                       return false;
                     }
                     else if (OverheadNumberPool.incomingNum.fillColor == Color.white){ 
                       OverheadNumberPool.incomingNum.fillColor = OverheadNumberPool.currentNum.fillColor;
                       OverheadNumberPool.incomingNum.icon = OverheadNumberPool.currentNum.icon;
                       return false;
                     }
                    }
                    return b;
                });
            }
         };
        }


        public static void Register(){
         var info = new ItemInfo();
         info.item = new BlackFlame();
         info.text.displayName = "Black Flame";
         info.text.itemID = info.item.ID;
         info.text.description = "Fire Arcana deal part of their damage as physical and have increased knockback";
         info.priceMultiplier = 4;
         info.icon = icon;
         Items.Register(info);
        }

	public override void Activate()
	{
            if(SetParentAsPlayer()){
		SetEventHandlers(givenStatus: true);
                Player.ModifySkills(kbMod,parentPlayer,"knockbackMultiplier",(s) => s.element == ElementType.Fire);
            }
	}

	public override void Deactivate()
	{
            if(SetParentAsPlayer()){
		SetEventHandlers(givenStatus: false);
                Player.ModifySkills(kbMod,parentPlayer,"knockbackMultiplier",(s) => s.element == ElementType.Fire,false);
            }
	}

	private void SetEventHandlers(bool givenStatus)
	{
                Health.globalTakeDamageEnterHandlers = (Health.GlobalOnTakeDamageHandler)Delegate.Remove(Health.globalTakeDamageEnterHandlers, new Health.GlobalOnTakeDamageHandler(HandleBisection));
                if (givenStatus)
                {
                  Health.globalTakeDamageEnterHandlers = (Health.GlobalOnTakeDamageHandler)Delegate.Combine(Health.globalTakeDamageEnterHandlers, new Health.GlobalOnTakeDamageHandler(HandleBisection));
                }
	}

        private void HandleBisection(AttackInfo givenInfo,Entity atkEntity,Entity hurtEntity){
            if((parentPlayer != null) && (hurtEntity != null) && (atkEntity == parentPlayer) && (givenInfo.elementType == ElementType.Fire || givenInfo.subElementType == ElementType.Fire)){
                givenInfo.isCritical = givenInfo.isCritical || (!hurtEntity.health.evadeCrit && (UnityEngine.Random.value < givenInfo.critHitChance));
                if(givenInfo.isCritical){
		    if (Attack.globalCritEventHandlers != null)
		    {
			Attack.globalCritEventHandlers(givenInfo, givenInfo.entity, hurtEntity.health.entityScript);
		    }
		    givenInfo.damage = Mathf.FloorToInt(givenInfo.critDmgModifier * (float)givenInfo.damage);
                }
                givenInfo.damage = givenInfo.damage/2;
                phyAtkInf = new AttackInfo(givenInfo);
                phyAtkInf.elementType = ElementType.Neutral;
                phyAtkInf.subElementType = ElementType.Neutral;
                hurtEntity.health.TakeDamage(phyAtkInf,parentEntity,true);
                hurtEntity.health.currentAtkInfo = givenInfo;
            }
        }
    }
}
