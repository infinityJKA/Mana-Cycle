using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Battle.Board;

namespace SoloMode {
    public class ObjectiveList : MonoBehaviour
    {
        /** List of objective list items that display the current level's objectives */
        [SerializeField] private List<ObjectiveListItem> objectiveItems;
        /** GameObject where objective list items are appended to */
        [SerializeField] private GameObject objectiveListLayout;
        /** Prefab used to initialize objective list items */
        [SerializeField] private ObjectiveListItem objectiveListItemPrefab;

        public void InitializeObjectiveListItems(GameBoard board) {
            objectiveItems = new List<ObjectiveListItem>();

            var level = board.GetLevel();

            // if above 0, add score requirement as objective 
            if (level.scoreGoal > 0) {
                Objective scoreObjective = new Objective();
                scoreObjective.condition = ObjectiveCondition.PointTotal;
                scoreObjective.value = level.scoreGoal;

                var scoreItem = Instantiate(objectiveListItemPrefab, objectiveListLayout.transform);
                scoreItem.objective = scoreObjective;
                objectiveItems.Add(scoreItem);
            }
            // if survival, add "survive" objective
            if (level.survivalWin) {
                Objective surviveObjective = new Objective();
                surviveObjective.condition = ObjectiveCondition.Survive;

                var survivalItem = Instantiate(objectiveListItemPrefab, objectiveListLayout.transform);
                survivalItem.objective = surviveObjective;
                objectiveItems.Add(survivalItem);
            }
            
            
            // add all other additional objectives
            foreach (Objective objective in level.objectives) {
                var objectiveListItem = Instantiate(objectiveListItemPrefab, objectiveListLayout.transform);
                objectiveListItem.objective = objective;
                objectiveItems.Add(objectiveListItem);
            }

            Refresh(board);
        }

        public void Refresh(GameBoard board) {
            if (board.singlePlayer && board.level == null) return;

            // if there are no objectives, don't auto complete
            if (objectiveItems.Count == 0) return;
            
            bool allObjectivesComplete = true;

            foreach (ObjectiveListItem objListItem in objectiveItems) {
                bool completed = objListItem.Refresh(board);
                if (!completed) allObjectivesComplete = false;
            }

            // if all objectives are complete, and score req. is met, level is won
            if (allObjectivesComplete && (board.hp >= board.GetLevel().scoreGoal || board.GetLevel().scoreGoal == 0)) {
                board.Win();
            }
        }
    }
}