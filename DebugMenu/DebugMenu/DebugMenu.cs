using System;
using BepInEx;
using BepInEx.Logging;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace DebugMenuPlugin {
    [BepInPlugin("xyz.yekoc.wizardoflegend.DebugMenu", "DebugMenu", "1.1.0")]
    public class DebugMenuPlugin : BaseUnityPlugin {
        internal new static ManualLogSource Logger { get; set; }
	public void Awake() {
            Logger = base.Logger; 
        }
        public void OnDestroy(){
        }
        public void Update(){
          if(Input.GetKeyDown(KeyCode.Backspace)){
             DebugMenu.Instance.Toggle();
          }
        }
    }
}
