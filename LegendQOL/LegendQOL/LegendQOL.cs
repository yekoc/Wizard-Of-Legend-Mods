using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;

namespace LegendQOL {
    [BepInPlugin("xyz.yekoc.wizardoflegend.QoL", "Legend QOL", "1.0.0")]
    public class LegendQOL : BaseUnityPlugin {
        internal new static ManualLogSource Logger { get; set; }
        public List<TweakBase> tweakList = new List<TweakBase>();

	public void Awake() {
            Logger = base.Logger;
            tweakList.Add(new OutfitPlus());
            tweakList.Add(new BetterItemDrop());
            tweakList.Add(new UsefulItemPool());
            tweakList.Add(new InformativeGroups());
        }
        public void Start(){
            foreach(var tweak in tweakList){
              tweak.Init(Config);
              if(tweak.enabled.Value){
                  tweak.Activate();
              }
            }
        }
        public void OnDestroy(){
            foreach(var tweak in tweakList){
              tweak.Deactivate();
            }
            tweakList.Clear();
        }
    }
}
