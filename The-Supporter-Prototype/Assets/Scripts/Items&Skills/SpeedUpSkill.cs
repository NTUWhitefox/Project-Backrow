using UnityEngine;

public class SpeedUpSkill : DraggableItem
{
    [Header("Speed Up Skill Settings")]
    [SerializeField] float speedMultiplier;
    [SerializeField] float duration;

    protected override bool ApplyEffect(GameObject target)
    {
        Adventurer adventurer = target.GetComponent<Adventurer>();
        if (adventurer != null)
        {
            adventurer.ApplySpeedBuff(speedMultiplier, duration);
            return true;
        }
        return false;
    }
}
