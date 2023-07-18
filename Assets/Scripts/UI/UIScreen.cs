using UnityEngine;

public class UIScreen : MonoBehaviour
{
	public bool isModal = false;
	[SerializeField] private UIScreen previousScreen = null;

	public static UIScreen activeScreen;

	// Class Methods
	
	public static void Focus(UIScreen screen)
	{
		if (screen == activeScreen)
			return;
		
		Debug.Log("Focus: " + screen.name);

		screen.Focus();
		screen.previousScreen = activeScreen;
		if(activeScreen)
			activeScreen.Defocus();
		activeScreen = screen;
	}

	public static void BackToInitial()
	{
		activeScreen?.BackTo(null);
	}

	// Instance Methods

	public void FocusScreen(UIScreen screen)
	{
		Focus(screen);
	}

	private void Focus()
	{
		if(gameObject)
			gameObject.SetActive(true);
	}

	private void Defocus()
	{
		if(gameObject)
			gameObject.SetActive(false);
	}

	public void Back()
	{
		if (previousScreen)
		{
			Defocus();
			activeScreen = previousScreen;
			activeScreen.Focus();
			previousScreen = null;
		}
	}

	public void BackTo(UIScreen screen)
	{
		while (activeScreen!=null && activeScreen.previousScreen!=null && activeScreen != screen)
			activeScreen.Back();
	}
}