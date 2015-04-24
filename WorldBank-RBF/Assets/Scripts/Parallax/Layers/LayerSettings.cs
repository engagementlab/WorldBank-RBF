﻿using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using JsonFx.Json;

public class LayerSettings : MB, IEditorPoolable {

	[SerializeField, HideInInspector] int index;
	public int Index { 
		get { return index; } 
		set { index = value; }
	}

	[SerializeField, HideInInspector] bool selected;
	public bool Selected { 
		get { return selected; }
		set { selected = value; }
	}

	[SerializeField, HideInInspector] float localSeparation = 0;
	public float LocalSeparation { 
		get { return localSeparation; }
		set { localSeparation = value; }
	}

	#if UNITY_EDITOR
	[SerializeField, HideInInspector] List<LayerImageSettings> imageSettings;
	public List<LayerImageSettings> ImageSettings { 
		get { return imageSettings; }
		set { imageSettings = value; }
	}

	public LayerSettingsJson Json {
		get { 

			// Update properties from accompanying DepthLayer
			DepthLayer layer = EditorObjectPool.GetObjectAtIndex<DepthLayer> (Index);
			if (layer != null) {
				LocalSeparation = layer.LocalSeparation;
				ImageSettings = layer.Images.ConvertAll (x => x.Json);
			}

			// Create a json serializable object
			LayerSettingsJson json = new LayerSettingsJson ();
			json.index = Index;
			json.local_separation = LocalSeparation;
			json.images = ImageSettings;
			return json;
		}
	}

	public void Init (int index) {
		this.index = index;
	}

	public void Init (LayerSettingsJson json) {
		this.imageSettings = json.images;
		this.index = json.index;
		this.localSeparation = json.local_separation;
	}
	#endif

	public void Init () {}
}