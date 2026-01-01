using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Adventurer : MonoBehaviour
{
    [SerializeField] Slider healthBar;
    [SerializeField] Slider countdownBar;
    TMP_Text countDownText;
    [SerializeField] float readyTime; 
    [SerializeField] int maxHealth;
    public int currentHealth;
    float currentTime;
    [SerializeField] int damagePerAttack;
    public int positionIndex; //0: left, 1: middle, 2: right

    float speedMultiplier = 1.0f;
    float speedBuffDuration = 0.0f;
    void syncUI()
    {
        healthBar.value = currentHealth;
        countdownBar.value = currentTime;
        //one digit after decimal point
        countDownText.text = currentTime.ToString("F1");
    }
    void Start()
    {
        //initialize UI bars
        healthBar.maxValue = maxHealth;
        countdownBar.maxValue = readyTime;
        currentHealth = maxHealth;
        currentTime = readyTime; 
        countDownText = countdownBar.GetComponentInChildren<TMP_Text>();
        syncUI();
    }

    void Update()
    {
        //timer countdown
        if (currentTime <= 0)
        {
            currentTime = readyTime;
            //ready to act
            onCountDownComplete();
        }
        if (speedBuffDuration <= 0.0f) {
            speedMultiplier = 1.0f;
            speedBuffDuration = 0.0f;
        }
        currentTime -= Time.deltaTime * speedMultiplier;
        syncUI();
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            //handle adventurer death
            Debug.Log("Adventurer has fallen!");
        }
        syncUI();
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        syncUI();
    }

    public void ApplySpeedBuff(float multiplier, float duration)
    {
        speedMultiplier = multiplier;
        speedBuffDuration = duration;
    }

    void onCountDownComplete()
    {
        //do the action when countdown completes
        // attack the enemy in front
        Enemy targetEnemy = EnemyManager.instance.getFrontEnemy(positionIndex);
        if (targetEnemy != null && targetEnemy.isHeadOfQueue)
        {
            targetEnemy.takeDamage(damagePerAttack);
        } else
        {
            Debug.Log("No enemy to attack!");
        }
    }
}
