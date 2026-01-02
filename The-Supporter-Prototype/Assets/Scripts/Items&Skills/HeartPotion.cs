using UnityEngine;

public class HeartPotion : DraggableItem
{
    [Header("Heart Potion Settings")]
    [SerializeField] int healAmount;

    protected override bool ApplyEffect(GameObject target)
    {
        Adventurer adventurer = target.GetComponent<Adventurer>();
        if (adventurer != null)
        {
            adventurer.Heal(healAmount);
            return true;
        }
        return false;
    }
}
