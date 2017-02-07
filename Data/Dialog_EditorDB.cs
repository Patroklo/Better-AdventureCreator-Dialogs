using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Dialogs
{

    sealed class RectSerializatoinSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj,
                                  SerializationInfo info, StreamingContext context)
        {

            Rect rectData = (Rect)obj;
            info.AddValue("x", rectData.x);
            info.AddValue("y", rectData.y);
            info.AddValue("width", rectData.width);
            info.AddValue("height", rectData.height);

        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj,
                                           SerializationInfo info, StreamingContext context,
                                           ISurrogateSelector selector)
        {
            Rect rectData = (Rect)obj;
            rectData.x = (float)info.GetValue("x", typeof(float));
            rectData.y = (float)info.GetValue("y", typeof(float));
            rectData.width = (float)info.GetValue("width", typeof(float));
            rectData.height = (float)info.GetValue("height", typeof(float));

            obj = rectData;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }

    }

    sealed class Vector3SerializationSurrogate : ISerializationSurrogate
    {

        // Method called to serialize a Vector3 object
        public void GetObjectData(System.Object obj,
                                  SerializationInfo info, StreamingContext context)
        {

            Vector3 v3 = (Vector3)obj;
            info.AddValue("x", v3.x);
            info.AddValue("y", v3.y);
            info.AddValue("z", v3.z);
        }

        // Method called to deserialize a Vector3 object
        public System.Object SetObjectData(System.Object obj,
                                           SerializationInfo info, StreamingContext context,
                                           ISurrogateSelector selector)
        {

            Vector3 v3 = (Vector3)obj;
            v3.x = (float)info.GetValue("x", typeof(float));
            v3.y = (float)info.GetValue("y", typeof(float));
            v3.z = (float)info.GetValue("z", typeof(float));
            obj = v3;
            return obj;   // Formatters ignore this return value //Seems to have been fixed!
        }
    }

    public struct DraggingData
    {
        public Vector2 DraggingVector;
        public string OriginUniqueID;
        public int OriginNodeLinkID;
    }

    public class Dialog_EditorDB : MonoBehaviour
    {

        // List of node types that can be casted
        protected List<System.Type> NodeTypeList = new List<System.Type>();
        protected Dictionary<int, string> PublicNodeTitleList = new Dictionary<int, string>();

        [HideInInspector]
        public List<AbstractNode> NodeList { get; set; }

        [HideInInspector]
        public Dictionary<String, AbstractNode> SearchableNodeList { get; set; }

        [HideInInspector]
        public Dictionary<ConnectionData, String> ConnectionNodeList = new Dictionary<ConnectionData, String>(new ConnectionDataEqualityComparer());

        public void New(string FileName)
        {
            Init();
            LoadNodeTypes();

            AddNode(GetLoadedNodeKey(typeof(PropertiesNode)));

            Save(FileName);

            FileManager.ResetFiles();

            Load(FileName);
        }

        public void Save(string FileName)
        {
            BinaryFormatter bf = new BinaryFormatter();

            // 1. Construct a SurrogateSelector object
            SurrogateSelector ss = new SurrogateSelector();

            RectSerializatoinSurrogate v3ss = new RectSerializatoinSurrogate();
            ss.AddSurrogate(typeof(Rect),
                            new StreamingContext(StreamingContextStates.All),
                            v3ss);

            // 2. Have the formatter use our surrogate selector
            bf.SurrogateSelector = ss;

            string fileLocation = FileManager.LoadFile(FileName);

            Stream stream = new FileStream(fileLocation, FileMode.Create, FileAccess.Write, FileShare.None);
            bf.Serialize(stream, NodeList);
            stream.Close();
        }

        public void Load(string FileName)
        {

            BinaryFormatter bf = new BinaryFormatter();

            // 1. Construct a SurrogateSelector object
            SurrogateSelector ss = new SurrogateSelector();

            RectSerializatoinSurrogate v3ss = new RectSerializatoinSurrogate();
            ss.AddSurrogate(typeof(Rect),
                            new StreamingContext(StreamingContextStates.All),
                            v3ss);

            // 2. Have the formatter use our surrogate selector
            bf.SurrogateSelector = ss;

            string fileLocation = FileManager.LoadFile(FileName);

            Stream stream = new FileStream(fileLocation, FileMode.Open);

            List<AbstractNode> _preNodeList = new List<AbstractNode>();

            _preNodeList = (List<AbstractNode>)bf.Deserialize(stream);

            foreach (AbstractNode _node in _preNodeList)
            {
                _node.SetDb(this);
            }

            stream.Close();

            Init();
            LoadNodeTypes();

            // Add Node to lists and dictionaries
            foreach (AbstractNode newNode in _preNodeList)
            {
                AddNodeToLists(newNode);
            }

            // Add connections
            foreach (AbstractNode node in NodeList)
            {
                Dictionary<int, string> activeConnections = node.GetActiveConnections();

                foreach (int key in activeConnections.Keys)
                {
                    AddConnection(node.UniqueID, key, activeConnections[key]);
                }
            }

            PropertiesNode propertiesNode = (PropertiesNode)GetNodeByType(typeof(PropertiesNode));
            propertiesNode.SelectedDialogIndex = FileManager.LoadFiles().FindIndex(a => a == FileName);

            propertiesNode.HasLoadedDialog = true;
        }

        public Dialog_EditorDB()
        {
            Init();

            // Load all node types from the directory
            LoadNodeTypes();

        }

        protected virtual void Init()
        {
            NodeList = new List<AbstractNode>();
            SearchableNodeList = new Dictionary<String, AbstractNode>();

            NodeTypeList = new List<System.Type>();
            PublicNodeTitleList = new Dictionary<int, string>();
            ConnectionNodeList = new Dictionary<ConnectionData, String>(new ConnectionDataEqualityComparer());
        }


        /// <summary>
        /// Loads the node types that can be used in the dialog system.
        /// </summary>
        protected virtual void LoadNodeTypes()
        {
            //AbstractNode newNode = new DialogSeed();
            //PublicNodeTitleList.Add(newNode.Title);
            //newNode = new DialogOption();
            //PublicNodeTitleList.Add(newNode.Title);
            //newNode = new Speech();
            //PublicNodeTitleList.Add(newNode.Title);

            //NodeTypeList.Add(typeof(DialogSeed));
            //NodeTypeList.Add(typeof(DialogOption));
            //NodeTypeList.Add(typeof(Speech));
            //NodeTypeList.Add(typeof(PropertiesNode));
            //NodeTypeList.Add(typeof(NewDialog));

            // Loads all the basic node classes from the main nodes directory
            LoadDirectoryClasses("AdventureCreatorDialogs/Nodes");

        }

        /// <summary>
        /// Loads all the Node Classes from the given folder path
        /// </summary>
        /// <param name="folderPath"></param>
        protected virtual void LoadDirectoryClasses(String folderPath)
        {
            DirectoryInfo dir = new DirectoryInfo("Assets/" + folderPath);
            FileInfo[] info = dir.GetFiles("*.cs");

            String nodeNamespace = "Dialogs";

            foreach (FileInfo f in info)
            {
                try
                {
                    int extentionPosition = f.Name.IndexOf(".cs");
                    string className = f.Name.Substring(0, extentionPosition);

                    StreamReader streamReader = new StreamReader(f.FullName);
                    string fileContents = streamReader.ReadToEnd();
                    streamReader.Close();

                    fileContents = fileContents.Replace(" ", "");

                    if (fileContents.Contains("class" + className + ":AbstractNode") ||
                        fileContents.Contains("class" + className + ":Dialogs.AbstractNode")||
                        fileContents.Contains("class" + className + ":AbstractChecker") ||
                        fileContents.Contains("class" + className + ":Dialogs.AbstractChecker"))
                    {

                        var tempNode = Activator.CreateInstance(Type.GetType(nodeNamespace + "." + className));

                        if (tempNode is AbstractNode)
                        {
                            NodeTypeList.Add(tempNode.GetType());

                            // If is a public node type, we will add it into the list with the last index id from the NodeTypeList
                            if (((AbstractNode)tempNode).isPublic)
                            {
                                PublicNodeTitleList.Add(NodeTypeList.Count - 1, ((AbstractNode)tempNode).Title);
                            }

                        }
                    }
                    else
                    {
                        Debug.Log("The script '" + f.FullName + "' must derive from Dialog's AbstractNode class in order to be available as an Action.");
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }
            }
        }


        /// <summary>
        /// Gets the node by unique ID identifier string
        /// </summary>
        /// <returns>The node by unique identifier.</returns>
        /// <param name="UniqueID">Unique identifier.</param>
        public virtual AbstractNode GetNodeByUniqueID(String UniqueID)
        {
            return SearchableNodeList[UniqueID];
        }


        /// <summary>
        /// Looks in the node list if there's a node of the defined type.
        /// </summary>
        /// <returns><c>true</c>, if node type was hased, <c>false</c> otherwise.</returns>
        /// <param name="nodeType">Node type.</param>
        public virtual bool HasNodeType(System.Type nodeType)
        {
            foreach (AbstractNode node in NodeList)
            {
                if (node.GetType() == nodeType)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the first node selected by type
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public virtual AbstractNode GetNodeByType(System.Type nodeType)
        {
            foreach (AbstractNode node in NodeList)
            {
                if (node.GetType() == nodeType)
                {
                    return node;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the a list of nodes selected by type
        /// </summary>
        /// <param name="nodeType"></param>
        /// <returns></returns>
        public virtual List<AbstractNode> GetNodesByType(System.Type nodeType)
        {
            List<AbstractNode> returnList = new List<AbstractNode>();

            foreach (AbstractNode node in NodeList)
            {
                if (node.GetType() == nodeType)
                {
                    returnList.Add(node);
                }
            }
            return returnList;
        }

        /// <summary>
        /// Returns the type of the loaded node or throws an exception in case it doesn't exist.
        /// </summary>
        /// <returns>The loaded node type.</returns>
        /// <param name="nodeType">Node type.</param>
        protected virtual System.Type GetLoadedNodeType(int nodeType)
        {
            if (NodeTypeList.ElementAtOrDefault(nodeType) != null)
            {
                return NodeTypeList[nodeType];
            }
            else
            {
                throw new IndexOutOfRangeException("Node type " + nodeType + " it's not defined in the node type list");
            }

        }


        public virtual int GetLoadedNodeKey(System.Type NodeType)
        {
            for (int i = 0; i < NodeTypeList.Count; i++)
            {
                if (NodeTypeList[i] == NodeType)
                {
                    return i;
                }
            }
            return -1;
        }

        public virtual Dictionary<int, string> GetPublicNodeList()
        {
            return PublicNodeTitleList;
        }




#if UNITY_EDITOR

        [HideInInspector]
        public DraggingData DragData;

        [HideInInspector]
        public bool DraggingLine = false;



        public virtual void AddConnection(string UniqueID, int NodeLinkID, String LinkedUniqueID)
        {
            ConnectionData keyData = new ConnectionData(UniqueID, NodeLinkID);
            ConnectionNodeList[keyData] = LinkedUniqueID;
        }


        public virtual void DeleteConnection(string UniqueID, int NodeLinkID)
        {
            ConnectionData keyData = new ConnectionData(UniqueID, NodeLinkID);

            ConnectionNodeList.Remove(keyData);
        }


        public virtual void DeleteNode(AbstractNode node)
        {

            // Delete connections from the node to delete
            Dictionary<int, string> connections = node.GetActiveConnections();

            foreach (int NodeLinkID in connections.Keys)
            {
                ConnectionData keyData = new ConnectionData(node.UniqueID, NodeLinkID);
                ConnectionNodeList.Remove(keyData);
            }

            // Delete connections to the node to delete
            var connectedToNode = (from p in ConnectionNodeList
                                   where p.Value == node.UniqueID
                                   select p.Key).ToList();

            foreach (ConnectionData connection in connectedToNode)
            {
                AbstractNode nodeLinked = GetNodeByUniqueID(connection.UniqueID);
                nodeLinked.DisconnectNode(connection.NodeLinkID);
            }

            // Remove node from list
            NodeList.Remove(node);

            // Remove node from list
            SearchableNodeList.Remove(node.UniqueID);
        }

        public virtual bool HasActiveConnection(string UniqueID, int NodeLinkID)
        {
            ConnectionData keyData = new ConnectionData(UniqueID, NodeLinkID);


            if (ConnectionNodeList.ContainsKey(keyData))
            {
                return true;
            }

            return false;
        }



        public virtual Vector2[] GetConnection(ConnectionData connectionData)
        {
            Vector2[] returnData = new Vector2[2];

            AbstractNode originNode = GetNodeByUniqueID(connectionData.UniqueID);

            Rect originNodeRect = originNode.RectWindow;

            Rect originButtonRect = originNode.GetConnection(connectionData.NodeLinkID);

            Rect finishRect = GetNodeByUniqueID(ConnectionNodeList[connectionData]).RectWindow;

            returnData[0] = new Vector2(
                originNodeRect.x
                + originNodeRect.width,
                originNodeRect.y + originButtonRect.y - 40
            );

            returnData[1] = new Vector2(
                finishRect.x, finishRect.y + (finishRect.height / 2)
            );

            return returnData;

        }

        public virtual List<Vector2[]> GetActiveConnectionList()
        {
            List<Vector2[]> returnData = new List<Vector2[]>();

            foreach (ConnectionData connectionData in ConnectionNodeList.Keys)
            {
                if (HasActiveConnection(connectionData.UniqueID, connectionData.NodeLinkID))
                {
                    returnData.Add(GetConnection(connectionData));
                }
            }

            return returnData;
        }




        public virtual AbstractNode AddNode(int nodeTypeID, Vector2 rectPosition)
        {
            Type nodeType = GetLoadedNodeType(nodeTypeID);

            AbstractNode disposableNode = (AbstractNode)Activator.CreateInstance(nodeType);

            // Get an array of FieldInfo objects.
            FieldInfo[] fields = nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance);

            var widthSize = (int)(from p in fields
                                  where p.Name == "widthSize"
                                  select p.GetValue(disposableNode)).First();

            var heightSize = (int)(from p in fields
                                   where p.Name == "heightSize"
                                   select p.GetValue(disposableNode)).First();

            Rect newRect = new Rect(rectPosition.x, rectPosition.y, widthSize, heightSize);
            AbstractNode newNode = (AbstractNode)Activator.CreateInstance(GetLoadedNodeType(nodeTypeID), newRect, this);

            AddNodeToLists(newNode);

            return newNode;
        }


        /// <summary>
        /// Adds a new node. If nodeType is null will insert the first public node type.
        /// </summary>
        /// <returns>The node.</returns>
        /// <param name="nodeTypeID">Node type.</param>
        public virtual AbstractNode AddNode(int nodeTypeID)
        {
            return AddNode(nodeTypeID, new Vector2(0, 0));
        }

        /// <summary>
        /// Adds the node passed via parameters to the List and Dictionaries
        /// </summary>
        /// <param name="node"></param>
        public virtual void AddNodeToLists(AbstractNode newNode)
        {
            NodeList.Add(newNode);

            SearchableNodeList.Add(newNode.UniqueID, newNode);
        }



        /// <summary>
        /// Starts the dragging from a button node link
        /// </summary>
        /// <param name="UniqueID">Unique identifier.</param>
        /// <param name="NodeLinkID">Node link identifier.</param>
        public virtual void StartDragging(string UniqueID, int NodeLinkID)
        {
            DragData = new DraggingData();

            AbstractNode originNode = GetNodeByUniqueID(UniqueID);

            Rect originNodeRect = originNode.RectWindow;

            Rect originButtonRect = originNode.GetConnection(NodeLinkID);

            //Rect cacafuti = new Rect(
            //	(baseNode.GetConnection(NodeLinkID).x +
            //	 baseNode.GetConnection(NodeLinkID).width) + baseNode.RectWindow.x,
            //	(baseNode.GetConnection(NodeLinkID).y +
            //	 baseNode.GetConnection(NodeLinkID).height
            //	) +
            //	baseNode.RectWindow.y,
            //0, 0);

            DragData.DraggingVector = new Vector2(
                                   originNodeRect.x
                                   + originNodeRect.width,
                                   originNodeRect.y + originButtonRect.y - 40
                               );

            DragData.OriginUniqueID = UniqueID;
            DragData.OriginNodeLinkID = NodeLinkID;
            DraggingLine = true;
        }


        public void SetFirstNode(AbstractNode node)
        {
            PropertiesNode propertyNode = (PropertiesNode)GetNodeByType(typeof(PropertiesNode));
            propertyNode.firstNode = node.UniqueID;
        }



#endif
    }
}

