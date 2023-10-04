using UnityEditor;
using UnityEngine.UIElements;
namespace DependencyViewer{
public class DependencyViewEditor : EditorWindow
{
        DependencyGraphView _graphView;

        [MenuItem("Window/Viewer/Dependency Viewer")]
        public static void Open(){
            GetWindow<DependencyViewEditor>("Dependency Viewer");
        }

        private void OnEnable(){
            AddGraphView();
        }

        private void AddGraphView(){
            _graphView = new DependencyGraphView();
            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void OnDisable(){
            rootVisualElement.Remove(_graphView);
        }

    }
}
