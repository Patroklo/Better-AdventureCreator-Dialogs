using System;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Threading;
using System.Runtime.Serialization;

namespace Dialogs
{
    [Serializable]
    public class ConnectionData
    {
        [SerializeField]
        public String UniqueID;
        [SerializeField]
        public int NodeLinkID;

        public ConnectionData(String UniqueID, int NodeLinkID)
        {
            this.UniqueID = UniqueID;
            this.NodeLinkID = NodeLinkID;
        }
    }

    // Buscador hecho a mano para el diccionario que usa ConnectionData como KEY
    public class ConnectionDataEqualityComparer : IEqualityComparer<ConnectionData>
    {
        public bool Equals(ConnectionData x, ConnectionData y)
        {
            return ((x.UniqueID.Equals(y.UniqueID)) & (x.NodeLinkID == y.NodeLinkID));
        }

        public int GetHashCode(ConnectionData obj)
        {
            string combined = obj.UniqueID + "|" + obj.NodeLinkID.ToString();
            return (combined.GetHashCode());
        }
    }

    [Serializable]
    abstract public class AbstractNode
    {
        [SerializeField]
        protected Dictionary<int, string> ActiveConnections = new Dictionary<int, string>();

        [NonSerialized]
        protected Dialog_EditorDB db;

        [SerializeField]
        public string UniqueID;

        [SerializeField]
        public int ID;

        [SerializeField]
        public string Title;

        [SerializeField]
        public bool isPublic = true;


        public AbstractNode()
        { }

        public virtual void InstallNode(GameObject workingNode)
        { }

        public virtual List<string> ChildNodes()
        {
            List<string> returnList = new List<string>();

            foreach (int childKey in ActiveConnections.Keys)
            {
                returnList.Add(ActiveConnections[childKey]);
            }

            return returnList;
        }


        public virtual void SetDb(Dialog_EditorDB db)
        {
            this.db = db;
        }


#if UNITY_EDITOR

        public int widthSize = 200;
        public int heightSize = 200;

        [SerializeField]
        public Rect RectWindow { get; set; }

        [SerializeField]
        protected Dictionary<int, Rect> ActiveConnectionRects = new Dictionary<int, Rect>();

        // static readonly object _disconnectNode = new object();

        static readonly object _connectNode = new object();


        public AbstractNode(Rect newRect, Dialog_EditorDB db)
        {
            this.db = db;

            RectWindow = newRect;

            UniqueID = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Adds a connection button linked to a NodeLinkID
        /// </summary>
        /// <param name="NodeLinkID">Node link identifier.</param>
        public virtual void AddConnectionButton(int NodeLinkID)
        {


            if (!db.HasActiveConnection(UniqueID, NodeLinkID))
            {
                if (GUILayout.RepeatButton("O", GUILayout.Width(30)))
                {
                    db.StartDragging(UniqueID, NodeLinkID);
                }

                UpdateConnectionRects(NodeLinkID);
            }
            else
            {
                if (GUILayout.Button("X", GUILayout.Width(30)))
                {
                    DisconnectNode(NodeLinkID);
                }

                UpdateConnectionRects(NodeLinkID);

            }
        }

        /// <summary>
        /// Returns all the active connections
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<int, string> GetActiveConnections()
        {
            return ActiveConnections;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NodeLinkID"></param>
        protected virtual void UpdateConnectionRects(int NodeLinkID)
        {
            if (Event.current.type == EventType.Repaint)
            {
                Rect lr = GUILayoutUtility.GetLastRect();
                ActiveConnectionRects[NodeLinkID] = lr;
            }
        }


        public virtual Rect GetConnection(int NodeLinkID)
        {
            return ActiveConnectionRects[NodeLinkID];
        }

        public virtual void OnGUI(int WindowID)
        { }


        /// <summary>
        /// Basic menu that appears when right clicking. In this case, a delete menu option
        /// </summary>
        public virtual void RightClickMenu()
        {
            // Now create the menu, add items and show it
            GenericMenu menu = new GenericMenu();
            menu.AddItem(new GUIContent("Delete"), false, DeleteCallback);
            //menu.AddItem(new GUIContent("MenuItem2"), false, Callback, "item 2");
            //menu.AddSeparator("");
            //menu.AddItem(new GUIContent("SubMenu/MenuItem3"), false, Callback, "item 3");
            menu.ShowAsContext();
        }


        /// <summary>
        /// Deletes the actual node
        /// </summary>
        protected virtual void DeleteCallback()
        {
            db.DeleteNode(this);
        }


        /// <summary>
        /// Disconnects the node linked to the link defined in the parameter
        /// </summary>
        /// <param name="NodeLinkID"></param>
        public virtual void DisconnectNode(int NodeLinkID)
        {
            db.DeleteConnection(UniqueID, NodeLinkID);
            ActiveConnectionRects.Remove(NodeLinkID);
            ActiveConnections.Remove(NodeLinkID);
        }

        /// <summary>
        /// Threaded method to connect the node due to problems with the gui
        /// </summary>
        /// <param name="NodeLinkID"></param>
        /// <param name="LinkedUniqueID"></param>
        private void ThreadedConnectNode(int NodeLinkID, string LinkedUniqueID)
        {
            lock (_connectNode)
            {
                ActiveConnections.Add(NodeLinkID, LinkedUniqueID);
                db.AddConnection(this.UniqueID, NodeLinkID, LinkedUniqueID);
            }
        }

        /// <summary>
        /// Connects this node to the defined in the LinkedUniqueID parameter.
        /// </summary>
        /// <param name="NodeLinkID"></param>
        /// <param name="LinkedUniqueID"></param>
        public virtual void ConnectNode(int NodeLinkID, string LinkedUniqueID)
        {
            ThreadStart start = new ThreadStart(() => ThreadedConnectNode(NodeLinkID, LinkedUniqueID));
            new Thread(start).Start();
        }


        /// <summary>
        /// Checks if the connection to the target node can be made.
        /// </summary>
        /// <param name="NodeTarget"></param>
        /// <returns></returns>
        public virtual bool CanConnectNode(AbstractNode NodeTarget)
        {
            return true;
        }


        /// <summary>
        /// If dragging a line ends in a empty area, it could
        /// make a node automatically depending of the parent type.
        /// </summary>
        public virtual void CreateAutomaticNode()
        { }

        /// <summary>
        /// Creates a new node and connects it to the loaded one.
        /// </summary>
        /// <param name="nodeType"></param>
        protected virtual void NewAutomaticNode(System.Type nodeType)
        {
            int nodeIndex = db.GetLoadedNodeKey(nodeType);

            Event currentEvent = Event.current;

            Vector2 mousePos = currentEvent.mousePosition;

            AbstractNode newNode = db.AddNode(nodeIndex, mousePos);

            ConnectNode(db.DragData.OriginNodeLinkID, newNode.UniqueID);
        }

        //protected virtual void InitializeDB()
        //{
        //    if (db == null)
        //    {
        //        GameObject dbObj = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AdventureCreatorDialogs/Editor/db.prefab", typeof(GameObject));
        //        db = dbObj.GetComponent<Dialog_EditorDB>();
        //    }
        //}

        /// <summary>
        /// Called when deserializing, loads the node database class
        /// </summary>
        /// <param name="context"></param>
        [OnDeserialized()]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            //InitializeDB();
        }

#endif
    }
}