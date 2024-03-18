using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using Mono.CSharp;
using Steamworks;

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
    public bool firstHeal,firstEnemyHeal,firstAttack,firstEnemyAttack,enemyHalf,playerHalf;
    public int textDelay = 2;
    public GameObject FishingBattleUI;

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
        firstHeal = firstEnemyHeal = firstAttack = firstEnemyAttack = enemyHalf = playerHalf = true;

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
                StartCoroutine(PlayerAction(attackNum));
            }
            else{
                // idk play a sound or something
            }
        }
        else if(battleState == FishingBattleState.EnemyWin || battleState == FishingBattleState.PlayerWin){
            // send you out the battle menu
            FishingBattleUI.gameObject.SetActive(false);
        }
    }

    private IEnumerator PlayerAction(int i){
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
            yield return new WaitForSeconds(textDelay);

            if(firstAttack){
                UpdateCombatLog(enemy.firstAttackWitnessedText);
                firstAttack = false;
                yield return new WaitForSeconds(textDelay);
            }

            if(enemyHalf && enemyHP <= Mathf.RoundToInt(enemyMaxHP/2)){
                UpdateCombatLog(enemy.halfHPText);
                enemyHalf = false;
                yield return new WaitForSeconds(textDelay);
            }

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
            yield return new WaitForSeconds(textDelay);

            if(firstHeal){
                UpdateCombatLog(enemy.firstHealWitnessedText);
                firstHeal = false;
                yield return new WaitForSeconds(textDelay);
            }

        }

        UpdateHPText();

        // Update the used move as unusable next turn
        playerLastAttackUsed = i;
        for(int x = 0; x < 3; x++){
            buttons[x].used.gameObject.SetActive(x==i);
        }

        // Check if you won, or if its now the enemy turn
        if(enemyHP <= 0){
            StartCoroutine(PlayerWins());
        }
        else{
            battleState = FishingBattleState.EnemyTurn;
            int c = ChooseEnemyAction();
            StartCoroutine(EnemyAction(c));
        }
    }

    private IEnumerator EnemyAction(int i){

        System.Random r = new System.Random();
        int talkRNG = r.Next(0, 101);
        if(talkRNG <= enemy.randomTextPercent){
            talkRNG = r.Next(0, enemy.randomTexts.Length);
            UpdateCombatLog(enemy.randomTexts[talkRNG]);
            yield return new WaitForSeconds(textDelay);
        }

        // If it is an attack
        if(IsAttack(enemyAttacks[i])){

            if(firstEnemyAttack){
                UpdateCombatLog(enemy.firstAttackPerformedText);
                firstEnemyAttack = false;
                yield return new WaitForSeconds(textDelay);
            }


            SuperEffective e;
            int d;
            if(enemyAttacks[i] is FishingArmor){
                FishingArmor a = enemyAttacks[i] as FishingArmor; 
                e = CheckEffectiveness(a.element,(playerAttacks[1] as FishingArmor).element);
                d = a.ATK;
            }
            else{
                FishingWeapon a = enemyAttacks[i] as FishingWeapon; 
                e = CheckEffectiveness(a.element,(playerAttacks[1] as FishingArmor).element);
                d = a.ATK;
            }
            int c = CalculateDamage(d, e);
            playerHP -= c;
            if(e == SuperEffective.Neutral){
                UpdateCombatLog(enemy.name + " used " + enemyAttacks[i].itemName + " and dealt " + c + " damage!" );
            } else if(e == SuperEffective.Effective){
                UpdateCombatLog("SUPER EFFECTIVE! "+ enemy.name + " used " + enemyAttacks[i].itemName + " and dealt " + c + " damage!" );
            } else{UpdateCombatLog(enemy.name+" used " + enemyAttacks[i].itemName + ", but it wasn't very effective... dealt " + c + " damage.");};
            yield return new WaitForSeconds(textDelay);

            if(playerHalf && playerHP <= Mathf.RoundToInt(playerMaxHP/2)){
                UpdateCombatLog(enemy.playerHalfHPText);
                playerHalf = false;
                yield return new WaitForSeconds(textDelay);
            }
        
        }

        // If it is healing
        else{

            if(firstEnemyHeal){
                UpdateCombatLog(enemy.firstHealPerformedText);
                firstEnemyHeal = false;
                yield return new WaitForSeconds(textDelay);
            }

            int h;
            if(enemyAttacks[i] is FishingArmor){
                FishingArmor a = enemyAttacks[i] as FishingArmor; 
                h = a.ATK;
            }
            else{
                FishingWeapon a = enemyAttacks[i] as FishingWeapon; 
                h = a.ATK;
            }
            int old = enemyHP;
            enemyHP += h;
            if(enemyHP > enemyMaxHP){
                enemyHP = enemyMaxHP;
                h = enemyMaxHP - old;
            }
            UpdateCombatLog(enemy.name+" used "+ enemyAttacks[i].itemName +" and recovered "+h+" HP! ("+old+" >> "+enemyHP+")");
            yield return new WaitForSeconds(textDelay);
        }

        UpdateHPText();

        // Update the used move as unusable next turn
        enemyLastAttackUsed = i;

        // Check if you lost, or if its now the player turn
        if(playerHP <= 0){
            StartCoroutine(PlayerLoses());
        }
        else{
            UpdateCombatLog("[player turn]");
            battleState = FishingBattleState.PlayerTurn;
        }
    }

    private IEnumerator PlayerWins(){
        UpdateCombatLog(enemy.deathText);
        yield return new WaitForSeconds(textDelay);
        
        // Roll for item drops
        string t = "";
        bool gotAnItem = false;
        System.Random r = new System.Random();
        foreach(FishingItemDrop i in enemy.drops){
            int n = 0;
            // Check if you get this item
            for(int c = 0; c < i.dropRolls; c++){
                int rng = r.Next(0, 101);
                if(rng >= i.dropRate){
                    n++;
                }
            }
            // Reward item if you get it
            if(n>0){
                gotAnItem = true;
                for(int c = 0; c < n; c++){
                    player.Add(i.itemToDrop);
                }
                t = t + "\n -"+i.itemToDrop.name+" (x"+n+")";
            }
        }
        if(gotAnItem){
            UpdateCombatLog("You win!\nItem Drops:"+t+"\n\n[press any button to exit]");
        }
        else{UpdateCombatLog("You win!\n(no items dropped)"+"\n\n[press any button to exit]");}
        battleState = FishingBattleState.PlayerWin;
    }

    private IEnumerator PlayerLoses(){
        UpdateCombatLog(enemy.playerDefeatText);
        yield return new WaitForSeconds(textDelay);
        UpdateCombatLog("You were defeated..."+"\n[press any button to exit]");
        battleState = FishingBattleState.EnemyWin;
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
                Debug.Log("Weapon attack!");
                return true;
            }
        }
        else if(i is FishingArmor){
            if((i as FishingArmor).healing == false){
                Debug.Log("Armor attack!");
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
        combatLog.text = "  > " + t + "\n\n" + combatLog.text; 
 
    }

}

public enum FishingBattleState{
    Starting,PlayerTurn,EnemyTurn,EnemyWin,PlayerWin
}

public enum SuperEffective{
    Weak,Neutral,Effective
}