using System.IO;
using System.Collections.Generic;
using UnityEngine;
using LegendAPI;

namespace Collect{
public class BadApple : Item{
        public static string staticID = "collect::BuffOnProjectileGraze";
        public static Sprite icon;
        public static GameObject prefab;
        public GameObject hitbox;

        public BadApple(){
            ID = staticID;
            category = Category.Defense;
        }
        static BadApple(){
         var texture = new Texture2D(2,2);
         texture.LoadImage(File.ReadAllBytes(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetCallingAssembly().Location),"Assets/BadApple.png")));
         icon = Sprite.Create(texture,new Rect(0f,0f,texture.height,texture.width),new Vector2(0.5f,0.5f),55); 
         icon.name = staticID + "icon";
         icon.texture.name = staticID + "texture";
         icon.texture.filterMode = FilterMode.Point;
         prefab = new GameObject("Grazebox");
         prefab.SetActive(false);
         prefab.AddComponent<BoxCollider2D>().enabled = false;
         prefab.AddComponent<Grazebox>();
         GameObject.DontDestroyOnLoad(prefab);
        }


        public static void Register(){
         var info = new ItemInfo();
         info.item = new BadApple();
         info.text.displayName = "Badly Monochromed Apple";
         info.text.itemID = info.item.ID;
         info.text.description = "Grazing attacks provides signature charge and healing.";
         info.priceMultiplier = 5; 
         info.icon = icon;
         Items.Register(info);
        }

	public override void Activate(){
           if(SetParentAsPlayer()){
            var col = Globals.ChaosInst<BoxCollider2D>(prefab,parentPlayer.playerObjTrans,parentPlayer.hurtBoxTransform.position);
            hitbox = col.gameObject;
            hitbox.SetActive(true);
            col.isTrigger = true;
            col.enabled = true;
            col.size = ((BoxCollider2D)parentPlayer.hurtBoxCollider).size + new Vector2(0.5f,0.5f);
            var graze = hitbox.GetComponent<Grazebox>();
            graze.parentPlayer = parentPlayer;
            hitbox.layer = ProjectileNegateShield.Prefab.transform.GetChild(2).gameObject.layer;
            graze.emitters = Globals.ChaosInst<OverdriveEffects>(OverdriveEffects.Prefab,parentPlayer.playerObjTrans,parentPlayer.hurtBoxTransform.position).overdriveAuraEmitters;
            parentPlayer.hurtBoxTransform.gameObject.AddComponent<GrazeFailBox>().grazebox = graze;
           }
	}

	public override void Deactivate(){
            if(hitbox){
              GameObject.Destroy(hitbox);
              GameObject.Destroy(parentPlayer.hurtBoxTransform.GetComponent<GrazeFailBox>());
            }
	}

        public class Grazebox : MonoBehaviour{
            public Player parentPlayer;
            public Transform parent;
            public List<GameObject> list;
            public ParticleSystem[] emitters;
            public void Start(){
              parent = parentPlayer?.hurtBoxTransform;
              list = new List<GameObject>();
              emitters[0]?.gameObject?.SetActive(true);
              foreach(var ps in emitters){
                 var main = ps.main;
                 main.loop = false;
                 main.duration = Mathf.Min(main.duration,0.5f);
              }
            }
            public void OnTriggerEnter2D(Collider2D proj){
                var projComp = proj.transform?.parent?.GetComponent<Projectile>();
                var attack = projComp ? projComp.attackBox : proj.transform?.GetComponent<Attack>();
                if((projComp || attack) && (attack.atkInfo != null && attack.atkInfo.targetNames != null && (attack.atkInfo.targetNames.Contains(Globals.allyHBStr) || attack.atkInfo.targetNames.Contains(Globals.allyFCStr)))){
                   list.Add(proj.gameObject);
                }
            }
            public void OnTriggerExit2D(Collider2D proj){
                if(proj?.gameObject && list.Contains(proj.gameObject)){
                   parentPlayer.health.RestoreHealth(1);
                   parentPlayer.OverdriveProgress += 5f;
                   list.Remove(proj.gameObject);
                   foreach(var ps in emitters){
                     ps.Play();
                   }
                }
            }
            public void FixedUpdate(){
               if(parent){
                transform.position = parent.position;
                emitters[0].transform.position = transform.position;
                emitters[1].transform.position = transform.position;
               }
            }
        }

        public class GrazeFailBox : MonoBehaviour{
           public Grazebox grazebox;

           public void OnTriggerEnter2D(Collider2D coll){
              grazebox?.list?.Remove(coll.gameObject);
           }
        }
    }
}
