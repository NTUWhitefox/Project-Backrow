using System.Runtime.Serialization;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager instance;
    enum StandPosition
    {
        LEFT = 0,
        MIDDLE = 1,
        RIGHT = 2
    }
    public GameObject[] adventurers;

    public GameObject getAdventurerAt(int positionIndex)
    {
        GameObject obj = adventurers[positionIndex];
        if (obj == null || obj.GetComponent<Adventurer>().currentHealth <= 0)
        {
            return null;
        }

        return obj;
    }
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
