using TMPro;
using UnityEngine;

public class AIController : MonoBehaviour
{
    // ✨ HAPUS moveSpeed dari sini, biar gak bentrok sama TeamAIManager
    // public float moveSpeed = 0.5f; // ❌ DIHAPUS!

    public string assignedRole;
    private TextMeshPro roleText;
    public Vector2Int gridCoord;

    void Start()
    {
        roleText = GetComponentInChildren<TextMeshPro>();
        if (roleText != null)
            roleText.text = assignedRole;
    }

    // ✨ Fungsi ini sekarang hanya untuk FormationManager (posisi awal)
    public void MoveTo(Vector3 newPos)
    {
        // Langsung set posisi tanpa animasi
        transform.position = newPos;
        Debug.Log($"{assignedRole} ({gameObject.name}) dipindah ke {newPos}");
    }

    public void SetRole(string role)
    {
        assignedRole = role;
        if (roleText != null)
            roleText.text = role;
    }

    public void SetGridCoord(Vector2Int coord)
    {
        gridCoord = coord;
    }
}