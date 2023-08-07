using System;
using System.Collections.Generic;
using System.Linq;
using LegendAPI;
using UnityEngine;
using Mono.Cecil.Cil;
using MonoMod.Cil;

namespace Collect{
    static class CourierOutfit{
        public static OutfitInfo info = new OutfitInfo();
        internal static List<string> chargeList = new List<string>();

        internal static void Register(){
	    info.name = "Delivery";
            var modlist = new List<OutfitModStat>{new OutfitModStat(LegendAPI.Outfits.CustomModType,0,0,0,false),new OutfitModStat(OutfitModStat.OutfitModType.AllowUpgrade)};
            modlist[1].multiModifier.modValue = 0.25f;
            info.outfit = new Outfit("COLLECT::courier",3,modlist,true,false);
	    info.customMod = (p,b1,b2,mod) => {
		Player.RunState run = (Player.RunState)p.fsm.GetState("Run");
		On.Fall.hook_Update fly = (orig,s) => {
			if(s.entityScript is Player && ((Player)s.entityScript).Equals(p) && p.fsm.currentStateName == "Run" && run.moveSpeedApplied && s.CheckOnPit()){
			 PoolManager.GetPoolItem<AirWalkEmitter>().EmitSingle(new int?(1), new Vector3?(new Vector3((float)s.entityScript.lastTileCol, (float)(-(float)s.entityScript.lastTileRow))), null, null, 0f, null);
			 return;
			}
			orig(s);
		};
                On.Player.RunState.hook_Update unbarrier = (orig,runstate) => {orig(runstate); if(runstate.parent.Equals(p))runstate.parent.preventFallCollider.gameObject.SetActive(!runstate.moveSpeedApplied);};
                On.Player.RunState.hook_OnExit barrier = (orig,runstate) => {orig(runstate); if(run.parent.Equals(p)) runstate.parent.preventFallCollider.gameObject.SetActive(true);};
                On.SpellBookUI.hook_LoadInfoSkills addmoves = (orig,self,skill,elem) =>{
                    orig(self,skill,elem);
                    if(skill == SpellBookUI.SkillEquipType.Dash && self.player == p){
                      foreach(var s in self.player.skillsDict.Values){
                         if(!s.isDash && s.isMovementSkill && !s.isBasic){
                            self.AddSkillToInfoSkills(s,elem);
                         }
                      }
                    }
                  };

                On.SpellBookUI.hook_AssignPlayerSkill asMod = (orig,self,focus) =>{
                    if(self.playerInfoSelectedType == SpellBookUI.SkillEquipType.Dash && self.player == p && self.currentSkill.isMovementSkill){
                      ModifySkill(self.currentSkill,true);
                    } 
                    return orig(self,focus);
                  };

                On.Player.SkillState.hook_Transition dummydash = (orig,self) =>{
                    var res = orig(self);
                    if(!res && self.isDash && !self.cooldownRef.Ready){
                      res = ((DummyDash)self.parent.fsm.states[DummyDash.staticID]).Transition();
                    }
                    return res;
                  };

                On.Player.hook_AssignSkillSlot asUnmod =   (orig,self,slot,skill,setsig,sig) =>{
                    if(self.assignedSkills[slot] != null && self.assignedSkills[slot].isDash && !(self.assignedSkills[slot] is Player.BaseDashState)){
                       ModifySkill(self.assignedSkills[slot],false);
                    }
                    orig(self,slot,skill,setsig,sig);
                  };

		if(b1){
                  if(!p.fsm.states.ContainsKey(DummyDash.staticID))
                    p.fsm.AddState(new DummyDash(p.fsm,p));
		  On.Fall.Update += fly;
                  On.Player.RunState.Update += unbarrier;
                  On.Player.RunState.OnExit += barrier;
                  On.SpellBookUI.LoadInfoSkills += addmoves;
                  On.SpellBookUI.AssignPlayerSkill += asMod;
                  On.Player.SkillState.Transition += dummydash;
                  On.Player.AssignSkillSlot += asUnmod;
                  On.Player.AnnounceSkillChanged += OnSkillChange;
                  On.EnhanceDash.SetEventHandlers += EmpowerDash;
                  IL.Player.RandomizeBuild += RandomBuildCompat;

                  FixSkillsToDash(p);
		}
		else{
		  On.Fall.Update -= fly;
                  On.Player.RunState.Update -= unbarrier;
                  On.Player.RunState.OnExit -= barrier;
                  On.SpellBookUI.LoadInfoSkills -= addmoves;
                  On.SpellBookUI.AssignPlayerSkill -= asMod;
                  On.Player.SkillState.Transition -= dummydash;
                  On.Player.AssignSkillSlot -= asUnmod;
                  On.Player.AnnounceSkillChanged -= OnSkillChange;
                  On.EnhanceDash.SetEventHandlers -= EmpowerDash;
                  IL.Player.RandomizeBuild -=RandomBuildCompat;

                  var slot = PlayerRoomUI.currentUI.spellBooks[p.playerID].sbRef.skillEquipSlots[SpellBookUI.SkillEquipType.Dash];
                  if(PlayerRoomUI.CurrentUIExists && !(p.assignedSkills[slot] is Player.BaseDashState)){
                    ModifySkill(p.assignedSkills[slot],false);
                    p.AssignSkillSlot(slot,Player.DashState.staticID);
                    p.lowerHUD.cooldownUI.RefreshEntries();
                    p.newItemNoticeUI.Display(TextManager.GetSkillName(Player.DashState.staticID), IconManager.GetSkillIcon(Player.DashState.staticID), CooldownUI.GetKeySpriteFromSkillSlot(slot, p.inputDevice), isSkill: true, false, false);
                  }
		}
	    };
	    info.customDesc = (b,m) => {return "- While at full speed,you can run over pits." + '\n' + "- Your dash slot can hold any movement arcana.";};
	    Outfits.Register(info);
            On.SpellBookUI.SetEquipSlots += (orig,self,player) =>{
               orig(self,player);
            };
        }

