using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportObjectAction : CutSceneAction
{
    [SerializeField] GameObject obj;
    [SerializeField] Vector2 position;

    public override IEnumerator Play()
    {
        obj.transform.position = position;
        yield break;
    }
}
