using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking;

public class ChatController : NetworkBehaviour
{
    GameObject chatInput;
    GameObject chatBoard;
    InputField chatInputField;
    // Al actualizarse la conversacion, se actualizan todos los clientes
    [SyncVar(hook = "UpdateChat")]
    public string conversation = "";

    public Color chatColor;
    public string chatColorString = "";
    private string chatMessageTemplate = "";

    Text chatText;
    public CharacterMovement characterMovement;
    public CharacterHealth characterHealth;

    void Start()
    {
        // Plantilla usada para enviar mensajes al servidor
        chatMessageTemplate = "<color={2}><b>" + GetComponent<PlayerInfo>().name + ":</b> {0}\n{1}</color>";

        chatInput = GameObject.Find("/Chat/Canvas/Input").gameObject;
        chatInputField = chatInput.GetComponentInChildren<InputField>();
        chatText = GameObject.Find("/Chat/Canvas/ChatBoard/ChatText").GetComponent<Text>();

        // Inicialmente se oculta la entrada de texto del chat y se reinicia el string
        chatInput.SetActive(false);
        chatText.text = "";
    }

    void UpdateChat(string updatedConversation) { chatText.GetComponent<Text>().text = updatedConversation; }

    void Update()
    {
        // Se conmuta la entrada de texto del chat para el usuario local
        if ((Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) && isLocalPlayer)
        {
            if (chatInput.active)
            {

                chatInput.SetActive(!chatInput.active);
                // Se captura la conversacion actualizada con el mensaje del usuario
                if (chatInputField.text != "")
                {
                    string updatedConversation = string.Format(chatMessageTemplate, chatInputField.text, chatText.text, chatColorString);
                    // Se envia el texto al server para mostrar al resto de jugadores y se reinica la entrada de texto
                    CmdSendMessageToServer(updatedConversation);
                }

                chatInputField.text = "";
                setCharacterChatState(true);
            }
            else
            {
                // Se muestra la ventana de chat y se deshabilita el movimiento del jugador actual
                chatInput.SetActive(!chatInput.active);
                chatInputField.ActivateInputField();
                setCharacterChatState(false);
            }
        }

    }
    void setCharacterChatState(bool enabled)
    {
        // Se cambian los estados del control del jugador y el controlador de salud
        characterMovement.enabled = enabled;
        characterHealth.enabled = enabled;
    }

    [Command]
    void CmdSendMessageToServer(string updatedConversation) { conversation = updatedConversation; }

    public static string getColorString(Color color) { return "#" + ColorUtility.ToHtmlStringRGB(color); }

}
