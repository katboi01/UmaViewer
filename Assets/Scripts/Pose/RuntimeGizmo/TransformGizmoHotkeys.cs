using CommandUndoRedo;
using UnityEngine;

public class TransformGizmoHotkeys : MonoBehaviour
{
    public void Undo()
    {
        UndoRedoManager.Undo();
    }

    public void Redo()
    {
        UndoRedoManager.Redo();
    }
}