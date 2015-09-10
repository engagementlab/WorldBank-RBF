﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TacticSlot : DragLocation, IDropHandler, IBeginDragHandler, IDragHandler {

	public int index;
	public Text text;
	public TacticsContainer container;

	Tactic currentTactic;
	TacticItem tacticItem;

	bool HasTactic {
		get { return currentTactic != null; }
	}

	public void ClearSlot () {
		currentTactic = null;
	}

	public void FillSlot (Tactic tactic) {
		if (tactic.Item == null)
			return;
		if (HasTactic) {
			TradeSlot (tactic);
		} else {
			FillEmptySlot (tactic);
		}
		currentTactic = tactic;
		tacticItem.Priority = index;
		PlayerData.TacticPriorityGroup.Add (tacticItem);
	}

	void TradeSlot (Tactic tactic) {
		Tactic newTactic = CreateTactic ();
		newTactic.Init (tacticItem);
		newTactic.ForceFromSlot (tactic);
		FillEmptySlot (tactic);
	}

	void FillEmptySlot (Tactic tactic) {
		tacticItem = tactic.Item;
	 	text.text = tacticItem.Title;
	}

	Tactic CreateTactic () {
		Tactic t = ObjectPool.Instantiate<Tactic> ();
		Vector3 createPosition = Position;
		t.Position = createPosition;
		t.Init (tacticItem);
		t.DragData.FromLocation = this;
		t.DragData.ToLocation = this;
		return t;
	}

	#region IBeginDragHandler, IDropHandler implementation
	public void OnBeginDrag (PointerEventData eventData) {
		if (currentTactic == null)
			return;
		Tactic t = CreateTactic ();
		t.StartDragging (this);
		currentTactic = null;
		tacticItem.Priority = -1;
		PlayerData.TacticPriorityGroup.Remove (tacticItem);
		tacticItem = null;
		text.text = "";
	}

	public void OnDrag (PointerEventData eventData) {}
	#endregion

	#region IDropHandler implementation
	public void OnDrop (PointerEventData eventData) {
		Tactic selectedTactic = Tactic.selected;
		if (selectedTactic == null)
			return;
		FillSlot (selectedTactic);
	 	selectedTactic.SetDropLocation (this);
	}
	#endregion
}