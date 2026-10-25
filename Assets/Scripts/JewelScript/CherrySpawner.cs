using UnityEngine;

public class CherrySpawner : MonoBehaviour
{
    [SerializeField] private GameObject cherryPrefab;
    [SerializeField] private Transform spawnPointLeft;
    [SerializeField] private Transform spawnPointRight;

    public void SpawnCherry()
    {
        GameObject cherry = Instantiate(cherryPrefab);
        CherryController controller = cherry.GetComponent<CherryController>();
        
        // Example: Spawn on the left and move right across the screen boundaries (-14 to 14)
        controller.InitializeCherry(spawnPointLeft.position, Vector3.right, -14f, 14f);
    }
}
