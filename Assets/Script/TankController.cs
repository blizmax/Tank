using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankController : MonoBehaviour {
    GameObject TankObj;
    public GameObject Turret;//炮台
    public bool isShoot = false;
    private float angle =0;
    public float Angle
    {
        set
        {
            angle = value;
            TurretAngle(angle);//旋转炮台

        }
        get {
            return angle;
        }
    }
	// Use this for initialization
	void Start () {
        InitTank(gameObject);
        //键值对儿的形式保存iTween所用到的参数  
        
    }
	
	// Update is called once per frame
	void Update () {
      
        if (isShoot)
        {
            Shoot();
            isShoot = false;
        }
        if (Input.GetKey(KeyCode.LeftArrow)) {
            Angle = -1;
            
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            Angle = 1;
        }
        
        if (Input.GetKey(KeyCode.UpArrow)) {
            Shake(0.05f, 2f, iTween.LoopType.none);
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Shoot();
        }
    }
    public void InitTank(GameObject tank) {
        TankObj = tank;
        Turret = TankObj.transform.Find("Turret").gameObject;
        Angle = 0;
    }
    void TurretAngle(float angle)
    {
        if (Turret)
        {
            //Turret.transform.DORotate(angle, 0.5f,RotateMode.Fast);
            Hashtable args = new Hashtable();
            //摇摆的幅度  
            args.Add("y", angle);
            //是世界坐标系还是局部坐标系  
            args.Add("islocal", true);
            //游戏对象是否将面向其方向  
            //args.Add("orienttopath", true);
            //动画的整体时间。如果与speed共存那么优先speed  
            args.Add("easeType", iTween.EaseType.linear);
            args.Add("time", 0.1f);
            args.Add("loopType", iTween.LoopType.none);
            iTween.RotateAdd(Turret, args);
        }
    }
    public void Shoot() {
        ForceBack(Turret.transform.forward * 0.1f,0.1f);
     
    }
    public void Shake(float strength,float duration, iTween.LoopType loopType) {
        if (TankObj)
        {
            Hashtable args = new Hashtable();
            //摇摆的幅度  
            args.Add("amount", new Vector3(strength, 0, strength));  
            //是世界坐标系还是局部坐标系  
            args.Add("islocal", true);
            //游戏对象是否将面向其方向  
            //args.Add("orienttopath", true);
            //动画的整体时间。如果与speed共存那么优先speed  
            args.Add("easeType",iTween.EaseType.linear);
            args.Add("time", duration);
            args.Add("loopType", loopType);

            iTween.ShakePosition(TankObj, args);
        }
       
    }
    public void ForceBack(Vector3 pos,float duration) {
        if (TankObj)
        {
            //TankObj.transform.DOPunchPosition(pos, duration);
            Hashtable args = new Hashtable();
            //摇摆的幅度  
            args.Add("amount", -pos);
            //是世界坐标系还是局部坐标系  
            args.Add("islocal", true);
            //动画的整体时间。如果与speed共存那么优先speed  
            args.Add("easeType", iTween.EaseType.linear);
            args.Add("time", duration);
            args.Add("loopType", iTween.LoopType.none);
            iTween.MoveAdd(TankObj,args);
        }
    }

}
