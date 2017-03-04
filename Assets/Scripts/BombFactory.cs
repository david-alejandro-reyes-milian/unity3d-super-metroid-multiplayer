using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BombFactory : NetworkBehaviour
{
    public GameObject bombPrefab;
    [SyncVar]
    public Queue bombs;
    int initialSize = 5;
    void Awake()
    {
        bombs = new Queue(initialSize);
        ExpandFactory();
    }

    public GameObject GetBomb(Vector3 position, Quaternion rotation)
    {
        if (bombs.Count <= 0) ExpandFactory();
        GameObject bomb = bombs.Dequeue() as GameObject;
        bomb.transform.position = position;
        bomb.transform.rotation = rotation;
        return Reset(bomb);
    }

    GameObject Reset(GameObject bomb)
    {
        bomb.GetComponent<Rigidbody>().velocity = Vector3.zero;
        bomb.SetActive(true);
        bomb.GetComponent<Rigidbody>().useGravity = true;
        bomb.GetComponent<BoxCollider>().isTrigger = false;
        bomb.tag = "Bomb";
        return bomb;
    }
    public void Destroy(GameObject bomb)
    {
        bomb.SetActive(false);
        bomb.GetComponent<BombDestruction>().destructionTime = 0;
        bomb.GetComponentInChildren<Animator>().Rebind();
        bombs.Enqueue(bomb);
    }
    [Server]
    void ExpandFactory()
    {
        for (int i = 0; i < initialSize; i++)
        {
            GameObject bomb = Instantiate(bombPrefab);
            bomb.SetActive(false);
            bombs.Enqueue(bomb);
            NetworkServer.Spawn(bomb);
        }
    }

}
