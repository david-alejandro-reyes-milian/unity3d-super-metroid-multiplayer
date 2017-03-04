using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using UnityEngine.Networking;
public class CharacterMovement : NetworkBehaviour
{
    /// <summary>
    /// Coeficiente de velocidad máxima que alcanza el jugador
    /// </summary>
    public float maxSpeed = 1.8f;
    /// <summary>
    /// Indica si el caracter mira hacia la derecha
    /// </summary>
    public bool facingRight = true;
    /// <summary>
    /// Velocidad de movimiento en <X>
    /// </summary>
    public float moveSpeedX;
    /// <summary>
    /// Velocidad de movimiento en <Y>
    /// </summary>
    public float moveSpeedY;

    // Character State
    /// <summary>
    /// Estado del cuerpo del caracter. 
    /// Puede ser una de las siguientes opciones:
    /// - playerStandingConst: El caracter está parado completamente
    /// - playerOnKneesConst: El caracter está de rodillas
    /// - playerAsBallConst: El caracter está en modo de esfera
    /// /// </summary>
    public int bodyState = 0;
    /// Estado del cuerpo del caracter mientras está en el aire.
    /// Puede ser una de las siguientes opciones:
    /// - playerStandingConst: El caracter está parado completamente
    /// - playerOnKneesConst: El caracter está de rodillas
    /// - playerAsBallConst: El caracter está en modo de esfera
    /// /// </summary>
    public int bodyStateAir = 0;

    //Ship
    public bool isOnShipEnter = false;
    ShipController shipController;

    // Jumps
    public int maxJumpCount = 5;
    public int jumpCount = 0;
    public float jumpSpeed = 70;

    private const int playerStandingConst = 0;
    private const int playerOnKneesConst = 1;
    private const int playerAsBallConst = 2;
    // Alturas relativas del collider del caracter en cada uno de los estados del cuerpo
    private const float playerStandingColliderheigthConst = .43f;
    private const float playerOnKneesColliderheigthConst = .33f;
    private const float playerAsBallColliderheigthConst = .0f;



    // Physics
    private Rigidbody rigidbody;
    private CapsuleCollider collider;
    public Transform groundCheck, obstacleCheck;
    public float groundRadius = 0.0001f;
    public LayerMask whatIsGround;
    public LayerMask whatIsObstacle;
    public LayerMask whatIsShipEnter;
    public bool grounded = false;
    public bool touchingObstacle = false;
    public bool goingUp = false;


    // Turning
    public float turnWaitTime = .2f;
    public float turnTime = 0;
    public bool turning = false;

    // Shotting
    public float shotSpeed = 200;
    public float shotWaitTime = .5f;
    public float lastShotTime = 0;

    public bool shootingOnAir = false;

    public GameObject currentShotSpawn;
    public Rigidbody shotPrefab;
    public Rigidbody bombPrefab;
    public Rigidbody currentWeaponPrefab;
    Rigidbody clone;

    public int aimingDirection = 0;
    // Estados en que puede estar el cañon
    private const int aimingIdleConst = 0;
    private const int aimingUpConst = 1;
    private const int aimingUpFrontConst = 2;
    private const int aimingFrontConst = 3;
    private const int aimingDownFrontConst = 4;
    private const int aimingDownConst = 5;

    // Weapon spawns:
    GameObject canonIdleSpawn, canonAimingFrontSpawn, canonAimingUpFrontSpawn,
        canonAimingDownFrontSpawn, canonAimingUp;
    GameObject canonIdleSpawnAir, canonAimingFrontSpawnAir, canonAimingUpFrontSpawnAir,
       canonAimingDownFrontSpawnAir, canonAimingUpAir, canonAimingDownSpawnAir;
    GameObject canonAimingFrontSpawnKnees, canonAimingUpFrontSpawnKnees,
           canonAimingDownFrontSpawnKnees, canonAimingUpSpawnKnees;
    //Sounds 
    AudioClip baseShotSound, bombSound;

    // Controlador de la animacion del caracter
    private Animator anim;

    // Texto de identificacion del jugador que se muestra encima
    public TextMesh playerIdText;

    public bool playerControlEnabled = true;

    // Pool de bombas, TODO falta retomar la idea del pool en modo multiplayer
    //BombFactory bombFactory;

