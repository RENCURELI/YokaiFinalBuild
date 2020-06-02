using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

public class StateMachineEditorWindow : EditorWindow
{
    public class StateMachineInspector : EditorWindow
    {
        public static StateMachineInspector main;
        public bool docked = false;

        public StateMachine.State state;
        public StateMachine.Transition transition;

        private void StateInspector()
        {
            state.OnGUI();
        }

        private void TransitionInspector()
        {
            transition.OnGUI();
        }

        private void OnGUI()
        {
            GUILayout.Label("State Machine Inspector");
            GUILayout.Space(20);
            if (state != null)
            {
                StateInspector();
            } else if (transition != null)
            {
                TransitionInspector();
            }
        }
    }

    public StateMachine target;
    private List<StateMachine.Transition> oldValidTransitions = new List<StateMachine.Transition>();

    public Texture backgroundTexture;

    private Vector2 viewPosition = Vector2.zero;

    //

    private void OnDestroy()
    {
        StateMachineInspector.main.Close();
    }

    public static void Open(StateMachine target)
    {
        if (target == null)
        {
            target = new StateMachine("Untitled", 1);
        }

        StateMachineEditorWindow w = GetWindow<StateMachineEditorWindow>();
        w.target = target;
        w.verified = true;
        //w.viewPosition = w.GetFirstPosition();
        w.Show();
        StateMachineInspector inspector = GetWindow<StateMachineInspector>();
        StateMachineInspector.main = inspector;
        inspector.titleContent = new GUIContent( "Inspector");
        inspector.Show();
    }

    private void Verify()
    {
        if (target == null)
        {
            Close();
            return;
        }
        if (target.states == null)
        {
            target.states = new List<StateMachine.State>();
            target.AddState("Start");
        }
    }

    private void DockInspector()
    {
        if (StateMachineInspector.main.docked) return;
        this.Dock(StateMachineInspector.main, Docker.DockPosition.Right);
        StateMachineInspector.main.docked = true;
    }

    private Vector2 GetFirstPosition()
    {
        if (target.states.Count > 0)
        {
            return target.states[0].position;
        }
        else
        {
            return Vector2.zero;
        }
    }

    private Vector2 ApplyView(Vector2 pos)
    {
        return pos + (viewPosition + position.size * 0.5f);
    }

    private void DrawBackground()
    {
        Vector2 size = new Vector2(backgroundTexture.width, backgroundTexture.height);
        Rect area = new Rect(Vector2.zero, this.position.size);
        Rect coords = new Rect(viewPosition / size*new Vector2(-1,1), position.size / size);
        GUI.DrawTextureWithTexCoords(area, backgroundTexture, coords);
    }

    private void DrawSingleState(StateMachine.State state)
    {
        Vector2 size = new Vector2(150, 50);
        Vector2 pos = ApplyView(state.position);
        string label = state.name;

        Rect rect = new Rect(pos - size * 0.5f, size);

        GUIStyle style = GUI.skin.label;
        GUI.color = Color.white;
        
        if (state.active)
        {
            GUI.color = Color.cyan;
            style = EditorStyles.boldLabel;
        }
        if (state == StateMachineInspector.main.state)
        {
            GUI.color = Color.yellow;
        }

        GUILayout.BeginArea(rect, GUI.skin.box);
        GUI.color = Color.white;
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(label, style);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.EndArea();
    }

    private void DrawDebugStatebox()
    {
        Vector2 size = new Vector2(150, 50);
        Vector2 pos = ApplyView(Vector2.zero);
        string label = "Hello World";

        Rect rect = new Rect(pos - size * 0.5f, size);

        GUILayout.BeginArea(rect, GUI.skin.box);
        GUILayout.Label(label);
        GUILayout.EndArea();
    }

