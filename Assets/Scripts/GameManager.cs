using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

    
public class GameManager : NetworkBehaviour
{
    
    public static GameManager Instance;

    public NetworkVariable<int> currentTurn = new NetworkVariable<int>(0);

    private void Awake(){
            if(Instance!=null && Instance!=this)
           {
                Destroy(gameObject);
            }
            else
           {
               Instance = this;
           }
     }

    public void Start(){
        NetworkManager.Singleton.OnClientConnectedCallback += (clientId) => {
            Debug.Log(clientId + "joined");
            if(NetworkManager.Singleton.IsHost && NetworkManager.Singleton.ConnectedClients.Count == 2){
                SpawnBoard();
            }
        };
     }

    [SerializeField] private GameObject boardPrefab;
    private GameObject newBoard;

    private void SpawnBoard(){
        GameObject newBoard = Instantiate(boardPrefab);
        newBoard.GetComponent<NetworkObject>().Spawn();
    }

    public void StartHost(){
         NetworkManager.Singleton.StartHost();
     }

     public void StartClient(){
         NetworkManager.Singleton.StartClient();
     }

     [SerializeField] private GameObject gameOverPanel;
     [SerializeField] private TextMeshProUGUI msgText;

     public void ShowMsg(string msg){
        if(msg.Equals("won")){
            msgText.text = "You Won";
            gameOverPanel.SetActive(true);
            //Show winner text
            ShowOpponentMsg("You Lose");
        }
        else if(msg.Equals("draw")){
            msgText.text = "Game Draw";
            gameOverPanel.SetActive(true);
            ShowOpponentMsg("Game Draw");
        }
     }

     private void ShowOpponentMsg(string msg){
        if(IsHost){
            //use client RPC to show message on client side
            OpponentMsgClientRpc(msg);
        }
        else {
            //calls server RPC to show message at server side
            OpponentMsgServerRpc(msg);
        }
     }

     [ClientRpc]
     private void OpponentMsgClientRpc(string msg){
        if(IsHost) return;
        msgText.text = msg;
        gameOverPanel.SetActive(true);
     }

     [ServerRpc(RequireOwnership = false)]
     private void OpponentMsgServerRpc(string msg){
        msgText.text = msg;
        gameOverPanel.SetActive(true);
     }

     public void Restart(){
        //Checks if client, then Call ServerRpc to reset game
        //If Client, then call ServerRpc to hide results on host side
        if(!IsHost){
            RestartServerRpc();
            gameOverPanel.SetActive(false);
        }
        else{
            Destroy(newBoard);
            SpawnBoard();
            RestartClientRpc();
        }

        //Destroys current Game

        //Resets the Board

        //Hides the GameOver Screen
     }

     [ServerRpc(RequireOwnership = false)]
     private void RestartServerRpc(){
        Destroy(newBoard);
        SpawnBoard();
        gameOverPanel.SetActive(false);
     }
     [ClientRpc]
    private void RestartClientRpc(){
        gameOverPanel.SetActive(false);
    }
}
