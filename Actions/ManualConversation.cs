using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AC
{

    public class ManualConversation : AC.Action
    {


        public int parameterID = -1;
        public int constantID = 0;
        public Dialogs.DialogConversation conversation;

        public ManualConversation()
        {
            this.isDisplayed = true;
            category = ActionCategory.Dialogue;
            title = "Start manual conversation";
            description = "Enters Conversation mode, and displays the available dialogue options in a specified conversation.";
            numSockets = 0;
        }

        override public void AssignValues(List<ActionParameter> parameters)
        {
            conversation = AssignFile<Dialogs.DialogConversation>(parameters, parameterID, constantID, conversation);
        }

        override public float Run()
        {

            if (conversation)
            {
                conversation.Interact();
            }

            return 0f;
        }

        override public void Skip()
        {
            if (KickStarter.actionListManager.ignoreNextConversationSkip)
            {
                KickStarter.actionListManager.ignoreNextConversationSkip = false;
                return;
            }
            Run();
        }

        override public ActionEnd End(List<AC.Action> actions)
        {
            if (conversation)
            {
                int _chosenOptionIndex = conversation.lastOption;
                conversation.lastOption = -1;
            }
            return GenerateStopActionEnd();
        }

#if UNITY_EDITOR

        override public void ShowGUI(List<ActionParameter> parameters)
        {
            parameterID = Action.ChooseParameterGUI("Conversation:", parameters, parameterID, ParameterType.GameObject);
            if (parameterID >= 0)
            {
                constantID = 0;
                conversation = null;
            }
            else
            {
                conversation = (Dialogs.DialogConversation)EditorGUILayout.ObjectField("Conversation:", conversation, typeof(Dialogs.DialogConversation), true);

                constantID = FieldToID<Conversation>(conversation, constantID);
                conversation = IDToField<Dialogs.DialogConversation>(conversation, constantID, false);
            }

            if (conversation)
            {
                conversation.Upgrade();
            }
        }

        override public void AssignConstantIDs(bool saveScriptsToo)
        {
            if (saveScriptsToo)
            {
                AddSaveScript<RememberConversation>(conversation);
            }
            AssignConstantID<Conversation>(conversation, constantID, parameterID);
        }


        override public string SetLabel()
        {
            string labelAdd = "";

            if (conversation)
            {
                labelAdd = " (" + conversation + ")";
            }

            return labelAdd;
        }

#endif


    }

}