    private void DrawSingleTransition(StateMachine.Transition transition)
    {
        StateMachine.State from = target.GetState(transition.fromName);
        StateMachine.State to = target.GetState(transition.toName);

        Vector2 start = ApplyView(from.position);
        Vector2 end = ApplyView(to.position);

        Vector2 offset = Vector2.down;
        if (start.x > end.x) offset = Vector2.up;

        float distance = Vector2.Distance(start, end);

        Vector2 startTan = start + offset * distance * 0.5f;
        Vector2 endTan = end + offset * distance * 0.5f;

        float width = 1.5f;
        Handles.color = Color.gray;
        if (transition == StateMachineInspector.main.transition)
        {
            Handles.color = Color.yellow;
            width = 3;
        }
        if (target.validTransition.Contains(transition))
        {
            Handles.color = Color.cyan;
        } else if (oldValidTransitions.Contains(transition))
        {
            Handles.color = Color.blue;
        }

        Handles.DrawBezier(start, end, startTan, endTan, Handles.color, Texture2D.whiteTexture, width);

        Vector2 handle = Vector2.Lerp(start*0.5f+end*0.5f, startTan*0.5f + endTan*0.5f, 0.75f);

        offset = Vector2.right;
        if (start.x > end.x) offset = Vector2.left;

        Handles.DrawAAConvexPolygon(handle + offset * 10, handle + Vector2.up * 8, handle + Vector2.down * 8);
    }

    private void UpdateValidTransitions()
    {
        if (target.validTransition.Count > 0)
        {
            oldValidTransitions.Clear();
            oldValidTransitions.AddRange(target.validTransition);
        }
    }

    private void DrawTransitions()
    {
        for (int i = 0; i < target.transitions.Count; i++)
        {
            DrawSingleTransition(target.transitions[i]);
        }
        UpdateValidTransitions();
    }

    private void DrawStates()
    {
        for (int i = 0; i < target.states.Count; i++)
        {
            StateMachine.State state = target.states[i];
            DrawSingleState(state);
        }
    }

    private bool viewPan = false;
    private void HandleViewPan(Event e)
    {
        Rect area = new Rect(Vector2.zero, this.position.size);
        EditorGUIUtility.AddCursorRect(new Rect(Vector2.zero, position.size), MouseCursor.Pan);
        if (viewPan)
        {
            if (e.type == EventType.MouseUp && e.button == 0)
            {
                viewPan = false;
                e.Use();
                Repaint();
            }
            else if (e.type != EventType.Repaint && e.type != EventType.Layout)
            {
                Vector2 delta = e.delta;
                viewPosition += delta;
                e.Use();
                Repaint();
            }
        } else
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                viewPan = true;
                e.Use();
                Repaint();
            }
        }
    }
    private void HandleSingleTransitionEvent(StateMachine.Transition transition, Event e)
    {
        StateMachine.State from = target.GetState(transition.fromName);
        StateMachine.State to = target.GetState(transition.toName);

        Vector2 start = ApplyView(from.position);
        Vector2 end = ApplyView(to.position);

        Vector2 offset = Vector2.down;
        if (start.x > end.x) offset = Vector2.up;

        float distance = Vector2.Distance(start, end);

        Vector2 startTan = start + offset * distance * 0.5f;
        Vector2 endTan = end + offset * distance * 0.5f;

        Vector2 handle = Vector2.Lerp(start * 0.5f + end * 0.5f, startTan * 0.5f + endTan * 0.5f, 0.75f);

        Rect rect = new Rect(handle - Vector2.one * 10, Vector2.one * 20);

        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDown && e.button == 0)
        {
            StateMachineInspector.main.transition = transition;
            StateMachineInspector.main.state = null;
            StateMachineInspector.main.Repaint();
            e.Use();
        }
    }
    private void HandleTransitionEvents(Event e)
    {
        for (int i = 0; i < target.transitions.Count; i++)
        {
            HandleSingleTransitionEvent(target.transitions[i], e);
        }
    }
    private void HandleSingleStateEvent(StateMachine.State state, Event e)
    {
        Vector2 size = new Vector2(150, 50);
        Vector2 pos = ApplyView(state.position);

        Rect rect = new Rect(pos - size * 0.5f, size);
        EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

        if (rect.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                StateMachineInspector.main.state = state;
                StateMachineInspector.main.transition = null;
                StateMachineInspector.main.Repaint();
                e.Use();
            }
        }
    }
    private void HandleStateEvents(Event e)
    {
        for (int i = 0; i < target.states.Count; i++)
        {
            HandleSingleStateEvent(target.states[i], e);
        }
    }

    private void HandleEvents()
    {
        Event e = Event.current;
        HandleTransitionEvents(e);
        HandleStateEvents(e);
        HandleViewPan(e);
    }

    private bool verified;
    private void OnGUI()
    {
        autoRepaintOnSceneChange = true;
        if (!verified) Close();
        verified = false;
        Verify();
        DockInspector();
        //
        DrawBackground();
        DrawTransitions();
        DrawStates();
        //DrawDebugStatebox();
        HandleEvents();
        //
        verified = true;
    }
}
