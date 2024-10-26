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

    public Node Parent;  // referencia al nodo padre de este nodo en el �rbol que se genera durante un Pathfinding.

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
    // Si son punteros/referencias pues nom�s comparan la direcci�n de memoria y ya.
    // PERO SI NO, ustedes tendr�an que comparar una o m�s cosas.
    // Por ejemplo podr�amos checar EdgeA.A == EdgeB.A && EdgeA.B == EdgeB.B && EdgeA.Weight == EdgeB.Weight

    // Un hash te da un solo n�mero que representa a ese objeto.

    // Vector3 A == Vector3 B?
    // A.x == B.x && A.y == B.y && A.z == B.z
}

public class Graph : MonoBehaviour
{

    // Podr�amos guardarlos en un array.
    // Podr�amos guardarlos en un List, Set
    // Dictionary, Queue, Stack, DynamicArray, Heap

    // Array:
    // Ventajas: super r�pido de acceder de manera secuencial. Te da el espacio de memoria completo.
    // int [10]Array
    // Desventajas: Te da el espacio de memoria completo (lo vayas a usar o no, lo que puede llevar a desperdicios).
    // desventajas: su tama�o (capacidad de almacenamiento) es totalmente est�tico.
    // desventajas: poner y quitar elementos que hagan que cambie el tama�o del array es MUY lento.


    // �Qu� es un "Set" en estructuras de datos / programaci�n?
    // Un set es una estructura de datos que no permite repetidos
    // espec�ficamente en nuestros grafos, no vamos a querer ni nodos ni aristas repetidas.

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
        List<Node> PathToGoalBFS = new List<Node>();

        // OJO: no olviden poner el out antes de los par�metros que son de salida.
        if (BFS(NodeH, NodeD, out PathToGoalBFS))
        {
            Debug.Log("BFS: S� hay camino del nodo: " + NodeH.Name + " hacia el nodo: " + NodeD.Name);
            PrintPath(PathToGoalBFS);
        }
        else
        {
            Debug.Log("BFS: No hay camino del nodo: " + NodeH.Name + " hacia el nodo: " + NodeD.Name);
        }

        ResetNodes(NodeSet);
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
        PathToGoal = new List<Node>();// Lo inicializamos en 0 por defecto por si no encontramos ning�n camino.
        Queue<Node> queue = new Queue<Node>();
        HashSet<Node> visited = new HashSet<Node>();

        // Con esto evitamos que alg�n otro nodo trate de meter al origin en los nodos por visitar.

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

    // M�todo para imprimir el camino
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
    // y si s� hay un camino, nos dice cu�l fue. Esto del camino tiene un truco interesante: El Backtracking.
    // El camino nos lo pasar� a trav�s del par�metro de salida: PathToGoal (n�tese el t�rmino "out" que lo marca como de salida).
    bool DFS(Node Origin, Node Goal, out List<Node> PathToGoal)
    {
        PathToGoal = new List<Node>(); // Lo inicializamos en 0 por defecto por si no encontramos ning�n camino.

        // Para saber cu�ntos nodos hay todav�a por visitar,
        // necesitamos llevar registro de cu�les nodos ya hemos visitado.
        // Necesitamos dos contenedores de nodos, uno para los ya visitados y otro para los conocidos.

        // Un Set es un contenedor perfecto para los visitados, 
        // ya que solo necesitamos saber si ya est� dentro de visitados o no.

        HashSet<Node> VisitedNodes = new HashSet<Node>();

        // Podemos usar la estructura de datos Pila (stack) para reemplazar la Pila de llamadas que usaba la versi�n recursiva
        // del algoritmo para mantener su orden.
        // �Cu�ndo se meten nodos en la pila? En cuanto tu nodo actual lo puede alcanzar (tiene una arista con �l), Y no 
        // tiene ya un padre asignado (el que no tenga parent quiere decir que ning�n otro nodo ha llegado ya a este nuevo nodo).
        // Los nodos que todav�a est�n en esta pila son los nodos que todav�a hay por visitar.
        Stack<Node> KnownStack = new Stack<Node>();

        // Con esto evitamos que alg�n otro nodo trate de meter al origin en los nodos por visitar.
        Origin.Parent = Origin;
        // Para que no se termine el While inmediatamente (porque la KnownStack est� vac�a)
        // nosotros tenemos que meter al menos un nodo a dicha Stack. Metemos el �nico no que tenemos certeza de que podemos alcanzar ahorita.
        KnownStack.Push(Origin);

        Node CurrentNode = null;


        // Para "simular" la recursividad, necesitamos hacer un ciclo, ya sea un for o un while, etc.
        // Nuestro ciclo va a tener como condici�n de finalizaci�n las mismas condiciones que la versi�n recursiva:
        // es decir: 1) Ya llegu� a la meta (goal); 2) No hay camino en absoluto,
        // esta condici�n 2, se cumple cuando ya visitaste TODOS los nodos que pudiste alcanzar y ninguno de ellos fue la meta (goal).
        while (CurrentNode != Goal && KnownStack.Count != 0)  /* todav�a haya nodos por visitar */
        {
            // Las pilas (Stack) se trabajan sobre el elemento que est� en el tope de la pila.
            CurrentNode = KnownStack.Peek(); // lee el elemento del tope de la pila PERO no lo saques.
            Debug.Log("Nodo: " + CurrentNode.Name);

            // Ahora queremos meter a la Pila a los vecinos de current que no tengan parent y que no est�n en los visitados.
            // paso 1) Obtener sus vecinos
            List<Node> currentNeighbors = GetNeighbors(CurrentNode);

            // paso 2) filtrar a los que ya est�n en visitados.
            List<Node> nonVisitedNodes = RemoveVisitedNodes(currentNeighbors, VisitedNodes);

            // paso 3) filtrar a los que tengan parent.
            List<Node> nonParentNeighbors = RemoveNodesWithParent(nonVisitedNodes);

            // Ahora s�, ya podemos meter a la pila al primero de esa lista de los que quedaron despu�s de filtrar (nonParentNeighbors)
            if (nonParentNeighbors.Count > 0)
            {
                // Como este nodo currentNode est� metiendo a la stack al nodo "nonParentNeighbors[0]", entonces currentNode se vuelve su padre.
                nonParentNeighbors[0].Parent = CurrentNode;
                // entonces s� hay alguien a quien meter en la pila, y metemos al primer elemento de dicha lista.
                KnownStack.Push(nonParentNeighbors[0]);
                continue;
            }
            // Un nodo no se saca de la pila hasta que ya no tiene otro nodo a quien meter a la pila.
            Node PoppedNode = KnownStack.Pop();

            // Despu�s de hacerle Pop, lo tenemos que meter a los visitados.
            VisitedNodes.Add(PoppedNode);
        }

        // Nos falta comprobar por qu� se rompi� el ciclo while de arriba.
        // Si esto se cumple, es porque s� llegamos a la meta.
        if (Goal == CurrentNode)
        {
            // Ahorita no hacemos nada m�s con ella, pero si lo quisi�ramos hacer, pues de aqu� la tomar�amos.
            PathToGoal = Backtrack(CurrentNode);
            return true;
        }
        // Si no, �pues no!
        return false;
    }

