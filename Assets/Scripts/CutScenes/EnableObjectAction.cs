using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableObjectAction : CutSceneAction
{
    [SerializeField] GameObject obj;

    public override IEnumerator Play()
    {
        obj.SetActive(true);
        yield break;
    }
}
