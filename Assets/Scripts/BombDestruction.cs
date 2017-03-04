using UnityEngine;
using System.Collections;

public class BombDestruction : MonoBehaviour
{
    public float destructionWaitTime = 2.0f;
    public float destructionTime = 0;
    public bool bombSoundEnabled = false;
    public bool turboFireEnabled = false;
    BombFactory bombFactory;
    void Awake()
    {
        bombSoundEnabled = Random.value >= .8f;
        //bombFactory = GameObject.Find("BombFactory").GetComponent<BombFactory>();
    }
    void Update()
    {
        destructionTime += Time.deltaTime;
        if (destructionTime >= destructionWaitTime)
        {
            gameObject.tag = "Weapon";
            Destroy(gameObject);
        }
    }
}

