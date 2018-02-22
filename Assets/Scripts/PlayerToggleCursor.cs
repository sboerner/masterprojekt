using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityStandardAssets.Characters.ThirdPerson;

public class PlayerToggleCursor : NetworkBehaviour {


    public ThirdPersonUserControl thirdPersonUserControl;

    void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (Input.GetButtonUp("Cancel"))
        {
            ToggleCursor();
        }

    }

    void ToggleCursor()
    {
        thirdPersonUserControl.enabled = !thirdPersonUserControl.enabled;
    }


}
