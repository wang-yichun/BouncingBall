using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class InGameHudMenu : MonoBehaviour, IMenu
{

	public int stageNum;
	public Text titleText;

	void FindChild ()
	{
		titleText = transform.FindChild ("Title/Text").GetComponent<Text> ();
	}

	public void SetStageNum (int stageNum)
	{
		if (titleText == null) {
			FindChild ();
		}
		this.stageNum = stageNum;
		titleText.text = string.Format ("Stage {0}", this.stageNum);
	}

	#region IMenu implementation

	public void Initialize ()
	{
		if (titleText == null) {
			FindChild ();
		}

		SetStageNum (stageNum);
	}

	#endregion
}
