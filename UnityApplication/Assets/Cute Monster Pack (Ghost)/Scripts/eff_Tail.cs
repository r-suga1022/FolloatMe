using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class eff_Tail : MonoBehaviour
{

    [SerializeField]
    float _shootWaitTime = 0.1f;
    [SerializeField]
    float _DestroyTime = 10f;
    public GameObject _Bullet;
    [SerializeField]
    Vector3 _StartPos = new Vector3();

    private void Awake()
    {
        _Bullet.SetActive(false);
    }

    private void Start()
    {

        StartCoroutine("Shoot");
    }

    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(_shootWaitTime);
        this.transform.position = Camera.main.GetComponent<MonsterGhostCharacterButton>().ShootPoint.transform.position + _StartPos;
        _Bullet.SetActive(true);
        if (_DestroyTime > 0) Destroy(gameObject, _DestroyTime);
    }
}
