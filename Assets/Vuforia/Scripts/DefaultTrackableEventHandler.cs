/*==============================================================================
Copyright (c) 2010-2014 Qualcomm Connected Experiences, Inc.
All Rights Reserved.
Confidential and Proprietary - Protected under copyright and other laws.
==============================================================================*/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Security;
using System.Text;
using System.Security.Cryptography.X509Certificates;
using SimpleJSON;

namespace Vuforia
{
    /// <summary>
    /// A custom handler that implements the ITrackableEventHandler interface.
    /// </summary>
    public class DefaultTrackableEventHandler : MonoBehaviour,
                                                ITrackableEventHandler
    {
        #region PRIVATE_MEMBER_VARIABLES
 
        private TrackableBehaviour mTrackableBehaviour;
    
        #endregion // PRIVATE_MEMBER_VARIABLES

		public int Score;
//		public GameObject spanishText;

        #region UNTIY_MONOBEHAVIOUR_METHODS
    
        void Start()
        {
            mTrackableBehaviour = GetComponent<TrackableBehaviour>();
            if (mTrackableBehaviour)
            {
                mTrackableBehaviour.RegisterTrackableEventHandler(this);
            }
			Score = 0;
			GameObject.Find ("Canvas/spanishText").gameObject.SetActive(true);
			GameObject.Find ("ScoreCanvas/Score").gameObject.SetActive (true);
        }

        #endregion // UNTIY_MONOBEHAVIOUR_METHODS



        #region PUBLIC_METHODS

        /// <summary>
        /// Implementation of the ITrackableEventHandler function called when the
        /// tracking state changes.
        /// </summary>
        public void OnTrackableStateChanged(
                                        TrackableBehaviour.Status previousStatus,
                                        TrackableBehaviour.Status newStatus)
        {
            if (newStatus == TrackableBehaviour.Status.DETECTED ||
                newStatus == TrackableBehaviour.Status.TRACKED ||
                newStatus == TrackableBehaviour.Status.EXTENDED_TRACKED)
            {
				// <comment>
				// move the textbox and text next to the detected phrase
				// mTrackableBehaviour.transform.localPosition gets the position of the detected phrase
				// GameObject.Find ("Canvas/spanishText").transform.localPosition gets the position of the Canvas/spanishText
				// </comment>
				GameObject.Find ("Canvas/spanishText").transform.localPosition = mTrackableBehaviour.transform.localPosition;

				// <debug>
				// print the locations
				// </debug>
				print (GameObject.Find ("Canvas/spanishText/Text").transform.localPosition);
				print (mTrackableBehaviour.transform.localPosition);
				// <comment>
				// this will call the OnTrackingFound function when the trackable object is found
				// </comment>
				OnTrackingFound();
            }
            else
            {
				// <comment>
				// this will call the OnTrackingLost function when the trackable object is lost
				// </comment>
                OnTrackingLost();
            }
        }

        #endregion // PUBLIC_METHODS



        #region PRIVATE_METHODS

		// <comment>
		// This function will be call when the target is found
		// </comment>
        private void OnTrackingFound()
        {
			// <comment>
			// sentence is a string that the detected phrase
			// </comment>
			string sentence = "";
			StateManager stateManager = TrackerManager.Instance.GetStateManager();
			// <comment>
			// this will get the words detected in the image
			// </comment>
			IEnumerable<WordResult> wordResults = stateManager.GetWordManager().GetActiveWordResults();
			foreach (WordResult wordResult in wordResults)
			{	
				Score++;
				// <comment>
				// append the detected words to "sentence" object
				// </comment>
				Word word = wordResult.Word;
				sentence += word.StringValue + " ";
				Debug.Log (sentence);
			}
			// <comment>
			// using the Trasnlator to translate the sentence from english to target language
			// </comment>
			string language = GameObject.Find("Language/Dropdown/Label").GetComponent<Text>().text;
			if (language == "Spanish") {
				language = "es";
			} else if (language == "French") {
				language = "fr";
			} else if (language == "Greek") {
				language = "el";
			}
			sentence = Translator.TranslateGoogleApisSimple (sentence, "en", language);
			// <comment>
			// add the translated phrase to the textbox
			// </comment>
			GameObject.Find ("ScoreCanvas/Score/Text").GetComponent<Text>().text = "Score: " + Score;
			GameObject.Find ("Canvas/spanishText/Text").GetComponent<Text> ().text = sentence;
            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Enable rendering:
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = true;
            }

            // Enable colliders:
            foreach (Collider component in colliderComponents)
            {
                component.enabled = true;
            }

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " found");
        }


        private void OnTrackingLost()
        {


            Renderer[] rendererComponents = GetComponentsInChildren<Renderer>(true);
            Collider[] colliderComponents = GetComponentsInChildren<Collider>(true);

            // Disable rendering:
            foreach (Renderer component in rendererComponents)
            {
                component.enabled = false;
            }

            // Disable colliders:
            foreach (Collider component in colliderComponents)
            {
                component.enabled = false;
            }

            Debug.Log("Trackable " + mTrackableBehaviour.TrackableName + " lost");
        }

        #endregion // PRIVATE_METHODS
    }
}

// <summary>
// a custom function which translated the phrase to a target language
// </summary>
public class Translator : MonoBehaviour {
	public static string TranslateGoogleApisSimple(string InitialText, string fromCulture, string toCulture)
	{
		string textToSearch = InitialText;
		string url = string.Format("https://translate.yandex.net/api/v1.5/tr.json/translate?key=trnsl.1.1.20171109T001143Z.669c8014fd9ccf31.b194f6b13bc767f1d5219f9600ac35dad1ed40c9&text={2}&lang={0}-{1}&plain", fromCulture, toCulture, WWW.EscapeURL(textToSearch));
		string responseText = "Not Set";
		try
		{
			HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
			ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
			using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
			{
				var encoding = ASCIIEncoding.UTF8;
				using (var reader = new System.IO.StreamReader(response.GetResponseStream(), encoding))
				{
					string text = reader.ReadToEnd();
					//Found JSON parse on https://gist.github.com/grimmdev/979877fcdc943267e44c

					var N = JSON.Parse(text);
					string translationString = N["text"][0];
					Debug.Log(translationString);
					responseText = translationString;
				}
			}

		}
		catch (Exception ex)
		{
			responseText = string.Format("Translation Error with GoogleAPI: {0}\r\nUrl:{1}", ex.Message, url);
		}
		return responseText;
	}

	public static bool MyRemoteCertificateValidationCallback(System.Object sender,
		X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
	{
		bool isOk = true;
		// If there are errors in the certificate chain,
		// look at each error to determine the cause.
		if (sslPolicyErrors != SslPolicyErrors.None) {
			for (int i=0; i<chain.ChainStatus.Length; i++) {
				if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown) {
					continue;
				}
				chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
				chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
				chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan (0, 1, 0);
				chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
				bool chainIsValid = chain.Build ((X509Certificate2)certificate);
				if (!chainIsValid) {
					isOk = false;
					break;
				}
			}
		}
		return isOk;
	}
}