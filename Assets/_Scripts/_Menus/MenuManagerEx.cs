using UnityEngine;
using System.Collections;

public class MenuManagerEx : MenuManager
{
	public void GoToMenu (GameObject target)
	{
		if (target == null) {
			return;
		}

		IMenu menu = target.GetComponent<IMenu> ();
		if (menu != null) {
			menu.Initialize ();
		}

		base.GoToMenu (target);
	}
}
