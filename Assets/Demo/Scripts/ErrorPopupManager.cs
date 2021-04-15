using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorPopupManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI errorMessage;
    private Transform errorPopup;

    private void Awake() 
    {
        errorPopup = transform.GetChild(0);

        errorPopup.gameObject.SetActive(false);
    }
    
    private void OnEnable() 
    {
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void OnDisable() 
    {
        Application.logMessageReceived -= OnLogMessageReceived;
    }

    private void OnLogMessageReceived(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            errorMessage.text = logString;
            errorPopup.gameObject.SetActive(true);
        }
    }
}
