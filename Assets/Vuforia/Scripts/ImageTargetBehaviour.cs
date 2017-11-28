/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using System.Collections.Generic;
using UnityEngine;

namespace Vuforia
{
    /// <summary>
    /// This class serves both as an augmentation definition for an ImageTarget in the editor
    /// as well as a tracked image target result at runtime
    /// </summary>
    public class ImageTargetBehaviour : ImageTargetAbstractBehaviour
    {
		// <summary>
		// function is credited to developer.vuforia.com
		// </summary>
		public GameObject augmentationObject = null;  // you can use teapot or other object
		public string dataSetName = "example";  

		void Start()
		{
			// Vuforia 6.2+
			VuforiaARController.Instance.RegisterVuforiaStartedCallback(LoadDataSet);
		}

		void LoadDataSet()
		{

			ObjectTracker objectTracker = TrackerManager.Instance.GetTracker<ObjectTracker>();

			DataSet dataSet = objectTracker.CreateDataSet();

			if (dataSet.Load(dataSetName)) {

				objectTracker.Stop();  // stop tracker so that we can add new dataset

				if (!objectTracker.ActivateDataSet(dataSet)) {
					// Note: ImageTracker cannot have more than 100 total targets activated
					Debug.Log("<color=yellow>Failed to Activate DataSet: " + dataSetName + "</color>");
				}

				if (!objectTracker.Start()) {
					Debug.Log("<color=yellow>Tracker Failed to Start.</color>");
				}

				int counter = 0;

				IEnumerable<TrackableBehaviour> tbs = TrackerManager.Instance.GetStateManager().GetTrackableBehaviours();
				foreach (TrackableBehaviour tb in tbs) {
					if (tb.name == "STOP") {

						// change generic name to include trackable name
						tb.gameObject.name = ++counter + ":DynamicImageTarget-" + tb.TrackableName;

						// add additional script components for trackable
						tb.gameObject.AddComponent<DefaultTrackableEventHandler>();
						tb.gameObject.AddComponent<TurnOffBehaviour>();

						if (augmentationObject != null) {
							// instantiate augmentation object and parent to trackable
							GameObject augmentation = (GameObject)GameObject.Instantiate(augmentationObject);
							augmentation.transform.parent = tb.gameObject.transform;
							augmentation.transform.localPosition = new Vector3(0f, 0f, 0f);
							augmentation.transform.localRotation = Quaternion.identity;
							augmentation.transform.localScale = new Vector3(0.005f, 0.005f, 0.005f);
							augmentation.gameObject.SetActive(true);
						} else {
							Debug.Log("<color=yellow>Warning: No augmentation object specified for: " + tb.TrackableName + "</color>");
						}
					}
				}
			} else {
				Debug.LogError("<color=yellow>Failed to load dataset: '" + dataSetName + "'</color>");
			}
		}
    }
}
