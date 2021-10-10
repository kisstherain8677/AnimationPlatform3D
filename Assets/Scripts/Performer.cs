using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Performer : MonoBehaviour
{
    protected GameObject genGo;//封装位置等信息后生成的go，传回
    protected Animation mAnim;//挂在物体的Animation组件

    //各个部位的gameobject字典
    protected Dictionary<int, GameObject> partGoDic = new Dictionary<int, GameObject>();

    //当前go的位置和角度
    public Vector3 position;
    public Quaternion rotation;

    public GameObject getGenGo()
    {
        return genGo;
    }
    public void setGenGo(GameObject go)
    {
        this.genGo = go;
    }
    //指定父go的名字
    public void setName(string name)
    {
        gameObject.name = name;
    }

    public virtual void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation)
    {
        Debug.Log("父类方法没有实现！");
    }

    public virtual void ChangeAnim(ActorRes actorRes)
    {
        Debug.Log("父类方法没有实现！");
    }

    public GameObject getGameObject()
    {
        return this.genGo;
    }
}
