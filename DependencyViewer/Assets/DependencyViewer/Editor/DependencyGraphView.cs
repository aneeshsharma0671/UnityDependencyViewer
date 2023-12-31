using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System.IO;


namespace DependencyViewer{
    public class DependencyGraphView : GraphView{
        Dictionary<string, Node> _nodes = new();
        Dictionary<Type,string> scriptPaths = new(); // Type, Path
        Dictionary<Type,Type[]> scriptFieldTypes = new(); 
        Dictionary<Node,List<Node>> edgesDic = new();
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
            var AllScrpits = AssetDatabase.FindAssets("t:Script", new string[]{"Assets/DependencyViewer","Assets/Resources"});

            int i = 0;
            foreach (var guid in AllScrpits){
                string path = AssetDatabase.GUIDToAssetPath(guid);

                var script = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
                Type scriptClass = script.GetClass();
                if(scriptPaths.ContainsKey(scriptClass)){
                    continue;
                }
                List<Type> typeRefs = new();
                FieldInfo[] fields = scriptClass.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                MethodInfo[] typeMethods = scriptClass.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                
                foreach(var field in fields){ typeRefs.Add(field.FieldType); }
                foreach(var typeMethod in typeMethods){ typeRefs.Add(typeMethod.ReturnType); }

                scriptPaths.Add(scriptClass, path);

                scriptFieldTypes.Add(scriptClass,typeRefs.ToArray());
                name = path.Split('/')[^1];
                var node = new DependencyNode(){
                    title = name,
                    GUID = guid,
                    type = scriptClass
                };
                _nodes.Add(guid, node);
                node.SetPosition(new Rect(200 * i, 200, 100, 150));
                nodes.Add(node);
                i++;
            }

            foreach(var node in _nodes){
                foreach(var dependency in GetDependencies(((DependencyNode)node.Value).type)){
                    if(_nodes.ContainsKey(AssetDatabase.AssetPathToGUID(dependency))){
                        // Check if edge already exists
                        Node targetNode = _nodes[AssetDatabase.AssetPathToGUID(dependency)];
                        GenerateEdge(node.Value, targetNode);
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
        
        private Port GeneratePort(Node node, Direction portDirection, Port.Capacity capacity = Port.Capacity.Single){
            return node.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
        }

        private Edge GenerateEdge(Node sourceNode, Node targetNode){
            if(edgesDic.ContainsKey(sourceNode) && edgesDic[sourceNode].Contains(targetNode)){
                return null;
            }
            var port = GeneratePort(sourceNode, Direction.Output);
            sourceNode.outputContainer.Add(port);
            var targetPort = GeneratePort(targetNode, Direction.Input);
            targetNode.inputContainer.Add(targetPort);
            var edge = port.ConnectTo(targetPort);
            AddElement(edge);
            sourceNode.RefreshExpandedState();
            sourceNode.RefreshPorts();
            targetNode.RefreshExpandedState();
            targetNode.RefreshPorts();
            if(edgesDic.ContainsKey(sourceNode)){
                edgesDic[sourceNode].Add(targetNode);
            }
            else{
                edgesDic.Add(sourceNode, new List<Node>(){targetNode});
            }
            return edge;
        }
        private string[] GetDependencies(Type type, bool recursive = false){
            List<string> dependencies = new List<string>();
            if(scriptFieldTypes.ContainsKey(type)){
                Debug.Log($"Type: {type}");
                foreach(var field in scriptFieldTypes[type]){
                    if(field.IsGenericType){
                        Type type1 = field.GetGenericArguments()[0];
                        Debug.Log($"Type1: {type1}");
                        if(scriptPaths.ContainsKey(type1)){
                            dependencies.Add(scriptPaths[type1]);
                        }
                    }else if(field.IsArray){
                        Type type1 = field.GetElementType();
                        Debug.Log($"Type1: {type1}");
                        if(scriptPaths.ContainsKey(type1)){
                            dependencies.Add(scriptPaths[type1]);
                        }
                    }else{
                        Type type1 = field;
                        Debug.Log($"Type1: {type1}");
                        if(scriptPaths.ContainsKey(type1)){
                            dependencies.Add(scriptPaths[type1]);
                        }
                    }
                }
            }
            return dependencies.ToArray() ;  // recursive false to remove self Dependency
        }

    }
}
