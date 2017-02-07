using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEditor;

namespace Dialogs
{
    [Serializable]
    public class DialogSeed : AbstractNode
    {
        public DialogSeed()
        {
            Init();
        }


        protected void Init()
        {
            Title = "Dialog seed";

            isPublic = true;
        }

        public bool IsFirstNode = false;

        protected List<string> ChildNodes = new List<string>();

        // Orders when installing the nodes the UniqueIDs of the option nodes
        public List<string> OrderedOptionUniqueIDs = new List<String>();

        public List<AbstractChecker> CheckerList = new List<AbstractChecker>();

        public override void InstallNode(GameObject workingNode)
        {
            OrderedOptionUniqueIDs = new List<string>();

            workingNode.AddComponent<AC.RememberConversation>();
            workingNode.AddComponent<AC.Conversation>();

            ChildNodes = new List<string>();

            // Get all the node options linked to this seed
            foreach (int connectionKey in ActiveConnections.Keys)
            {
                AbstractNode childNode = db.GetNodeByUniqueID(ActiveConnections[connectionKey]);

                if (childNode.GetType() == typeof(DialogOption))
                {
                    childNode.InstallNode(workingNode);

                    List<string> childs = childNode.GetChildNodes();

                    foreach (string childUniqueID in childs)
                    {
                        ChildNodes.Add(childUniqueID);
                    }
                }
                // If it's a varcheck, then we will get all its childs, 
                // add them as buttons into the dialog and then get the
                // childs of those DialogOptions and add them into the
                // ChildNodes list
                else if (childNode.GetType() == typeof(VarCheck))
                {
                    childNode.InstallNode(workingNode);
                }
            }
        }

        public override void MakeConnections(GameObject workingNode)
        {
            AC.Conversation conversation = workingNode.GetComponent<AC.Conversation>();

            Transform parentTransform = workingNode.transform.parent;

            foreach (string childUniqueID in ChildNodes)
            {
                // We get each child node. Only can be right now a DialogOption type.
                AbstractNode dialogOptionNode = db.GetNodeByUniqueID(childUniqueID);
                List<AbstractNode> dialogChilds = ConnectNodeChilds(dialogOptionNode);

                int dialogIndex = OrderedOptionUniqueIDs.FindIndex(a => a == dialogOptionNode.UniqueID);

                AC.ButtonDialog dialogButton = conversation.options[dialogIndex];

                foreach (AbstractNode childNode in dialogChilds)
                {
                    GameObject childObject = parentTransform.FindChild(childNode.UniqueID).gameObject;

                    if (childNode.GetType() == typeof(Speech))
                    {
                        AC.DialogueOption dialogueOption = childObject.GetComponent<AC.DialogueOption>();

                        dialogButton.dialogueOption = dialogueOption;
                    }
                    if (childNode.GetType() == typeof(DialogSeed))
                    {
                        if (childNode.UniqueID == UniqueID)
                        {
                            dialogButton.conversationAction = AC.ConversationAction.ReturnToConversation;
                        }
                        else
                        {
                            AC.Conversation nextConversation = childObject.GetComponent<AC.Conversation>();
                            dialogButton.conversationAction = AC.ConversationAction.RunOtherConversation;
                            dialogButton.newConversation = nextConversation;
                        }
                    }
                }
            }
        }


        private List<AbstractNode> ConnectNodeChilds(AbstractNode node)
        {
            List<AbstractNode> returnList = new List<AbstractNode>();

            List<string> childList = node.GetChildNodes();

            foreach (string childUniqueID in childList)
            {
                AbstractNode childNode = db.GetNodeByUniqueID(childUniqueID);

                returnList.Add(childNode);

                if (childNode.GetType() == typeof(Speech))
                {
                    returnList.AddRange(ConnectNodeChilds(childNode));
                }
            }

            return returnList;
        }


        public override List<string> GetChildNodes()
        {
            return ChildNodes;
        }

        // Will execute all the checkers that are linked to this node
        public void ExecuteCheckers()
        {
            foreach (IChecker checker in CheckerList)
            {
                checker.Check();
            }
        }

#if UNITY_EDITOR


        public new int widthSize = 50;
        public new int heightSize = 200;


        List<int> keys = new List<int>();

        protected int Coursor = 0;

        public DialogSeed(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            Init();
        }


        public override void OnGUI(int WindowID)
        {
            if (Event.current.type == EventType.Layout)
            {
                keys = ActiveConnections.Keys.ToList();
            }

            GUILayout.BeginVertical();
            GUILayout.Label("Check to make this node as the starter dialog node.");

            Color preColor = GUI.color;

            if (!IsFirstNode)
            {
                GUI.color = Color.green;
                if (GUILayout.Button("Set as first dialog node"))
                {
                    IsFirstNode = true;
                    db.SetFirstNode(this);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("This node is initial in the dialog.", MessageType.Info);
            }
            GUI.color = preColor;



            GUILayout.EndVertical();

            GUILayout.Label("Drag buttons to connect dialog options.");

            foreach (int keyConnection in keys)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.BeginVertical();
                AddConnectionButton(keyConnection);
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.BeginVertical();
            AddConnectionButton(Coursor);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();


            GUI.DragWindow();

        }



        public override void ConnectNode(int NodeLinkID, string LinkedUniqueID)
        {
            base.ConnectNode(NodeLinkID, LinkedUniqueID);
            Coursor++;
        }

        public override bool CanConnectNode(AbstractNode NodeTarget)
        {
            if (NodeTarget.GetType() == typeof(DialogOption)
            || NodeTarget.GetType() == typeof(VarCheck))
            {
                return true;
            }

            return false;
        }

        public override void CreateAutomaticNode()
        {
            NewAutomaticNode(typeof(DialogOption));
        }


#endif

    }
}
