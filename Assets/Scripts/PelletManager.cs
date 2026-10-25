using UnityEngine;
using UnityEngine.Tilemaps;

public class PelletManager : MonoBehaviour
{
    [SerializeField] private Tilemap coinTilemap;
    [SerializeField] private HUDManager hudManager;

    private int totalPellets = 0;
    private int pelletsEaten = 0;

    void Start()
    {
        if (coinTilemap != null)
        {
            BoundsInt bounds = coinTilemap.cellBounds;
            for (int x = bounds.min.x; x < bounds.max.x; x++)
            {
                for (int y = bounds.min.y; y < bounds.max.y; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    if (coinTilemap.HasTile(pos))
                    {
                        totalPellets++;
                    }
                }
            }
        }
    }

    public void PelletEaten()
    {
        pelletsEaten++;
        if (pelletsEaten >= totalPellets)
        {
            if (hudManager != null)
            {
                hudManager.TriggerWinOrGameOver(true);
            }
        }
    }
}
