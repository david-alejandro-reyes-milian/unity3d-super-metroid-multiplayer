using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class CharacterHealth : NetworkBehaviour
{
    public const int maxEnergy = 99;
    [SyncVar(hook = "UpdateEnergyOnGui")]
    public int energy = maxEnergy;
    CharacterMovement characterMovement;
    Animator anim;
    Rigidbody rigidbody;
    SpriteRenderer spriteRenderer;
    BackScreenController backScreenController;
    EnergyNumberSpriteRenderer energyNumberSpriteRenderer;

    Color transparentColor = new Color(1, 1, 1, 0);
    Color normalColor = new Color(1, 1, 1, 1);

    AudioClip itemCollectedSoundClip;

    public float recoveryWaitTime = 2;
    public float recoveryTime;
    public float recoveryAnimationWaitTime = .04f;
    public float recoveryAnimationTime;
    public bool inRecovery = false, receiveFriendFire = true;

    public float damageReceivedForceX = 200;
    public float damageReceivedForceY = 10;
    void Awake()
    {
        // Inicializacion
        characterMovement = GetComponent<CharacterMovement>();
        anim = transform.Find("CharacterSprite").GetComponent<Animator>();
        rigidbody = GetComponent<Rigidbody>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        backScreenController = GameObject.Find("BlackScreen").GetComponent<BackScreenController>();
        energyNumberSpriteRenderer = GameObject.Find("/GUI/TopPanel/EnergyNumber").GetComponent<EnergyNumberSpriteRenderer>();
        itemCollectedSoundClip = Resources.Load<AudioClip>("Sounds/LiveItemCollected_clean");
    }
    void Start()
    {
        // Al iniciar el caracter actualiza su GUI de energia de acuerdo a la energia actual
        if (isLocalPlayer)
            energyNumberSpriteRenderer.UpdateEnergyGui(energy);
    }

    void Update()
    {
        // Solo el jugador local actualiza su estado
        if (!isLocalPlayer) return;
        if (inRecovery && recoveryTime <= recoveryWaitTime)
        {
            recoveryTime += Time.deltaTime;
            RecoveryAnimation();
        }
        else { inRecovery = false; recoveryTime = 0; spriteRenderer.color = normalColor; }
    }


    void OnTriggerStay(Collider other)
    {
        if (!isLocalPlayer || !enabled) return;

        if ((other.tag == "EnemyAttack" || (other.tag == "Weapon" && receiveFriendFire)) && !inRecovery && energy > 0)
        {
            // Actualizando la salud. Si el jugador muere sale de la funcion
            energy = Mathf.Clamp(energy - 15, 0, 99);

            CmdUpdateEnergyOnServer(energy);

            if (energy <= 0) { PlayerDied(); return; }

            // Play DamageReceiveddAnimation
            anim.SetTrigger("DamageReceived");
            // Fuerza que aleja al caracter del enemigo
            Vector2 force = new Vector2(characterMovement.facingRight ?
                -damageReceivedForceX : damageReceivedForceX, damageReceivedForceY);
            rigidbody.AddForce(force);
            // Mientras esta en recuperacion el personaje no recibe dannos;
            inRecovery = true;
            //Cuando recibe danno siempre se retorna al estado de pie
            characterMovement.bodyState = 0;
        }
    }
    void RecoveryAnimation()
    {
        if (recoveryAnimationTime <= recoveryAnimationWaitTime) { recoveryAnimationTime += Time.deltaTime; }
        else
        {
            if (spriteRenderer.color.a == 0) { spriteRenderer.color = normalColor; }
            else { spriteRenderer.color = transparentColor; }
            recoveryAnimationTime = 0;
        }

    }

    [Command]
    void CmdUpdateEnergyOnServer(int energy) { this.energy = energy; }
    public void UpdateEnergyOnGui(int newEnergy)
    {
        if (!isLocalPlayer) return;
        energyNumberSpriteRenderer.UpdateEnergyGui(newEnergy);
    }

    /// <summary>
    /// Revive el jugador en un punto aleatorio reiniciando el estado de todas las variables
    /// </summary>
    public void RespawnPlayer()
    {
        GameObject[] startPoints = GameObject.FindGameObjectsWithTag("StartPoint");
        energy = maxEnergy;
        // Se habilita el script de salud y la fisica del caracter
        // Se habilita el control de movimiento del jugador
        this.enabled = true;
        rigidbody.isKinematic = false;
        characterMovement.enabled = true;

        // Se mueve el caracter a la nueva posicion tras la muerte
        transform.position = startPoints[Random.Range(0, startPoints.Length)].transform.position;

        //Se habilita el script de seguimiento
        GameObject cTarget = transform.Find("CameraTarget").gameObject;
        cTarget.GetComponent<CameraTargetAutoMove>().enabled = true;
        cTarget.transform.position = transform.position;

        // Se aclara la escena:
        backScreenController.turnScreenBlack = false;

        // Si en el momento de muerte se mira a la izquierda, se corrige la direccion del caracter para coincidir con el estado inicial
        if (!characterMovement.facingRight) characterMovement.Flip();
        anim.Rebind();
    }
    void PlayerDied()
    {
        // Se deshabilita el script de salud y se para la fisica del caracter
        // Se deshabilita el control de movimiento del jugador
        this.enabled = false;
        rigidbody.isKinematic = true;
        characterMovement.enabled = false;

        // Se mueve el caracter a la posicion de la camara para relizar la animacion de muerte
        transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z + .5f);

        //Se enfoca la camara al caracter para ver la animacion de muerte
        GameObject cTarget = transform.Find("CameraTarget").gameObject;
        cTarget.GetComponent<CameraTargetAutoMove>().enabled = false;
        cTarget.transform.position = transform.position;

        // Se oscurece el resto de la escena:
        backScreenController.turnScreenBlack = true;

        // Animacion de muerte
        anim.SetTrigger("Died");
    }
}
