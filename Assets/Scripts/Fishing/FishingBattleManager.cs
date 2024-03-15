using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
// using UnityEditor.Localization.Plugins.XLIFF.V12;
// using UnityEngine.Localization.PropertyVariants.TrackedProperties;

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
    public bool firstHeal,firstEnemyHeal,firstAttack,firstEnemyAttack,enemyHalfTriggered,playerHalfTriggered;

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

        // Reset wacky dialogue variables
        firstHeal = firstEnemyHeal = firstAttack = firstEnemyAttack = enemyHalfTriggered = playerHalfTriggered = false;

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
        UpdateCombatLog("[Player turn]");

    }

    public void AttackButtonPressed(int attackNum){
        if(battleState == FishingBattleState.PlayerTurn){
            if(attackNum != playerLastAttackUsed){
                battleState = FishingBattleState.EnemyTurn;
                PlayerAction(attackNum);
            }
            else{
                // idk play a sound or something
            }
        }
        else if(battleState == FishingBattleState.EnemyWin || battleState == FishingBattleState.PlayerWin){
            // send you out the battle menu
        }
    }

    private void PlayerAction(int i){
        // If it is an attack
        if(IsAttack(playerAttacks[i])){
            SuperEffective e;
            int d;
            if(playerAttacks[i] is FishingArmor){
                FishingArmor a = playerAttacks[i] as FishingArmor; 
                e = CheckEffectiveness(a.element,enemy.element);
                d = a.ATK;
            }
            else{
                FishingWeapon a = playerAttacks[i] as FishingWeapon; 
                e = CheckEffectiveness(a.element,enemy.element);
                d = a.ATK;
            }
            int c = CalculateDamage(d, e);
            enemyHP -= c;
            if(e == SuperEffective.Neutral){
                UpdateCombatLog("You used " + playerAttacks[i].itemName + " and dealt " + c + " damage!" );
            } else if(e == SuperEffective.Effective){
                UpdateCombatLog("SUPER EFFECTIVE! You used " + playerAttacks[i].itemName + " and dealt " + c + " damage!" );
            } else{UpdateCombatLog("You used " + playerAttacks[i].itemName + ", but it wasn't very effective... dealt " + c + " damage.");};
        }
        
        // If it is a healing move
        else{
            int h;
            if(playerAttacks[i] is FishingArmor){
                FishingArmor a = playerAttacks[i] as FishingArmor; 
                h = a.ATK;
            }
            else{
                FishingWeapon a = playerAttacks[i] as FishingWeapon; 
                h = a.ATK;
            }
            int old = playerHP;
            playerHP += h;
            if(playerHP > playerMaxHP){
                playerHP = playerMaxHP;
                h = playerMaxHP - old;
            }
            UpdateCombatLog("You used "+ playerAttacks[i].itemName +" and recovered "+h+" HP! ("+old+" >> "+playerHP+")");
        }

        UpdateHPText();

        // Update the used move as unusable next turn
        playerLastAttackUsed = i;
        for(int x = 0; x < 3; x++){
            buttons[x].used.gameObject.SetActive(x==i);
        }

        // Check if you won, or if its now the enemy turn
        if(enemyHP <= 0){
            PlayerWins();
        }
        else{
            battleState = FishingBattleState.EnemyTurn;
            int c = ChooseEnemyAction();
            EnemyAction(c);
        }
    }

    private void EnemyAction(int i){

    }

    private void PlayerWins(){

    }

    private int ChooseEnemyAction(){
        System.Random r = new System.Random();
        int rInt = r.Next(0, enemy.attacks.Length);
        if(rInt == enemyLastAttackUsed){
            return ChooseEnemyAction();  // epic recursion idea bc too lazy to write a while loop, hopefully this doesn't explode the game
        }
        return rInt;
    }

    private bool IsAttack(FishingItem i){
        if(i is FishingWeapon){
            if((i as FishingWeapon).healing == false){
                return true;
            }
        }
        else if(i is FishingArmor){
            if((i as FishingArmor).healing == false){
                return true;
            }
        }
        return false;
    }

    private int CalculateDamage(int damage, SuperEffective e){
        if(e == SuperEffective.Effective){
            return (int) Math.Round(damage*1.5);
        }
        else if(e == SuperEffective.Weak){
            return (int) Math.Round(damage*0.5);
        }
        return damage;
    }

    private SuperEffective CheckEffectiveness(Element e, Element target){
        if(e == Element.Vnd){
            if(target == Element.Ignem){
                return SuperEffective.Effective;
            }
            if(target == Element.Florous){
                return SuperEffective.Weak;
            }
        }
        else if(e == Element.Ignem){
            if(target == Element.Florous){
                return SuperEffective.Effective;
            }
            if(target == Element.Vnd){
                return SuperEffective.Weak;
            }
        }
        else if(e == Element.Florous){
            if(target == Element.Vnd){
                return SuperEffective.Effective;
            }
            if(target == Element.Ignem){
                return SuperEffective.Weak;
            }
        }
        else if(e == Element.Luminous){
            if(target == Element.Crepuscule){
                return SuperEffective.Effective;
            }
        }
        else if(e == Element.Crepuscule){
            if(target == Element.Luminous){
                return SuperEffective.Effective;
            }
        }
        return SuperEffective.Neutral;
    }

    private void UpdateHPText(){
        HPText.text = "ENEMY HPL\n  "+enemyHP+"/"+enemyMaxHP+"\n\n"+"YOUR HP:"+"\n"+"  "+playerHP+"/"+playerMaxHP;
    }

    private void UpdateCombatLog(string t){
        combatLog.text = t + "\n\n > " + combatLog.text; 
    }

}

public enum FishingBattleState{
    Starting,PlayerTurn,EnemyTurn,EnemyWin,PlayerWin
}

public enum SuperEffective{
    Weak,Neutral,Effective
}