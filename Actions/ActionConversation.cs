using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dialogs
{


    [System.Serializable]
    public class ActionConversation : AC.ActionConversation
    {

        public DialogHandler dialogHandler;

        public bool dialogLoaded = false;


		public ActionConversation ()
		{
			this.isDisplayed = true;
			category = AC.ActionCategory.Dialogue;
			title = "Start manually made conversation";
			description = "Enters Conversation mode, and displays the available dialogue options in a specified conversation.";
			numSockets = 0;
		}

        override public float Run()
        {
            List<AbstractNode> dialogSeeds = dialogHandler.GetNodesByType(typeof(DialogSeed));

            foreach (AbstractNode dialogSeed in dialogSeeds)
            {
                ((DialogSeed)dialogSeed).ExecuteCheckers();
            }
            return base.Run();
        }


#if UNITY_EDITOR

        override public void ShowGUI(List<AC.ActionParameter> parameters)
        {
            parameterID = AC.Action.ChooseParameterGUI("Conversation:", parameters, parameterID, AC.ParameterType.GameObject);
            if (parameterID >= 0)
            {
                constantID = 0;
                conversation = null;
            }
            else
            {
                dialogHandler = (DialogHandler)EditorGUILayout.ObjectField("Conversation:", dialogHandler, typeof(DialogHandler), true);

                if (dialogHandler != null)
                {
                    dialogLoaded = dialogHandler.LoadedDialogue;

                    if (dialogLoaded == true)
                    {
                        // Gets the first conversation's gameobject
                        GameObject firstDialogObject = dialogHandler.gameObject.transform.FindChild(dialogHandler.GetFirstNode().UniqueID).gameObject;

                        conversation = firstDialogObject.GetComponent<AC.Conversation>();

                        constantID = FieldToID<AC.Conversation>(conversation, constantID);
                        conversation = IDToField<AC.Conversation>(conversation, constantID, false);
                    }

                }
            }

            if (dialogHandler != null && dialogLoaded == false)
            {
                EditorGUILayout.HelpBox("The select conversation it's not properly loaded.", MessageType.Warning);
            }


            if (conversation)
            {
                conversation.Upgrade();
                overrideOptions = EditorGUILayout.Toggle("Override options?", overrideOptions);

                if (overrideOptions)
                {
                    numSockets = conversation.options.Count;
                }
                else
                {
                    numSockets = 0;
                }
            }
            else
            {
                if (isAssetFile && overrideOptions && constantID != 0)
                {
                    EditorGUILayout.HelpBox("Cannot find linked Conversation - please open its scene file.", MessageType.Warning);
                }
                else
                {
                    numSockets = 0;
                }
            }
        }
#endif

    }

}