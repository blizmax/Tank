using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class TankController : MonoBehaviour {
    GameObject TankObj;
    GameObject Turret;//炮台
    public bool isShoot = false;
    private Vector3 angle = Vector3.forward;
    public Vector3 Angle
    {
        set
        {
            if (angle != value)
            {
                angle = value;
                TurretAngle(angle);
            }
        }
        get {
            return angle;
        }
    }
	// Use this for initialization
	void Start () {
        InitTank(gameObject);

    }
	
	// Update is called once per frame
	void Update () {
        Shake(0.005f,0.3f);
        if (isShoot)
        {
            Shoot(Angle);
            isShoot = false;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            Angle = new Vector3(0, Angle.y-1f, 0);
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Angle = new Vector3(0, Angle.y+1f,0);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot(Angle);
        }
    }
    public void InitTank(GameObject tank) {
        TankObj = tank;
        Turret = TankObj.transform.Find("Turret").gameObject;
        angle = TankObj.transform.forward;
    }
    void TurretAngle(Vector3 angle)
    {
        if (Turret)
            Turret.transform.DORotate(angle, 0.5f,RotateMode.Fast);
    }
    public void Shoot(Vector3 angle) {
        ForceBack(-Turret.transform.forward * 0.1f,0.1f);
        Shake(0.008f, 0.1f);
    }
    public void Shake(float strength,float duration) {
        if (TankObj)
        {
            TankObj.transform.DOShakePosition(duration, strength,0,0);
        }
    }
    public void ForceBack(Vector3 pos,float duration) {
        if (TankObj)
        {
            TankObj.transform.DOPunchPosition(pos, duration);
        }
    }
}
