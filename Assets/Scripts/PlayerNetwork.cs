using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.ThirdPerson;

//wichtig Networkbehaviour statt MonoBehaviour
public class PlayerNetwork : NetworkBehaviour {

    public GameObject ThirdPersonCharacter;

    //public GameObject[] characterModel;

    public override void OnStartLocalPlayer()
    {
        GetComponent<ThirdPersonUserControl>().enabled = true;
        ThirdPersonCharacter.SetActive(true);

        /*foreach (GameObject go in characterModel)
        {
            go.SetActive(false);
        }*/
    }

}
