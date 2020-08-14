using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class CameraControll : MonoBehaviour {
 
    //カメラを格納する変数
    public Camera SingleCam;
    public Camera SecondCam;
    public Camera ThirdViewCam;
 
    //キャンバスを格納
    public GameObject Canvas;
 
    // Use this for initialization
    void Start () {
 
        //初めはサブカメラをオフにしておく
        SecondCam.enabled = false;
        ThirdViewCam.enabled = false;
    }
     
    // Update is called once per frame
    void Update () {
         
    }
 
    //ボタンを押した時の処理
    public void PushButton()
    {
        //もしサブカメラがオフだったら
        if (SingleCam.enabled)
        {
            //Secondカメラをオンにして
            SecondCam.enabled = true;
 
            //Mainカメラをオフにする
            SingleCam.enabled = false;
 
            //キャンバスを映すカメラをSecondカメラオブジェクトにする
            Canvas.GetComponent<Canvas>().worldCamera = SecondCam;
        }
        //もしサブカメラがオンだったら  C#はelif使えないんだっけ
        else if (SecondCam.enabled)
        {
            //Thirdカメラをオンにして
            ThirdViewCam.enabled = true;
 
            //Secondカメラをオフにする
            SecondCam.enabled = false;
 
            //キャンバスを映すカメラをThirdカメラオブジェクトにする
            Canvas.GetComponent<Canvas>().worldCamera = ThirdViewCam;
        }
        else if (ThirdViewCam.enabled)
        {
            SingleCam.enabled = true;
            ThirdViewCam.enabled = false;

            Canvas.GetComponent<Canvas>().worldCamera = SingleCam;
        }
    }
}