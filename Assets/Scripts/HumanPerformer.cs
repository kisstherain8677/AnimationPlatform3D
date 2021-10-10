using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HumanPerformer : Performer
{
    private GameObject mSkeleton;
    private void DestroyAll()
    {
        if (mSkeleton != null)
            GameObject.DestroyImmediate(mSkeleton);
        //partGoDic.Clear();
        foreach (var key in partGoDic.Keys.ToList())
        {
            partGoDic[key] = null;
        }
    }

    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation)
    {
        DestroyAll();
        partGoDic.Clear();
        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            GameObject go = new GameObject();
            partGoDic.Add(code, go);
        }

        this.genGo = Instantiate(actorRes.prefab, startPosition, startRotation);
        mSkeleton = GameObject.Instantiate(actorRes.mSkeleton);
        mSkeleton.Reset(this.genGo);
        mSkeleton.name = actorRes.mSkeleton.name;

        mAnim = mSkeleton.GetComponent<Animation>();

        foreach (int code in Enum.GetValues(typeof(EPart)))
        {
            if (code != (int)EPart.EP_All)
            {
                ChangeEquip(code, actorRes);
            }

        }

        ChangeAnim(actorRes);
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

    public override void ChangeAnim(ActorRes ActorRes)
    {
        if (mAnim == null)
            return;

        AnimationClip animclip = ActorRes.mAnimList[ActorRes.mAnimIdx];
        mAnim.wrapMode = WrapMode.Loop;
        mAnim.Play(animclip.name);
    }



}
