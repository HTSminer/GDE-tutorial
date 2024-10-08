using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public bool IsUpdating { get; private set; } = false;

    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1f);
        health.GetComponent<Image>().color = GlobalSettings.i.HealthBarGradient.Evaluate(hpNormalized);
    }

    public IEnumerator SetHPSmooth(float newHp)
    {
        IsUpdating = true;

        float curHp = health.transform.localScale.x;
        bool isDamaging = (curHp - newHp > 0);
        float changeAmt = curHp - newHp;

        while ((isDamaging) ? (curHp - newHp > Mathf.Epsilon) : (curHp - newHp < Mathf.Epsilon))
        {
            curHp -= changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHp, 1f);
            health.GetComponent<Image>().color = GlobalSettings.i.HealthBarGradient.Evaluate(curHp);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHp, 1f);
        health.GetComponent<Image>().color = GlobalSettings.i.HealthBarGradient.Evaluate(newHp);

        IsUpdating = false;
    }
}
