using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CandleRenderer : MonoBehaviour
{
    /// <summary>
    /// 炎のRendere
    /// </summary>
    [SerializeField]
    private Renderer fire = null;

    public void Disappear()
    {
        fire.enabled = false;
    }
}
