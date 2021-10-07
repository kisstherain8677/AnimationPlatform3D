using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;

public class Character : MonoBehaviour
{
    #region 变量


    private GameObject genGo;//实例化后的游戏物体
    private GameObject mSkeleton;
  
    private Animation mAnim;
    //public int CharacterID;//在一个包里面，该角色的编号

    //各个部位的gameobject字典
    Dictionary<int, GameObject> partGoDic = new Dictionary<int, GameObject>();

    //当前go的位置和角度
    public Vector3 position;
    public Quaternion rotation;



    #endregion
    private void Start()
    {
       

    }


    #region 函数

    public GameObject getGameObject()
    {
        return this.genGo;
    }

    public void SetName(string name)
    {
        gameObject.name = name;
    }


    private void DestroyAll()
    {
        this.genGo = null;
        if (mSkeleton != null)
            GameObject.DestroyImmediate(mSkeleton);
        //partGoDic.Clear();
        foreach(var key in partGoDic.Keys.ToList())
        {
            partGoDic[key] = null;
        }
    }

    /// <summary>
    /// unCombine方式生成角色
    /// </summary>
    public void Generate(ActorRes ActorRes, GameObject inputgo, Vector3 startPosition, Quaternion startRotation)
    {
        DestroyAll();
        partGoDic.Clear();
        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            GameObject go = new GameObject();
            partGoDic.Add(code, go);
        }

        this.genGo = Instantiate(inputgo, startPosition, startRotation);
        mSkeleton = GameObject.Instantiate(ActorRes.mSkeleton);
        mSkeleton.Reset(this.genGo);
        mSkeleton.name = ActorRes.mSkeleton.name;

        mAnim = mSkeleton.GetComponent<Animation>();

        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            if (code != (int)EPart.EP_All)
            {
                ChangeEquip(code, ActorRes);
            }
            
        }
       
        ChangeAnim(ActorRes);
    }

    public void Generate(GameObject go, Vector3 startPosition, Quaternion startRotation)
    {
        DestroyAll();
        this.genGo = GameObject.Instantiate(go, startPosition, startRotation);
    }

    /// <summary>
    /// 更换某个部位的服装 
    /// </summary>
    /// <param name="type">部位枚举名</param>
    /// <param name="ActorRes">包含各个部位的资源索引号</param>
    public void ChangeEquip(int type, ActorRes ActorRes)
    {
        GameObject tmpGo = partGoDic[type];
        ChangeEquip(ref tmpGo, ActorRes.assetListDic[type][ActorRes.partIndexDic[type]]);
        partGoDic[type] = tmpGo;
       
    }

    /// <summary>
    /// 指定类型进行该部位gameobject的替换操作
    /// </summary>
    private void ChangeEquip(ref GameObject go, GameObject resgo)
    {
        if (go != null)
        {
            GameObject.DestroyImmediate(go);
        }

        go = GameObject.Instantiate(resgo);
        go.Reset(mSkeleton);//设置
        go.name = resgo.name;

        //获取render组件
        SkinnedMeshRenderer render = go.GetComponentInChildren<SkinnedMeshRenderer>();
        ShareSkeletonInstanceWith(render, mSkeleton);
    }

    // 共享骨骼
    public void ShareSkeletonInstanceWith(SkinnedMeshRenderer selfSkin, GameObject target)
    {
        Transform[] newBones = new Transform[selfSkin.bones.Length];
        for (int i = 0; i < selfSkin.bones.GetLength(0); ++i)
        {
            GameObject bone = selfSkin.bones[i].gameObject;
            
            // 目标的SkinnedMeshRenderer.bones保存的只是目标mesh相关的骨骼,要获得目标全部骨骼,可以通过查找的方式.
            newBones[i] = FindChildRecursion(target.transform, bone.name);
        }

        selfSkin.bones = newBones;
    }

    // 递归查找
    public Transform FindChildRecursion(Transform t, string name)
    {
        foreach (Transform child in t)
        {
            if (child.name == name)
            {
                return child;
            }
            else
            {
                Transform ret = FindChildRecursion(child, name);
                if (ret != null)
                    return ret;
            }
        }

        return null;
    }

    public void ChangeAnim(ActorRes ActorRes)
    {
        if (mAnim == null)
            return;

        AnimationClip animclip = ActorRes.mAnimList[ActorRes.mAnimIdx];
        mAnim.wrapMode = WrapMode.Loop;
        mAnim.Play(animclip.name);
    }

    #endregion
}
