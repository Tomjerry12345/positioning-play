using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class EnemyAIManager : MonoBehaviour
{
    [HideInInspector] public List<AIController> enemyTeammates = new List<AIController>();
    public GridManager gridManager;
    [HideInInspector] public FormationManager enemyFormationManager;
    [HideInInspector] public BallController ball;
    public TeamAIManager playerTeam;
    public GameOverManager gameOverManager;

    [Header("Defense Settings")]
    public float moveDuration = 0.5f;
    public int maxDefenseRadius = 3;

    [Header("Defensive Strategy")]
    [Tooltip("Hanya pemain terdekat yang akan pressure bola")]
    public int maxPressurePlayers = 2;
    [Tooltip("Jarak maksimal untuk pressure bola")]
    public float pressureDistance = 3f;
    [Tooltip("Posisi X gawang sendiri")]
    public float ownGoalX = 4f;
    [Tooltip("Posisi Y gawang sendiri - ENEMY di ATAS (Y=14)")]
    public float ownGoalY = 14f;

    [Header("Role Movement Boundaries - Grid 9x15 (X:0-8, Y:0-14)")]
    [Tooltip("GK: Posisi paling ATAS (Y=14), area sempit")]
    public Vector4 gkBoundary = new Vector4(2, 14, 6, 14);

    [Tooltip("Defender: Area belakang dekat gawang (Y tinggi: 11-13)")]
    public Vector4 defenderBoundary = new Vector4(0, 11, 8, 13);

    [Tooltip("Midfielder: Area tengah (Y sedang: 7-11)")]
    public Vector4 midfielderBoundary = new Vector4(0, 7, 8, 11);

    [Tooltip("Forward: Area depan serang ke BAWAH (Y rendah: 0-7)")]
    public Vector4 forwardBoundary = new Vector4(0, 0, 8, 7);

    [Header("Horizontal Tracking Settings")]
    [Tooltip("Jarak Y minimum bola dari gawang enemy agar tracking horizontal aktif")]
    public float trackingActivationDistance = 6f;
    [Tooltip("Weight untuk tracking horizontal saat bola dekat")]
    public float horizontalTrackingWeight = 12f;

    // 🔥 RESERVED CELLS SYSTEM
    private HashSet<Vector2Int> occupiedCells = new HashSet<Vector2Int>();

    private CellReservationManager reservation;
    private Dictionary<AIController, Vector2Int> plannedMoves = new Dictionary<AIController, Vector2Int>();

    void Start()
    {
        reservation = CellReservationManager.Instance;
    }

    //public void Action()
    //{
    //    if (enemyTeammates == null || enemyTeammates.Count == 0)
    //    {
    //        Debug.LogWarning("⚠️ [ENEMY] Tidak ada AI terdaftar di tim.");
    //        return;
    //    }

    //    Debug.Log("🛡️ [ENEMY] Semua musuh mulai bergerak bersamaan...");
    //    StartCoroutine(EnemyDefendRoutine());
    //}

    //private IEnumerator EnemyDefendRoutine()
    //{
    //    Dictionary<AIController, Vector2Int> plannedMoves = new Dictionary<AIController, Vector2Int>();

    //    List<AIController> pressurePlayers = GetPressurePlayers();
    //    List<AIController> sortedEnemies = PrioritizeDefenders();

    //    // 🔥 FASE PLANNING dengan Shared Reservation
    //    foreach (AIController enemy in sortedEnemies)
    //    {
    //        if (enemy == null) continue;

    //        bool shouldPressure = pressurePlayers.Contains(enemy);

    //        Vector2Int currentPos = enemy.gridCoord;
    //        Vector2Int bestMove = CalculateBestDefensiveMove(enemy, shouldPressure);

    //        plannedMoves[enemy] = bestMove;

    //        // 🔥 RESERVE via CellReservationManager
    //        reservation.ReleaseCell(currentPos); // lepas cell lama
    //        reservation.TryReserveCell(bestMove, currentPos, $"ENEMY-{enemy.assignedRole}");

    //        string mode = shouldPressure ? "PRESSURE" : "HOLD";
    //        Debug.Log($"📍 [ENEMY] {enemy.assignedRole} ({mode}): {currentPos} → {bestMove}");
    //    }

    //    // 🔥 FASE EXECUTION
    //    List<Coroutine> moveCoroutines = new List<Coroutine>();

    //    foreach (var kvp in plannedMoves)
    //    {
    //        AIController enemy = kvp.Key;
    //        Vector2Int targetCell = kvp.Value;

    //        if (targetCell != enemy.gridCoord)
    //        {
    //            moveCoroutines.Add(StartCoroutine(MoveEnemyToCell(enemy, targetCell)));
    //        }
    //    }

    //    foreach (var co in moveCoroutines)
    //    {
    //        yield return co;
    //    }

    //    yield return new WaitForSeconds(0.1f);
    //    Debug.Log("✅ [ENEMY] Semua musuh selesai bergerak!");
    //}

    public void Action()
    {
        // Legacy method - sekarang pakai PlanMoves() dan ExecuteMoves()
        StartCoroutine(ActionRoutine());
    }

    private IEnumerator ActionRoutine()
    {
        yield return PlanMoves();
        yield return ExecuteMoves();
    }

    // 🔥 FASE 1: PLANNING (tidak ada movement, hanya decide target)
    public IEnumerator PlanMoves()
    {
        if (enemyTeammates == null || enemyTeammates.Count == 0)
        {
            Debug.LogWarning("⚠️ [ENEMY] Tidak ada AI terdaftar di tim.");
            yield break;
        }

        //Debug.Log("🧠 [ENEMY] Planning phase started...");

        plannedMoves.Clear();

        List<AIController> pressurePlayers = GetPressurePlayers();
        List<AIController> sortedEnemies = PrioritizeDefenders();

        // 🔥 PLANNING LOOP - CEK RESERVASI REAL-TIME
        foreach (AIController enemy in sortedEnemies)
        {
            if (enemy == null) continue;

            bool shouldPressure = pressurePlayers.Contains(enemy);
            Vector2Int currentPos = enemy.gridCoord;
            Vector2Int bestMove = CalculateBestDefensiveMove(enemy, shouldPressure);

            plannedMoves[enemy] = bestMove;

            // 🔥 TRY RESERVE - jika gagal, tetap di tempat
            bool reserved = reservation.TryReserveCell(bestMove, currentPos, $"ENEMY-{enemy.assignedRole}");

            if (!reserved && bestMove != currentPos)
            {
                //Debug.LogWarning($"⚠️ [ENEMY] {enemy.assignedRole} gagal reserve {bestMove}, tetap di {currentPos}");
                plannedMoves[enemy] = currentPos; // fallback ke posisi sekarang
            }

            string mode = shouldPressure ? "PRESSURE" : "HOLD";
            //Debug.Log($"📍 [ENEMY] {enemy.assignedRole} ({mode}): {currentPos} → {plannedMoves[enemy]}");
        }

        //Debug.Log("✅ [ENEMY] Planning phase complete!");
        yield return null;
    }

    // 🔥 FASE 2: EXECUTION (movement bersamaan)
    public IEnumerator ExecuteMoves()
    {
        //Debug.Log("⚡ [ENEMY] Execution phase started...");

        List<Coroutine> moveCoroutines = new List<Coroutine>();

        foreach (var kvp in plannedMoves)
        {
            AIController enemy = kvp.Key;
            Vector2Int targetCell = kvp.Value;

            if (targetCell != enemy.gridCoord)
            {
                moveCoroutines.Add(StartCoroutine(MoveEnemyToCell(enemy, targetCell)));
            }
            else
            {
                //Debug.Log($"🚫 [ENEMY] {enemy.assignedRole} tetap di {enemy.gridCoord}");
            }
        }

        // 🔥 TUNGGU SEMUA MOVEMENT SELESAI BERSAMAAN
        foreach (var co in moveCoroutines)
        {
            yield return co;
        }

        yield return new WaitForSeconds(0.1f);
        //Debug.Log("✅ [ENEMY] Execution phase complete!");
    }

    private List<AIController> GetPressurePlayers()
    {
        Cell ballCell = gridManager.GetNearestCell(ball.transform.position);
        Vector2Int ballPos = ballCell != null ? ballCell.gridCoord : Vector2Int.zero;

        return enemyTeammates
            .Where(e => e != null && !e.assignedRole.Contains("GK"))
            .OrderBy(e => Vector2Int.Distance(e.gridCoord, ballPos))
            .Take(maxPressurePlayers)
            .ToList();
    }

    private List<AIController> PrioritizeDefenders()
    {
        Cell ballCell = gridManager.GetNearestCell(ball.transform.position);
        Vector2Int ballPos = ballCell != null ? ballCell.gridCoord : Vector2Int.zero;

        return enemyTeammates
            .Where(e => e != null)
            .OrderBy(e => Vector2Int.Distance(e.gridCoord, ballPos))
            .ThenBy(e => e.gridCoord.x)
            .ToList();
    }

    private Vector2Int CalculateBestDefensiveMove(AIController enemy, bool shouldPressure)
    {
        Vector2Int currentPos = enemy.gridCoord;

        Cell ballCell = gridManager.GetNearestCell(ball.transform.position);
        Vector2Int ballPos = ballCell != null ? ballCell.gridCoord : Vector2Int.zero;

        List<Vector2Int> possibleMoves = new List<Vector2Int>
    {
        currentPos,
        currentPos + Vector2Int.up,
        currentPos + Vector2Int.down,
        currentPos + Vector2Int.left,
        currentPos + Vector2Int.right,
        currentPos + new Vector2Int(1, 1),
        currentPos + new Vector2Int(1, -1),
        currentPos + new Vector2Int(-1, 1),
        currentPos + new Vector2Int(-1, -1)
    };

        Vector2Int bestMove = currentPos;
        float bestScore = float.MinValue;

        foreach (Vector2Int move in possibleMoves)
        {
            if (!IsValidMove(enemy, move))
            {
                continue;
            }

            float score = EvaluateDefensivePosition(enemy, move, ballPos, shouldPressure);

            if (score > bestScore)
            {
                bestScore = score;
                bestMove = move;
            }
        }

        return bestMove;
    }

    private bool IsValidMove(AIController enemy, Vector2Int targetPos)
    {
        Cell targetCell = gridManager.GetCellAt(targetPos);
        if (targetCell == null) return false;

        if (targetPos != enemy.gridCoord)
        {
            if (reservation.IsReserved(targetPos))
            {
                return false;
            }

            if (reservation.IsOccupied(targetPos))
            {
                return false;
            }
        }

        if (!IsWithinRoleBoundary(enemy, targetPos))
        {
            return false;
        }

        return true;
    }

    private bool IsWithinRoleBoundary(AIController enemy, Vector2Int position)
    {
        Vector4 boundary = Vector4.zero;

        if (enemy.assignedRole.Contains("GK"))
        {
            boundary = gkBoundary;
        }
        else if (IsDefender(enemy.assignedRole))
        {
            boundary = defenderBoundary;
        }
        else if (IsMidfielder(enemy.assignedRole))
        {
            boundary = midfielderBoundary;
        }
        else if (IsForward(enemy.assignedRole))
        {
            boundary = forwardBoundary;
        }
        else
        {
            return true;
        }

        bool withinX = position.x >= boundary.x && position.x <= boundary.z;
        bool withinY = position.y >= boundary.y && position.y <= boundary.w;

        return withinX && withinY;
    }

    private bool IsDefender(string role)
    {
        return role == "LB" || role == "RB" || role == "CBL" || role == "CBR";
    }

    private bool IsMidfielder(string role)
    {
        return role == "CM" || role == "AM" || role == "DM";
    }

    private bool IsForward(string role)
    {
        return role == "ST" || role == "LW" || role == "RW";
    }

    private float EvaluateDefensivePosition(AIController enemy, Vector2Int position, Vector2Int ballPos, bool shouldPressure)
    {
        float score = 0f;
        float distToBall = Vector2Int.Distance(position, ballPos);

        // ===== GOALKEEPER - TIDAK BERGERAK =====
        if (enemy.assignedRole.Contains("GK"))
        {
            if (position == enemy.gridCoord)
            {
                score += 1000f;
            }
            else
            {
                score -= 500f;
            }
            return score;
        }

        float ballDistanceFromEnemyGoal = Mathf.Abs(ownGoalY - ballPos.y);
        bool ballIsClose = ballDistanceFromEnemyGoal <= trackingActivationDistance;

        // ===== TRACKING HORIZONTAL (HANYA JIKA BOLA DEKAT) =====
        if (ballIsClose)
        {
            float ballX = ballPos.x;
            float horizontalAlignment = -Mathf.Abs(position.x - ballX);
            score += horizontalAlignment * horizontalTrackingWeight;
        }

        if (shouldPressure)
        {
            score += (pressureDistance - distToBall) * 20f;

            if (position.y > ballPos.y && position.y < ownGoalY)
            {
                score += 15f;
            }
        }
        else
        {
            if (IsDefender(enemy.assignedRole))
            {
                if (ballPos.y < 7)
                {
                    float idealY = (ownGoalY + ballPos.y) / 2f;
                    idealY = Mathf.Clamp(idealY, defenderBoundary.y, defenderBoundary.w);

                    float distToIdealY = Mathf.Abs(position.y - idealY);
                    score += (3f - distToIdealY) * 15f;

                    if (position.y >= 12)
                    {
                        score += 10f;
                    }
                }
                else
                {
                    float distanceFromOwnGoal = Mathf.Abs(position.y - ownGoalY);
                    score += (3f - distanceFromOwnGoal) * 30f;

                    if (position.y < 11)
                    {
                        score -= 70f;
                    }

                    if (position.y >= 11 && position.y <= 13)
                    {
                        score += 25f;
                    }
                }
            }
            else if (IsMidfielder(enemy.assignedRole))
            {
                float idealY = (ownGoalY + ballPos.y) / 2f;
                idealY = Mathf.Clamp(idealY, midfielderBoundary.y, midfielderBoundary.w);

                float distToIdealY = Mathf.Abs(position.y - idealY);
                score += (5f - distToIdealY) * 18f;

                if (ballIsClose)
                {
                    score += 10f;
                }

                if (position.y >= 7 && position.y <= 11)
                {
                    score += 15f;
                }

                if (ballPos.y < 5 && position.y < 9)
                {
                    score += 12f;
                }
            }
            else if (IsForward(enemy.assignedRole))
            {
                if (ballPos.y > position.y)
                {
                    float idealY = ballPos.y - 1.5f;
                    idealY = Mathf.Clamp(idealY, forwardBoundary.y, forwardBoundary.w);

                    float distToIdealY = Mathf.Abs(position.y - idealY);
                    score += (5f - distToIdealY) * 20f;

                    if (position.y < ballPos.y && position.y > ballPos.y - 3)
                    {
                        score += 25f;
                    }
                }
                else
                {
                    if (position.y <= 5)
                    {
                        score += 25f;
                    }

                    float forwardBonus = (7f - position.y) * 4f;
                    score += forwardBonus;

                    if (position.y > 7)
                    {
                        score -= 35f;
                    }

                    if (position.y < ballPos.y && position.y >= ballPos.y - 2)
                    {
                        score += 12f;
                    }
                }
            }

            if (distToBall < 2f)
            {
                score -= 25f;
            }
        }

        // ===== ANTI-GEROMBOL =====
        int nearbyTeammates = CountNearbyTeammates(position, enemy);
        score -= nearbyTeammates * 10f;

        // ===== BLOCKING PASSING LANE =====
        if (position.y > ballPos.y && position.y < ownGoalY - 1)
        {
            score += 12f;
        }

        return score;
    }

    private int CountNearbyTeammates(Vector2Int position, AIController excludeThis)
    {
        int count = 0;
        foreach (AIController teammate in enemyTeammates)
        {
            if (teammate == null || teammate == excludeThis) continue;

            float dist = Vector2Int.Distance(position, teammate.gridCoord);
            if (dist <= 1.5f)
                count++;
        }
        return count;
    }

    private IEnumerator MoveEnemyToCell(AIController enemy, Vector2Int targetCell)
    {
        Vector2Int oldPos = enemy.gridCoord;
        enemy.SetGridCoord(targetCell);

        Vector3 targetWorldPos = gridManager.GetWorldPositionFromGrid(targetCell);

        float elapsed = 0f;
        Vector3 startPos = enemy.transform.position;

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / moveDuration);

            enemy.transform.position = Vector3.Lerp(startPos, targetWorldPos, t);
            yield return null;
        }

        enemy.transform.position = targetWorldPos;

        //Debug.Log($"⚔️ [ENEMY] {enemy.assignedRole} bergerak dari {oldPos} → {targetCell}");
    }

    private void UpdateOccupiedCells()
    {
        occupiedCells.Clear();
        foreach (AIController enemy in enemyTeammates)
        {
            if (enemy != null)
                occupiedCells.Add(enemy.gridCoord);
        }
    }
}