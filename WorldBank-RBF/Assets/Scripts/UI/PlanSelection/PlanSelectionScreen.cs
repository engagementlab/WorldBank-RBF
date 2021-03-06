using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using JsonFx.Json;

public class PlanSelectionScreen : MonoBehaviour {

	public MenusManager menus;

	public Text header;
	public PlanContainer plan;
	public IndicatorsSummary yourPlan;
	public IndicatorsSummary thisPlan;
	public PreviewPosition previewPosition;

	public GameObject leftArrow;
	public GameObject rightArrow;

	public Text[] tacticsLabels;
	public Text[] yourIndicatorsLabels;
	public Text[] indicatorsLabels;

	Animator planAnimator;

	List<Models.PlanRecord> plans;

	int PlanCount {
		get { return plans.Count-1; }
	}
	
	int planIndex;
	int PlanIndex {
		get { return planIndex; }
		set {
			planIndex = value;
			previewPosition.Position = value;
		}
	}

	void Start() {

		planAnimator = gameObject.GetComponent<Animator>();

	}

	void OnEnable () {

    // Insert user ID
		Dictionary<string, object> userField = new Dictionary<string, object> {{ "user_id", PlayerManager.Instance.ID }};

    // Get plans
    NetworkManager.Instance.PostURL("/plan/all/", userField, PlansRetrieved);

	}

	public void NextPlan () {
		if (PlanIndex < PlanCount)
			PlanIndex ++;

		rightArrow.SetActive(!(PlanIndex == PlanCount));
		leftArrow.SetActive(!(PlanIndex == 0));

		StartCoroutine(ShowPlan(PlanIndex));
	}

	public void PreviousPlan () {
		if (PlanIndex > 0)
			PlanIndex --;

		rightArrow.SetActive(!(PlanIndex == PlanCount));
		leftArrow.SetActive(!(PlanIndex == 0));

		StartCoroutine(ShowPlan(PlanIndex, true));
	}

	public void Continue () {
		
		AudioManager.Music.FadeOut ("title_theme", 0.5f, () => {
				MenusManager.gotoSceneOnLoad = "PhaseTwo";
				AudioManager.StopAll ();
				StartCoroutine (CoGotoLoad ());
			}
		);
		
	}

	public void GoBack () {
		// menus.SetScreen ("title");
		menus.SetScreen ("phase");
	}

	IEnumerator ShowPlan(int planIndex, bool prev=false) {

		planAnimator.Play("PlanColumnHide" + (prev ? "Prev" : "Next"));

		yield return new WaitForSeconds(.5f);

		SetPlanData(planIndex);

		planAnimator.Play("PlanColumnShow" + (prev ? "Prev" : "Next"));

	}

	void SetPlanData(int planIndex) {

		// Failsafe
		if(planIndex > plans.Count-1 || planIndex < 0)
			return;

		int tactInt = 0;

		int affect1 = plans[planIndex].default_affects[0];
		int affect2 = plans[planIndex].default_affects[1];
		int affect3 = plans[planIndex].default_affects[2];
    	
    	foreach(Text label in tacticsLabels) {

    		label.text = PlayerData.TacticGroup.GetName(plans[planIndex].tactics[tactInt]);
    		tactInt++;

    	}

    	indicatorsLabels[0].text = "Facility Births: +" + affect1 +"%";
    	indicatorsLabels[1].text = "Vaccinations: +" + affect2 +"%";
    	indicatorsLabels[2].text = "Quality of Care: +" + affect3 +"%";

    	header.text = plans[planIndex].name;
    	DataManager.currentPlanId = plans[planIndex]._id;

	}

    /// <summary>
    /// Callback that handles all display for plans after they are retrieved.
    /// </summary>
    /// <param name="response">Textual response from /plan/all/ endpoint.</param>
    void PlansRetrieved(Dictionary<string, object> response) {
			
			plans = new List<Models.PlanRecord>();

    	// Local fallback -- no network
    	if(response.ContainsKey("local"))
    	{

				// Open stream to plans JSON config file
				TextAsset plansJson = (TextAsset)Resources.Load("plans", typeof(TextAsset) );
				StringReader strPlansData = new StringReader(plansJson.text);

        plans.Add(JsonReader.Deserialize<Models.PlanRecord>(PlayerPrefs.GetString("current plan")));
				plans.AddRange(JsonReader.Deserialize<Models.PlanRecord[]>(strPlansData.ReadToEnd()));
	    	
    	}
    	// Network response
    	else
				plans.AddRange(JsonReader.Deserialize<Models.PlanRecord[]>(response["plans"].ToString()));

			PlanIndex = 0;
    	SetPlanData(0);
    	ShowPlan(0);

    	yourIndicatorsLabels[0].text = "Facility Births: +" + plans[0].default_affects[0]+"%";
    	yourIndicatorsLabels[1].text = "Vaccinations: +" + plans[0].default_affects[1]+"%";
    	yourIndicatorsLabels[2].text = "Quality of Care: +" + plans[0].default_affects[2]+"%";

    }

	IEnumerator CoGotoLoad () {
		yield return new WaitForFixedUpdate ();
		ObjectPool.Clear ();
		menus.SetScreen ("loading");
	}
}
