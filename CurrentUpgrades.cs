using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;


public class CurrentUpgrades : MonoBehaviour
{
   public float soulMultiplier, reviveHealthAmount, floorHealthAmount;
   public float extraSoulChance;
   public float critChance;
   public float critMultiplier = 2f;
   public float critHealAmount;
   public bool critHealActive, fireDashEnabled;
   public float speedIncrease;
   public float speedIncreaseDuration;
   public float baseMoveSpeed;
   public float dashCostReduction;
   public bool fireDash, hasRevive, canRevive = true, canRestoreHealthAfterFloor;
   public bool moveSpeedAfterKill;
   public float killTimer;
   public GameObject fire;
   public ParticleSystem reviveParticle;


   public void ClearShopBuffs()
   {
       soulMultiplier = 0f;
       extraSoulChance = 0f;
       critChance = 0f;
       critHealAmount = 0f;
       fireDashEnabled = false;
       speedIncreaseDuration = 0f;
       dashCostReduction = 0f;
       killTimer = 0f;

   }

   public float CheckCrit(float playerDamage)
   {
       var random = Random.Range(0, 100f);
       var newDamageValue = playerDamage;
       if (critChance > random)
       {
           newDamageValue *= critMultiplier;
           return newDamageValue;
       }

       return newDamageValue;
   }

   

   public IEnumerator KillTimer()
   {
       while (true)
       {
           yield return new WaitForSeconds(1f);
          
           if (killTimer != 0)
           {
               killTimer-=1f;
               
           }
          
       }
       
   }
   
   public void RestoreHealthAfterFloors(GameController gc, Player player)
   {
       if (!canRestoreHealthAfterFloor)
       {
           return;
       }
       
       if (gc.floorNo % 3 == 0)
       {
           var newHealth = Mathf.Clamp(player.health + floorHealthAmount, 0f, player.maxHealth);
           player.health = newHealth;
       }
   }

   public void RevivePlayer(Player player)
   {
       player.health = reviveHealthAmount;
       StartCoroutine(TemporaryInvulnerability(player));
       canRevive = false;
   }
   
   public IEnumerator TemporaryInvulnerability(Player player)
   {
       player.isInvulnerable = true;
       yield return new WaitForSeconds(2f);
       player.isInvulnerable = false;
   }

   public void CreateFireOnDash(Vector3 startPos, Vector3 endPos)
   {
       for (int i = 0; i < 5; i++)
       {
           var newPos = startPos;
           var posNormalized = Vector3.Normalize(endPos-startPos);
           newPos = startPos + (posNormalized * i);
           var firePos = newPos;

           Instantiate(fire, firePos, quaternion.identity);
       }
       
   }

  
   
  
   
}
