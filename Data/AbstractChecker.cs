using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dialogs
{
    public abstract class AbstractChecker : AbstractNode
    {
        public AbstractChecker()
        { }

        [NonSerializedAttribute]
        public Dictionary<int, AC.ButtonDialog> AssignedOptionButtons = new Dictionary<int, AC.ButtonDialog>();

        protected List<string> ChildNodes = new List<string>();
        public override void InstallNode(GameObject workingNode)
        {
            // Get all childs and install them.
            foreach (int connectionKey in ActiveConnections.Keys)
            {
                String childUniqueID = ActiveConnections[connectionKey];

                AbstractNode childNode = db.GetNodeByUniqueID(childUniqueID);

                if (childNode.GetType() == typeof(DialogOption))
                {
                    childNode.InstallNode(workingNode);

                    ChildNodes.AddRange(childNode.GetChildNodes());

                    // We get the parent node (conversation seed node) and then add to the
                    // ordered option unique ids this uniqueID.
                    String parentNodeUniqueID = workingNode.name;
                    DialogSeed parentSeed = (DialogSeed)db.GetNodeByUniqueID(parentNodeUniqueID);

                    AC.Conversation dialogScript = workingNode.GetComponent<AC.Conversation>();

                    AssignedOptionButtons.Add(
                        connectionKey,
                        dialogScript.options.Last()
                    );

                    parentSeed.CheckerList.Add(this);
                }
            }
        }



        public override List<string> GetChildNodes()
        {
            return ChildNodes;
        }


        protected void ProcessResult(bool result)
        {
            if (result == true && AssignedOptionButtons.ContainsKey(1))
            {
                AssignedOptionButtons[1].isOn = false;
            }
            else if (result == false && AssignedOptionButtons.ContainsKey(0))
            {
                AssignedOptionButtons[0].isOn = false;
            }
        }



#if UNITY_EDITOR

        public AbstractChecker(Rect newRect, Dialog_EditorDB db) : base(newRect, db)
        {
            this.db = db;

            RectWindow = newRect;

            UniqueID = Guid.NewGuid().ToString();
        }

#endif



    }



}