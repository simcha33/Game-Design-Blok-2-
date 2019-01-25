using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;
using System;

public abstract class AbstractMap {
	public const float BLOCK_SIZE = 2f;
	public const int HISTORY_SIZE = 3000;

	public static System.Random Random;

	public readonly RingBuffer<HistoryItem> History;
	public readonly QueueDictionary<Vector3i, ModuleSet> RemovalQueue;
	private HashSet<Slot> workArea;
	public readonly Queue<Slot> BuildQueue;

	private int backtrackBarrier;
	private int backtrackAmount = 0;

	public readonly short[][] InitialModuleHealth;

	public AbstractMap() {
		InfiniteMap.Random = new System.Random();

		this.History = new RingBuffer<HistoryItem>(AbstractMap.HISTORY_SIZE);
		this.History.OnOverflow = item => item.Slot.Forget();
		this.RemovalQueue = new QueueDictionary<Vector3i, ModuleSet>(() => new ModuleSet());
		this.BuildQueue = new Queue<Slot>();

		this.InitialModuleHealth = this.createInitialModuleHealth(Module.All);

		this.backtrackBarrier = 0;
	}

	public abstract Slot GetSlot(Vector3i position, bool create);
	
	public Slot GetSlot(Vector3i position) {
		return this.GetSlot(position, true);
	}

	public abstract IEnumerable<Slot> GetAllSlots();

	public abstract void ApplyBoundaryConstraints(IEnumerable<BoundaryConstraint> constraints);	

	public void NotifySlotCollapsed(Slot slot) {
		if (this.workArea != null) {
			this.workArea.Remove(slot);
		}
		this.BuildQueue.Enqueue(slot);
	}

	public void NotifySlotCollapseUndone(Slot slot) {
		if (this.workArea != null) {
			this.workArea.Add(slot);
		}
	}	
	
	public void FinishRemovalQueue() {
		while (this.RemovalQueue.Any()) {
			var kvp = this.RemovalQueue.Dequeue();
			var slot = this.GetSlot(kvp.Key);
			if (!slot.Collapsed) {
				slot.RemoveModules(kvp.Value, false);
			}
		}
	}

	public void EnforceWalkway(Vector3i start, int direction) {
		var slot = this.GetSlot(start);
		var toRemove = slot.Modules.Where(module => !module.GetFace(direction).Walkable);
		slot.RemoveModules(ModuleSet.FromEnumerable(toRemove));
	}

	public void EnforceWalkway(Vector3i start, Vector3i destination) {
		int direction = Orientations.GetIndex((destination - start).ToVector3());
		this.EnforceWalkway(start, direction);
		this.EnforceWalkway(destination, (direction + 3) % 6);
	}

	public void Collapse(IEnumerable<Vector3i> targets, bool showProgress = false) {
		Slot.ResetIterationCount();
		this.RemovalQueue.Clear();
		this.workArea = new HashSet<Slot>(targets.Select(target => this.GetSlot(target)).Where(slot => slot != null && !slot.Collapsed));

		while (this.workArea.Any()) {
			float minEntropy = float.PositiveInfinity;
			Slot selected = null;

			foreach (var slot in workArea) {
				float entropy = slot.Modules.Entropy;
				if (entropy < minEntropy) {
					selected = slot;
					minEntropy = entropy;
				}
			}
			try {
				selected.CollapseRandom();
			}
			catch (CollapseFailedException) {
				this.RemovalQueue.Clear();
				if (this.History.TotalCount > this.backtrackBarrier) {
					this.backtrackBarrier = this.History.TotalCount;
					this.backtrackAmount = 2;
				} else {
					this.backtrackAmount *= 2;
				}
				if (this.backtrackAmount > 10) {
					Debug.Log("Backtracking " + this.backtrackAmount + " steps...");
				}
				this.Undo(this.backtrackAmount);
			}

#if UNITY_EDITOR
			if (showProgress) {
				if (EditorUtility.DisplayCancelableProgressBar("Collapsing area... ", this.workArea.Count + " left...", 1f - (float)this.workArea.Count() / targets.Count())) {
					EditorUtility.ClearProgressBar();
					throw new Exception("Map generation cancelled.");
				}
			}
#endif
		}

#if UNITY_EDITOR
		if (showProgress) {
			EditorUtility.ClearProgressBar();
		}
		//Debug.Log("Collapsed " + targets.Count() + " slots in " + Slot.GetIterationCount() + " iterations (" + (float)Slot.GetIterationCount() / targets.Count() + " iterations per slot)");
#endif
	}

	public void Collapse(Vector3i start, Vector3i size, bool showProgress = false) {
		var targets = new List<Vector3i>();
		for (int x = 0; x < size.X; x++) {
			for (int y = 0; y < size.Y; y++) {
				for (int z = 0; z < size.Z; z++) {
					targets.Add(start + new Vector3i(x, y, z));
				}
			}
		}
		this.Collapse(targets, showProgress);
	}

	public void Undo(int steps) {
		while (steps > 0 && this.History.Any()) {
			var item = this.History.Pop();

			foreach (var slotAddress in item.RemovedModules.Keys) {
				this.GetSlot(slotAddress).AddModules(item.RemovedModules[slotAddress]);
			}
			steps--;
		}
	}

	private short[][] createInitialModuleHealth(Module[] modules) {
		var initialModuleHealth = new short[6][];
		for (int i = 0; i < 6; i++) {
			initialModuleHealth[i] = new short[modules.Length];
			foreach (var module in modules) {
				foreach (var possibleNeighbor in module.PossibleNeighbors[(i + 3) % 6]) {
					initialModuleHealth[i][possibleNeighbor.Index]++;
				}
			}
		}

		for (int i = 0; i < modules.Length; i++) {
			for (int d = 0; d < 6; d++) {
				if (initialModuleHealth[d][i] == 0) {
					Debug.LogError("Module " + modules[i].Name + " cannot be reached from direction " + d + " (" + modules[i].GetFace(d).ToString() + ")!", modules[i].Prototype.gameObject);
					throw new Exception("Unreachable module.");
				}
			}
		}
		return initialModuleHealth;
	}

	public short[][] CopyInititalModuleHealth() {
		return this.InitialModuleHealth.Select(a => a.ToArray()).ToArray();
	}
}
