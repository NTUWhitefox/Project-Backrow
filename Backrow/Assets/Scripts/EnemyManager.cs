using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    enum SpawnPosition
    {
        LEFT = 0,
        MIDDLE = 1,
        RIGHT = 2
    }
    [SerializeField] private GameObject[] enemyPrefabs;
    int numEnemyTypes;
    public int maxQueueSize = 5;
    List<GameObject>[] enemyQueues; //0: left, 1: middle, 2: right

    public GameObject[] queueHeads;
    public GameObject[] queueTails;
    public Slider[] queueHealthBars; //Only the Enemy at the head has active health bar
    public Slider[] queueCountdownBars; //Only the Enemy at the head has active countdown bar
    public GameObject VanishingPoint;

    bool isAnimating = false;
    bool[] frontRowCleared;
    public float stepDuration = 0.5f;

    void SetTransform(GameObject enemy, int queueIndex, float t)
    {
        Vector3 startPos = queueHeads[queueIndex].transform.position;
        Vector3 endPos = queueTails[queueIndex].transform.position;
        enemy.transform.position = Vector3.Lerp(startPos, endPos, t);

        Vector3 startScale = queueHeads[queueIndex].transform.localScale;
        Vector3 endScale = queueTails[queueIndex].transform.localScale;
        float scaleFactor = enemy.GetComponent<Enemy>().scaleFactor;
        enemy.transform.localScale = Vector3.Lerp(startScale, endScale, t) * scaleFactor;
    }

    void UpdateSortingOrder(GameObject enemy, int index)
    {
        SpriteRenderer enemySR = enemy.GetComponent<SpriteRenderer>();
        if (enemySR != null)
        {
            enemySR.sortingOrder = -index;
        }
    }

    void spawnAt(SpawnPosition pos)
    {
        int queueIndex = (int)pos;
        // Check if queue is full
        if (enemyQueues[queueIndex].Count >= maxQueueSize) return;

        GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, numEnemyTypes)]);
        enemyQueues[queueIndex].Add(enemy);
        
        //based on the queue size, position the enemy accordingly and set its transform
        enemy.transform.SetParent(queueHeads[queueIndex].transform);

        int index = enemyQueues[queueIndex].Count - 1;
        // Calculate interpolation factor t based on index
        // Index 0 is at Head (t=0), larger indices towards Tail
        float t = (float)index / maxQueueSize; 

        SetTransform(enemy, queueIndex, t);
        UpdateSortingOrder(enemy, index);
        enemy.GetComponent<Enemy>().positionIndex = queueIndex;
    }

    public Enemy getFrontEnemy(int queueIndex)
    {
        if (enemyQueues[queueIndex].Count == 0) return null;
        return enemyQueues[queueIndex][0].GetComponent<Enemy>();
    }
    
    void Start()
    {
        instance = this;
        enemyQueues = new List<GameObject>[3];
        frontRowCleared = new bool[3];
        numEnemyTypes = enemyPrefabs.Length;
        //initialize enemy queues
        for (int i = 0; i < enemyQueues.Length; i++)
        {
            enemyQueues[i] = new List<GameObject>();
            for (int j = 0; j < maxQueueSize; j++)
            {
                spawnAt((SpawnPosition)i);
            }
            enemyQueues[i][0].GetComponent<Enemy>().setSliders(queueHealthBars[i], queueCountdownBars[i]);
            enemyQueues[i][0].GetComponent<Enemy>().isHeadOfQueue = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isAnimating) return;

        /* if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (hit.collider.gameObject == queueHeads[i])
                    {
                        TryKillEnemy(i);
                        break;
                    }
                }
            }
        } */
    }

    public void TryKillEnemy(int queueIndex)
    {
        if (frontRowCleared[queueIndex]) return;
        if (enemyQueues[queueIndex].Count == 0) return;

        // Kill the front enemy
        GameObject enemy = enemyQueues[queueIndex][0];
        enemyQueues[queueIndex].RemoveAt(0);
        Destroy(enemy);

        frontRowCleared[queueIndex] = true;

        // Check if all cleared
        bool allCleared = true;
        for (int i = 0; i < 3; i++)
        {
            if (!frontRowCleared[i])
            {
                allCleared = false;
                break;
            }
        }

        if (allCleared)
        {
            StartCoroutine(StepForwardRoutine());
        }
    }

    IEnumerator StepForwardRoutine()
    {
        isAnimating = true;

        // Spawn new enemies at the back
        for (int i = 0; i < 3; i++)
        {
            GameObject enemy = Instantiate(enemyPrefabs[Random.Range(0, numEnemyTypes)]);
            enemyQueues[i].Add(enemy);
            enemy.transform.SetParent(queueHeads[i].transform);

            // Set initial pos/scale to "Tail" (t=1.0)
            SetTransform(enemy, i, 1.0f);
            enemy.GetComponent<Enemy>().positionIndex = i;
            // Sorting order will be fixed at the end
        }

        float elapsed = 0;
        while (elapsed < stepDuration)
        {
            elapsed += Time.deltaTime;
            float p = elapsed / stepDuration;

            // Animate all enemies
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < enemyQueues[i].Count; j++)
                {
                    // Target index is j.
                    // Start index was j+1 (physically).
                    // Interpolate t from (j+1)/max to j/max.

                    float t_start = (float)(j + 1) / maxQueueSize;
                    float t_end = (float)j / maxQueueSize;
                    float t_current = Mathf.Lerp(t_start, t_end, p);

                    SetTransform(enemyQueues[i][j], i, t_current);
                }
            }
            yield return null;
        }

        // Finalize positions and sorting orders
        for (int i = 0; i < 3; i++)
        {
            frontRowCleared[i] = false;
            for (int j = 0; j < enemyQueues[i].Count; j++)
            {
                SetTransform(enemyQueues[i][j], i, (float)j / maxQueueSize);
                UpdateSortingOrder(enemyQueues[i][j], j);
            }
            // Set sliders for new front enemy
            enemyQueues[i][0].GetComponent<Enemy>().setSliders(queueHealthBars[i], queueCountdownBars[i]);
            enemyQueues[i][0].GetComponent<Enemy>().isHeadOfQueue = true;
        }

        isAnimating = false;
    }
}
