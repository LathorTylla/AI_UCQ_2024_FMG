using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using static UnityEngine.UI.Image;

public class Node
{
    public Node(string inName)
    {
        Name = inName;
        Parent = null;
    }

    public string Name = "";  // es string por pura claridad, idealmente se usan ints para diferenciar objetos.

    public Node Parent;  // referencia al nodo padre de este nodo en el árbol que se genera durante un Pathfinding.

    public float Priority = Single.PositiveInfinity;
}

public class Edge
{
    public Edge(string inName, Node inA, Node inB, float inWeight = 1.0f)
    {
        Name = inName;
        A = inA;
        B = inB;
        Weight = inWeight;
    }

    public string Name = ""; // es string por pura claridad, las aristas normalmente no necesitan un nombre.
    public Node A;
    public Node B;
    public float Weight = 1.0f;

    // EdgeA == EdgeB
    // Si son punteros/referencias pues nomás comparan la dirección de memoria y ya.
    // PERO SI NO, ustedes tendrían que comparar una o más cosas.
    // Por ejemplo podríamos checar EdgeA.A == EdgeB.A && EdgeA.B == EdgeB.B && EdgeA.Weight == EdgeB.Weight

    // Un hash te da un solo número que representa a ese objeto.

    // Vector3 A == Vector3 B?
    // A.x == B.x && A.y == B.y && A.z == B.z
}

public class Graph : MonoBehaviour
{
    private List<Node> PathToGoalBFS = new List<Node>(); // Declarada a nivel de clase

    // Podríamos guardarlos en un array.
    // Podríamos guardarlos en un List, Set
    // Dictionary, Queue, Stack, DynamicArray, Heap

    // Array:
    // Ventajas: super rápido de acceder de manera secuencial. Te da el espacio de memoria completo.
    // int [10]Array
    // Desventajas: Te da el espacio de memoria completo (lo vayas a usar o no, lo que puede llevar a desperdicios).
    // desventajas: su tamaño (capacidad de almacenamiento) es totalmente estático.
    // desventajas: poner y quitar elementos que hagan que cambie el tamaño del array es MUY lento.

    // ¿Qué es un "Set" en estructuras de datos / programación?
    // Un set es una estructura de datos que no permite repetidos
    // específicamente en nuestros grafos, no vamos a querer ni nodos ni aristas repetidas.

    protected HashSet<Node> NodeSet = new HashSet<Node>();
    protected HashSet<Edge> EdgeSet = new HashSet<Edge>();

    // Start is called before the first frame update
    void Start()
    {
        // Vamos a llenar nuestros sets de nodos y aristas.
        // Comenzamos creando todos los nodos, porque las aristas necesitan que ya existan los nodos.
        Node NodeA = new Node("A");
        Node NodeB = new Node("B");
        Node NodeC = new Node("C");
        Node NodeD = new Node("D");
        Node NodeE = new Node("E");
        Node NodeF = new Node("F");
        Node NodeG = new Node("G");
        Node NodeH = new Node("H");

        NodeSet.Add(NodeA);
        NodeSet.Add(NodeB);
        NodeSet.Add(NodeC);
        NodeSet.Add(NodeD);
        NodeSet.Add(NodeE);
        NodeSet.Add(NodeF);
        NodeSet.Add(NodeG);
        NodeSet.Add(NodeH);

        // Ahora queremos declarar las aristas.
        Edge EdgeAB = new Edge("AB", NodeA, NodeB);
        Edge EdgeAE = new Edge("AE", NodeA, NodeE);
        Edge EdgeBC = new Edge("BC", NodeB, NodeC);
        Edge EdgeBD = new Edge("BD", NodeB, NodeD);
        Edge EdgeEF = new Edge("EF", NodeE, NodeF);
        Edge EdgeEG = new Edge("EG", NodeE, NodeG);
        Edge EdgeEH = new Edge("EH", NodeE, NodeH);

        EdgeSet.Add(EdgeAB);
        EdgeSet.Add(EdgeAE);
        EdgeSet.Add(EdgeBC);
        EdgeSet.Add(EdgeBD);
        EdgeSet.Add(EdgeEF);
        EdgeSet.Add(EdgeEG);
        EdgeSet.Add(EdgeEH);

        // Prueba BFS desde H hacia D
        if (BFS(NodeH, NodeD, out PathToGoalBFS)) // Uso la variable a nivel de clase
        {
            Debug.Log("BFS: Sí hay camino del nodo: " + NodeH.Name + " hacia el nodo: " + NodeD.Name);
            PrintPath(PathToGoalBFS);
        }
        else
        {
            Debug.Log("BFS: No hay camino del nodo: " + NodeH.Name + " hacia el nodo: " + NodeD.Name);
        }

        // Reiniciar los nodos antes de hacer una nueva búsqueda
        ResetNodes(NodeSet);

        // Escenario sin camino (remover aristas para que no haya conexión entre H y D)
        EdgeSet.RemoveWhere(edge => edge.Name == "EH" || edge.Name == "EG");

        if (BFS(NodeH, NodeD, out PathToGoalBFS))
        {
            Debug.Log("BFS (sin camino): Sí hay camino del nodo: " + NodeH.Name + " hacia el nodo: " + NodeD.Name);
            PrintPath(PathToGoalBFS);
        }
        else
        {
            Debug.Log("BFS (sin camino): No hay camino del nodo: " + NodeH.Name + " hacia el nodo: " + NodeD.Name);
        }
    }

