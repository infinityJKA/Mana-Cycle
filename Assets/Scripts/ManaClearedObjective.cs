using System.Net.Mime;
public class ManaClearedObjective : Objective {
    public int quota;
    public override bool IsCompleted(GameBoard board) {
        return board.getTotalManaCleared() >= quota;
    }

    public override void Refresh(GameBoard board) {
        textbox.text = board.getTotalManaCleared()+"/"+quota+" Mana Cleared";
    }
}