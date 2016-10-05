using UnityEngine;
using System.Collections;

public class MenuManagerEx : MenuManager
{
	public static MenuManagerEx Instance {
		get {
			return GameObject.FindGameObjectWithTag ("MenuManager").GetComponent<MenuManagerEx> ();
		}
	}

	public static Transform MainCanvas {
		get {
			return GameObject.FindGameObjectWithTag ("MainCanvas").transform;
		}
	}

	public static GameObject GetMenuGOByName (string menuName)
	{
		Transform t = MainCanvas.FindChild (menuName);
		if (t != null) {
			return t.gameObject;
		}
		return null;
	}

	public void GoToMenu (string menuName)
	{
		GoToMenu (GetMenuGOByName (menuName));
	}

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
