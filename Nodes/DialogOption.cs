using System;
using UnityEngine;
using System.Collections.Generic;

namespace Dialogs
{
    [Serializable]
    public class DialogOption : AbstractNode
    {
        [SerializeField]
        public String DialogText = "";

        public DialogOption()
        {
            Init();
        }

        protected void Init()
        {
            Title = "Dialog option";
        }



        public override void InstallNode(GameObject workingNode)
        {
             AC.Conversation conversation = workingNode.GetComponent<AC.Conversation>();

            if (conversation.options == null)
            {
                conversation.options = new List<AC.ButtonDialog>();
            }

            int[] keyList = new int[1] { conversation.options.Count };

            AC.ButtonDialog newButtonDialog = new AC.ButtonDialog(keyList);
            newButtonDialog.label = DialogText;
            newButtonDialog.isOn = true;

            // By default all conversation options will stop
            newButtonDialog.conversationAction = AC.ConversationAction.Stop;
            
            conversation.options.Add(newButtonDialog);
        }


#if UNITY_EDITOR

        public new int heightSize = 50;

        protected bool overrideConnectionNodes = true;

        public DialogOption(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            Init();
        }

        public override void OnGUI(int WindowID)
        {
            GUILayout.BeginHorizontal();
            DialogText = GUILayout.TextArea(DialogText);
            AddConnectionButton(0);
            GUILayout.EndHorizontal();
            GUI.DragWindow();
        }


        public override bool CanConnectNode(AbstractNode NodeTarget)
        {
            if (NodeTarget.GetType() == typeof(DialogSeed)
                || NodeTarget.GetType() == typeof(Speech))
            {
                return true;
            }

            return false;
        }

        public override void CreateAutomaticNode()
        {
            NewAutomaticNode(typeof(Speech));
        }

#endif



    }
}
