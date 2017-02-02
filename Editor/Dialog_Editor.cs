using UnityEngine;
using UnityEditor;

namespace Dialogs
{


    public class Dialog_Editor : ExtendedEditorWindow
    {
        protected UnityEngine.GUI.WindowFunction propertyWindowCreator;

        // Editor title
        string editorTitle = "Dialog Editor";

        Dialog_Editor editor;

        protected Dialog_EditorDB db;

        // Property window Rect definition
        Rect propertyWindow;

        [MenuItem("Window/Dialog editor")]
        static void ShowEditor()
        {
            // Initialize the editor
            Dialog_Editor editor = EditorWindow.GetWindow<Dialog_Editor>();
            editor.Populate();
        }

        public virtual void Populate()
        {
            InitializeDB();

            EditorWindow.GetWindow<Dialog_Editor>().titleContent.text = editorTitle;

            // If there are no nodes, will add the property node
            if (!db.HasNodeType(typeof(PropertiesNode)))
            {
                db.AddNode(db.GetLoadedNodeKey(typeof(PropertiesNode)));
            }

        }


        void DrawNodeLine3(Vector2 start, Vector2 end)
        {
            Handles.DrawBezier(
                new Vector3(start.x, start.y, 0),
                new Vector3(end.x, end.y, 0),
                new Vector3(end.x, end.y, 0),
                new Vector3(start.x, start.y, 0),
                Color.red,
                null,
                5
                );
        }



        public virtual void InitializeDB()
        {
            if (db == null)
            {
                GameObject dbObj = (GameObject)AssetDatabase.LoadAssetAtPath("Assets/AdventureCreatorDialogs/Editor/db.prefab", typeof(GameObject));
                db = dbObj.GetComponent<Dialog_EditorDB>();
            }
        }

        protected override void OnGUI()
        {


            base.OnGUI();

            BeginWindows();
            {
                if (db.DraggingLine)
                {
                    DrawDragNodeLine(db.DragData.DraggingVector, Event.current.mousePosition);

                    Repaint();
                }

                DrawNodes();
                DrawConnections();
            }
            EndWindows();

        }


        protected virtual void DrawConnections()
        {
            if (db.NodeList.Count > 0 && db.ConnectionNodeList.Count > 0)
            {
                foreach (var connectionData in db.GetActiveConnectionList())
                {
                    DrawVectorNodeLine(connectionData[0], connectionData[1]);
                }

                Repaint();
            }
        }




        /// <summary>
        /// Draws the node windows that are stored in the Dialog_EditorDB instance.
        /// </summary>
        protected virtual void DrawNodes()
        {
            if (db.NodeList.Count > 0)
            {
                for (int i = 0; i <= db.NodeList.Count - 1; i++)
                {
                    db.NodeList[i].RectWindow = GUILayout.Window(i, db.NodeList[i].RectWindow, db.NodeList[i].OnGUI, db.NodeList[i].Title);
                }
            }
        }

        protected virtual void checkConnectionLine()
        {
            db.DraggingLine = false;

            bool inNode = false;

            // Check if there's a Dialog rect where we have stopped dragging
            if (db.NodeList.Count > 0)
            {
                for (int i = 0; i <= db.NodeList.Count - 1; i++)
                {
                    if (db.NodeList[i].RectWindow.Contains(Event.current.mousePosition) && !db.NodeList[i].UniqueID.Equals(db.DragData.OriginUniqueID))
                    {
                        AbstractNode nodeInit = db.GetNodeByUniqueID(db.DragData.OriginUniqueID);

                        inNode = true;

                        if (nodeInit.CanConnectNode(db.NodeList[i]))
                        {
                            nodeInit.ConnectNode(db.DragData.OriginNodeLinkID, db.NodeList[i].UniqueID);
                        }
                    }
                }
            }

            if (inNode == false)
            {
                AbstractNode nodeInit = db.GetNodeByUniqueID(db.DragData.OriginUniqueID);
                nodeInit.CreateAutomaticNode();
            }

        }


