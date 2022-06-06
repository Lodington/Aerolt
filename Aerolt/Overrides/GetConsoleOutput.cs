using RoR2;
using TMPro;
using UnityEngine;

namespace Aerolt.Overrides
{
    public class GetConsoleOutput : MonoBehaviour
    {
        private static TMP_Text _consoleOutput;
        [SerializeField] private TMP_InputField textField;

        public void SendCommand()
        {
            if (textField.text == "")
                return;
            RoR2.Console.instance.SubmitCmd(LocalUserManager.GetFirstLocalUser(), textField.text, true);
            textField.text = "";
        }

        public static void OnLogReceived(RoR2.Console.Log log)
        {
            _consoleOutput = GameObject.Find("Output").GetComponent<TMP_Text>();

            _consoleOutput.text += $"\n[<color=#ffffff>{log.logType}] {log.message}</color>";
        }
    }   
}
