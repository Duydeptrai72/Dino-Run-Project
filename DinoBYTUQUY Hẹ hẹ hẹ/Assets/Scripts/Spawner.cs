using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstacles;
    [SerializeField] private Transform highPos;
    [SerializeField] private Transform lowPos;
    private float timer = 0f;
    [SerializeField] private float spawnRate = 2f;
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnRate)
        {
            SpawnObstacle();
            timer = 0f;
        }
    }
    private void SpawnObstacle()
    {
        int index = Random.Range(0, obstacles.Length);
        if (index == 0 || index == 1 || index == 2 || index == 3 || index == 4)
        {
            GameObject obstacle = Instantiate(obstacles[index], lowPos.position, Quaternion.identity);
        }
        else if (index == 5 || index == 6)
        {
            GameObject obstacle = Instantiate(obstacles[index], highPos.position, Quaternion.identity);
        }
    }
}
