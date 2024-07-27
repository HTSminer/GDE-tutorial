using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnActorAction : CutSceneAction
{
    [SerializeField] CutsceneActor actor;
    [SerializeField] FacingDir dir;

    public override IEnumerator Play()
    {
        actor.GetCharacter().Animator.SetFacingDir(dir);
        yield break;
    }
}