    // Reset para los nodos
    void ResetNodes(HashSet<Node> inNodeSet)
    {
        foreach (Node node in inNodeSet)
        {
            node.Parent = null;
        }
    }

    bool BFS(Node Origin, Node Goal, out List<Node> PathToGoal)
    {
        PathToGoal = new List<Node>();// Lo inicializamos en 0 por defecto por si no encontramos ningún camino.
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        // Con esto evitamos que algún otro nodo trate de meter al origin en los nodos por visitar.

        Origin.Parent = null;
        queue.Enqueue(Origin);
        visited.Add(Origin);

        while (queue.Count > 0)
        {
            Node currentNode = queue.Dequeue();

            if (currentNode == Goal)
            {
                PathToGoal = Backtrack(currentNode);
                return true;
            }

            List<Node> neighbors = GetNeighbors(currentNode);

            foreach (Node neighbor in neighbors)
            {
                if (!visited.Contains(neighbor))
                {
                    neighbor.Parent = currentNode;
                    visited.Add(neighbor);
                    queue.Enqueue(neighbor);
                }
            }
        }

        return false; // Si no encuentra un camino
    }

    // Backtrack para reconstruir el camino
    List<Node> Backtrack(Node inNode)
    {
        List<Node> path = new List<Node>();
        Node current = inNode;

        while (current != null)
        {
            path.Add(current);
            current = current.Parent;
        }

        path.Reverse(); // Invertir el camino para mostrarlo en el orden correcto
        return path;
    }

    // Método para imprimir el camino
    void PrintPath(List<Node> PathToGoal)
    {
        foreach (Node node in PathToGoal)
        {
            Debug.Log("El nodo: " + node.Name + " es parte del camino.");
        }
    }

    // Obtener vecinos de un nodo
    List<Node> GetNeighbors(Node inNode)
    {
        List<Node> Neighbors = new List<Node>();

        foreach (Edge currentEdge in EdgeSet)
        {
            if (currentEdge.A == inNode)
            {
                Neighbors.Add(currentEdge.B);
            }
            else if (currentEdge.B == inNode)
            {
                Neighbors.Add(currentEdge.A);
            }
        }

        return Neighbors;
    }

    // Nuestro DFS iterativo debe dar exactamente los mismos resultados que el recursivo.
    // Nos dice si hay un camino desde un Nodo Origen hasta un nodo Destino (de un grafo)
    // y si sí hay un camino, nos dice cuál fue. Esto del camino tiene un truco interesante: El Backtracking.
    // El camino nos lo pasará a través del parámetro de salida: PathToGoal (nótese el término "out" que lo marca como de salida).
    bool DFS(Node Origin, Node Goal, out List<Node> PathToGoal)
    {
        PathToGoal = new List<Node>(); // Lo inicializamos en 0 por defecto por si no encontramos ningún camino.

        // Para saber cuántos nodos hay todavía por visitar,
        // necesitamos llevar registro de cuáles nodos ya hemos visitado.
        // Necesitamos dos contenedores de nodos, uno para los ya visitados y otro para los conocidos.

        // Un Set es un contenedor perfecto para los visitados, 
        // ya que solo necesitamos saber si ya está dentro de visitados o no.

        HashSet<Node> VisitedNodes = new HashSet<Node>();

        // Podemos usar la estructura de datos Pila (stack) para reemplazar la Pila de llamadas que usaba la versión recursiva
        // del algoritmo para mantener su orden.
        // ¿Cuándo se meten nodos en la pila? En cuanto tu nodo actual lo puede alcanzar (tiene una arista con él), Y no 
        // tiene ya un padre asignado (el que no tenga parent quiere decir que ningún otro nodo ha llegado ya a este nuevo nodo).
        // Los nodos que todavía están en esta pila son los nodos que todavía hay por visitar.
        Stack<Node> KnownStack = new Stack<Node>();

        // Con esto evitamos que algún otro nodo trate de meter al origin en los nodos por visitar.
        Origin.Parent = Origin;
        // Para que no se termine el While inmediatamente (porque la KnownStack está vacía)
        // nosotros tenemos que meter al menos un nodo a dicha Stack. Metemos el único no que tenemos certeza de que podemos alcanzar ahorita.
        KnownStack.Push(Origin);

        Node CurrentNode = null;

        // Para "simular" la recursividad, necesitamos hacer un ciclo, ya sea un for o un while, etc.
        // Nuestro ciclo va a tener como condición de finalización las mismas condiciones que la versión recursiva:
        // es decir: 1) Ya llegué a la meta (goal); 2) No hay camino en absoluto,
        // esta condición 2, se cumple cuando ya visitaste TODOS los nodos que pudiste alcanzar y ninguno de ellos fue la meta (goal).
        while (CurrentNode != Goal && KnownStack.Count != 0)  /* todavía haya nodos por visitar */
        {
            // Las pilas (Stack) se trabajan sobre el elemento que está en el tope de la pila.
            CurrentNode = KnownStack.Peek(); // lee el elemento del tope de la pila PERO no lo saques.
            Debug.Log("Nodo: " + CurrentNode.Name);

            // Ahora queremos meter a la Pila a los vecinos de current que no tengan parent y que no estén en los visitados.
            // paso 1) Obtener sus vecinos
            List<Node> currentNeighbors = GetNeighbors(CurrentNode);

            // paso 2) filtrar a los que ya están en visitados.
            List<Node> nonVisitedNodes = RemoveVisitedNodes(currentNeighbors, VisitedNodes);

            // paso 3) filtrar a los que tengan parent.
            List<Node> nonParentNeighbors = RemoveNodesWithParent(nonVisitedNodes);

            // Ahora sí, ya podemos meter a la pila al primero de esa lista de los que quedaron después de filtrar (nonParentNeighbors)
            if (nonParentNeighbors.Count > 0)
            {
                // Como este nodo currentNode está metiendo a la stack al nodo "nonParentNeighbors[0]", entonces currentNode se vuelve su padre.
                nonParentNeighbors[0].Parent = CurrentNode;
                // entonces sí hay alguien a quien meter en la pila, y metemos al primer elemento de dicha lista.
                KnownStack.Push(nonParentNeighbors[0]);
                continue;
            }
            // Un nodo no se saca de la pila hasta que ya no tiene otro nodo a quien meter a la pila.
            Node PoppedNode = KnownStack.Pop();

            // Después de hacerle Pop, lo tenemos que meter a los visitados.
            VisitedNodes.Add(PoppedNode);
        }

        // Nos falta comprobar por qué se rompió el ciclo while de arriba.
        // Si esto se cumple, es porque sí llegamos a la meta.
        if (Goal == CurrentNode)
        {
            // Ahorita no hacemos nada más con ella, pero si lo quisiéramos hacer, pues de aquí la tomaríamos.
            PathToGoal = Backtrack(CurrentNode);
            return true;
        }
        // Si no, ¡pues no!
        return false;
    }

