using UnityEngine;
using System.Collections;
using Unity.Linq;

public class LevelMenu : MonoBehaviour, IMenu
{
	public GameObject LevelButtonPrefab;

	public int ButtonCount;

	public int firstStageNum;

	private Transform LevelButtonsContainer;

	void FindChilds ()
	{
		LevelButtonsContainer = transform.FindChild ("LevelMenu/LevelButtons");
	}

	#region IMenu implementation

	private int initCount = 0;

	public void Initialize ()
	{
		if (initCount == 0) {
			initCount++;

			if (LevelButtonsContainer == null) {
				FindChilds ();
			}

			LevelButtonsContainer.gameObject.Children ().Destroy ();

			int stageNum = firstStageNum;
			for (int i = 0; i < ButtonCount; i++,stageNum++) {
				GameObject levelButtonGO = Instantiate<GameObject> (LevelButtonPrefab);
				levelButtonGO.name = "LevelButton_" + i;
				levelButtonGO.transform.SetParent (LevelButtonsContainer);

				LevelButton levelButton = levelButtonGO.GetComponent<LevelButton> ();
				levelButton.SetButtonStageNum (stageNum);
			}
		}
	}

	#endregion


}
