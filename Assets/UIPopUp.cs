using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UIPopUp : MonoBehaviour
{
    public Text popUpTitle;
    public Text popUpText;
    public Button popUpConfirmButton;
    public Button popUpCancelButton;
    public Action popUpConfimed;
    public Action popUpCanceled;

    public void SetDataAndOpen(string title, string text, string confirmButtonText)
    {
        popUpTitle.text = title;
        popUpText.text = text;
        popUpConfirmButton.GetComponentInChildren<Text>().text = confirmButtonText;
        if(confirmButtonText == "")
            popUpConfirmButton.gameObject.SetActive(false);
        else
            popUpConfirmButton.gameObject.SetActive(true);
        Open();
    }
    private void Open()
    {
        popUpCancelButton.onClick.AddListener(() => Close());
        popUpConfirmButton.onClick.AddListener(onConfirmButtonClicked);
        gameObject.SetActive(true);
    }

    private void onConfirmButtonClicked()
    {
        //do something\
        if(popUpConfimed != null)
        {
            popUpConfimed?.Invoke();
            Close();
        }
        Close();
        return;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        if(popUpCanceled != null)
        {
            popUpCanceled?.Invoke();
        }
        return;
    }
}
