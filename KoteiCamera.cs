using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class KoteiCamera : MonoBehaviour
{
    public GameObject Player;
    public GameObject lookTarget;

   void Start()
    {
    }
 
    void Update()
    {
        //ただ一定の後方距離から観察するタイプ
        //float x = 0;
        //this.transform.rotation = Quaternion.Euler(x,Player.transform.rotation.y,Player.transform.rotation.z);
        Vector3 v = new Vector3(Player.transform.position.x,2.8f,Player.transform.position.z-8f );
        this.transform.position = v;

        //お札のほうで向き調整タイプ
        Quaternion lockRotation = Quaternion.LookRotation(lookTarget.transform.position - transform.position, Vector3.up);
        lockRotation.x = 0; 
        transform.rotation = Quaternion.Lerp(transform.rotation, lockRotation, 0.1f);

    }
}