    List<Node> RemoveNodesWithParent(List<Node> NodesToBeFiltered)
    {
        List<Node> FilteredNeighbors = new List<Node>();
        foreach (Node neighbor in NodesToBeFiltered)
        {
            // �Este nodo tiene Parent? Si no, lo a�adimos a los que vamos a regresar.
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


    // Vamos a implementar el algoritmo de depth-first search (DFS) usando la pila de llamadas,
    // de manera recursiva.
    // �Qu� pregunta nos va a responder o qu� resultado nos va a dar?
    // Pues nos dice si hay un camino desde un Nodo Origen hasta un nodo Destino (de un grafo)
    // y si s� hay un camino, nos dice cu�l fue. Esto del camino tiene un truco interesante.
    bool RecursiveDFS(Node Origin, Node Goal)
    {
        // Para evitar que alguien se vuelva padre del nodo ra�z de todo el �rbol
        // hacemos que el nodo ra�z del �rbol sea su propio padre.
        if (Origin.Parent == null)
        {
            // si esto se cumple, entonces este nodo es la ra�z del �rbol.
            Origin.Parent = Origin;
        }
        // La condici�n de terminaci�n de esta funci�n recursiva es 
        // "el nodo en el que estoy actualmente (Origin) es la meta (Goal)"
        if (Origin == Goal)
            return true;

        // Desde el nodo donde estamos ahorita, checamos cu�les son nuestros vecinos.
        // Los vecinos de este nodo son los que comparten una arista con �l.
        // lo que podemos hacer es revisar todas las aristas y obtener las que hagan referencia a este nodo.
        // 1) Checar todas las aristas.
        foreach (Edge currentEdge in EdgeSet)
        {
            bool Result = false;
            Node Neighbor = null;

            // Vamos a checar si la arista en cuesti�n hace referencia a este nodo "Origin"
            // Checamos su A y su B. Y tenemos que checar que el vecino NO tenga padre, para que Origin 
            // se convierta en su padre.

            if (currentEdge.A == Origin && currentEdge.B.Parent == null)
            {
                // entonces s� hace referencia a este nodo. Lo que hacemos es meter al Nodo vecino.
                // Si encontramos un vecino, primero le decimos que el nodo Origin actual es
                // su nodo padre, y despu�s mandamos a llamar la funci�n de nuevo, pero 
                // usando a este vecino como el nuevo origen.
                currentEdge.B.Parent = Origin;
                Neighbor = currentEdge.B;
            }
            else if (currentEdge.B == Origin && currentEdge.A.Parent == null)
            {
                // entonces s� hace referencia a este nodo. Lo que hacemos es meter al Nodo vecino.
                // Si encontramos un vecino, primero le decimos que el nodo Origin actual es
                // su nodo padre, y despu�s mandamos a llamar la funci�n de nuevo, pero 
                // usando a este vecino como el nuevo origen.
                currentEdge.A.Parent = Origin;
                Neighbor = currentEdge.A;
            }

            // Necesitamos esta comprobaci�n por si no entra ni al if ni al if else de arriba.
            if (Neighbor != null)
                Result = RecursiveDFS(Neighbor, Goal);

            if (Result == true)
            {
                // Si este nodo fue parte del camino al Goal, le decimos que imprima su nombre.
                Debug.Log("El nodo: " + Origin.Name + " fue parte del camino a la meta.");
                // entonces el hijo de este nodo ya encontr� el camino
                // y eso hace una reacci�n en cadena de "Pap�, s� encontr� el camino".
                return true;
            }
        }
        // si ya acab� el ciclo de intentar visitar a sus vecinos, se regresa a la funci�n del nodo
        // que es su padre.
        return false;
    }

    // Funciones recursivas VS funciones iterativas.

    // las funciones recursivas son funciones que se mandan a llamar a s� mismas.
    void FuncionRecursiva(int Counter)
    {
        Debug.Log("Hola n�mero: " + Counter);
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
    // sacas el 3, que es el �ltimo que metiste, y te quedar�a:
    // 2, 1, 0
    // 1, 0, 
    // 0
    // Last in, First out
    // solo puedes sacar el �ltimo elemento que metiste.

    // Update is called once per frame
    void Update()
    {
    }
}