        /// <summary>
        /// EVENTS
        /// </summary>



        void OnSelectionChange() { Populate(); Repaint(); }
        void OnEnable() { Populate(); }
        void OnFocus() { Populate(); }

        protected override void OnMouseUp(MouseButton button, Vector2 position)
        {
            if (db.DraggingLine)
            {
                checkConnectionLine();
            }
        }


        protected override void OnMouseDown(MouseButton button, Vector2 position)
        {

            if (button == MouseButton.Right)
            {
                Event currentEvent = Event.current;

                Vector2 mousePos = currentEvent.mousePosition;

                // Check if there's a Dialog rect where we have stopped dragging
                if (db.NodeList.Count > 0)
                {
                    for (int i = 0; i <= db.NodeList.Count - 1; i++)
                    {
                        if (db.NodeList[i].RectWindow.Contains(Event.current.mousePosition))
                        {
                            db.NodeList[i].RightClickMenu();
                        }
                    }
                }
            }
        }


        protected override void OnMouseDrag(MouseButton button, Vector2 position, Vector2 delta)
        {
            /// <summary>
            /// If we are dragging and we don't have any tool selected, we will
            /// move all the node map.
            /// </summary>
            if (!db.DraggingLine && GUIUtility.hotControl == 0)
            {
                for (int i = 0; i <= db.NodeList.Count - 1; i++)
                {
                    Rect a = db.NodeList[i].RectWindow;

                    a.x += delta.x;
                    a.y += delta.y;

                    db.NodeList[i].RectWindow = a;

                    Repaint();
                }
            }
        }

        void DrawDragNodeLine(Vector2 start, Vector2 mouse)
        {

            Color nc2 = Color.black;


            Handles.DrawBezier(
                new Vector3(start.x, start.y + 52, 0),
           new Vector3(mouse.x, mouse.y, 0),
           new Vector3(mouse.x, mouse.y, 0),
                new Vector3(start.x, start.y + 52, 0),
           nc2,
           null,
           7
           );
        }

        void DrawVectorNodeLine(Vector2 start, Vector2 end)
        {
            Color nc = Color.black;
            Color nc2 = Color.black;
            nc.a = 0.25f;
            nc2.r -= 0.15f;
            nc2.g -= 0.15f;
            nc2.b -= 0.15f;
            nc2.a = 1;
            Handles.DrawBezier(
                new Vector3(start.x, start.y + 52, 0),
           new Vector3(end.x, end.y + 2, 0),
           new Vector3(end.x, end.y + 2, 0),
                new Vector3(start.x, start.y + 52, 0),
           nc,
           null,
           7
           );
            Handles.DrawBezier(
                new Vector3(start.x, start.y + 50, 0),
                new Vector3(end.x, end.y, 0),
                new Vector3(end.x, end.y, 0),
                new Vector3(start.x, start.y + 50, 0),
                nc2,
                null,
                5
                );


        }

        void DrawNodeLine(Rect start, Rect end)
        {
            Color nc = Color.black;
            Color nc2 = Color.black;
            nc.a = 0.25f;
            nc2.r -= 0.15f;
            nc2.g -= 0.15f;
            nc2.b -= 0.15f;
            nc2.a = 1;
            Handles.DrawBezier(
                new Vector3(start.x + start.width, start.y + 52, 0),
           new Vector3(end.x, end.y + (end.height / 2) + 2, 0),
           new Vector3(end.x, end.y + (end.height / 2) + 2, 0),
                new Vector3(start.x + start.width, start.y + 52, 0),
           nc,
           null,
           7
           );
            Handles.DrawBezier(
                new Vector3(start.x + start.width, start.y + 50, 0),
                new Vector3(end.x, end.y + (end.height / 2), 0),
                new Vector3(end.x, end.y + (end.height / 2), 0),
                new Vector3(start.x + start.width, start.y + 50, 0),
                nc2,
                null,
                5
                );


        }
    }
}
