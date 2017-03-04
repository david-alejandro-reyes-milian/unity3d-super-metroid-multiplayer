using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerInfo : NetworkBehaviour
{
    [SyncVar]
    public string name = "Player";
    [SyncVar(hook = "OnColorVectorUpdated")]
    public Vector3 colorVector;
    [SyncVar]
    public int id;

    void Start()
    {
        // Ocurre para todos los clientes que se conectan nuevos al juego actualizando los datos
        // del resto de clientes en su instancia del juego
        setPlayerColor(colorVector);
        setPlayerName(name);
    }

    void OnColorVectorUpdated(Vector3 colorVector)
    {
        setPlayerColor(colorVector);
    }
    void setPlayerColor(Vector3 colorVector)
    {
        Color color = new Color(colorVector.x, colorVector.y, colorVector.z);
        ChatController chatController = GetComponent<ChatController>();
        chatController.chatColor = color;
        chatController.chatColorString = ChatController.getColorString(color);
        GetComponentInChildren<TextMesh>().color = color;

        // Se habilita el indicador para el usuario local asignando el color del jugador
        if (isLocalPlayer)
        {
            GameObject indicator = transform.Find("CharacterSprite/ArrowIndicator").gameObject;
            indicator.GetComponent<SpriteRenderer>().color = chatController.chatColor;
            indicator.SetActive(true);
        }

    }
    void OnNameUpdated(string name)
    {
        Debug.Log("OnNameUpdated by " + netId);
        setPlayerColor(colorVector);
    }
    void setPlayerName(string name)
    {
        GetComponentInChildren<TextMesh>().text = name;
    }
}