    void Awake()
    {
        collider = GetComponent<CapsuleCollider>();

        rigidbody = GetComponent<Rigidbody>();
        // Para que el rigidbody no deje de recibir eventos mientras esta inmovil
        rigidbody.sleepThreshold = 0.0f;

        // Para comprobar cuando se toca el suelo
        groundCheck = transform.Find("GroundCheck").transform;

        // Para comprobar si se toca un obstaculo
        obstacleCheck = transform.Find("ObstacleCheck").transform;

        // Objeto que manipula las animaciones
        anim = transform.Find("CharacterSprite").GetComponent<Animator>();

        // Control de la nave
        shipController = GameObject.Find("Ship").GetComponent<ShipController>();

        currentWeaponPrefab = shotPrefab;
        // Inicializando posiciones de cañon
        canonIdleSpawn = transform.Find("CanonAiming/CanonIdleSpawn").gameObject;
        canonAimingFrontSpawn = transform.Find("CanonAiming/CanonAimingFrontSpawn").gameObject;
        canonAimingUpFrontSpawn = transform.Find("CanonAiming/CanonAimingUpFrontSpawn").gameObject;
        canonAimingDownFrontSpawn = transform.Find("CanonAiming/CanonAimingDownFrontSpawn").gameObject;
        canonAimingUp = transform.Find("CanonAiming/CanonAimingUp").gameObject;

        canonAimingFrontSpawnAir = transform.Find("CanonAimingAir/CanonAimingFrontSpawn").gameObject;
        canonAimingUpFrontSpawnAir = transform.Find("CanonAimingAir/CanonAimingUpFrontSpawn").gameObject;
        canonAimingDownFrontSpawnAir = transform.Find("CanonAimingAir/CanonAimingDownFrontSpawn").gameObject;
        canonAimingUpAir = transform.Find("CanonAimingAir/CanonAimingUp").gameObject;
        canonAimingDownSpawnAir = transform.Find("CanonAimingAir/CanonAimingDownSpawn").gameObject;

        canonAimingFrontSpawnKnees = transform.Find("CanonAimingKnees/CanonAimingFrontSpawn").gameObject;
        canonAimingUpFrontSpawnKnees = transform.Find("CanonAimingKnees/CanonAimingUpFrontSpawn").gameObject;
        canonAimingDownFrontSpawnKnees = transform.Find("CanonAimingKnees/CanonAimingDownFrontSpawn").gameObject;
        canonAimingUpSpawnKnees = transform.Find("CanonAimingKnees/CanonAimingUpSpawn").gameObject;

        //bombFactory = GameObject.Find("BombFactory").GetComponent<BombFactory>();

        // Cargando sonidos
        baseShotSound = Resources.Load("Sounds/BaseShot_clean", typeof(AudioClip)) as AudioClip;
        bombSound = Resources.Load("Sounds/Bomb", typeof(AudioClip)) as AudioClip;

        playerIdText = transform.GetComponentInChildren<TextMesh>();
    }
    void Start()
    {
        playerIdText.text = "Player " + (netId.Value);
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        anim.SetFloat("MoveSpeedX", moveSpeedX);
        anim.SetFloat("MoveSpeedY", moveSpeedY);
        anim.SetBool("GoingUp", goingUp);
        anim.SetInteger("BodyState", bodyState);

        // Se ajusta el cuerpo del caracter segun el bodyState
        AdjustBody();

        // Se chekea si se toca el suelo y se actualizan los estados de la animacion
        grounded =
            Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsGround);

        // Se chekea si esta en la entrada de la nave
        isOnShipEnter = Physics2D.OverlapCircle(groundCheck.position, groundRadius, whatIsShipEnter);

        anim.SetBool("Grounded", grounded);
        // Si se esta sobre el suelo se inicializan los estados de salto y disparos en el aire
        if (grounded)
        {
            jumpCount = 0;
            shootingOnAir = false;
            bodyStateAir = playerStandingConst;
        }
        anim.SetBool("ShootingOnAir", shootingOnAir);

        // Se chekea si se toca algun obstaculo
        touchingObstacle =
            Physics2D.OverlapCircle(obstacleCheck.position, groundRadius, whatIsObstacle);

        // En funcion del movimiento se cambia la orientacion del personaje
        if ((moveSpeedX > 0.0f && !facingRight) || (moveSpeedX < 0.0f && facingRight)) { Flip(); }

        // Mientras ocurre el giro del personaje no ocurre movimiento
        if (!turning)
        {
            anim.SetBool("Turning", turning);
            // Si se toca un obstaculo se para el movimiento del cuerpo
            if (!touchingObstacle)
                rigidbody.velocity =
                new Vector2(moveSpeedX * maxSpeed, rigidbody.velocity.y);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) { SyncOnlinePlayer(); return; }

