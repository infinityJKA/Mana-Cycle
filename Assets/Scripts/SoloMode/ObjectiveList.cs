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

            // if above 0, add score requirement as objective 
            if (Storage.level.scoreGoal > 0) {
                Objective scoreObjective = new Objective
                {
                    condition = ObjectiveCondition.PointTotal,
                    value = Storage.level.scoreGoal
                };

                var scoreItem = Instantiate(objectiveListItemPrefab, objectiveListLayout.transform);
                scoreItem.objective = scoreObjective;
                objectiveItems.Add(scoreItem);
            }
            // if survival, add "survive" objective
            if (Storage.level.survivalWin) {
                Objective surviveObjective = new Objective
                {
                    condition = ObjectiveCondition.Survive
                };
                // rename survive objective if in endless arcade
                if (Storage.level.generateNextLevel) surviveObjective.statusOverride = "Get as many points as possible!";

                var survivalItem = Instantiate(objectiveListItemPrefab, objectiveListLayout.transform);
                survivalItem.objective = surviveObjective;
                objectiveItems.Add(survivalItem);
            }
            
            
            // add all other additional objectives
            foreach (Objective objective in Storage.level.objectives) {
                var objectiveListItem = Instantiate(objectiveListItemPrefab, objectiveListLayout.transform);
                objectiveListItem.objective = objective;
                objectiveItems.Add(objectiveListItem);
            }

            Refresh(board);
        }

        public void Refresh(GameBoard board) {
            if (board.singlePlayer && !Storage.level) return;

            // if there are no objectives, don't auto complete
            if (objectiveItems.Count == 0) return;
            
            bool allObjectivesComplete = true;

            foreach (ObjectiveListItem objListItem in objectiveItems) {
                bool completed = objListItem.Refresh(board);
                if (!completed && !objListItem.objective.inverted) allObjectivesComplete = false;

                // if an inverted condition is met, die
                if (objListItem.objective.inverted && completed) board.Defeat();
            }

            // if all objectives are complete, and score req. is met, level is won
            if (allObjectivesComplete && (board.hp >= Storage.level.scoreGoal || Storage.level.scoreGoal == 0)) {
                board.Win();
            }
        }
    }
}