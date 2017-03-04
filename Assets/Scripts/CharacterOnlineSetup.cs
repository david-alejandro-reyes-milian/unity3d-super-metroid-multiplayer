using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CharacterOnlineSetup : NetworkBehaviour
{
    SmothFollow smoothFollow;
    public GameObject guiPrefab;
    public ChatController chatController;
    public ShipController shipController;
    private Color[] availableColors = new Color[] { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };
    GameObject arrowIndicator;

    public override void OnStartLocalPlayer()
    {
        // Asignar el seguimiento de la camara al jugador local
        smoothFollow = Camera.main.GetComponent<SmothFollow>();
        smoothFollow.cameraTarget = transform.Find("CameraTarget");
        smoothFollow.enabled = true;

        // Se inicializan los scripts dentro del controlador del chat
        chatController = GetComponent<ChatController>();
        chatController.characterMovement = GetComponent<CharacterMovement>();
        chatController.characterHealth = GetComponent<CharacterHealth>();

        shipController = GameObject.Find("Ship").GetComponent<ShipController>();
        shipController.characterMovement = GetComponent<CharacterMovement>();

        GameObject.Find("NetworkManager").GetComponent<NetworkManagerHUD>().showGUI = false;

    }
}