        // Se captura la direccion y velocidad del movimiento horizontal
        // Controles para movil:
        if (Application.platform == RuntimePlatform.Android)
        {
            moveSpeedX = CrossPlatformInputManager.GetAxis("Horizontal") > .2f ? 1 : CrossPlatformInputManager.GetAxis("Horizontal") < -.2f ? -1 : 0;
            moveSpeedY = CrossPlatformInputManager.GetAxis("Vertical") > .2f ? 1 : CrossPlatformInputManager.GetAxis("Vertical") < -.2f ? -1 : 0;
        }
        else
        {
            // Controles PC
            //moveSpeedX = Input.GetAxis("Horizontal");
            moveSpeedX = Input.GetKey(KeyCode.D) ? 1 : Input.GetKey(KeyCode.A) ? -1 : 0;
            moveSpeedY = Input.GetKey(KeyCode.W) ? 1 : Input.GetKey(KeyCode.S) ? -1 : 0;
        }

        HandleBodyState();
        HandleJump();
        HandleAimingDirection();
        HandleShipInteraction();

        // Se actualiza el estado del giro
        if (turning)
        {
            turnTime += Time.deltaTime;
            if (turnTime >= turnWaitTime)
            {
                turning = false;
                turnTime = 0;
            }
        }

        // Se controla la frecuencia de disparos
        if (lastShotTime >= shotWaitTime && CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            lastShotTime = 0;
            Attack();
        }
        // Turbo tiro woaaaaaaaaaaaw!!!!!
        else if (lastShotTime >= shotWaitTime && CrossPlatformInputManager.GetButton("Fire2"))
        {
            lastShotTime = 0;
            Attack();
        }
        else { lastShotTime += Time.deltaTime; }

        // Cambiar velocidad de turbo con rueda del mouse!
        shotWaitTime += Input.mouseScrollDelta.y * .01f;
        shotWaitTime = Mathf.Abs(shotWaitTime);

