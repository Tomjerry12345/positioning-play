using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class RoleArea
{
    public List<ZoneRect> zones = new List<ZoneRect>(); // Multiple rectangular zones
    public List<Vector2Int> customCells = new List<Vector2Int>(); // Specific cells

    // Check if coordinate is within ANY zone
    public bool Contains(Vector2Int coord)
    {
        // Check rectangular zones
        foreach (var zone in zones)
        {
            if (coord.x >= zone.min.x && coord.x <= zone.max.x &&
                coord.y >= zone.min.y && coord.y <= zone.max.y)
                return true;
        }

        // Check custom cells
        return customCells.Contains(coord);
    }
}

[System.Serializable]
public class ZoneRect
{
    public Vector2Int min;
    public Vector2Int max;

    public ZoneRect(int minX, int minY, int maxX, int maxY)
    {
        min = new Vector2Int(minX, minY);
        max = new Vector2Int(maxX, maxY);
    }
}

public class GridManager : MonoBehaviour
{
    private List<Cell> allCells = new List<Cell>();

    public GameObject cellPrefab;
    public GameObject lapangan; // referensi ke sprite lapangan
    public int cellCountX = 10;
    public int cellCountY = 5;

    public Vector2 MinBounds { get; private set; }
    public Vector2 MaxBounds { get; private set; }


    void Awake()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        if (lapangan == null || cellPrefab == null)
        {
            Debug.LogError("Lapangan atau Cell Prefab belum diatur di Inspector!");
            return;
        }

        SpriteRenderer sr = lapangan.GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("Lapangan tidak memiliki SpriteRenderer!");
            return;
        }

        Vector2 lapanganSize = sr.bounds.size;
        float cellWidth = lapanganSize.x / cellCountX;
        float cellHeight = lapanganSize.y / cellCountY;

        // karena lapangan di orientasi XY, maka Y = vertikal, bukan Z
        Vector3 startPos = lapangan.transform.position - new Vector3(lapanganSize.x / 2f, lapanganSize.y / 2f, 0f);

        for (int x = 0; x < cellCountX; x++)
        {
            for (int y = 0; y < cellCountY; y++)
            {
                // tetap di bidang XY, bukan XZ
                Vector3 cellPos = startPos + new Vector3((x + 0.5f) * cellWidth, (y + 0.5f) * cellHeight, 0f);
                //GameObject cell = Instantiate(cellPrefab, cellPos, Quaternion.identity);
                //cell.transform.parent = transform;
                GameObject cell = Instantiate(cellPrefab, transform);
                cell.transform.localPosition = new Vector3(
                    (x + 0.5f) * cellWidth - lapanganSize.x / 2f,
                    (y + 0.5f) * cellHeight - lapanganSize.y / 2f,
                    0f
                );
                cell.transform.localScale = new Vector3(cellWidth, cellHeight, 1f) * 0.95f;

                Cell cellComponent = cell.GetComponent<Cell>();
                if (cellComponent != null)
                {
                    // ✅ Tambahkan baris ini:
                    cellComponent.gridCoord = new Vector2Int(x, y);
                    allCells.Add(cellComponent);
                }
            }
        }

        // ----- Hitung batas lapangan sebagai Vector2 (benar tipe datanya) -----
        Vector3 center = lapangan.transform.position;
        MinBounds = new Vector2(center.x - lapanganSize.x / 2f, center.y - lapanganSize.y / 2f);
        MaxBounds = new Vector2(center.x + lapanganSize.x / 2f, center.y + lapanganSize.y / 2f);

        cellSize = lapanganSize.y / cellCountY;

        //Debug.Log($"Grid bounds set. Min: {MinBounds}, Max: {MaxBounds}");
    }

    public List<Cell> GetAllCells()
    {
        if (allCells.Count == 0)
            allCells = new List<Cell>(FindObjectsOfType<Cell>());
        return allCells;
    }

    public float cellSize { get; private set; }
    public Cell GetNearestCell(Vector3 pos)
    {
        return allCells.OrderBy(c => Vector3.Distance(c.transform.position, pos)).FirstOrDefault();
    }

    public Vector3 GetWorldPositionFromGrid(Vector2Int coord)
    {
        var cell = allCells.FirstOrDefault(c => c.gridCoord == coord);
        return cell != null ? cell.transform.position : Vector3.zero;
    }

    public Cell GetCellAt(Vector2Int coord)
    {
        return allCells.FirstOrDefault(c => c.gridCoord == coord);
    }



}
