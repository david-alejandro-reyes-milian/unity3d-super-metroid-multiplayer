using UnityEngine;
using System.Collections;

public class ShipController : MonoBehaviour
{
    #region Public variables
    Rigidbody rigidbody;
    public bool playerIn = false;
    public float speed = 5;
    public float brakeSpeed = .05f;
    public bool isBeingControlled = false;
    public float axisX = 0, axisY = 0;
    public CharacterMovement characterMovement;
    #endregion

    void Start() { rigidbody = GetComponentInChildren<Rigidbody>(); }
    void FixedUpdate()
    {
        // Se controla la nave segun los valores de entrada horizontal y vertical
        rigidbody.AddRelativeForce(axisX * speed, 0, 0);
        rigidbody.AddRelativeForce(0, axisY * speed, 0);

        // Si no se esta controlando la nave, se detiene su movimiento, tener en cuenta
        // que en el espacio no hay friccion
        if (!isBeingControlled) { rigidbody.velocity = Vector3.Lerp(rigidbody.velocity, Vector3.zero, brakeSpeed); }
    }
    void Update()
    {
        if (!playerIn) return;
        if (Input.GetKeyUp(KeyCode.C))
        {
            characterMovement.transform.parent = null;
            characterMovement.gameObject.SetActive(true);
            if (!characterMovement.facingRight) characterMovement.Flip();
            characterMovement.facingRight = true;
            characterMovement.enabled = true;
            //characterMovement.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            playerIn = false;
            isBeingControlled = false;
            axisX = axisY = 0f;
            return;
        }

        axisX = Input.GetAxis("Horizontal");
        axisY = Input.GetAxis("Vertical");
        isBeingControlled = Mathf.Abs(axisX) >= .5f || Mathf.Abs(axisY) >= .5f;
    }
}