        if (Input.GetKeyDown(KeyCode.LeftShift)) { CmdChangeWeapon(); }
    }

    void AdjustBody()
    {
        // Se ajusta el collider del cuerpo y el punto de checkeo del suelo de acuerdo al tamanno del cuerpo
        if (bodyState == playerStandingConst && collider.height != playerStandingColliderheigthConst)
        {
            collider.height = playerStandingColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.2158f);
        }
        else if (bodyState == playerOnKneesConst && collider.height != playerOnKneesColliderheigthConst)
        {
            collider.height = playerOnKneesColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.1598f);
        }
        else if (bodyState == playerAsBallConst && collider.height != playerAsBallColliderheigthConst)
        {
            collider.height = playerAsBallColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.0808f);
        }
        else if (bodyStateAir == playerAsBallConst && collider.height != playerAsBallColliderheigthConst)
        {
            collider.height = playerAsBallColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.0808f);
            bodyState = bodyStateAir;
        }
    }
    void AdjustBody(int bodyState)
    {
        // Se ajusta el collider del cuerpo y el punto de checkeo del suelo de acuerdo al tamanno del cuerpo
        if (bodyState == playerStandingConst && collider.height != playerStandingColliderheigthConst)
        {
            collider.height = playerStandingColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.2158f);
        }
        else if (bodyState == playerOnKneesConst && collider.height != playerOnKneesColliderheigthConst)
        {
            collider.height = playerOnKneesColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.1598f);
        }
        else if (bodyState == playerAsBallConst && collider.height != playerAsBallColliderheigthConst)
        {
            collider.height = playerAsBallColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.0808f);
        }
        else if (bodyStateAir == playerAsBallConst && collider.height != playerAsBallColliderheigthConst)
        {
            collider.height = playerAsBallColliderheigthConst;
            groundCheck.transform.localPosition = new Vector2(0, -0.0808f);
            bodyState = bodyStateAir;
        }
    }
    void HandleAimingDirection()
    {
        if (moveSpeedX == 0 && moveSpeedY == 0)
        {
            aimingDirection = aimingIdleConst;//idle
            // Si el caracter esta de rodillas, usa el spawn especial de esa posicion
            currentShotSpawn = bodyState == playerOnKneesConst ? canonAimingFrontSpawnKnees : canonIdleSpawn;
        }
        else if (moveSpeedX == 0 && moveSpeedY > 0)
        {
            aimingDirection = aimingUpConst;//up
            currentShotSpawn = shootingOnAir ? canonAimingUpAir : bodyState == playerOnKneesConst ? canonAimingUpSpawnKnees : canonAimingUp;
        }
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY > 0)
        {
            aimingDirection = aimingUpFrontConst;//up-right 45 grados
            currentShotSpawn = shootingOnAir ? canonAimingUpFrontSpawnAir : bodyState == playerOnKneesConst ? canonAimingUpFrontSpawnKnees : canonAimingUpFrontSpawn;
        }
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY == 0)
        {
            aimingDirection = aimingFrontConst;//front
            currentShotSpawn = shootingOnAir ? canonAimingFrontSpawnAir : canonAimingFrontSpawn;
        }
        else if (Mathf.Abs(moveSpeedX) > 0 && moveSpeedY < 0)
        {
            aimingDirection = aimingDownFrontConst;//down-right 315 grados 
            currentShotSpawn = shootingOnAir ? canonAimingDownFrontSpawnAir : bodyState == playerOnKneesConst ? canonAimingDownFrontSpawnKnees : canonAimingDownFrontSpawn;
        }
        else if (moveSpeedX == 0 && moveSpeedY < 0)
        {
            aimingDirection = aimingDownConst;//down 270 grados
            currentShotSpawn = canonAimingDownSpawnAir;
        }

        // Apuntando arriba-delante
        if (Input.GetKey(KeyCode.R) && isLocalPlayer || aimingDirection == aimingUpFrontConst)
        {
            aimingDirection = aimingUpFrontConst;
            currentShotSpawn = shootingOnAir || bodyState == playerOnKneesConst ?
                canonAimingUpFrontSpawnKnees : canonAimingUpFrontSpawn;

        }
        // Apuntando debajo-delante
        if (Input.GetKey(KeyCode.F) && isLocalPlayer || aimingDirection == aimingDownFrontConst)
        {
            aimingDirection = aimingDownFrontConst;
            currentShotSpawn = shootingOnAir || bodyState == playerOnKneesConst ?
                canonAimingDownFrontSpawnKnees : canonAimingDownFrontSpawn;
        }

        anim.SetInteger("AimingDirection", aimingDirection);
        // Una copia como float usada en arbol de mezclas
        anim.SetFloat("AimingDirectionF", aimingDirection);

        // Si cambia el objetivo de disparo o se salta se deshabilita el disparo al frente
        if (aimingDirection != aimingFrontConst || !grounded) { anim.SetBool("ShootingFront", false); }
    }

    void HandleBodyState()
    {
        if (grounded)
        {
            // Agacharse con S
            if (Input.GetKeyDown(KeyCode.S) && moveSpeedX == 0) { bodyState = bodyState < 2 ? bodyState + 1 : 2; }
            // Agacharse con W
            else if (Input.GetKeyDown(KeyCode.W)) { bodyState = bodyState > 0 ? bodyState - 1 : 0; }
            if (moveSpeedX != 0 && bodyState == playerOnKneesConst) bodyState = playerStandingConst;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.S) && moveSpeedX == 0) { bodyStateAir = bodyStateAir < 2 ? bodyStateAir + 1 : 2; }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                bodyStateAir = bodyStateAir > 0 ? bodyStateAir - 1 : 0;
                // Mientras se encuentra en el aire si se presiona arriba mientras esta en forma de bola se transforma
                if (bodyState == playerAsBallConst && !grounded)
                {
                    bodyStateAir = playerStandingConst;
                    bodyState = playerStandingConst;
                }
            }
        }
    }
    void HandleShipInteraction()
    {
        //Entrar a la nave
        if (isOnShipEnter && Input.GetKeyUp(KeyCode.C))
        {
            shipController.playerIn = true;
            this.enabled = false;
            gameObject.transform.parent = shipController.transform;
            gameObject.SetActive(false);
            //rigidbody.isKinematic = true;
        }
    }
    void SyncOnlinePlayer()
    {
        // Se toma el estado del cuerpo desde las propiedades del animator del jugador
        int currentBodyState = anim.GetInteger("BodyState");
        AdjustBody(currentBodyState);
    }

    /// <summary>
    /// Maneja el comportamiento de los saltos segun la entrada del usuario y el estado del caracter
    /// </summary>
    void HandleJump()
    {
        /// Incialmente se captura el estado del salto para reaccionar en caida libre y animaciones.
        if (rigidbody.velocity.y >= 0.5f) goingUp = true; else goingUp = false;

        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            /// Si modo bola, se actualiza el estado del cuerpo a de rodillas
            if (bodyState == playerAsBallConst)
            {
                if (grounded) { bodyState = playerOnKneesConst; }
                else { bodyState = playerStandingConst; bodyStateAir = playerStandingConst; }
            }

            // Si el personaje esta en el suelo o si kedan saltos permisibles aun y se presiona saltar
            else if ((grounded || jumpCount < maxJumpCount))
            {
                // Se annade una fuerza vertical (Salto)
                rigidbody.AddForce(new Vector2(0.0f, jumpSpeed));
                // Se actualiza la cantidad de saltos
                if (jumpCount < maxJumpCount && !grounded) jumpCount++;
                // Al saltar se deshabilita el disparo hacia el frente
                anim.SetBool("ShootingFront", false);

                // Al saltar se reinicia el estado del jugador en el suelo
                bodyState = playerStandingConst;
                bodyStateAir = playerStandingConst;
            }
        }
        // Al soltar el boton de salto se detiene el movimiento de salto
        if (CrossPlatformInputManager.GetButtonUp("Jump") && goingUp) { rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0f); }
    }

    /// <summary>
    /// Gira el pesonaje 180 grados en el eje <Y> 
    /// y actualiza el estado de las animaciones
    /// </summary>
    public void Flip()
    {
        // Se activa el estado de giro
        turning = true;
        transform.Rotate(Vector3.up, 180.0f, Space.World);
        // Se ignora el giro para el CharacterSprite porque la animacion 
        // es asimetrica y el giro ocurre dentro de la animacion
        anim.transform.Rotate(Vector3.up, 180.0f, Space.World);

        facingRight = !facingRight;
        anim.SetBool("FacingRight", facingRight);
        anim.SetBool("Turning", turning);
        anim.SetBool("ShootingFront", false);
    }

    void Attack()
    {
        if (bodyState == playerAsBallConst)
        { CmdCreateBombOnServer(); }
        else { Shoot(); }
    }

    void Shoot()
    {
        // En el suelo no se puede disparar hacia abajo
        if (aimingDirection == aimingDownConst && grounded) return;
        // Sonido del disparo
        Camera.main.GetComponent<AudioSource>().PlayOneShot(baseShotSound, .3f);

        Vector3 newPosition = currentShotSpawn.transform.position;
        Quaternion newRotation = currentShotSpawn.transform.rotation;
        //if (!facingRight) newRotation = 
        //    new Quaternion(newRotation.x, newRotation.y + 180, newRotation.z, newRotation.w);

        Vector3 velocity = rigidbody.velocity;
        // Si se apunta horizontalmente se quita la componente <y> de la velocidad
        // para que los tiros tengan trayectoria horizontal recta
        if (aimingDirection == aimingFrontConst || aimingDirection == aimingIdleConst)
        {
            Vector2 horizontalOnlyVelocity = velocity;
            horizontalOnlyVelocity.y = 0;
            velocity = horizontalOnlyVelocity;
        }
        Vector3 direction = currentShotSpawn.transform.right;

        // Si al atacar se apunta al frente se habilita la bandera
        if (aimingDirection == aimingFrontConst)
            anim.SetBool("ShootingFront", true);

        if (!grounded) { shootingOnAir = true; }

        CmdCreateShootOnServer(newPosition, newRotation, velocity, direction);
    }

    [Command(channel = 1)]
    void CmdCreateShootOnServer(Vector3 newPosition, Quaternion newRotation, Vector3 velocity, Vector3 direction)
    {
        // Se crea la bala en cada instancia del juego
        clone = Instantiate(currentWeaponPrefab) as Rigidbody;

        NetworkServer.Spawn(clone.gameObject);
        // Se annade fuerza al disparo en cada instancia del juego
        RpcAddBulletForce(clone.gameObject, velocity, direction, newPosition, newRotation);
    }
    [ClientRpc]
    void RpcAddBulletForce(GameObject bullet, Vector3 velocity, Vector3 direction, Vector3 newPosition, Quaternion newRotation)
    {
        bullet.transform.position = newPosition;
        bullet.transform.rotation = newRotation;
        Rigidbody bRigidbody = bullet.GetComponent<Rigidbody>();
        bRigidbody.velocity = velocity;
        bRigidbody.AddForce(direction * shotSpeed);
    }

    [Command(channel = 1)]
    void CmdCreateBombOnServer()
    {
        //clone = bombFactory.GetBomb(transform.position, transform.rotation).GetComponent<Rigidbody>();
        clone = Instantiate(bombPrefab, transform.position, transform.rotation) as Rigidbody;
        NetworkServer.Spawn(clone.gameObject);
    }
    [Command]
    void CmdChangeWeapon()
    {
        if (currentWeaponPrefab == shotPrefab) currentWeaponPrefab = bombPrefab;
        else currentWeaponPrefab = shotPrefab;
    }
}
