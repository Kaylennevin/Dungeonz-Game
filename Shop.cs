using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class Shop : MonoBehaviour
{

   public GameController gameController;
   public EnemyController enemyController;
   public UpgradeController UpgradeController;
   public UpgradeUI[] upgradeUISlots;
   public GameObject smokeTransition;
   public GameObject shopUI;
   public Slider ShopHealth;
   public Text shopHealthValue, shopSoulsCount;
   public HUD Hud;
   public float consumablePrice = 30f;
   private PlaySound sound;
   


   private void Start()
   {
      sound = GetComponent<PlaySound>();
      ResetQuantitiesPurchased();
   }

   private void Update()
   {
      /*if (Input.GetKeyDown("space"))
      {
         RestockShop();
      }*/
      
      if (Input.GetKeyDown("f"))
      {
         gameController.player.soulCount += 900f;
         UpdateShopSouls();
      }
      

      StartCoroutine(UpdateShopPlayerHealth());
   }

   public void ExitShop()
   {
      sound.PlaySoundOnce(3, 0.3f);
      gameController.player.soulCount = 0f;
      enemyController.endFloorAnim.SetTrigger("endFloor");
      shopUI.SetActive(false);
      smokeTransition.SetActive(true);
      SoundManager.Instance._playerSource.Stop();
      gameController.sound.PlayLoop(0, 0.1f);
      sound.PlaySoundOnce(6, 0.3f);
      enemyController.StartCoroutine(enemyController.AssignExit());

   }

   public void RestockShop()
   {
      RemoveSoldOutUI();
      AssignUpgradesToShop();
      AssignUpgradesToUI();
   }
   

   public void AssignUpgradesToShop()
   {
     
      var tempUpgrades = UpgradeController.upgrades.ToList();

      foreach (var upgrade in tempUpgrades.ToList())
      {
         if (upgrade.quantityPurchased >= upgrade.quantityAvailable)
         {
            tempUpgrades.Remove(upgrade);
         }
      }
      UpgradeController.upgrades = tempUpgrades.ToArray();
      
      if (UpgradeController.upgrades.Length == 3)
      {
         UpgradeController.upgradesInShop[0] = UpgradeController.upgrades[0];
         UpgradeController.upgradesInShop[1] = UpgradeController.upgrades[1];
         UpgradeController.upgradesInShop[2] = UpgradeController.upgrades[2];
      }
      else
      {
         for (int i = 0; i < UpgradeController.upgradesInShop.Length; i++)
         {
            var randomNo = Random.Range(0, tempUpgrades.Count);
            UpgradeController.upgradesInShop[i] = tempUpgrades[randomNo];
            tempUpgrades.Remove(tempUpgrades[randomNo]);
         }
      }
     
   }

   public void AssignUpgradesToUI()
   {
      for (int i = 0; i < UpgradeController.upgradesInShop.Length; i++)
      {
         
         upgradeUISlots[i].description.text = UpgradeController.upgradesInShop[i].description;
         upgradeUISlots[i].price.text = UpgradeController.upgradesInShop[i].price.ToString();
         upgradeUISlots[i].icon.sprite = UpgradeController.upgradesInShop[i].icon;
         upgradeUISlots[i].rarity.sprite = UpgradeController.upgradesInShop[i].rarity;
         upgradeUISlots[i].effect = UpgradeController.upgradesInShop[i].upgradeEffect;
         upgradeUISlots[i].priceValue = UpgradeController.upgradesInShop[i].price;
         upgradeUISlots[i].quantity.text = UpgradeController.upgradesInShop[i].quantityPurchased + " / " +
                                           UpgradeController.upgradesInShop[i].quantityAvailable;
         upgradeUISlots[i].quantityAvailable = UpgradeController.upgradesInShop[i].quantityAvailable;
         upgradeUISlots[i].quantityPurchased = UpgradeController.upgradesInShop[i].quantityPurchased;
         upgradeUISlots[i].rarityAnimator.SetInteger("rarity", UpgradeController.upgradesInShop[i].rarityValue);
         upgradeUISlots[i].upgradeInSlot = UpgradeController.upgradesInShop[i];
        
      }
   }

   public void PurchaseHealthPot()
   {
      sound.PlaySoundOnce(3, 0.3f);

      if (gameController.player.soulCount >= consumablePrice)
      {
         gameController.player.soulCount -= consumablePrice;
         var newHealth = gameController.player.health += 30;
         gameController.player.health = Mathf.Clamp(newHealth, 0, gameController.player.maxHealth);
         UpdateShopSouls();
         sound.PlaySoundOnce(0, 0.5f);
      }
      else
      {
         sound.PlaySoundOnce(7, 0.3f);
      }
      
   }

   public IEnumerator UpdateShopPlayerHealth()
   {
      yield return new WaitForSeconds(.2f);
      ShopHealth.value = gameController.player.health;
      shopHealthValue.text = gameController.player.health + " / " + gameController.player.maxHealth;
      Hud.healthBar.value = gameController.player.health;
      Hud.healthText.text = gameController.player.health + " / " + gameController.player.maxHealth;

   }

   public void UpdateShopSouls()
   {
      shopSoulsCount.text = gameController.player.soulCount.ToString();
   }

   public void ResetQuantitiesPurchased()
   {
      foreach (var upgrade in UpgradeController.upgrades)
      {
         upgrade.quantityPurchased = 0;
      }
   }

   public void RemoveSoldOutUI()
   {
      foreach (var upgradeUISlot in upgradeUISlots)
      {
         upgradeUISlot.soldOutUI.SetActive(false);
      }
   }
   
   
}
