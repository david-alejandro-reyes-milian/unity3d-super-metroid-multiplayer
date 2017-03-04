using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CustomNetworkManager : NetworkManager
{
    public int networkPlayer = 1;
    private Color[] availableColors = new Color[] { Color.blue, Color.cyan, Color.green, Color.magenta, Color.red, Color.white, Color.yellow };
    private string[] availableNames = new string[] { "Tomas Ferguson", "Steve Jobs", "Bill Gates", "Obama12", 
        "Michael Jackson", "Anthony Hopkins"};
    public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
    {
        GameObject player = (GameObject)Instantiate(playerPrefab, startPositions[0].position, Quaternion.identity);

        // Se annade la informacion propia de cada jugador
        PlayerInfo pinfo = player.GetComponent<PlayerInfo>();
        Color randomColor = availableColors[Random.Range(0, availableColors.Length)];
        pinfo.colorVector = new Vector3(randomColor.r, randomColor.g, randomColor.b);
        pinfo.name = availableNames[Random.Range(0, availableNames.Length)]; ;
        pinfo.id = networkPlayer++;

        NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
    }
    public void PlayerWasKilled(GameObject oldPlayer)
    {
        var conn = oldPlayer.GetComponent<NetworkConnection>();
        var newPlayer = Instantiate<GameObject>(playerPrefab);
        Destroy(oldPlayer.gameObject);
        NetworkServer.ReplacePlayerForConnection(conn, newPlayer, 0);
    }
}
