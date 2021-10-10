using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalPerformer : Performer
{
    public override void Generate(ActorRes actorRes, Vector3 startPosition, Quaternion startRotation)
    {
        this.setGenGo(GameObject.Instantiate(actorRes.prefab, startPosition, startRotation));
    }
   
    
    public override void ChangeAnim(ActorRes actorRes)
    {
       Animation animation= actorRes.prefab.GetComponent<Animation>();
       animation.Play(actorRes.mAnimName);
    }
}
