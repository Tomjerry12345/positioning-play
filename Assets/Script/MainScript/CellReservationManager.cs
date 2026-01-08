using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized cell reservation system untuk mencegah collision antara Team AI dan Enemy AI
/// </summary>
public class CellReservationManager : MonoBehaviour
{
    private static CellReservationManager _instance;
    public static CellReservationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<CellReservationManager>();
                if (_instance == null)
                {
                    GameObject go = new GameObject("CellReservationManager");
                    _instance = go.AddComponent<CellReservationManager>();
                }
            }
            return _instance;
        }
    }

    // 🔥 SHARED RESERVED CELLS - diakses oleh Team AI dan Enemy AI
    private HashSet<Vector2Int> reservedCells = new HashSet<Vector2Int>();

    // 🔥 OCCUPIED CELLS - posisi aktual semua unit saat ini
    private Dictionary<Vector2Int, string> occupiedCells = new Dictionary<Vector2Int, string>();

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
    }

    /// <summary>
    /// Reset semua reservasi di awal turn (dipanggil GameManager)
    /// </summary>
    public void ResetReservations()
    {
        reservedCells.Clear();
        Debug.Log("🔄 [RESERVATION] All reservations cleared");
    }

    /// <summary>
    /// Update occupied cells dengan posisi aktual semua unit
    /// </summary>
    public void UpdateOccupiedCells(List<AIController> teamAI, List<AIController> enemyAI, GridManager gridManager, PlayerController player = null)
    {
        occupiedCells.Clear();

        // Team AI positions
        foreach (var ai in teamAI)
        {
            if (ai == null) continue;
            Cell cell = gridManager.GetNearestCell(ai.transform.position);
            if (cell != null)
            {
                occupiedCells[cell.gridCoord] = $"TEAM-{ai.assignedRole}";
            }
        }

        // Enemy AI positions
        foreach (var enemy in enemyAI)
        {
            if (enemy == null) continue;
            Cell cell = gridManager.GetNearestCell(enemy.transform.position);
            if (cell != null)
            {
                occupiedCells[cell.gridCoord] = $"ENEMY-{enemy.assignedRole}";
            }
        }

        // Player position
        if (player != null)
        {
            Cell cell = gridManager.GetNearestCell(player.transform.position);
            if (cell != null)
            {
                occupiedCells[cell.gridCoord] = "PLAYER";
            }
        }

        Debug.Log($"📍 [RESERVATION] Occupied cells updated: {occupiedCells.Count} cells");
    }

    /// <summary>
    /// Reserve cell untuk unit tertentu
    /// </summary>
    public bool TryReserveCell(Vector2Int cell, Vector2Int currentCell, string unitName)
    {
        // Boleh reserve cell sendiri (tetap di tempat)
        if (cell == currentCell)
        {
            reservedCells.Add(cell);
            Debug.Log($"✅ [RESERVATION] {unitName} stays at {cell}");
            return true;
        }

        // Cek apakah sudah di-reserve
        if (reservedCells.Contains(cell))
        {
            Debug.Log($"❌ [RESERVATION] {unitName} BLOCKED - {cell} already reserved");
            return false;
        }

        // Cek apakah cell ditempati unit lain
        if (occupiedCells.ContainsKey(cell))
        {
            Debug.Log($"❌ [RESERVATION] {unitName} BLOCKED - {cell} occupied by {occupiedCells[cell]}");
            return false;
        }

        // Reserve berhasil
        reservedCells.Add(cell);
        //Debug.Log($"✅ [RESERVATION] {unitName} reserved {cell}");
        return true;
    }

    /// <summary>
    /// Release cell yang sudah di-reserve (saat unit pindah)
    /// </summary>
    public void ReleaseCell(Vector2Int cell)
    {
        reservedCells.Remove(cell);
    }

    /// <summary>
    /// Cek apakah cell sudah di-reserve
    /// </summary>
    public bool IsReserved(Vector2Int cell)
    {
        return reservedCells.Contains(cell);
    }

    /// <summary>
    /// Cek apakah cell ditempati unit lain
    /// </summary>
    public bool IsOccupied(Vector2Int cell)
    {
        return occupiedCells.ContainsKey(cell);
    }

    /// <summary>
    /// Get info siapa yang menempati cell
    /// </summary>
    public string GetOccupant(Vector2Int cell)
    {
        return occupiedCells.ContainsKey(cell) ? occupiedCells[cell] : "NONE";
    }

    /// <summary>
    /// Debug visualization
    /// </summary>
    private void DebugPrintReservations()
    {
        Debug.Log($"🔒 Reserved Cells ({reservedCells.Count}): {string.Join(", ", reservedCells)}");
        Debug.Log($"📍 Occupied Cells ({occupiedCells.Count}): {string.Join(", ", occupiedCells.Keys)}");
    }
}