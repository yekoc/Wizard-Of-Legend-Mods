using System;
using BepInEx.Configuration;

namespace LegendQOL{

    public class TweakBase{

        public bool active;
        public ConfigEntry<bool> enabled;

        public virtual void Init(ConfigFile config){
            enabled = config.Bind<bool>(this.GetType().Name,"Enabled",true,"Enables this tweak.");
            enabled.SettingChanged += (conf,val) =>{
               var c = val as SettingChangedEventArgs;
               if((c.ChangedSetting as ConfigEntry<bool>).Value){
                  Activate();
               }
               else{
                  Deactivate();
               }
            };
        }
        public virtual void Activate(){
            active = true;
        }
        public virtual void Deactivate(){
            active = false;
        }
    }
}
