using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class effects : MonoBehaviour
{
    public GameObject effect;
    public Vector3 position = new Vector3(0,3,0);
    public GameObject estrellitas;
    // Start is called before the first frame update

    //estrellas para evento
    public void estre()
    {
        estrellitas.SetActive(true);
    }
    void showEffect(Animator animator)
    {
        Vector3 gam = this.gameObject.transform.position;
        Vector3 pos = new Vector3(gam.x, gam.y , gam.z);
        GameObject pap = Instantiate(effect, pos, Quaternion.Euler(0, 0, 0));
        pap.gameObject.transform.parent = this.gameObject.transform;
        pap.SetActive(true);
        pap.gameObject.transform.localRotation = Quaternion.Euler(0,0,0);
        pap.gameObject.transform.localScale = new Vector3(1, 1, 1);
        pap.gameObject.transform.Translate(position, Space.Self);
    }

}
