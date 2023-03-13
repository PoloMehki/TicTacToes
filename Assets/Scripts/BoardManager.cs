using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class BoardManager : NetworkBehaviour
{
    Button[,] buttons = new Button[3,3];

    public override void OnNetworkSpawn(){
        var cells = GetComponentsInChildren<Button>();
        int n =0;
        for(int i =0; i < 3; i++){
            for(int k = 0; k < 3; k++){
                buttons[i,k] = cells[n];
                n++;

                int r = i;
                int c = k;

                buttons[i,k].onClick.AddListener(delegate{
                    OnClickCell(r,c);
                });
            }
        }
    }

    [SerializeField] private Sprite xSprite, oSprite;

    private void OnClickCell(int r, int c){

        //If Button is clicked by host set sprite to X

        if(NetworkManager.Singleton.IsHost && GameManager.Instance.currentTurn.Value ==0){
            buttons[r,c].GetComponent<Image>().sprite = xSprite;
            //Lock button
            buttons[r,c].interactable = false;
            //Also update client side
            ChangeSpriteClientRpc(r,c);
            //Checks if won/not
            CheckResult(r, c);
            GameManager.Instance.currentTurn.Value = 1;
            

        }

        //If button is clicked by client set sprite to O

        else if(!NetworkManager.Singleton.IsHost && GameManager.Instance.currentTurn.Value == 1){
            buttons[r,c].GetComponent<Image>().sprite = oSprite;
            //lock button
            buttons[r,c].interactable = false;
            //Checks if won/not
            CheckResult(r, c);
            //Also update host side
            ChangeSpriteServerRpc(r, c);
        }
    }

    [ClientRpc]
    private void ChangeSpriteClientRpc(int r, int c){
        buttons[r, c].GetComponent<Image>().sprite = xSprite;
        buttons[r,c].interactable = false;
    }
    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpriteServerRpc(int r, int c){
        buttons[r, c].GetComponent<Image>().sprite = oSprite;
        buttons[r,c].interactable = false;
        GameManager.Instance.currentTurn.Value = 0;
    }

    private void CheckResult(int r, int c){
        if(IsWon(r, c)){
            GameManager.Instance.ShowMsg("won");
        }
        else{
            if(IsGameDraw()){
                GameManager.Instance.ShowMsg("draw");
            }
        }
    }



    //Win Condition
    public bool IsWon(int r, int c){
        Sprite clickedButtonSprite = buttons[r,c].GetComponent<Image>().sprite;

        //column
        if(buttons[0,c].GetComponentInChildren<Image>().sprite == clickedButtonSprite 
        && buttons[1,c].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[2,c].GetComponentInChildren<Image>().sprite == clickedButtonSprite){
            return true;
        }

        //rows
        else if(buttons[r, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[r, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[r, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite){
            return true;
        }

        //diag 1
        else if(buttons[0, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[2, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite){
            return true;
        }
        
        //diag 2
        else if(buttons[0, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite
        && buttons[2, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite){
            return true;
        }

        return false;
    }

    private bool IsGameDraw(){
        for(int i = 0; i<3; i++){
            for(int k = 0; k<3; k++){
                if(buttons[i, k].GetComponentInChildren<Image>().sprite != xSprite &&
                buttons[i, k].GetComponentInChildren<Image>().sprite != oSprite){
                    return false;
                }
            }
        }
        return true;
    }
}
