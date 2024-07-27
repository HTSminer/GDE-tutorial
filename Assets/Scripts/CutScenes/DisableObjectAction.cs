using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableObjectAction : CutSceneAction
{
    [SerializeField] GameObject obj;

    public override IEnumerator Play()
    {
        obj.SetActive(false);
        yield break;
    }
}
