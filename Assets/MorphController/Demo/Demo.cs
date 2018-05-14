using UnityEngine;
using MorphController;

public class Demo : MonoBehaviour {

    public MorphAnimator Ani;

    public void Walk()
    {
        MorphDebug.Log("Walk!", gameObject);
    }

    public void Idle()
    {
        MorphDebug.Log("Idle!", gameObject);
    }

    public void SpinKick()
    {
        MorphDebug.Log("SpinKick!", gameObject);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MorphDebug.Log("Current State: " + Ani.CurrentState(), gameObject);
            Ani.SwitchState("Spin Kick");
        }
    }
}
