﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(InfiniteMap))]
public class FollowPlayer : MonoBehaviour {

	private MapBehaviour mapBehaviour;
	private InfiniteMap map;

	public Transform Target;

	public int ChunkSize = 4;

	public float Range = 30;

	public float UnloadRange = 60;

	private Dictionary<Vector3i, bool> chunkVisibility;

	private Vector3 targetPosition;
	private Vector3 mapPosition;

	private Queue<Vector3i> showQueue;
	private Queue<Vector3i> hideQueue;

	private Thread thread;

	private int stepsWithoutVisibilityUpdate = 0;

	void Start() {
		this.chunkVisibility = new Dictionary<Vector3i, bool>();
		this.mapBehaviour = this.GetComponent<MapBehaviour>();
		this.mapBehaviour.Initialize();
		this.map = this.mapBehaviour.Map;
		this.generate();
		this.mapBehaviour.BuildAllSlots();

		this.showQueue = new Queue<Vector3i>();
		this.hideQueue = new Queue<Vector3i>();

		this.thread = new Thread(this.generatorThread);
		this.thread.Start();
	}

	public void OnDisable() {
		this.thread.Abort();
	}

	private void generate() {
		float chunkSize = InfiniteMap.BLOCK_SIZE * this.ChunkSize;

		float targetX = this.targetPosition.x - this.mapPosition.x + InfiniteMap.BLOCK_SIZE / 2;
		float targetZ = this.targetPosition.z - this.mapPosition.z + InfiniteMap.BLOCK_SIZE / 2;

		int chunkX = Mathf.FloorToInt(targetX / chunkSize);
		int chunkZ = Mathf.FloorToInt(targetZ / chunkSize);

		Vector3i closestMissingChunk = Vector3i.zero;
		float closestDistance = this.Range;
		bool any = false;

		for (int x = Mathf.FloorToInt(chunkX - this.Range / chunkSize); x < chunkX + this.Range / chunkSize; x++) {
			for (int z = Mathf.FloorToInt(chunkZ - this.Range / chunkSize); z < chunkZ + this.Range / chunkSize; z++) {
				var chunk = new Vector3i(x, 0, z);
				if (this.chunkVisibility.ContainsKey(chunk)) {
					if (!this.chunkVisibility[chunk]) {
						this.setChunkVisible(chunk, true);
					}
					continue;
				}
				var center = (chunk.ToVector3() + new Vector3(0.5f, 0f, 0.5f)) * chunkSize - new Vector3(1f, 0f, 1f) * InfiniteMap.BLOCK_SIZE / 2;
				float distance = Vector3.Distance(center, this.targetPosition + Vector3.down * this.targetPosition.y);

				if (distance < closestDistance) {
					closestMissingChunk = chunk;
					any = true;
					closestDistance = distance;
				}
			}
		}

		if (any) {
			this.createChunk(closestMissingChunk);
			this.stepsWithoutVisibilityUpdate++;
		}

		if (!any || this.stepsWithoutVisibilityUpdate > 15) {
			foreach (var kvp in this.chunkVisibility.ToList()) {
				var chunk = kvp.Key;
				var center = (chunk.ToVector3() + new Vector3(0.5f, 0f, 0.5f)) * chunkSize - new Vector3(1f, 0f, 1f) * InfiniteMap.BLOCK_SIZE / 2;
				bool inRange = Vector3.Distance(center, this.targetPosition - Vector3.up * this.targetPosition.y) < this.UnloadRange;
				if (inRange != kvp.Value) {
					this.setChunkVisible(chunk, inRange);
				}
			}
			this.stepsWithoutVisibilityUpdate = 0;
			Thread.Sleep(80);
		}
	}

	private void setChunkVisible(Vector3i chunkAddress, bool visible) {
		if (visible) {
			this.showQueue.Enqueue(chunkAddress);
		} else {
			this.hideQueue.Enqueue(chunkAddress);
		}
		this.chunkVisibility[chunkAddress] = visible;
	}

	private void createChunk(Vector3i chunkAddress) {
		this.map.rangeLimitCenter = chunkAddress * this.ChunkSize + new Vector3i(this.ChunkSize / 2, 0, this.ChunkSize / 2);
		this.map.rangeLimit = this.ChunkSize + 20;
		this.map.Collapse(chunkAddress * this.ChunkSize, new Vector3i(this.ChunkSize, this.map.Height, this.ChunkSize));
		this.chunkVisibility[chunkAddress] = true;
	}

	private void generatorThread() {
		try {
			while (true) {
				this.generate();
			}
		}
		catch (Exception exception) {
			if (exception is System.Threading.ThreadAbortException) {
				return;
			}
			Debug.LogError(exception);
		}
		
	}

	private IEnumerable<Slot> getSlotsInChunk(Vector3i chunkAddress) {
		for (int x = 0; x < this.ChunkSize; x++) {
			for (int y = 0; y < this.map.Height; y++) {
				for (int z = 0; z < this.ChunkSize; z++) {
					yield return this.map.GetSlot(chunkAddress * this.ChunkSize + new Vector3i(x, y, z));
				}
			}
		}
	}
	
	void Update () {
		this.targetPosition = this.Target.position;
		this.mapPosition = this.mapBehaviour.transform.position;

		if (this.showQueue.Count != 0) {
			foreach (var slot in this.getSlotsInChunk(this.showQueue.Dequeue())) {
				if (slot.GameObject != null) {
					slot.GameObject.SetActive(true);
				}
			}
		}
		if (this.hideQueue.Count != 0) {
			foreach (var slot in this.getSlotsInChunk(this.hideQueue.Dequeue())) {
				if (slot.GameObject != null) {
					slot.GameObject.SetActive(false);
				}
			}
		}
	}
}
