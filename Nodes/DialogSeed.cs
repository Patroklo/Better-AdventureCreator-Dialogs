using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Threading;
using UnityEditor;
using AC;

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

        protected List<string> childNodes = new List<string>();

        public override void InstallNode(GameObject workingNode)
        {
            workingNode.AddComponent<AC.RememberConversation>();
            workingNode.AddComponent<DialogConversation>();

            childNodes = new List<string>();

            // Get all the node options linked to this seed
            foreach (int connectionKey in ActiveConnections.Keys)
            {
                AbstractNode childNode = db.GetNodeByUniqueID(ActiveConnections[connectionKey]);

                if (childNode.GetType() == typeof(DialogOption))
                {
                    childNode.InstallNode(workingNode);

                    List<string> childs = childNode.ChildNodes();

                    foreach (string childUniqueID in childs)
                    {
                        childNodes.Add(childUniqueID);
                    }
                }
            }
        }

        public override List<string> ChildNodes()
        {
            return childNodes;
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
            if (NodeTarget.GetType() == typeof(DialogOption))
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
