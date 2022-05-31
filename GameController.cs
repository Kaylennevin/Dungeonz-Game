using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public class GameController : MonoBehaviour
{

   public GameData data;

   public float floorNo = 1f, skillSoulsThisRun;
   public Animator smokeAnim, deathAnim, screenAnim, healthBarAnim;
   public GameObject shop, deathScreen, soulTree, howToPlay;
   public EnemyController enemyController;
   public Player player;
   public Text dungeonSouls, skillSouls, floorsCleared, skillSoulsDeath, skillSoulsTree;
   public GameObject square, continueScreen;
   public Tilemap floorTiles;
   public CorridorDungeonGenerator dungeonGenerator;
   public ExitScript exitScript;
   public float elapsedTime, prevSkillSoulCount, newSkillSoulCount;
   public bool updatingSouls, addedNewSoulAmount;
   public Soul[] soulsOnFloor;
   public PlaySound sound;
   public List<GameObject> exits;
   public Shop shopRef;

   private void Awake()
   {
      sound = GetComponent<PlaySound>();
      sound.PlayLoop(1, 0.2f);
   }

   private void Update()
   {
      Timer();
      UpdateSkillSoulsAmount();
   }

   public void IncrementFloorNo()
   {
      floorNo++;
   }

   public IEnumerator SpawnSouls(GameObject soul, Entity entity, float soulCount, float soulChance,
      float soulMultiplier)
   {
      var entityPos = entity.transform.position;
      var soulSpawned = soul;
      var extraSoulChance = math.floor(Random.Range(0, 100));
      var soulsToDrop = math.floor(soulCount / 4) ;

      if (extraSoulChance <= soulChance)
      {
         soulsToDrop *= soulMultiplier;
      }
      else
      {
         soulsToDrop = soulCount;
      }

      for (int i = 0; i < soulsToDrop; i++)
      {

         Instantiate(soulSpawned, soulSpawned.GetComponent<Soul>().GetRandomLocationNearPoint(entityPos),
            Quaternion.identity);
         yield return new WaitForSeconds(.07f);
      }


   }

   public void Timer()
   {
      elapsedTime += Time.unscaledDeltaTime;
   }

   public void ExitFloorAnim()
   {
      ClearSoulsOnFloor();
      shop.SetActive(true);
      smokeAnim.SetTrigger("exitFloor");
      
   }

   public void ResetShopAnimTrigger()
   {
      smokeAnim.ResetTrigger("exitFloor");
   }

   public void EndDungeonRun()
   {
      deathScreen.SetActive(true);
      DisplayStats();
      player.currentUpgrades.ClearShopBuffs();
      shopRef.ResetQuantitiesPurchased();
      deathAnim.SetTrigger("death");
      Time.timeScale = 0;
      data.dungeonSouls = 0f;
   }

   public void DisplayStats()
   {
      dungeonSouls.text = data.dungeonSouls.ToString();
      floorsCleared.text = data.floorsCleared.ToString();
      skillSouls.text = CalculateSkillSouls().ToString();
      skillSoulsThisRun = CalculateSkillSouls();
      skillSoulsDeath.text = data.skillSouls.ToString();
      SetSoulCount();
      AddSkillSoulsToData();

   }

   public float CalculateSkillSouls()
   {
      var soulsCount = math.floor(data.dungeonSouls / 6) ;
      var floorBonus = data.floorsCleared * 9;
      math.clamp(soulsCount, 5, soulsCount);
      return soulsCount + floorBonus;

   }

   public void SetSoulCount()
   {
      prevSkillSoulCount = data.skillSouls;
      newSkillSoulCount = math.floor(data.skillSouls + skillSoulsThisRun);
   }
   
   
   public void UpdateSkillSoulsAmount()
   {
      if (!updatingSouls) return;
      
      var duration = 3f;
      if (elapsedTime <= duration)
      {
         var percentageComplete = elapsedTime / duration;
         skillSoulsDeath.text  = math.ceil(math.lerp(prevSkillSoulCount,
            newSkillSoulCount, percentageComplete)).ToString();
      }
      else
      {
         updatingSouls = false;
         
      }

   }

   public void AddSkillSoulsToData()
   {
     data.skillSouls = math.floor(data.skillSouls + skillSoulsThisRun);
   }
   
   public void ClearPostGameStats()
   {
      data.dungeonSouls = 0;
      data.floorsCleared = 0;
   }

   public void OpenSoulTree()
   {
      soulTree.SetActive(true);
      skillSoulsTree.text = data.skillSouls.ToString();
      ClearPostGameStats();
      deathScreen.SetActive(false);
   }
   
   public void CloseSoulTree()
   {
      SetPlayerBonuses();
      continueScreen.SetActive(true);
      soulTree.SetActive(false);
   }

   public void SetPlayerBonuses()
   {
      player.health = player.maxHealth;
      player.stamina = player.maxStam;
   }

   public void OpenCrypt()
   {
      sound.PlaySoundOnce(3, 0.3f);
      smokeAnim.SetTrigger("enter");
      
      player.health = player.maxHealth;
      player.isDead = false;
      Time.timeScale = 0;
      
      exitScript.MovePlayerToStart();
      enemyController.ClearEnemies();
      enemyController.ClearObstacles();
      ClearSoulsOnFloor();
      ClearAllExits();
      floorNo = 1f;
      dungeonGenerator.GenerateDungeon();
      StartCoroutine(SetTime());
      continueScreen.SetActive(false);
      StartCoroutine(enemyController.AssignExit());
      SoundManager.Instance._playerSource.Stop();
      sound.PlayLoop(0, 0.1f);

   }

   public IEnumerator SetTime()
   {
      yield return new WaitForSecondsRealtime(1f);
      Time.timeScale = 1;
   }

   public void ClearSoulsOnFloor()
   {
      soulsOnFloor = FindObjectsOfType<Soul>();
      
      foreach (var soul in soulsOnFloor)
      {
         Destroy(soul.gameObject);
      }
      Array.Clear(soulsOnFloor, 0, soulsOnFloor.Length);
      
   }

   public void OpenHowToPlayWindow()
   {
      howToPlay.SetActive(true);
   }
   
   public void CloseHowToPlayWindow()
   {
      howToPlay.SetActive(false);
   }

   public void CloseGame()
   {
      Application.Quit();
   }


   public void ClearAllExits()
   {
      if (exits.Count != 0)
      {
         foreach (var exit in exits)
         {
            Destroy(exit);
         }
         exits.Clear();
      }
      
   }

   public void ScreenShake()
   {
      healthBarAnim.SetTrigger("playerHit");
      screenAnim.SetTrigger("playerHit");
   }
   

}
