using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuButtons : MonoBehaviour
{
   public void QuitGame()
    {
        Application.Quit();
    }

    public void Rate()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=com.companyname.game");
    }

    public void Feedback()
    {
        Application.OpenURL("mailto:agonajrediniyt@gmail.com?subject=Feedback");
    }
}
