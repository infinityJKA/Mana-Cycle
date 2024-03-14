using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FishingBattleManager : MonoBehaviour
{
    public FishingEnemy enemy;
    public FishingInventory player;
    public int enemyMaxHP, enemyHP, playerMaxHP, playerHP;
    public FishingItem[] enemyAttacks, playerAttacks;
    public int enemyLastAttackUsed, playerLastAttackUsed;
    public TextMeshProUGUI nameText,HPText,combatLog;
    public FishingBattleState battleState;
    public Image enemySprite;
    public FishingBattleButton[] buttons;

    void OnEnable()
    {
        battleState = FishingBattleState.Starting;
        InitializeBattle();
        battleState = FishingBattleState.PlayerTurn;
    }

    private void InitializeBattle(){
        // Set stats
        enemyMaxHP = enemy.MaxHP;
        enemyHP = enemyMaxHP;
        playerMaxHP = player.maxHP;
        playerHP = player.maxHP;
        
        enemyAttacks = enemy.attacks;
        playerAttacks = new FishingItem[3];
        playerAttacks[0] = player.weapon1;
        playerAttacks[1] = player.armor1;
        playerAttacks[2] = player.weapon2;

        enemyLastAttackUsed = 99;        
        playerLastAttackUsed = 99;

        // Set visuals
        nameText.text = "vs. " + enemy.enemyName;
        UpdateHPText();

        enemySprite.sprite = enemy.battleSprite;

        for(int i = 0; i < 3; i++){
            buttons[i].weaponImage.sprite = playerAttacks[i].icon;
            buttons[i].used.gameObject.SetActive(false);
        }

        combatLog.text = "";
        UpdateCombatLog("Battle start!");
        UpdateCombatLog(enemy.entryText);
        UpdateCombatLog("<player turn>");

    }

    public void AttackButtonPressed(int attackNum){
        if(battleState == FishingBattleState.PlayerTurn){
            if(attackNum != playerLastAttackUsed){
                // attack
            }
            else{
                // idk play a sound or something
            }
        }
        else if(battleState == FishingBattleState.EnemyWin || battleState == FishingBattleState.PlayerWin){
            // send you out the battle menu
        }
    }


    private void UpdateHPText(){
        HPText.text = "ENEMY HPL\n  "+enemyHP+"/"+enemyMaxHP+"\n\n"+"YOUR HP:"+"\n"+"  "+playerHP+"/"+playerMaxHP;
    }

    private void UpdateCombatLog(string t){
        combatLog.text = t + "\n\n" + combatLog.text; 
    }

}

public enum FishingBattleState{
    Starting,PlayerTurn,EnemyTurn,EnemyWin,PlayerWin
}