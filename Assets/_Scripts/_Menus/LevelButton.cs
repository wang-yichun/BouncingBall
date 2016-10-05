using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class LevelButton : MonoBehaviour
{
	public Button Button;
	public int stageNum;
	public Text ButtonText;

	void FindChild ()
	{
		Button = GetComponent<Button> ();
		ButtonText = GetComponentInChildren<Text> ();
		Button.onClick.AddListener (Clicked);
	}

	public void SetButtonStageNum (int stageNum)
	{
		if (Button == null) {
			FindChild ();
		}

		this.stageNum = stageNum;
		ButtonText.text = Convert.ToString (stageNum);
	}

	void Clicked ()
	{
		Stage.Current.stage_num = this.stageNum;
//		Stage.Current.LoadStage ();
		Stage.Current.StartGame ();

		GameObject menuGO = MenuManagerEx.GetMenuGOByName ("InGameHudMenu");
		InGameHudMenu menu = menuGO.GetComponent<InGameHudMenu> ();
		menu.SetStageNum (this.stageNum);

		MenuManagerEx.Instance.GoToMenu ("InGameHudMenu");
	}
}
