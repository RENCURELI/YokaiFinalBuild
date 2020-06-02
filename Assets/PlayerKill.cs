using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKill : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (GameManager.main.GetCurrentGamePhase().Equals(GamePhase.Spirit))
        { Player player = other.gameObject.GetComponent<Player>();
            if(player != null)
            {
                GameManager.main.PlayerGotHit(20f, this.gameObject);
            }
        }
    }
}
