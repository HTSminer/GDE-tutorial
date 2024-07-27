using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Quest/Create a new quest")]
public class QuestBase : ScriptableObject
{
    [SerializeField] string questName;
    [SerializeField] string description;

    [SerializeField] Dialog startDialog;
    [SerializeField] Dialog inProgressDialog;
    [SerializeField] Dialog completedDialog;

    [SerializeField] ItemBase requiredItem;
    [SerializeField] ItemBase rewardItem;

    public string Name => questName;
    public string Decription => description;
    public Dialog StartDialog => startDialog;
    public Dialog InProgressDialog => inProgressDialog?.Lines?.Count > 0 ? inProgressDialog : startDialog;
    public Dialog CompletedDialog => completedDialog;
    public ItemBase RequiredItem => requiredItem;
    public ItemBase RewardItem => rewardItem;
}