    List<Node> RemoveNodesWithParent(List<Node> NodesToBeFiltered)
    {
        List<Node> FilteredNeighbors = new List<Node>();
        foreach (Node neighbor in NodesToBeFiltered)
        {
            // ¿Este nodo tiene Parent? Si no, lo añadimos a los que vamos a regresar.
            if (neighbor.Parent == null)
            {
                FilteredNeighbors.Add(neighbor);
            }
        }
        return FilteredNeighbors;
    }

    List<Node> RemoveVisitedNodes(List<Node> NodesToBeFiltered, HashSet<Node> VisitedNodesSet)
    {
        List<Node> nonVisitedNodes = new List<Node>();
        foreach (Node neighbor in NodesToBeFiltered)
        {
            // Si los nodos visitados no contienen a este nodo, no lo quitamos.
            if (!VisitedNodesSet.Contains(neighbor))
                nonVisitedNodes.Add(neighbor);
        }
        return nonVisitedNodes;
    }

    // Funciones recursivas VS funciones iterativas.

    // las funciones recursivas son funciones que se mandan a llamar a sí mismas.
    void FuncionRecursiva(int Counter)
    {
        Debug.Log("Hola número: " + Counter);
        if (Counter == 10)
            return;
        FuncionRecursiva(Counter + 1);
    }

    // MyArray [0, 1, 2, 3, 4...]

    // MyStack [0]
    // [1, 0]
    // 2, 1, 0
    // 3, 2, 1, 0
    // Ahora vamoas a sacar elementos
    // sacas el 3, que es el último que metiste, y te quedaría:
    // 2, 1, 0
    // 1, 0, 
    // 0
    // Last in, First out
    // solo puedes sacar el último elemento que metiste.

    // Update is called once per frame
    void Update()
    {
    }

    void OnDrawGizmos()
    {
        if (PathToGoalBFS != null && PathToGoalBFS.Count > 1)
        {
            for (int i = 0; i < PathToGoalBFS.Count - 1; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(GetNodePosition(PathToGoalBFS[i]), GetNodePosition(PathToGoalBFS[i + 1]));
            }
        }
    }

    Vector3 GetNodePosition(Node node)
{
    // Forzar las posiciones manualmente para depuración
    switch (node.Name)
    {
        case "A":
            return new Vector3(0, 1, 0);
        case "B":
            return new Vector3(2, 1, 0);
        case "C":
            return new Vector3(4, 1, 0);
        case "D":
            return new Vector3(6, 1, 0);
        case "E":
            return new Vector3(0, 3, 0);
        case "F":
            return new Vector3(2, 3, 0);
        case "G":
            return new Vector3(4, 3, 0);
        case "H":
            return new Vector3(6, 3, 0);
        default:
            Debug.LogWarning("Nodo no encontrado: " + node.Name);
            return Vector3.zero; // Devolver una posición por defecto si no se encuentra el nodo
    }
}

}