        internal static void FixSkillsToDash(Player p){
           if(p && p.assignedSkills != null && !p.assignedSkills.Any((s) => s != null && s.isDash)){
             for(int i = 0; i < 6;i++){
                if(p.assignedSkills[i].isMovementSkill){
                  ModifySkill(p.assignedSkills[i],true);
                  break;
                }
             }
           }
        }

        internal static void ModifySkill(Player.SkillState s,bool status){
          if(status){
              s.isDash = true;
              if(!s.isChargeSkill){
                s.InitChargeSkillSettings(1,0f,s.skillData,s);
                chargeList.Add(s.name);
              }
          }
          else{
              s.isDash = false;
              if(chargeList.Contains(s.name) && s.cooldownRef.MaxChargeCount == 1){
                s.isChargeSkill = false;
              }
          }
        }

        internal static void RandomBuildCompat(ILContext il){
            ILCursor c = new ILCursor(il);
            if(c.TryGotoNext(MoveType.After,x => x.MatchLdfld(typeof(Player).GetField(nameof(Player.randSkillsDict))),x => x.MatchLdcI4(1),x => x.MatchCallOrCallvirt(out _))){
              c.Emit(OpCodes.Ldarg_0);
              c.EmitDelegate<Func<List<string>,Player,List<string>>>((list,player) =>{
                if(Collection.PlayerIsWearing(player,info.outfit.outfitID)){
                 list.AddRange(player.skillsDict.Where((k) => !k.Value.isDash && !k.Value.isBasic && k.Value.isMovementSkill).Select(kvp => kvp.Key)); 
                }
                return list;
              });
            }
        }

        internal static void OnSkillChange(On.Player.orig_AnnounceSkillChanged orig,Player self,Player.SkillState skill){
            orig(self,skill);
            if(Collection.PlayerIsWearing(self,info.outfit.outfitID)){
             FixSkillsToDash(self);
            }
        }

        internal static void EmpowerDash(On.EnhanceDash.orig_SetEventHandlers orig,EnhanceDash self,bool status){
            orig(self,status);
            Player.ModifySkills(self.cdMod,self.parentPlayer,"cooldown",(s) => s.isDash && !s.name.Contains("Dash"),status);
        }

        class DummyDash : State<Player>{
            public static string staticID = "CourierNoCooldownDash";
            public DummyDash(FSM fsm,Player parentPlayer) : base(staticID,fsm,parentPlayer){
                    forceEvadeStat = new BoolVarStat(newBaseVal: false);
                    evadeMod = new NumVarStatMod("DashEvade", 1f, 10, VarStatModType.Override);
                    airborneMod = new BoolVarStatMod(staticID, newModValue: true);
                    dashDurationMod = new NumVarStatMod("dashOverride", dashDuration, 10, VarStatModType.OverrideWithMods);
            }

