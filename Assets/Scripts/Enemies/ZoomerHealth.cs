using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class ZoomerHealth : NetworkBehaviour
{
    [SyncVar]
    public int health = 3;
    public bool isAlive = true;
    Animator anim;
    ZoomerMovement movement;
    public GameObject[] bonusItems;
    public GameObject randomBonusItem;
    void Awake()
    {
        anim = GetComponent<Animator>(); movement = GetComponent<ZoomerMovement>();
        randomBonusItem = bonusItems[Random.Range(0, bonusItems.Length)];
    }

    [Server]
    void OnTriggerEnter(Collider other)
    {
        // Si entra un disparo proveniente de algun arma del caracter 
        // al area de colisiones y aun esta vivo el enemigo se actualiza el estado
        if (other.tag == "Weapon" && isAlive)
        {
            health--;
            anim.SetTrigger("AttackReceived");
        }
    }
    void Update()
    {
        if (health <= 0)
        {
            CmdMakeBonusItem();
            isAlive = false;
            // Se deshabilita el area de ataque para no afectar al caracter
            transform.Find("AttackArea").gameObject.SetActive(false);
            // Se para el movimiento del zoomer
            movement.enabled = false;
            // Se cambia el tag a cualquier otro para que los tiros no colisionen mas con el objeto
            gameObject.tag = "Untagged";
            // Se reproduce la animacion de muerte
            anim.SetTrigger("IsDead");
            // Se destruye el objeto pasado un tiempo
            Destroy(gameObject, .8f);
        }
    }
    [Command]
    void CmdMakeBonusItem()
    {
        if (isAlive)
        {
            GameObject clone = GameObject.Instantiate(randomBonusItem, transform.position, transform.rotation) as GameObject;
            NetworkServer.Spawn(clone);
        }
    }
}
