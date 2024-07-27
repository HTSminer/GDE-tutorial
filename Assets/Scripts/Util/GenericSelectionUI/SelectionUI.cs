using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PKMNUtils.GenericSelectionUI
{
    public enum SelectionType { List, Grid }

    public class SelectionUI<T> : MonoBehaviour where T : ISelectableItem
    {
        List<T> items;
        protected int selectedItem = 0;

        SelectionType selectionType;
        int gridWidth = 2;

        float hSelectionTimer = 0;
        float vSelectionTimer = 0;

        const float selectionSpeed = 5;

        public event Action<int> OnSelected;
        public event Action OnBack;

        public int SelectedItem { get => selectedItem; set => selectedItem = value; }

        public void SetSelectionSettings(SelectionType selectionType, int gridWidth)
        {
            this.selectionType = selectionType;
            this.gridWidth = gridWidth;
        }

        public void SetItems(List<T> items)
        {
            this.items = items;

            items.ForEach(i => i.Init());
            UpdateSelectionInUI();
        }

        public void ClearItems()
        {
            items?.ForEach(i => i.Clear());

            this.items = null;
        }

        public virtual void HandleUpdate()
        {
            UpdateSelectionTimer();
            int prevSelection = SelectedItem;

            if (selectionType == SelectionType.List)
                HandleListSelection();
            else if (selectionType == SelectionType.Grid)
                HandleGridSelection();

            SelectedItem = Mathf.Clamp(SelectedItem, 0, items.Count - 1);

            if (SelectedItem != prevSelection)
                UpdateSelectionInUI();

            if (Input.GetButtonDown("ActionSelected"))
                OnSelected?.Invoke(SelectedItem);
            else if (Input.GetButtonDown("BackSelected"))
                OnBack?.Invoke();
        }

        void HandleListSelection()
        {
            float v = Input.GetAxisRaw("Vertical");

            if (vSelectionTimer == 0 && Mathf.Abs(v) > 0.2f)
            {
                SelectedItem += -(int)Mathf.Sign(v);

                vSelectionTimer = 1 / selectionSpeed;
            }
        }

        void HandleGridSelection()
        {
            float v = Input.GetAxisRaw("Vertical");
            float h = Input.GetAxisRaw("Horizontal");

            if (hSelectionTimer == 0 && Mathf.Abs(h) > 0.2f)
            {
                SelectedItem += (int)Mathf.Sign(h);
                hSelectionTimer = 1 / selectionSpeed;
            }

            if (vSelectionTimer == 0 && Mathf.Abs(v) > 0.2f)
            {
                SelectedItem += -(int)Mathf.Sign(v) * gridWidth;
                vSelectionTimer = 1 / selectionSpeed;
            }
        }

        public virtual void UpdateSelectionInUI()
        {
            for (int i = 0; i < items.Count; i++)
            {
                items[i].OnSelectionChanged(i == SelectedItem);
            }
        }

        void UpdateSelectionTimer()
        {
            if (hSelectionTimer > 0)
                hSelectionTimer = Mathf.Max(hSelectionTimer - Time.deltaTime, 0);

            if (vSelectionTimer > 0)
                vSelectionTimer = Mathf.Max(vSelectionTimer - Time.deltaTime, 0);
        }
    }
}
