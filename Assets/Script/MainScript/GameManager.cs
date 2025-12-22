using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public TeamAIManager teamAI;
    public EnemyAIManager enemyAI;
    public TimeManager timeManager;
    public GridManager gridManager;

    private bool isPlayerTurn = true;
    private CellReservationManager reservation;

    void Start()
    {
        isPlayerTurn = true;
        reservation = CellReservationManager.Instance;
    }

    public void OnPlayerMoveEnd()
    {
        if (isPlayerTurn)
        {
            isPlayerTurn = false;
            StartCoroutine(AITurnRoutine());
        }
    }

    private IEnumerator AITurnRoutine()
    {
        yield return new WaitForSeconds(0.5f);

        Debug.Log("🤖 === AI TURN START (SIMULTANEOUS) ===");

        // 🔥 STEP 1: RESET RESERVATIONS
        reservation.ResetReservations();

        // 🔥 STEP 2: UPDATE OCCUPIED CELLS (posisi aktual semua unit)
        reservation.UpdateOccupiedCells(
            teamAI.teammates,
            enemyAI.enemyTeammates,
            gridManager,
            teamAI.player
        );

        // 🔥 STEP 3: PLANNING PHASE - BERSAMAAN!
        Debug.Log("📘🔴 [BOTH] Planning phase - deciding moves simultaneously...");

        // Start both planning coroutines at the same time
        Coroutine teamPlanning = StartCoroutine(teamAI.PlanMoves());
        Coroutine enemyPlanning = StartCoroutine(enemyAI.PlanMoves());

        // Wait for both to finish planning
        yield return teamPlanning;
        yield return enemyPlanning;

        Debug.Log("✅ Both teams finished planning!");

        // 🔥 STEP 4: EXECUTION PHASE - BERSAMAAN!
        Debug.Log("⚡ [BOTH] Execution phase - moving simultaneously...");

        Coroutine teamExecution = StartCoroutine(teamAI.ExecuteMoves());
        Coroutine enemyExecution = StartCoroutine(enemyAI.ExecuteMoves());

        // Wait for both to finish moving
        yield return teamExecution;
        yield return enemyExecution;

        Debug.Log("✅ Both teams finished moving!");

        // 🔥 STEP 5: Player turn
        yield return new WaitForSeconds(0.5f);
        isPlayerTurn = true;
        Debug.Log("✅ Giliran player lagi!");

        if (timeManager != null)
            timeManager.StartMovement();

        reservation.DebugPrintReservations();
    }

    public bool IsPlayerTurn => isPlayerTurn;
}