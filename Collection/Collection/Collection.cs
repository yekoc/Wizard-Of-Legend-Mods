using BepInEx;
using BepInEx.Logging;

namespace Collect {
    [BepInDependency("xyz.yekoc.wizardoflegend.LegendAPI",BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin("xyz.yekoc.wizardoflegend.Collection", "COLLECT", "1.0.3")]
    public class Collection : BaseUnityPlugin {
        public static ManualLogSource logger;
        public void Awake() {
         logger = base.Logger;
         NoxOutfit.Register();
         CollectorOutfit.Register();
         CourierOutfit.Register();
         BlackFlame.Register();
         BadApple.Register();
        }

     public static void recursebull(UnityEngine.Transform transform,int acc = 0){
        string log = "";
        for(int i = 0; i<acc;i++){
            log += "-";
        }
        UnityEngine.Debug.Log(log + transform + " : " + transform.gameObject.layer);
        foreach(var comp in transform.gameObject.GetComponents<UnityEngine.Component>()){
           UnityEngine.Debug.Log(log + "-  -" + comp);
        }
        for(int i = 0;i < transform.childCount;i++){
          recursebull(transform.GetChild(i),acc +1);
        }
     }
     
     public static bool PlayerIsWearing(Player p,string outfitID){
        return p && (p.outfitID == outfitID || (p.outfitID == Outfit.normalID && LegendAPI.Outfits.shadowSource == outfitID));
     }
        public void Start(){
         BlueBlood.Register();
         ForgedPapers.Register();
        }
    }

}