            public bool showEffects = true;

            public bool playSFX = true;

            public bool cooldownReady;

            public bool prevCDWasReady;

            public bool finishedDashing;

            public float dashDuration = 0.125f;

            public NumVarStatMod dashDurationMod;

            public BoolVarStat forceEvadeStat;

            public NumVarStatMod evadeMod;

            public BoolVarStatMod airborneMod;

            public bool Transition()
            {
                    if (!parent.MovementEnabled)
                    {
                            return false;
                    }
                    fsm.ChangeState(staticID, allowSelfTransition: true);
                    return true;
            }


            public override void OnEnter()
            {
                    var dashSkill = parent.skillsDict["Dash"];
                    forceEvadeStat = (dashSkill as Player.BaseDashState).forceEvadeStat;
                    if (parent.dashSlideIgnored)
                    {
                            cooldownReady = prevCDWasReady;
                    }
                    else
                    {
                            prevCDWasReady = cooldownReady;
                    }
                    parent.dashSlideIgnored = false;
                    if (parent.skillEnterEventHandlers != null)
                    {
                            parent.skillEnterEventHandlers(dashSkill);
                    }
                    if (fsm.previousState is Player.SkillState)
                    {
                            var prevSkillState = fsm.previousState as Player.SkillState;
                            if (prevSkillState.isBasic && parent.cancelFromBasicEventHandlers != null)
                            {
                                    parent.cancelFromBasicEventHandlers(dashSkill, prevSkillState);
                            }
                            else if (parent.cancelToDashEventHandlers != null)
                            {
                                    parent.cancelToDashEventHandlers(dashSkill, prevSkillState);
                            }
                    }
                    if (forceEvadeStat.CurrentValue)
                    {
                            parent.health.evadeStat.AddMod(evadeMod);
                    }
                    finishedDashing = false;
                    dashDurationMod.modValue = dashDuration;
                    parent.movement.dashDurationStat.AddMod(dashDurationMod);
                    parent.movement.dashTimer = dashDuration;
                    Vector2 inputVector;
                    if (parent.inputDevice == null)
                    {
                            inputVector = Vector2.zero;
                    }
                    else if (parent.inputDevice.IsMouseDash)
                    {
                            inputVector = parent.GetInputVector();
                    }
                    else
                    {
                            inputVector = parent.GetInputVector(faceInputVector: true, (parent.inputDevice.inputScheme == ChaosInputDevice.InputScheme.Gamepad && ChaosInputDevice.lockControllerAim) ? true : false);
                    }
                    parent.movement.dashVector = inputVector;
                    parent.anim.Play(parent.DashAnimStr);
                    if (showEffects && !cooldownReady)
                    {
                            PoolManager.GetPoolItem<DashAirBurst>().Burst(parent.attackOriginTrans.position, inputVector);
                            parent.dustEmitter.EmitDirBurst(15, Globals.GetRotationVector(-inputVector).z);
                            parent.dashTrails.SetActive(value: true);
                    }
                    if (playSFX)
                    {
                            SoundManager.PlayWithDistAndSPR("StandardDash", parent.transform.position);
                    }
                    parent.TogglePreventFallCollider(status: false);
                    parent.ToggleEnemyFloorCollisions(status: false);
                    parent.airborneStat.AddMod(airborneMod);
                    
            }

            public override void Update()
            {
                    if (finishedDashing)
                    {
                            ExitTransition();
                    }
            }

            public override void FixedUpdate()
            {
                    if (!finishedDashing)
                    {
                            finishedDashing = parent.movement.DashToTarget();
                    }
            }

            public override void OnExit()
            {
                    base.OnExit();
                    parent.movement.dashDurationStat.RemoveMod(dashDurationMod.ID);
                    DashFinished();
                    if (forceEvadeStat.CurrentValue)
                    {
                            parent.health.evadeStat.RemoveMod(evadeMod);
                    }
            }

            public virtual void DashFinished()
            {
                    parent.dashTrails.SetActive(value: false);
                    parent.TogglePreventFallCollider(status: true);
                    parent.airborneStat.RemoveMod(airborneMod);
                    parent.ToggleEnemyFloorCollisions();
                    parent.movement.EndMovement();
            }

            public virtual void ExitTransition()
            {
                    fsm.ChangeState(Player.SlideState.staticID);
            }
        }

    }
}
