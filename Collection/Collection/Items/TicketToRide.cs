
using System;
using System.IO;
using UnityEngine;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using LegendAPI;

namespace Collect{
public class TicketToRide : Item{
        public static string staticID = "collect::RoomShortcut";
        public static Sprite icon;

        private int currentSkillIn = 0;

        public TicketToRide(){
            ID = staticID;
            category = Category.Misc;
        }
        static TicketToRide(){

         var texture = new Texture2D(2,2);
         texture.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location),"Assets/Forged.png")));
         icon = Sprite.Create(texture,new Rect(0f,0f,texture.height,texture.width),new Vector2(0.5f,0.5f),55); 
         icon.name = staticID + "icon";
         icon.texture.name = staticID + "texture";
         icon.texture.filterMode = FilterMode.Point;

         IL.Player.HandleOverdrive += (il) =>{
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(x => x.MatchRet()) && c.TryGotoNext(MoveType.After,x => x.MatchCallOrCallvirt(typeof(Player).GetMethod(nameof(Player.GetSignatureSkill))),x => x.MatchStfld(typeof(Player).GetField(nameof(Player.overdriveSkillRef),(System.Reflection.BindingFlags)(-1))))){
                ILLabel lab = c.MarkLabel();
                if(c.TryGotoNext(MoveType.After,x => x.MatchLdcI4(0),x => x.MatchStfld(typeof(Player).GetField(nameof(Player.overdriveTimedOut),(System.Reflection.BindingFlags)(-1))))){
                    c.MoveAfterLabels();
                    c.Emit(OpCodes.Ldarg_0);
                    c.EmitDelegate<Func<Player,bool>>((player) =>{
                        TicketToRide item = (TicketToRide)player.inventory.GetItem(staticID);
                        if(item == null || !player.overdriveReady || player.OverdriveProgress <= player.overdriveMinValue)
                          return false;
                        Player.SkillState skill = null;
                        for(;item.currentSkillIn<player.assignedSkills.Length;item.currentSkillIn++){
                           skill = player.assignedSkills[item.currentSkillIn];
                           if(skill != null && skill.hasSignatureVariant && !skill.isSignature && skill != player.overdriveSkillRef){
                             player.overdriveSkillRef = skill;
                             return true;
                           }
                        }
                        item.currentSkillIn = 0;
                        player.overdriveSkillRef = null;
                        return false;
                    });
                    c.Emit(OpCodes.Brtrue,lab);
                }
            }
         };
        }


        public static void Register(){
         var info = new ItemInfo();
         info.item = new TicketToRide();
         info.text.displayName = "Permit of Travel";
         info.text.itemID = info.item.ID;
         info.text.description = "Completed rooms can be skipped via portal.";
         info.icon = icon;
         info.tier = 3;
         info.priceMultiplier = 8;
         Items.Register(info);
        }
    }
}
