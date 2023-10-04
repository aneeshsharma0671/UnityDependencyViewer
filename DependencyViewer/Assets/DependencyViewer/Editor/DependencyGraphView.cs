using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;


namespace DependencyViewer{
    public class DependencyGraphView : GraphView{
        Dictionary<string, DependencyNode> _nodes = new Dictionary<string, DependencyNode>();
        public DependencyGraphView(){
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            foreach (var node in GetNodes()){
                AddElement(node);
            }
        }

        List<DependencyNode> GetNodes(){
            List<DependencyNode> nodes = new List<DependencyNode>();
            var AllScrpits = AssetDatabase.FindAssets("t:Script", new string[]{"Assets"});
            int i = 0;
            foreach (var guid in AllScrpits){
                string path = AssetDatabase.GUIDToAssetPath(guid);
                name = path.Split('/')[^1];
                var node = new DependencyNode(){
                    title = name,
                    GUID = guid
                };
                _nodes.Add(guid, node);
                node.SetPosition(new Rect(200 * i, 200, 100, 150));
                nodes.Add(node);
                i++;
            }

            foreach(var node in _nodes){
                foreach(var dependency in GetDependencies(AssetDatabase.GUIDToAssetPath(node.Key))){
                    if(_nodes.ContainsKey(AssetDatabase.AssetPathToGUID(dependency))){
                        var port = GeneratePort(node.Value, Direction.Output);
                        node.Value.outputContainer.Add(port);
                        var targetNode = _nodes[AssetDatabase.AssetPathToGUID(dependency)];
                        var targetPort = GeneratePort(targetNode, Direction.Input);
                        targetNode.inputContainer.Add(targetPort);
                        var edge = port.ConnectTo(targetPort);
                        AddElement(edge);
                        node.Value.RefreshExpandedState();
                        node.Value.RefreshPorts();
                        targetNode.RefreshExpandedState();
                        targetNode.RefreshPorts();
                    }
                }
            }

            // Position Nodes
            int loner = 0;
            int outputOnly = 0;
            int inputOnly = 0;
            int both = 0;
            foreach(var node in _nodes){
                if(node.Value.inputContainer.childCount == 0 && node.Value.outputContainer.childCount == 0){
                    node.Value.SetPosition(new Rect(200 * 0, 100 * loner, 100, 150));
                    loner++;
                }else if(node.Value.inputContainer.childCount == 0){
                    node.Value.SetPosition(new Rect(200 * 1, 100 * outputOnly, 100, 150));
                    outputOnly++;
                }else if(node.Value.outputContainer.childCount == 0){
                    node.Value.SetPosition(new Rect(200 * 3, 100 * inputOnly, 100, 150));
                    inputOnly++;
                }else{
                    node.Value.SetPosition(new Rect(200 * 2, 100 * both, 100, 150));
                    both++;
                }
            }

            return nodes;
        }
        
        private Port GeneratePort(DependencyNode node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single){
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        private List<Type> types = new List<Type>();
        private string[] GetDependencies(string path, bool recursive = false){
            // Get Script Dependencies
            var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
        
            return AssetDatabase.GetDependencies(path, recursive);  // recursive false to remove self Dependency
        }

    }
}
