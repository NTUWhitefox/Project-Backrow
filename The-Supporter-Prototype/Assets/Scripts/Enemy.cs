using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    //public bool clickDestroyable; //for queue function testing
    public bool isDummy; //for queue function testing
    public float scaleFactor;

    public float readyTime; 
    public int maxHealth;

    int currentHealth;
    float currentTime;

    public Slider healthBar;
    public Slider countdownBar;
    public int positionIndex; //0: left, 1: middle, 2: right
    public bool isHeadOfQueue = false;

    [SerializeField] int damagePerAttack;

    public void setSliders(Slider health, Slider countdown) //When this Enemy become the head of the queue
    {
        healthBar = health;
        countdownBar = countdown;
        //initialize UI bars
        healthBar.maxValue = maxHealth;
        countdownBar.maxValue = readyTime;
        currentHealth = maxHealth;
        currentTime = readyTime; 
    }

    void syncUI()
    {
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        if (countdownBar != null)
        {
            countdownBar.value = currentTime;
        }
    }

    void onCountDownComplete()
    {
        //do the action when countdown completes
        GameObject target = TeamManager.instance.getAdventurerAt(positionIndex);
        if (target != null)
        {
            target.GetComponent<Adventurer>().takeDamage(damagePerAttack);
        }
        
    }

    public void takeDamage(int damage)
    {
        if (!isHeadOfQueue) return;
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //handle enemy death
            EnemyManager.instance.TryKillEnemy(positionIndex);
        }
        syncUI();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!isHeadOfQueue) return;
        if (countdownBar != null)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = readyTime;
                onCountDownComplete();
            }
            syncUI();
        }
    }

    void OnDestroy()
    {
        
    }
}
